using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TechJuego.GetOut.Utils;
using TechJuego.GetOut.Sound;
using Kilimanjaro.Portals;

namespace TechJuego.GetOut
{
    /// <summary>
    /// "Second Chance" offer shown when the ball dies on a portal that sells
    /// items (Lagos Life). Buying it resumes the same run with the score kept;
    /// skipping or letting the timer run out goes to the normal Game Over.
    ///
    /// Payment happens in Lagos Life's own pay sheet (we never draw one), and
    /// the purchase is verified by our server before the run continues.
    /// </summary>
    public class SecondChancePanel : MonoBehaviour
    {
        [SerializeField] private Button m_BuyButton;
        [SerializeField] private Button m_SkipButton;

        [Tooltip("Optional: short status line (\"Opening payment...\", errors).")]
        [SerializeField] private TMP_Text m_StatusText;

        [Header("Optional countdown")]
        [SerializeField] private TMP_Text m_TimerText;
        [SerializeField] private Image m_TimerFill;
        [SerializeField] private float m_OfferSeconds = 6f;

        private float timeLeft;
        private bool purchasing;
        private bool closed;

        // ==========================================
        // AUTO-BUILT PANEL (used when none is wired in the Inspector)
        // ==========================================

        private static readonly Color Dim = new Color(0f, 0f, 0f, 0.75f);
        private static readonly Color Card = new Color(0.11f, 0.12f, 0.16f, 0.98f);
        private static readonly Color Green = new Color(0.18f, 0.74f, 0.36f, 1f);
        private static readonly Color Grey = new Color(0.30f, 0.32f, 0.38f, 1f);
        private static readonly Color Soft = new Color(1f, 1f, 1f, 0.75f);
        private static readonly Color Notice = new Color(1f, 0.85f, 0.35f, 1f);

