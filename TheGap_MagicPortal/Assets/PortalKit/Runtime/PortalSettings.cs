using UnityEngine;

namespace Kilimanjaro.Portals
{
    /// <summary>
    /// Per-game portal settings. Create one per game with
    /// Assets > Create > Kilimanjaro > Portal Settings, name it exactly
    /// "PortalSettings" and keep it in a "Resources" folder.
    ///
    /// If the asset is missing, defaults are used (MagicGames REST calls are
    /// skipped because there is no secret).
    /// </summary>
    [CreateAssetMenu(fileName = "PortalSettings", menuName = "Kilimanjaro/Portal Settings")]
    public class PortalSettings : ScriptableObject
    {
        public const string ResourcePath = "PortalSettings";

        [Header("General")]
        [Tooltip("Live score is sent to the portal at most once per this many points.")]
        public int progressStep = 50;

        [Header("MagicGames")]
        public string magicGamesApiBaseUrl = "https://game-portal-cyan.vercel.app/api";

        [Tooltip("Webhook secret for this game. NOTE: anything here ships inside the WebGL build.")]
        public string magicGamesWebhookSecret = "";

        [Tooltip("After each RESTART_ACK, also post SESSION_UPDATED { sessionId } to the portal. Turn on for games whose original MagicGames code sent it (e.g. Loopstruck).")]
        public bool magicGamesSendSessionUpdated = false;

        [Tooltip("Editor only: a real MagicGames session id to test against from Play mode. Leave empty to run the Editor with no portal session. Never used in builds.")]
        public string editorTestSessionId = "";

        [Header("Other portals on the MagicGames API (e.g. Kick)")]
        [Tooltip("Portals that run the same API as MagicGames on their own server. The game uses the first entry whose 'Host Contains' text appears in the address the game is hosted at; otherwise it uses MagicGames above.")]
        public MagicGamesServer[] otherMagicGamesServers = new MagicGamesServer[0];

        [Tooltip("Editor only: name of an entry above to use in Play mode (e.g. Kick). Empty = MagicGames. Never used in builds.")]
        public string editorServer = "";

        [Header("Lagos Life")]
        [Tooltip("Turn off to never try Lagos Life for this game.")]
        public bool enableLagosLife = true;

        [Tooltip("Your server endpoint that verifies and delivers Lagos Life purchases (see PortalKit/Server). A path like /api/lagos-claim is resolved against where the game is hosted.")]
        public string lagosLifeClaimUrl = "/api/lagos-claim";

        [Header("Editor")]
        [Tooltip("Editor only: pretend purchases are available and succeed, so you can test purchase UI in Play mode. Never used in builds.")]
        public bool editorSimulatePurchases = false;

        [System.Serializable]
        public class MagicGamesServer
        {
            [Tooltip("Shown in logs, e.g. Kick.")]
            public string name = "Kick";

            [Tooltip("Use this server when the game's own web address contains this text (not case-sensitive). E.g. 'kick' matches thegap-kick-build.vercel.app.")]
            public string hostContains = "kick";

            [Tooltip("Base URL for /game/init and /game/match/complete, e.g. https://kick-portal.example.com/api")]
            public string apiBaseUrl = "";

            [Tooltip("This game's webhook secret on that server. NOTE: ships inside the WebGL build.")]
            public string webhookSecret = "";
        }

        /// <summary>
        /// Picks the MagicGames-API server for this run: an entry of
        /// otherMagicGamesServers whose hostContains appears in the game's own
        /// address (builds), or the one named by editorServer (Editor);
        /// otherwise MagicGames itself.
        /// </summary>
        public MagicGamesServer ResolveMagicGamesServer(string pageUrl)
        {
            MagicGamesServer fallback = new MagicGamesServer
            {
                name = "MagicGames",
                hostContains = "",
                apiBaseUrl = magicGamesApiBaseUrl,
                webhookSecret = magicGamesWebhookSecret
            };

            MagicGamesServer chosen = fallback;
            MagicGamesServer[] others = otherMagicGamesServers ?? new MagicGamesServer[0];

#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(editorServer))
            {
                foreach (MagicGamesServer s in others)
                {
                    if (s != null && string.Equals(s.name, editorServer, System.StringComparison.OrdinalIgnoreCase)) { chosen = s; break; }
                }
            }
#else
            string host = "";
            try { host = new System.Uri(pageUrl).Host.ToLowerInvariant(); } catch (System.Exception) { }

            if (!string.IsNullOrEmpty(host))
            {
                foreach (MagicGamesServer s in others)
                {
                    if (s != null && !string.IsNullOrEmpty(s.hostContains) && host.Contains(s.hostContains.Trim().ToLowerInvariant())) { chosen = s; break; }
                }
            }
#endif

            return new MagicGamesServer
            {
                name = chosen.name,
                hostContains = chosen.hostContains,
                apiBaseUrl = (chosen.apiBaseUrl ?? "").Trim().TrimEnd('/'),
                webhookSecret = (chosen.webhookSecret ?? "").Trim()
            };
        }

        private static PortalSettings loaded;

        public static PortalSettings Load()
        {
            if (loaded == null)
            {
                loaded = Resources.Load<PortalSettings>(ResourcePath);

                if (loaded == null)
                {
                    Debug.LogWarning("[PortalKit] No Resources/PortalSettings asset found - using defaults.");
                    loaded = CreateInstance<PortalSettings>();
                }
            }

            return loaded;
        }
    }
}
