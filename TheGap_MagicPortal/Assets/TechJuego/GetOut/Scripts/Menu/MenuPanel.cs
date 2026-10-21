using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using TechJuego.GetOut.Utils;
using TechJuego.GetOut.Sound;

namespace TechJuego.GetOut
{
    public class MenuPanel : MonoBehaviour
    {
        public Button m_PlayButton;
        public Button m_SettingButton;
        public TextMeshProUGUI m_HighScore;
        private void OnEnable()
        {
            Utility.SetButton(m_PlayButton, OnClickPlayButton);
            Utility.SetButton(m_SettingButton, OnClickSettingButton);
            m_HighScore.text = "HighScore:-" + DataHandler.Instance.Highscore.ToString();
        }
        private void OnClickPlayButton()
        {
            SoundEvents.OnPlaySingleShotSound?.Invoke("Click");
            SceneLoader.LoadScene("Game");
        }
        private void OnClickSettingButton()
        {
            SoundEvents.OnPlaySingleShotSound?.Invoke("Click");
            MainMenuPanel.Instance. m_SettingPanel.gameObject.SetActive(true);
        }
    }
}