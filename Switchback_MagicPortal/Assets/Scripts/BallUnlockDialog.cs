using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Kilimanjaro.Portals;

namespace TunnelGame
{
    /// <summary>
    /// Unlock dialog for a locked ball, shown instead of the instant coin unlock
    /// when the portal sells items (Lagos Life). Offers:
    ///   - Use N coins            (the original unlock; greyed out if not enough)
    ///   - Buy with Lagos Life    (real-money purchase, product id "ball_&lt;index&gt;")
    ///   - Cancel
    /// Built entirely in code on its own overlay canvas - no scene setup needed.
    /// </summary>
    public class BallUnlockDialog : MonoBehaviour
    {
        /// <summary>Lagos Life product id for a ball. Create these in the Lagos Life dashboard.</summary>
        public static string ProductIdFor(int playerInfoIndex)
        {
            return "ball_" + playerInfoIndex;
        }

        private static BallUnlockDialog open;

        private int index;
        private Action unlockWithCoins;
        private Action unlockPurchased;

        private Button coinsButton;
        private Button buyButton;
        private Button cancelButton;
        private Text statusText;
        private bool purchasing;

        /// <summary>
        /// Shows the dialog for playerInfoIndex. unlockWithCoins runs the original coin
        /// unlock; unlockPurchased unlocks without spending coins (already paid).
        /// font: pass the game's own UI font so the dialog matches the shop.
        /// </summary>
        public static void Show(int playerInfoIndex, Font font, Action unlockWithCoins, Action unlockPurchased)
        {
            if (open != null) return;

            GameManager.PlayerInfo info = GameManager.Instance.PlayerInfos[playerInfoIndex];
            float coins = GameManager.Instance.DropsCollected;
            bool canAfford = coins >= info.unlockAmount;
            string productId = ProductIdFor(playerInfoIndex);

            // ---- canvas ----
            GameObject canvasGo = new GameObject("BallUnlockDialog", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            if (FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            BallUnlockDialog dialog = canvasGo.AddComponent<BallUnlockDialog>();
            dialog.index = playerInfoIndex;
            dialog.unlockWithCoins = unlockWithCoins;
            dialog.unlockPurchased = unlockPurchased;
            open = dialog;

            if (font == null) font = DefaultFont();

            // ---- dimmer (blocks taps behind) ----
            GameObject dim = NewUi("Dim", canvasGo.transform);
            Stretch(dim);
            dim.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

            // ---- card ----
            GameObject card = NewUi("Card", dim.transform);
            Place(card, new Vector2(880f, 1120f), Vector2.zero);
            card.AddComponent<Image>().color = new Color(0.11f, 0.12f, 0.16f, 0.98f);

            Label(card.transform, font, "UNLOCK THIS BALL", 64, FontStyle.Bold, Color.white, new Vector2(800f, 100f), new Vector2(0f, 460f));

            GameObject icon = NewUi("Icon", card.transform);
            Place(icon, new Vector2(240f, 240f), new Vector2(0f, 270f));
            Image iconImage = icon.AddComponent<Image>();
            iconImage.sprite = info.icon;
            iconImage.color = info.iconColor;
            iconImage.preserveAspect = true;

            string coinsLabel = canAfford
                ? "Use " + info.unlockAmount + " coins"
                : "Need " + info.unlockAmount + " coins (you have " + coins + ")";

            dialog.coinsButton = NewButton(card.transform, font, "CoinsButton", coinsLabel, 40, new Color(0.85f, 0.62f, 0.12f, 1f), new Vector2(720f, 150f), new Vector2(0f, 40f));
            dialog.coinsButton.interactable = canAfford;

            string buyLabel = PortalManager.HasUnclaimedPurchase(productId) ? "Unlock (already paid)" : "Buy with Lagos Life";
            dialog.buyButton = NewButton(card.transform, font, "BuyButton", buyLabel, 46, new Color(0.18f, 0.74f, 0.36f, 1f), new Vector2(720f, 150f), new Vector2(0f, -140f));

            dialog.statusText = Label(card.transform, font, "", 34, FontStyle.Normal, new Color(1f, 0.85f, 0.35f, 1f), new Vector2(780f, 100f), new Vector2(0f, -290f));

            dialog.cancelButton = NewButton(card.transform, font, "CancelButton", "Cancel", 40, new Color(0.30f, 0.32f, 0.38f, 1f), new Vector2(420f, 110f), new Vector2(0f, -430f));

            dialog.coinsButton.onClick.AddListener(dialog.OnCoins);
            dialog.buyButton.onClick.AddListener(dialog.OnBuy);
            dialog.cancelButton.onClick.AddListener(dialog.Close);
        }

        private void OnCoins()
        {
            if (purchasing) return;

            Close();
            if (unlockWithCoins != null) unlockWithCoins();
        }

        private void OnBuy()
        {
            if (purchasing) return;

            purchasing = true;
            SetButtons(false);
            statusText.text = "Opening payment...";

            PortalManager.Purchase(ProductIdFor(index), result =>
            {
                if (this == null) return; // closed/destroyed meanwhile

                purchasing = false;

                if (result.success)
                {
                    Close();
                    if (unlockPurchased != null) unlockPurchased();
                    return;
                }

                SetButtons(true);

                if (result.Cancelled)
                    statusText.text = "";
                else if (result.error == "claim_failed")
                    statusText.text = "Payment received - still confirming. Tap Buy again to retry.";
                else
                    statusText.text = "Payment didn't go through. Please try again.";
            });
        }

        private void Close()
        {
            if (purchasing) return;

            if (open == this) open = null;
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (open == this) open = null;
        }

        private void SetButtons(bool on)
        {
            buyButton.interactable = on;
            cancelButton.interactable = on;

            // Coins button keeps its affordability state.
            if (!on) coinsButton.interactable = false;
            else coinsButton.interactable = GameManager.Instance.DropsCollected >= GameManager.Instance.PlayerInfos[index].unlockAmount;
        }

        // ---------- tiny UI helpers ----------

        private static Font DefaultFont()
        {
            Font f = null;
            try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch (Exception) { }
            if (f == null)
            {
                try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch (Exception) { }
            }
            return f;
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

        private static void Place(GameObject go, Vector2 size, Vector2 position)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
        }

        private static Text Label(Transform parent, Font font, string text, int size, FontStyle style, Color color, Vector2 box, Vector2 position)
        {
            GameObject go = NewUi("Text", parent);
            Place(go, box, position);

            Text t = go.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        private static Button NewButton(Transform parent, Font font, string name, string text, int textSize, Color color, Vector2 size, Vector2 position)
        {
            GameObject go = NewUi(name, parent);
            Place(go, size, position);

            Image bg = go.AddComponent<Image>();
            bg.color = color;

            Button b = go.AddComponent<Button>();
            b.targetGraphic = bg;

            Text label = Label(go.transform, font, text, textSize, FontStyle.Bold, Color.white, size, Vector2.zero);
            Stretch(label.gameObject);
            return b;
        }
    }
}
