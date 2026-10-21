using UnityEngine;
using UnityEngine.UI;
using TechJuego.GetOut.Sound;
using TechJuego.GetOut.Utils;
namespace TechJuego.GetOut
{
    public class SettingsPannel : MonoBehaviour
    {

        [SerializeField] private Button m_BackButton;
        [Header("Setting")]
        [Header("-----------------------------------------------------------------------")]
        [SerializeField] private SwitchButton m_SoundButton;
        [Header("Privacy Policy")]
        [Header("-----------------------------------------------------------------------")]
        [SerializeField] private Button m_PrivacyPolicyButton;
        private void OnEnable()
        {

            Utility.SetButton(m_BackButton, CloseSettingPannel);
            Utility.SetButton(m_PrivacyPolicyButton, OpenPolicy);

            m_SoundButton.Initialize(SoundSetting.SoundVariable);
            m_SoundButton.OnClicEvent.RemoveAllListeners();
            m_SoundButton.OnClicEvent.AddListener(Button_Sound);

            transform.SetAsLastSibling();
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
           
            SoundEvents.OnPlaySingleShotSound?.Invoke("Click");
        }
    }

}