// LagosLifeBridge.jslib
// Bridges Unity <-> the Lagos Life (Vatar) game player via the official
// Vatar Games SDK (https://lagoslife.app/sdk/vatar-games-v1.js).
//
// Lives alongside PortalBridge.jslib (MagicGames) - the two never touch each
// other. This file must live under Assets/Plugins/WebGL/.
//
// The SDK is loaded on demand from lagoslife.app (as their docs ask: don't
// self-host a copy) and only when the game is inside an iframe, so MagicGames
// and local test builds never download it.
//
// Results are delivered to the GameObject named exactly "LagosLifeBridge".

mergeInto(LibraryManager.library, {

	// Starts the Lagos Life handshake. Replies (via SendMessage) with
	// OnLagosInit(json) on success or OnLagosInitFailed(errorCode) on failure.
	LL_Init: function ()
	{
		var GO = "LagosLifeBridge";

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
	LL_SubmitScore: function (score)
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

	// Fetches the top-50 leaderboard. Replies with OnLagosLeaderboard(json)
	// ({ top: [{rank, name, score}], me }) or OnLagosLeaderboardFailed(code).
	LL_GetLeaderboard: function ()
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
