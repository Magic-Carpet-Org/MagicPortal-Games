using System;
using UnityEngine;
#if UnityAds
using UnityEngine.Advertisements;

namespace TechJuego.Monetization
{
    public class UnityAdHandler : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener,IAdGetDetail
    {
        public AdManager m_AdManager;
        public string _adUnitId;
        public AdType adType;
        public void Initialize()
        {
            switch (adType)
            {
                case AdType.Banner:
                    Advertisement.Banner.SetPosition(UnityEngine.Advertisements.BannerPosition.BOTTOM_CENTER);
                    LoadBanner();
                    break;
                case AdType.Interstitial:
                    Advertisement.Load(_adUnitId, this);
                    break;
                case AdType.Reward:
                    Advertisement.Load(_adUnitId, this);
                    break;
            }
        }
        public string GetAdId()
        {
            return _adUnitId;
        }
        public void ShowInstestitial(string id)
        {
            if (_adUnitId == id)
            {
                Advertisement.Show(_adUnitId, this);
            }
        }
        private Action rewardAction;
        public void ShowRewardAds(string id, Action OnComplete)
        {
            if (_adUnitId == id)
            {
                rewardAction = OnComplete;
                Advertisement.Show(_adUnitId, this);
            }
        }
        public void ShowBanner(string id)
        {
            ShowBannerAd();
        }
        public void HideBanner()
        {
            Advertisement.Banner.Hide();
        }
        public void OnUnityAdsAdLoaded(string placementId)
        {
            if(_adUnitId == placementId)
            {
                switch (adType)
                {
                    case AdType.Banner:
                        break;
                    case AdType.Interstitial:
                        isInterstitialLoaded = true;
                        break;
                    case AdType.Reward:
                        isRewardLoaded = true;
                        break;
                }
            }
        }
        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            switch (adType)
            {
                case AdType.Banner:
                    break;
                case AdType.Interstitial:
                    isInterstitialLoaded = false;
                    break;
                case AdType.Reward:
                    isRewardLoaded = false;
                    rewardAction?.Invoke();
                    break;
            }
            if (_adUnitId == placementId)
            {
                Advertisement.Load(_adUnitId, this);
            }
        }
        // Implement Load and Show Listener error callbacks:
        public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
        {
            Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
            // Use the error details to determine whether to try to load another ad.
        }
        public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
        {
            Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
            // Use the error details to determine whether to try to load another ad.
        }
        public void OnUnityAdsShowStart(string adUnitId)
        {
        }
        public void OnUnityAdsShowClick(string adUnitId)
        {
        }
        void ShowBannerAd()
        {
            // Set up options to notify the SDK of show events:
            BannerOptions options = new BannerOptions
            {
                clickCallback = OnBannerClicked,
                hideCallback = OnBannerHidden,
                showCallback = OnBannerShown
            };
            // Show the loaded Banner Ad Unit:
            Advertisement.Banner.Show(_adUnitId, options);
        }
        // Implement a method to call when the Load Banner button is clicked:
        public void LoadBanner()
        {
            // Set up options to notify the SDK of load events:
            BannerLoadOptions options = new BannerLoadOptions
            {
                loadCallback = () => { },
                errorCallback = (message) => { }
            };
            // Load the Ad Unit with banner content:
            Advertisement.Banner.Load(_adUnitId, options);
        }
        void OnBannerClicked() { }
        void OnBannerShown() { }
        void OnBannerHidden() { }
        bool isInterstitialLoaded = false;
        bool isRewardLoaded = false;
        public bool IsAddAvailable(AdType adType)
        {
            bool isAvailable = false;
            switch (adType)
            {
                case AdType.Banner:
                    break;
                case AdType.Interstitial:
                    isAvailable = isInterstitialLoaded;
                    break;
                case AdType.Reward:
                    isAvailable = isRewardLoaded;
                    break;
            }
            return isAvailable;
        }
  
    }
}
#endif