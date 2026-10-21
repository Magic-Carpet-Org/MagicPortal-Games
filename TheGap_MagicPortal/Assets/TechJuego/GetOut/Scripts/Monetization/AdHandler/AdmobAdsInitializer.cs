#if ADMOB
using GoogleMobileAds.Api;
#endif
using UnityEngine;
namespace TechJuego.GetOut.Monetization
{
    public class AdmobAdsInitializer : MonoBehaviour
    {
        public void Initialize()
        {
#if ADMOB
            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                foreach (var item in AdsHandler.Instance.m_AdManager.monitizationAds)
                {
                    if (item.providers == "Admob")
                    {
                        AdmobHandler admobHandler = gameObject.AddComponent<AdmobHandler>();
                        admobHandler.m_AdManager = AdsHandler.Instance.m_AdManager;
#if UNITY_ANDROID
                        admobHandler._adUnitId = item.Android_ID;
#elif UNITY_IPHONE
                        admobHandler._adUnitId = item.IOS_ID;
#endif
                        admobHandler.adType = item.AdType;
                        admobHandler.Initialize();
                        AdsHandler.Instance.adGetDetails.Add(admobHandler);
                    }
                }
            });
#endif
        }
    }
}