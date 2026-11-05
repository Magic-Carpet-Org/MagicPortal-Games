using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Kilimanjaro.Portals;

namespace TunnelGame
{
    public class PlayerSelectListItem : MonoBehaviour
    {
        #region Inspector Variables

        [SerializeField] private Image iconImage;
        [SerializeField] private Text coinAmountText;
        [SerializeField] private GameObject unlockButton;
        [SerializeField] private GameObject selectButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.white;

        #endregion

        #region Member Variables

        private int playerInfoIndex;

        #endregion

        #region Unity Methods

        private void Update()
        {
            backgroundImage.color = (playerInfoIndex == GameManager.Instance.CurrentPlayerIndex) ? backgroundImage.color = selectedColor : normalColor;
        }

        #endregion

        #region Public Methods

        public void Setup(int playerInfoIndex)
        {
            this.playerInfoIndex = playerInfoIndex;

            GameManager.PlayerInfo playerInfo = GameManager.Instance.PlayerInfos[playerInfoIndex];

            iconImage.sprite = playerInfo.icon;
            iconImage.color = playerInfo.iconColor;

            coinAmountText.text = playerInfo.unlockAmount.ToString();

            unlockButton.SetActive(playerInfo.locked);
            selectButton.SetActive(!playerInfo.locked);
        }

        public void UnlockClicked()
        {
            // Lagos Life sells balls: let the player choose coins or a real-money unlock.
            if (PortalManager.SupportsPurchases)
            {
                BallUnlockDialog.Show(playerInfoIndex, coinAmountText != null ? coinAmountText.font : null, UnlockWithCoins, UnlockPurchased);
                return;
            }

            UnlockWithCoins();
        }

        // The original coin unlock.
        private void UnlockWithCoins()
        {
            GameManager.PlayerInfo playerInfo = GameManager.Instance.PlayerInfos[playerInfoIndex];

            if (GameManager.Instance.DropsCollected >= playerInfo.unlockAmount)
            {
                GameManager.Instance.DropsCollected -= playerInfo.unlockAmount;
                MarkUnlocked(playerInfo);
            }
        }

        // Paid with real money on Lagos Life (already verified by our server): no coins spent.
        private void UnlockPurchased()
        {
            GameManager.PlayerInfo playerInfo = GameManager.Instance.PlayerInfos[playerInfoIndex];

            if (playerInfo.locked)
            {
                MarkUnlocked(playerInfo);
            }
        }

        private void MarkUnlocked(GameManager.PlayerInfo playerInfo)
        {
            GameManager.Instance.SwapPlayer(playerInfoIndex);
            GameManager.Instance.UnlockedPlayerInfos += ";" + playerInfoIndex;

            playerInfo.locked = false;

            unlockButton.SetActive(playerInfo.locked);
            selectButton.SetActive(!playerInfo.locked);
        }

        public void SelectClicked()
        {
            GameManager.Instance.SwapPlayer(playerInfoIndex);
        }

        #endregion
    }
}