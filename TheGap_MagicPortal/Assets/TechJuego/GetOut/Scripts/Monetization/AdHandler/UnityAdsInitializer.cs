using UnityEngine;
#if UnityAds
using UnityEngine.Advertisements;

namespace TechJuego.Monetization
{
    public class UnityAdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
    {
        string gameid;
        public void Initialize()
        {
            if (!string.IsNullOrEmpty(AdsHandler.Instance.m_AdManager.UnityAppID_Android))
            {
#if UNITY_ANDROID
                gameid = AdsHandler.Instance.m_AdManager.UnityAppID_Android;
#endif
                Advertisement.Initialize(gameid, AdsHandler.Instance.testMode, this);
            }
            if (!string.IsNullOrEmpty(AdsHandler.Instance.m_AdManager.AdmobAppID_IOS))
            {
#if UNITY_IPHONE
                gameid = m_AdManager.AdmobAppID_IOS;
#endif
                Advertisement.Initialize(gameid, AdsHandler.Instance.testMode, this);
            }
        }

        public void OnInitializationComplete()
        {
            foreach (var item in AdsHandler.Instance.m_AdManager.monitizationAds)
            {
                Debug.Log(item);
                if (item.providers == "Unity")
                {
                    UnityAdHandler unityAdHandler = gameObject.AddComponent<UnityAdHandler>();
                    unityAdHandler.m_AdManager = AdsHandler.Instance.m_AdManager;
                    unityAdHandler.adType = item.AdType;
#if UNITY_ANDROID
                    unityAdHandler._adUnitId = item.Android_ID;
#elif UNITY_IPHONE
                    unityAdHandler._adUnitId = item.IOS_ID;
#endif
                    unityAdHandler.Initialize();
                    AdsHandler.Instance.adGetDetails.Add(unityAdHandler);
                }
            }
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
        }
    }
}
#endif