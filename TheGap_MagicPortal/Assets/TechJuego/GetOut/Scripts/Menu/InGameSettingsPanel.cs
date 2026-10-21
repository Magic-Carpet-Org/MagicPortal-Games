using UnityEngine;
using UnityEngine.UI;
using TechJuego.GetOut.Sound;
using TechJuego.GetOut.Utils;
namespace TechJuego.GetOut
{
    public class InGameSettingsPanel : MonoBehaviour
    {
        [SerializeField] private Button m_BackButton;
        [SerializeField] private SwitchButton m_SoundButton;
        [SerializeField] private Button m_PrivacyPolicyButton;
        [SerializeField] private Button m_HomeButton;
        private void OnEnable()
        {
            Utility.SetButton(m_HomeButton, OnClickHomeButton);
            Utility.SetButton(m_BackButton, CloseSettingPannel);
            Utility.SetButton(m_PrivacyPolicyButton, OpenPolicy);
            m_SoundButton.Initialize(SoundSetting.SoundVariable);
            m_SoundButton.OnClicEvent.RemoveAllListeners();
            m_SoundButton.OnClicEvent.AddListener(Button_Sound);
            transform.SetAsLastSibling();
        }
        void OnClickHomeButton()
        {
            SoundEvents.OnPlaySingleShotSound?.Invoke("Click");
            SceneLoader.LoadScene("Menu");
        }
        void Button_Sound(bool value)
        {
            AudioListener.volume = value ? 1 : 0;
        }
        public void CloseSettingPannel()
        {
            gameObject.SetActive(false);
            SoundEvents.OnPlaySingleShotSound?.Invoke("Click");
        }
        void OpenPolicy()
        {
            Application.OpenURL("https://techjuego.com/privacypolicy/");
            SoundEvents.OnPlaySingleShotSound?.Invoke("Click");
        }
    }
}