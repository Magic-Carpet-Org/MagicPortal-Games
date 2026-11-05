// PortalKit_MagicGames.jslib (part of Kilimanjaro PortalKit)
// Bridges Unity <-> the MagicGames portal parent window, per the Switchback
// "Iframe postMessage Bridge" contract (SCORE_UPDATE, MATCH_COMPLETE,
// REQUEST_RESTART, RESTART_GAME, RESTART_ACK).
//
// Results are delivered to the GameObject named exactly "PortalKit_MagicGames".

mergeInto(LibraryManager.library, {

	// Sends a raw JSON string to the parent portal window.
	PKMG_SendMessage: function (jsonPtr)
	{
		var json = UTF8ToString(jsonPtr);
		var data = null;

		try
		{
			data = JSON.parse(json);
		}
		catch (e)
		{
			console.error("[MagicGamesBridge] Failed to parse outgoing message JSON:", json, e);
			return;
		}

		if (window.parent && window.parent !== window)
		{
			window.parent.postMessage(data, "*");
		}
	},

	// Attaches one window "message" listener that forwards RESTART_GAME into Unity.
	PKMG_RegisterListener: function ()
	{
		if (window.__pkmgListenerAttached)
		{
			return;
		}

		window.__pkmgListenerAttached = true;

		var GO = "PortalKit_MagicGames";

		window.addEventListener("message", function (event)
		{
			if (!event.data || event.data.type !== "RESTART_GAME")
			{
				return;
			}

			var sessionId = event.data.sessionId || "";

			if (typeof unityInstance !== "undefined" && unityInstance.SendMessage)
			{
				unityInstance.SendMessage(GO, "OnRestartGameFromJs", sessionId);
			}
			else if (typeof SendMessage === "function")
			{
				SendMessage(GO, "OnRestartGameFromJs", sessionId);
			}
			else
			{
				console.warn("[MagicGamesBridge] No Unity instance available to deliver RESTART_GAME.");
			}
		});
	}

});
