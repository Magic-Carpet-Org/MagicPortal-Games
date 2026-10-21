using UnityEngine;
using System;
using System.Runtime.InteropServices;

/// <summary>
/// Talks to the MagicGames portal parent window using the FreeFall
/// "Iframe postMessage Bridge" contract (SCORE_UPDATE, MATCH_COMPLETE,
/// REQUEST_RESTART / RESTART_GAME / RESTART_ACK).
///
/// This intentionally does NOT call the signed REST endpoints
/// (GET /game/init, POST /game/match/complete) or embed the private
/// webhook secret - see ApiManager.cs for that.
///
/// Unlike GameManager/Player in this project, this needs to survive scene
/// changes (Game -> Game Over -> Game again on replay), since the portal
/// doesn't know or care about Unity's scene loads. Create ONE GameObject
/// named exactly "PortalBridge" in whichever scene loads first, attach this,
/// and it will persist itself via DontDestroyOnLoad.
/// </summary>
public class PortalBridge : MonoBehaviour
{
	#region Native Bindings

#if UNITY_WEBGL && !UNITY_EDITOR
	[DllImport("__Internal")] private static extern void PB_SendMessage(string json);
	[DllImport("__Internal")] private static extern void PB_RegisterListener();
#endif

	#endregion

	#region Events

	/// <summary>
	/// Fired after a new session id has been received from the portal
	/// and acknowledged (RESTART_ACK already sent).
	/// </summary>
	public event Action<string> RestartGameReceived;

	#endregion

	#region Member Variables

	private static PortalBridge instance;
	private string currentSessionId;

	#endregion

	#region Properties

	public static PortalBridge Instance { get { return instance; } }

	public string CurrentSessionId { get { return currentSessionId; } }

	/// <summary>
	/// True if we're actually running inside the portal iframe with a
	/// valid session (i.e. a real WebGL build, not a local test build).
	/// </summary>
	public bool IsPortalSession { get { return !string.IsNullOrEmpty(currentSessionId); } }

	#endregion

	#region Unity Methods

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			Destroy(gameObject);
			return;
		}

		instance = this;

		// The WebGL .jslib delivers RESTART_GAME via
		// unityInstance.SendMessage("PortalBridge", ...), which does an
		// exact, case-sensitive name lookup for a GameObject in the scene.
		gameObject.name = "PortalBridge";

		if (transform.parent != null)
		{
			transform.SetParent(null, false);
		}

		DontDestroyOnLoad(gameObject);

		currentSessionId = ParseSessionIdFromUrl();

#if UNITY_WEBGL && !UNITY_EDITOR
		PB_RegisterListener();
#endif
	}

	#endregion

	#region Public Methods - Outgoing

	/// <summary>
	/// Broadcasts a real-time score milestone. Call this sparingly
	/// (e.g. throttled), not every frame.
	/// </summary>
	public void SendScoreUpdate(int score)
	{
		Send("SCORE_UPDATE", score);
	}

	/// <summary>
	/// Broadcasts that the current match/run has ended with a final score.
	/// </summary>
	public void SendMatchComplete(int score)
	{
		Send("MATCH_COMPLETE", score);
	}

	/// <summary>
	/// Asks the portal to spin up a new session for a replay. The portal
	/// responds asynchronously with RESTART_GAME, which arrives via
	/// OnRestartGameFromJs below and fires RestartGameReceived once
	/// applied and acknowledged.
	/// </summary>
	public void RequestRestart()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		PB_SendMessage("{\"type\":\"REQUEST_RESTART\"}");
#else
		// No portal to talk to outside of a real WebGL build (editor /
		// standalone testing) - simulate an immediate new session so
		// gameplay logic that depends on RestartGameReceived still works.
		Debug.Log("[PortalBridge] REQUEST_RESTART (simulated outside WebGL build)");
		OnRestartGameFromJs(Guid.NewGuid().ToString());
#endif
	}

	#endregion

	#region Incoming (called by PortalBridge.jslib via GameObject.SendMessage)

	// NOTE: must stay named exactly "OnRestartGameFromJs" - the .jslib
	// calls it by string name via SendMessage("PortalBridge", ...).
	public void OnRestartGameFromJs(string sessionId)
	{
		currentSessionId = sessionId;

		AcknowledgeRestart();

		if (RestartGameReceived != null)
		{
			RestartGameReceived(sessionId);
		}
	}

	#endregion

	#region Private Methods

	private void AcknowledgeRestart()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		PB_SendMessage("{\"type\":\"RESTART_ACK\"}");
#else
		Debug.Log("[PortalBridge] RESTART_ACK (no-op outside WebGL build)");
#endif
	}

	private void Send(string type, int score)
	{
#if UNITY_WEBGL && !UNITY_EDITOR
		PB_SendMessage("{\"type\":\"" + type + "\",\"score\":" + score + "}");
#else
		Debug.Log(string.Format("[PortalBridge] {0} score={1} (no-op outside WebGL build)", type, score));
#endif
	}

	/// <summary>
	/// Pulls sessionId out of the page's query string, e.g.
	/// https://.../index.html?sessionId=4a71efc5-...
	/// </summary>
	private string ParseSessionIdFromUrl()
	{
		string url = null;

#if UNITY_WEBGL && !UNITY_EDITOR
		url = Application.absoluteURL;
#endif

		if (string.IsNullOrEmpty(url) || url.IndexOf('?') < 0)
		{
			return null;
		}

		string query = url.Substring(url.IndexOf('?') + 1);
		string[] pairs = query.Split('&');

		for (int i = 0; i < pairs.Length; i++)
		{
			string[] kv = pairs[i].Split('=');

			if (kv.Length == 2 && kv[0] == "sessionId")
			{
				return Uri.UnescapeDataString(kv[1]);
			}
		}

		return null;
	}

	#endregion
}
