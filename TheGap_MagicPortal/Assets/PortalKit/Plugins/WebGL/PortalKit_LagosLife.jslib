// PortalKit_LagosLife.jslib (part of Kilimanjaro PortalKit)
// Bridges Unity <-> the Lagos Life (Vatar) game player via the official
// Vatar Games SDK (https://lagoslife.app/sdk/vatar-games-v1.js).
//
// Lives alongside PortalKit_MagicGames.jslib - the two never touch each
// other. Unity picks up .jslib files anywhere under Assets/.
//
// The SDK is loaded on demand from lagoslife.app (as their docs ask: don't
// self-host a copy) and only when the game is inside an iframe, so MagicGames
// and local test builds never download it.
//
// Results are delivered to the GameObject named exactly "PortalKit_LagosLife".

mergeInto(LibraryManager.library, {

	// Starts the Lagos Life handshake. Replies (via SendMessage) with
	// OnLagosInit(json) on success or OnLagosInitFailed(errorCode) on failure.
	PKLL_Init: function ()
	{
		var GO = "PortalKit_LagosLife";

		// Keep a sender that captures this framework scope's SendMessage.
		window.__llSend = function (method, arg)
		{
			arg = (arg === undefined || arg === null) ? "" : String(arg);

			if (typeof unityInstance !== "undefined" && unityInstance.SendMessage)
			{
				unityInstance.SendMessage(GO, method, arg);
			}
			else if (typeof SendMessage === "function")
			{
				SendMessage(GO, method, arg);
			}
			else
			{
				console.warn("[LagosLifeBridge] No Unity instance to deliver " + method);
			}
		};

		var send = window.__llSend;

		var inFrame = false;
		try { inFrame = window.self !== window.top; } catch (e) { inFrame = true; }

		if (!inFrame)
		{
			send("OnLagosInitFailed", "not_framed");
			return;
		}

		function startSdk()
		{
			Vatar.init().then(function (res)
			{
				window.__llReady = true;

				Vatar.onPause(function () { send("OnLagosPause", ""); });
				Vatar.onResume(function () { send("OnLagosResume", ""); });

				var p = res && res.player ? res.player : {};
				send("OnLagosInit", JSON.stringify({
					playerId: p.id || "",
					playerName: p.name || "",
					avatar: p.avatar || "",
					mode: res && res.mode ? res.mode : ""
				}));

				// --- Purchases: remember paid-but-undelivered ones so they're never lost ---
				window.__llUnclaimed = window.__llUnclaimed || [];

				window.__llKeep = function (item)
				{
					if (!item) return;
					var id = item.purchaseId || item.id;
					var product = item.productId || item.product;
					if (!id || !product) return;

					for (var i = 0; i < window.__llUnclaimed.length; i++)
					{
						if (window.__llUnclaimed[i].purchaseId === id) return;
					}

					window.__llUnclaimed.push({ purchaseId: id, productId: product });
				};

				window.__llReportUnclaimed = function ()
				{
					send("OnLagosUnclaimed", JSON.stringify({
						productIds: window.__llUnclaimed.map(function (x) { return x.productId; })
					}));
				};

				// Asks OUR server to verify the purchase with Lagos Life and mark it
				// delivered. Only a server holding the secret can do that safely.
				window.__llClaim = function (item, claimUrl)
				{
					return Vatar.session().then(function (s)
					{
						return fetch(claimUrl, {
							method: "POST",
							headers: { "Content-Type": "application/json" },
							body: JSON.stringify({ purchaseId: item.purchaseId, session: s && s.session })
						});
					}).then(function (r)
					{
						return r.json().catch(function () { return {}; }).then(function (j)
						{
							if (r.ok && j.ok) return j;

							var e = new Error("claim failed");
							e.code = j.error || ("http_" + r.status);
							e.final = !!j.final; // true = can never be delivered (refunded, wrong player...)
							throw e;
						});
					}).catch(function (err)
					{
						// Paid but not verified yet (network, expired token, server down):
						// keep it so the next attempt delivers it without charging again.
						if (!err || !err.final) window.__llKeep(item);
						if (!err || !err.code) { err = new Error("claim failed"); err.code = "claim_failed"; }
						throw err;
					});
				};

				if (Vatar.purchases)
				{
					Vatar.purchases().then(function (list)
					{
						var arr = Array.isArray(list) ? list : (list && (list.purchases || list.items)) || [];
						arr.forEach(window.__llKeep);
						window.__llReportUnclaimed();
					}).catch(function () { });
				}

				if (Vatar.onPurchase)
				{
					// Purchases confirmed outside a pending purchase() call.
					Vatar.onPurchase(function (item)
					{
						window.__llKeep(item);
						window.__llReportUnclaimed();
					});
				}
			}).catch(function (err)
			{
				send("OnLagosInitFailed", (err && err.code) ? err.code : "error");
			});
		}

		if (typeof Vatar !== "undefined")
		{
			startSdk();
			return;
		}

		var s = document.createElement("script");
		s.src = "https://lagoslife.app/sdk/vatar-games-v1.js";
		s.async = true;
		s.onload = function ()
		{
			if (typeof Vatar === "undefined") { send("OnLagosInitFailed", "sdk_missing"); return; }
			startSdk();
		};
		s.onerror = function () { send("OnLagosInitFailed", "sdk_load_failed"); };
		document.head.appendChild(s);
	},

	// Submits a score. Lagos Life accepts one submission per 20 s and keeps the
	// player's highest score, so if we're inside the cooldown we hold on to the
	// best pending score and send it as soon as the window reopens. Replies with
	// OnLagosScoreResult(json) or OnLagosScoreFailed(errorCode).
	PKLL_SubmitScore: function (score)
	{
		if (!window.__llReady || typeof Vatar === "undefined") return;

		var COOLDOWN = 20500; // 20 s + a little slack
		var st = window.__llScore || (window.__llScore = { last: 0, pending: -1, timer: null });

		st.pending = Math.max(st.pending, Math.floor(score));

		function flush()
		{
			st.timer = null;
			if (st.pending < 0) return;

			var n = st.pending;
			st.pending = -1;
			st.last = Date.now();

			Vatar.submitScore(n).then(function (res)
			{
				window.__llSend("OnLagosScoreResult", JSON.stringify({
					best: res && typeof res.best === "number" ? res.best : n,
					accepted: !!(res && res.accepted)
				}));
			}).catch(function (err)
			{
				var code = (err && err.code) ? err.code : "error";

				// Re-queue if we were too early or the server hiccuped.
				if (code === "too_soon" || code === "rate_limited" || code === "timeout" || code === "error")
				{
					st.pending = Math.max(st.pending, n);
					if (!st.timer) st.timer = setTimeout(flush, COOLDOWN);
				}

				window.__llSend("OnLagosScoreFailed", code);
			});
		}

		if (st.timer) return; // already scheduled; it will pick up the new best

		var wait = COOLDOWN - (Date.now() - st.last);
		if (wait <= 0) flush();
		else st.timer = setTimeout(flush, wait);
	},

	// Buys productId: reuses an unclaimed paid purchase if there is one, otherwise
	// opens the Lagos Life pay sheet (Vatar.purchase pauses the game itself).
	// Then our server verifies + delivers it. Replies with
	// OnLagosPurchaseResult({ success, productId, error }).
	PKLL_Purchase: function (productIdPtr, claimUrlPtr)
	{
		var productId = UTF8ToString(productIdPtr);
		var claimUrl = UTF8ToString(claimUrlPtr);

		function done(ok, code, owned)
		{
			window.__llBuying = false;
			if (window.__llReportUnclaimed) window.__llReportUnclaimed();

			window.__llSend("OnLagosPurchaseResult", JSON.stringify({
				success: ok,
				productId: productId,
				error: ok ? "" : (code || "error"),
				alreadyOwned: !!owned
			}));
		}

		if (!window.__llReady || typeof Vatar === "undefined" || !window.__llClaim)
		{
			window.__llSend("OnLagosPurchaseResult", JSON.stringify({ success: false, productId: productId, error: "not_ready" }));
			return;
		}

		if (window.__llBuying)
		{
			window.__llSend("OnLagosPurchaseResult", JSON.stringify({ success: false, productId: productId, error: "busy" }));
			return;
		}

		window.__llBuying = true;

		var list = window.__llUnclaimed;
		var reuse = null;

		for (var i = 0; i < list.length; i++)
		{
			if (list[i].productId === productId)
			{
				reuse = list.splice(i, 1)[0];
				break;
			}
		}

		var paid = reuse ? Promise.resolve(reuse) : Vatar.purchase(productId).then(function (res)
		{
			return { purchaseId: res && (res.purchaseId || res.id), productId: (res && res.productId) || productId };
		});

		paid.then(function (item)
		{
			return window.__llClaim(item, claimUrl);
		}).then(function ()
		{
			done(true, "");
		}).catch(function (err)
		{
			var code = err && err.code ? err.code : "error";

			// One-off product the player already bought (e.g. their save was wiped):
			// Lagos Life refuses to charge again, so give it back for free.
			if (code === "owned") { done(true, "", true); return; }

			done(false, code);
		});
	},

	// Fetches the top-50 leaderboard. Replies with OnLagosLeaderboard(json)
	// ({ top: [{rank, name, score}], me }) or OnLagosLeaderboardFailed(code).
	PKLL_GetLeaderboard: function ()
	{
		if (!window.__llReady || typeof Vatar === "undefined") return;

		Vatar.leaderboard().then(function (res)
		{
			window.__llSend("OnLagosLeaderboard", JSON.stringify(res || {}));
		}).catch(function (err)
		{
			window.__llSend("OnLagosLeaderboardFailed", (err && err.code) ? err.code : "error");
		});
	}

});