        /// <summary>
        /// Builds a complete Second Chance panel in code on its own overlay canvas
        /// (portrait 1080x1920 reference). Returned inactive - activate it to show.
        /// </summary>
        public static SecondChancePanel CreateDefault()
        {
            // Own canvas so it always sits on top and scales the same on every screen.
            GameObject canvasGo = new GameObject("SecondChanceCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            // Buttons need an EventSystem; the game normally has one already.
            if (FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            // Root = full-screen dimmer that also blocks taps on the game behind it.
            GameObject root = NewUi("SecondChancePanel", canvasGo.transform);
            root.SetActive(false); // so OnEnable runs only after the fields are set
            Stretch(root);
            root.AddComponent<Image>().color = Dim;

            GameObject card = NewUi("Card", root.transform);
            Place(card, new Vector2(0.5f, 0.5f), new Vector2(880f, 1040f), Vector2.zero);
            card.AddComponent<Image>().color = Card;

            Label(card.transform, "Title", "SECOND CHANCE", 84f, FontStyles.Bold, Color.white, new Vector2(800f, 110f), new Vector2(0f, 400f));
            Label(card.transform, "Subtitle", "Keep your score and continue this run", 42f, FontStyles.Normal, Soft, new Vector2(780f, 120f), new Vector2(0f, 280f));

            TMP_Text timer = Label(card.transform, "Timer", "6", 170f, FontStyles.Bold, Color.white, new Vector2(400f, 220f), new Vector2(0f, 90f));
            TMP_Text status = Label(card.transform, "Status", "", 36f, FontStyles.Normal, Notice, new Vector2(780f, 110f), new Vector2(0f, -70f));

            Button buy = NewButton(card.transform, "BuyButton", "CONTINUE", Green, 64f, new Vector2(700f, 170f), new Vector2(0f, -230f));
            Button skip = NewButton(card.transform, "SkipButton", "No thanks", Grey, 44f, new Vector2(460f, 110f), new Vector2(0f, -410f));

            SecondChancePanel panel = root.AddComponent<SecondChancePanel>();
            panel.m_BuyButton = buy;
            panel.m_SkipButton = skip;
            panel.m_StatusText = status;
            panel.m_TimerText = timer;

            return panel;
        }

        private static GameObject NewUi(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Stretch(GameObject go)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void Place(GameObject go, Vector2 anchor, Vector2 size, Vector2 position)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
        }

        private static TMP_Text Label(Transform parent, string name, string text, float size, FontStyles style, Color color, Vector2 box, Vector2 position)
        {
            GameObject go = NewUi(name, parent);
            Place(go, new Vector2(0.5f, 0.5f), box, position);

            TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            return t;
        }

        private static Button NewButton(Transform parent, string name, string text, Color color, float textSize, Vector2 size, Vector2 position)
        {
            GameObject go = NewUi(name, parent);
            Place(go, new Vector2(0.5f, 0.5f), size, position);

            Image bg = go.AddComponent<Image>();
            bg.color = color;

            Button b = go.AddComponent<Button>();
            b.targetGraphic = bg;

            TMP_Text label = Label(go.transform, "Label", text, textSize, FontStyles.Bold, Color.white, size, Vector2.zero);
            Stretch(label.gameObject);
            return b;
        }

        private void OnEnable()
        {
            transform.SetAsLastSibling();

            purchasing = false;
            closed = false;
            timeLeft = m_OfferSeconds;

            Utility.SetButton(m_BuyButton, OnClickBuy);
            Utility.SetButton(m_SkipButton, OnClickSkip);
            SetButtons(true);

            // Paid earlier but the game reloaded before it was delivered: it's free now.
            string productId = GameManager.Instance.secondChanceProductId;
            SetStatus(PortalManager.HasUnclaimedPurchase(productId) ? "You already paid - tap to use your Second Chance" : "");

            UpdateTimerUi();
        }

        private void Update()
        {
            // Hold the countdown while the pay sheet is open or the portal paused us.
            if (closed || purchasing || PortalManager.IsPaused) return;

            timeLeft -= Time.unscaledDeltaTime;
            UpdateTimerUi();

            if (timeLeft <= 0f)
            {
                Close(false);
            }
        }

        private void OnClickBuy()
        {
            if (purchasing || closed) return;

            purchasing = true;
            SetButtons(false);
            SetStatus("Opening payment...");
            SoundEvents.OnPlaySingleShotSound?.Invoke("Click");

            PortalManager.Purchase(GameManager.Instance.secondChanceProductId, result =>
            {
                // The panel may have been destroyed (scene reload) while paying.
                if (this == null) return;

                purchasing = false;

                if (result.success)
                {
                    Close(true);
                    return;
                }

                // Give them the full time again to decide.
                timeLeft = m_OfferSeconds;
                SetButtons(true);

                if (result.Cancelled)
                    SetStatus("");
                else if (result.error == "claim_failed")
                    SetStatus("Payment received - still confirming. Tap again to retry.");
                else
                    SetStatus("Payment didn't go through. Try again or skip.");
            });
        }

        private void OnClickSkip()
        {
            if (purchasing || closed) return;

            SoundEvents.OnPlaySingleShotSound?.Invoke("Click");
            Close(false);
        }

        private void Close(bool bought)
        {
            if (closed) return;
            closed = true;

            gameObject.SetActive(false);

            if (bought)
                GameManager.Instance.ContinueWithSecondChance();
            else
                GameManager.Instance.DeclineSecondChance();
        }

        private void SetButtons(bool interactable)
        {
            if (m_BuyButton != null) m_BuyButton.interactable = interactable;
            if (m_SkipButton != null) m_SkipButton.interactable = interactable;
        }

        private void SetStatus(string text)
        {
            if (m_StatusText != null) m_StatusText.text = text;
        }

        private void UpdateTimerUi()
        {
            float t = Mathf.Max(0f, timeLeft);

            if (m_TimerText != null) m_TimerText.text = Mathf.CeilToInt(t).ToString();
            if (m_TimerFill != null) m_TimerFill.fillAmount = m_OfferSeconds > 0f ? t / m_OfferSeconds : 0f;
        }
    }
}
