using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TechJuego.GetOut.Utils;
using TechJuego.GetOut.Sound;

namespace TechJuego.GetOut
{
    public class MainMenuPanel : MonoBehaviour
    {
        public static MainMenuPanel Instance;
        public MenuPanel m_MenuPanel;
        public SettingsPannel m_SettingPanel;

        private void Awake()
        {
            Instance = this;
        }
        private void OnEnable()
        {
            DataHandler.Instance.gameState = GameState.Menu;
            m_MenuPanel.gameObject.SetActive(true);
            m_SettingPanel.gameObject.SetActive(false);

            SoundEvents.OnPlayLoopSound?.Invoke("BGMUSIC");
        }
    }
}