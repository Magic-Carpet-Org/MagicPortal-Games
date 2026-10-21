using UnityEngine;
using UnityEngine.UI;
using TechJuego.GetOut.Utils;
using TechJuego.GetOut.Rateus;
using TechJuego.GetOut.Sound;

namespace TechJuego.GetOut
{
    public class RateUsPopup : MonoBehaviour
    {
        private RateUsData m_RateUsData;
        [SerializeField] private Button m_LaterButton;
        [SerializeField] private Button m_RateUsButton;
        private void OnEnable()
        {
            transform.SetAsLastSibling();

            if (m_RateUsData == null)
            {
                m_RateUsData = Resources.Load<RateUsData>("Rateus/RateUsSetting");
            }
            Utility.SetButton(m_LaterButton, OnClickLaterButton);
            Utility.SetButton(m_RateUsButton, OnClickRateUsButton);
        }
        private void OnClickLaterButton()
        {
            gameObject.SetActive(false);
            SoundEvents.OnPlaySingleShotSound?.Invoke("Click");
        }
        private void OnClickRateUsButton()
        {
            SoundEvents.OnPlaySingleShotSound?.Invoke("Click");
            Application.OpenURL(m_RateUsData.googlePlayBundleID);
#if UNITY_ANDROID
            Application.OpenURL("https://play.google.com/store/apps/details?id=" + m_RateUsData.googlePlayBundleID);
#endif

#if UNITY_IOS
            Application.OpenURL("https://itunes.apple.com/app/id"+m_RateUsData.iosAppID);
#endif
            RateUsSetting.SetRateDone();
        }
    }
}