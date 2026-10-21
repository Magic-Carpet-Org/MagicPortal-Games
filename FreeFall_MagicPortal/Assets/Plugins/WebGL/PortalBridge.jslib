// PortalBridge.jslib
// Bridges Unity <-> the MagicGames portal parent window, per the FreeFall
// "Iframe postMessage Bridge" contract. This file must live under
// Assets/Plugins/WebGL/ so Unity includes it only in WebGL builds.
//
// IMPORTANT: This plugin intentionally never touches the private webhook
// secret. It only sends/receives the public postMessage events
// (SCORE_UPDATE, MATCH_COMPLETE, REQUEST_RESTART, RESTART_GAME, RESTART_ACK).
// HMAC-signed REST calls are made separately by ApiManager.cs.

mergeInto(LibraryManager.library, {

	// Sends a raw JSON string to the parent portal window.
	PB_SendMessage: function (jsonPtr)
	{
		var json = UTF8ToString(jsonPtr);
		var data = null;

		try
		{
			data = JSON.parse(json);
		}
		catch (e)
		{
			console.error("[PortalBridge] Failed to parse outgoing message JSON:", json, e);
			return;
		}

		if (window.parent)
		{
			window.parent.postMessage(data, "*");
		}
	},

	// Registers a single window "message" listener that forwards
	// RESTART_GAME events into Unity via SendMessage. Safe to call more than
	// once; only attaches the listener the first time.
	PB_RegisterListener: function ()
	{
		if (window.__portalBridgeListenerAttached)
		{
			return;
		}

		window.__portalBridgeListenerAttached = true;

		window.addEventListener("message", function (event)
		{
			if (!event.data || typeof event.data.type !== "string")
			{
				return;
			}

			if (event.data.type === "RESTART_GAME")
			{
				var sessionId = event.data.sessionId || "";

				// Target GameObject must be named exactly "PortalBridge" in the scene.
				if (typeof unityInstance !== "undefined" && unityInstance.SendMessage)
				{
					unityInstance.SendMessage("PortalBridge", "OnRestartGameFromJs", sessionId);
				}
				else if (typeof SendMessage === "function")
				{
					// Older Unity WebGL loader templates expose a global SendMessage.
					SendMessage("PortalBridge", "OnRestartGameFromJs", sessionId);
				}
				else
				{
					console.warn("[PortalBridge] No Unity instance available to deliver RESTART_GAME.");
				}
			}
		});
	}

});
