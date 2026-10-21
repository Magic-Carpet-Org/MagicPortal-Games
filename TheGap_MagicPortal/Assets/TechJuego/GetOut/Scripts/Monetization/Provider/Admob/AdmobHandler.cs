#if ADMOB
using GoogleMobileAds.Api;
#endif
using System;
using UnityEngine;
namespace TechJuego.GetOut.Monetization
{
    public class AdmobHandler : MonoBehaviour, IAdGetDetail
    {
        public string _adUnitId = "unused";
        public AdType adType;
        public AdManager m_AdManager;
        private bool isInterstitialLoaded = false;
        private bool isRewardLoaded = false;
#if ADMOB
        private InterstitialAd _interstitialAd;
        private RewardedAd _rewardedAd;
        private BannerView _bannerView;
#endif
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
        public string GetAdId()
        {
            return _adUnitId;
        }
        public void ShowInstestitial(string id)
        {
            if (_adUnitId == id)
            {
                #if ADMOB
                    ShowInterstitialAd();
                #endif
            }
        }
        public void ShowRewardAds(string id, Action OnComplete)
        {
            if (_adUnitId == id)
            {
                #if ADMOB
                ShowRewardAd(OnComplete);
                #endif
            }
        }
        public void ShowBanner(string id)
        {
            if (id == _adUnitId)
            {
                #if ADMOB
                    LoadBannerAd();
                #endif
            }
        }
        public void HideBanner()
        {
            #if ADMOB
                HideBannerAd();
            #endif
        }

#if ADMOB
        public void Initialize()
        {
            switch (adType)
            {
                case AdType.Banner:
                    LoadBannerAd();
                break;
                case AdType.Interstitial:
                    LoadInterstitialAd();
                break;
                case AdType.Reward:
                    LoadRewardAd();
                break;
            }
        }
    /// <summary>
    /// Creates a 320x50 banner at top of the screen.
    /// </summary>
    public void CreateBannerView()
    {
        Debug.Log("Creating banner view.");

        // If we already have a banner, destroy the old one.
        if (_bannerView != null)
        {
            DestroyBannerAd();
        }

        // Create a 320x50 banner at top of the screen.
        _bannerView = new BannerView(_adUnitId, AdSize.Banner, AdPosition.Top);

        // Listen to events the banner may raise.
        ListenToBannerAdEvents();

        Debug.Log("Banner view created.");
    }

    /// <summary>
    /// Destroys the ad.
    /// When you are finished with a BannerView, make sure to call
    /// the Destroy() method before dropping your reference to it.
    /// </summary>
    public void DestroyBannerAd()
    {
        if (_bannerView != null)
        {
            Debug.Log("Destroying banner view.");
            _bannerView.Destroy();
            _bannerView = null;
        }
    }

    public void LoadBannerAd()
    {
        // Create an instance of a banner view first.
        if (_bannerView == null)
        {
            CreateBannerView();
        }

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // Send the request to load the ad.
        Debug.Log("Loading banner ad.");
        _bannerView.LoadAd(adRequest);
    }

    /// <summary>
    /// Shows the ad.
    /// </summary>
    public void ShowBannerAd()
    {
        if (_bannerView != null)
        {
            Debug.Log("Showing banner view.");
            _bannerView.Show();
        }
    }

    /// <summary>
    /// Hides the ad.
    /// </summary>
    public void HideBannerAd()
    {
        if (_bannerView != null)
        {
            Debug.Log("Hiding banner view.");
            _bannerView.Hide();
        }
    }
       

    /// <summary>
    /// Loads the ad.
    /// </summary>
    public void LoadInterstitialAd()
    {
        isInterstitialLoaded = false;
        // Clean up the old ad before loading a new one.
        if (_interstitialAd != null)
        {
            DestroyInterstitialAd();
        }
        Debug.Log("Loading interstitial ad.");
        // Create our request used to load the ad.
        var adRequest = new AdRequest();
        // Send the request to load the ad.
        InterstitialAd.Load(_adUnitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            // If the operation failed with a reason.
            if (error != null)
            {
                Debug.LogError("Interstitial ad failed to load an ad with error : " + error);
                return;
            }
            // If the operation failed for unknown reasons.
            // This is an unexpected error, please report this bug if it happens.
            if (ad == null)
            {
                Debug.LogError("Unexpected error: Interstitial load event fired with null ad and null error.");
                return;
            }

            // The operation completed successfully.
            Debug.Log("Interstitial ad loaded with response : " + ad.GetResponseInfo());
            _interstitialAd = ad;

            // Register to ad events to extend functionality.
            RegisterInterstitialEventHandlers(ad);

            isInterstitialLoaded = true;
            // Inform the UI that the ad is ready.
        });
    }
      
