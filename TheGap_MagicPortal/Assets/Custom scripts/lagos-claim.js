// api/lagos-claim.js  (Kilimanjaro PortalKit - Lagos Life purchase verification)
//
// Vercel serverless function. Put this file at  api/lagos-claim.js  in the same
// Vercel project as the game, so the game can call  /api/lagos-claim.
//
// Lagos Life requires the SERVER to verify every purchase and mark it delivered;
// the secret key must never be inside the WebGL build. This endpoint:
//   1. verifies the player's session token with Lagos Life  -> who is buying
//   2. fetches the purchase                                 -> paid? same player? known product?
//   3. marks it delivered                                   -> it can't be claimed twice
// and only then tells the game { ok: true } so it grants the item.
//
// Vercel > Project > Settings > Environment Variables:
//   LAGOS_SECRET    your Lagos Life secret (vtr_test_... while testing, vtr_live_... after approval)
//   LAGOS_PRODUCTS  comma-separated product ids this game sells, e.g. "second_chance"

const API = "https://lagoslife.app/api/games/v1";

function products() {
  return (process.env.LAGOS_PRODUCTS || "second_chance")
    .split(",")
    .map((s) => s.trim())
    .filter(Boolean);
}

async function lagos(path, secret, body) {
  const res = await fetch(API + path, {
    method: body === undefined ? "GET" : "POST",
    headers: {
      Authorization: "Bearer " + secret,
      "Content-Type": "application/json",
    },
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  let data = {};
  try {
    data = await res.json();
  } catch (e) {
    data = {};
  }

  return { ok: res.ok, status: res.status, data };
}

function reply(res, status, payload) {
  res.status(status).json(payload);
}

module.exports = async function handler(req, res) {
  // The Lagos Life player runs the game in a sandboxed iframe, so requests may
  // arrive with "Origin: null". Auth is the Lagos Life session token, not
  // cookies, so allowing any origin is safe here.
  res.setHeader("Access-Control-Allow-Origin", "*");
  res.setHeader("Access-Control-Allow-Methods", "POST, OPTIONS");
  res.setHeader("Access-Control-Allow-Headers", "Content-Type");
  res.setHeader("Cache-Control", "no-store");

  if (req.method === "OPTIONS") return res.status(204).end();
  if (req.method !== "POST") return reply(res, 405, { ok: false, error: "method_not_allowed" });

  const secret = process.env.LAGOS_SECRET;
  if (!secret) return reply(res, 500, { ok: false, error: "server_not_configured" });

  let body = req.body || {};
  if (typeof body === "string") {
    try {
      body = JSON.parse(body);
    } catch (e) {
      body = {};
    }
  }

  const purchaseId = typeof body.purchaseId === "string" ? body.purchaseId : "";
  const session = typeof body.session === "string" ? body.session : "";

  if (!purchaseId || purchaseId.length > 128 || !/^[A-Za-z0-9_\-:.]+$/.test(purchaseId) || !session) {
    return reply(res, 400, { ok: false, error: "bad_request" });
  }

  try {
    // 1. Who is asking?
    const who = await lagos("/session/verify", secret, { session });
    if (!who.ok) {
      // Token problems are temporary from the game's point of view (it fetches a
      // fresh token on the next try), so not final.
      return reply(res, 401, { ok: false, error: who.data.error || "invalid_session" });
    }
    const playerId = who.data.playerId;

    // 2. What did they pay for?
    const p = await lagos("/purchases/" + encodeURIComponent(purchaseId), secret);
    if (p.status === 404) return reply(res, 404, { ok: false, error: "not_found", final: true });
    if (!p.ok) return reply(res, 502, { ok: false, error: p.data.error || "lagos_error" });

    const purchase = p.data.purchase || p.data;
    const buyer = purchase.playerId || purchase.player_id;
    const productId = purchase.productId || purchase.product_id;
    const status = purchase.status;

    if (!buyer || buyer !== playerId) return reply(res, 403, { ok: false, error: "wrong_player", final: true });
    if (status === "refunded") return reply(res, 409, { ok: false, error: "refunded", final: true });
    if (status === "delivered") return reply(res, 409, { ok: false, error: "already_delivered", final: true });
    if (status !== "paid") return reply(res, 409, { ok: false, error: "not_paid" });

    // Unknown product = our configuration is wrong; keep it claimable (not final)
    // so fixing LAGOS_PRODUCTS lets the player still receive what they paid for.
    if (!products().includes(productId)) return reply(res, 400, { ok: false, error: "unknown_product" });

    // 3. Mark delivered (safe to repeat per Lagos Life docs).
    const d = await lagos("/purchases/" + encodeURIComponent(purchaseId) + "/deliver", secret, {});
    if (!d.ok) {
      const final = d.data.error === "refunded";
      return reply(res, d.status === 409 ? 409 : 502, { ok: false, error: d.data.error || "deliver_failed", final });
    }

    return reply(res, 200, { ok: true, purchaseId, productId, mode: who.data.mode || "" });
  } catch (err) {
    console.error("[lagos-claim]", err);
    return reply(res, 500, { ok: false, error: "server_error" });
  }
};
