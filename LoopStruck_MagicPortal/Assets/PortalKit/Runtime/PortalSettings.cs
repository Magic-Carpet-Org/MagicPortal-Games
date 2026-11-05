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

        [Header("Lagos Life")]
        [Tooltip("Turn off to never try Lagos Life for this game.")]
        public bool enableLagosLife = true;

        [Tooltip("Your server endpoint that verifies and delivers Lagos Life purchases (see PortalKit/Server). A path like /api/lagos-claim is resolved against where the game is hosted.")]
        public string lagosLifeClaimUrl = "/api/lagos-claim";

        [Header("Editor")]
        [Tooltip("Editor only: pretend purchases are available and succeed, so you can test purchase UI in Play mode. Never used in builds.")]
        public bool editorSimulatePurchases = false;

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