        /// <summary>
        /// Loads the ad.
        /// </summary>
        public void LoadRewardAd()
    {
        // Clean up the old ad before loading a new one.
        if (_rewardedAd != null)
        {
            DestroyRewardAd();
        }
        Debug.Log("Loading rewarded ad.");
        // Create our request used to load the ad.
        var adRequest = new AdRequest();
        // Send the request to load the ad.
        RewardedAd.Load(_adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            // If the operation failed with a reason.
            if (error != null)
            {
                Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
                return;
            }
            // If the operation failed for unknown reasons.
            // This is an unexpected error, please report this bug if it happens.
            if (ad == null)
            {
                Debug.LogError("Unexpected error: Rewarded load event fired with null ad and null error.");
                return;
            }

            // The operation completed successfully.
            Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
            _rewardedAd = ad;

            // Register to ad events to extend functionality.
            RegisterRewardEventHandlers(ad);

            isRewardLoaded = true;
        });
    }

    /// <summary>
    /// Shows the ad.
    /// </summary>
    public void ShowRewardAd(Action oncomplete)
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            Debug.Log("Showing rewarded ad.");
            _rewardedAd.Show((Reward reward) =>
            {
                isRewardLoaded = false;
                oncomplete?.Invoke();
                Debug.Log(String.Format("Rewarded ad granted a reward: {0} {1}",
                                        reward.Amount,
                                        reward.Type));
                LoadRewardAd();
            });
        }
        else
        {
            Debug.LogError("Rewarded ad is not ready yet.");
        }

    }

    /// <summary>
    /// Destroys the ad.
    /// </summary>
    public void DestroyRewardAd()
    {
        if (_rewardedAd != null)
        {
            Debug.Log("Destroying rewarded ad.");
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

    }

    /// <summary>
    /// Logs the ResponseInfo.
    /// </summary>
    public void LogResponseInfo()
    {
        if (_rewardedAd != null)
        {
            var responseInfo = _rewardedAd.GetResponseInfo();
            UnityEngine.Debug.Log(responseInfo);
        }
        if (_interstitialAd != null)
        {
            var responseInfo = _interstitialAd.GetResponseInfo();
            UnityEngine.Debug.Log(responseInfo);
        }
        if (_bannerView != null)
        {
            var responseInfo = _bannerView.GetResponseInfo();
            if (responseInfo != null)
            {
                UnityEngine.Debug.Log(responseInfo);
            }
        }
    }
    /// <summary>
    /// Listen to events the banner may raise.
    /// </summary>
    private void ListenToBannerAdEvents()
    {
        // Raised when an ad is loaded into the banner view.
        _bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + _bannerView.GetResponseInfo());

        };
        // Raised when an ad fails to load into the banner view.
        _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : " + error);
        };
        // Raised when the ad is estimated to have earned money.
        _bannerView.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        _bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        _bannerView.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        _bannerView.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        _bannerView.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }
    private void RegisterRewardEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when the ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            LoadRewardAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content with error : "
                + error);
            LoadRewardAd();
        };
    }
    /// <summary>
    /// Shows the ad.
    /// </summary>
    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
        }
        // Inform the UI that the ad is not ready.
    }

    /// <summary>
    /// Destroys the ad.
    /// </summary>
    public void DestroyInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            Debug.Log("Destroying interstitial ad.");
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        // Inform the UI that the ad is not ready.
    }

    private void RegisterInterstitialEventHandlers(InterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");

            LoadInterstitialAd();

        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content with error : "
                + error);
        };

        }

 



#endif
    }
}