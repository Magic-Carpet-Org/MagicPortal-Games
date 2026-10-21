
using System.Collections.Generic;
using System;
using UnityEngine;

namespace TechJuego.GetOut.Monetization
{
    public class AdsHandler : MonoBehaviour
    {
        public static AdsHandler Instance;
        public AdManager m_AdManager;
        public List<IAdGetDetail> adGetDetails = new List<IAdGetDetail>();
        public bool testMode;
        private void Awake()
        {
            if (m_AdManager == null)
            {
                m_AdManager = Resources.Load("Monetization/AdManager") as AdManager;
                foreach (var item in m_AdManager.adsEvents)
                {
                    item.calls = 0;
                }
            }
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                DestroyImmediate(this.gameObject);
            }
        }
        private void OnEnable()
        {
            if (MonetizationEvents.HasConcentSet())
            {
                InitializeAds();
            }
        }
        public void InitializeAds()
        {
#if UnityAds
            gameObject.AddComponent<UnityAdsInitializer>().Initialize();
#endif
            gameObject.AddComponent<AdmobAdsInitializer>().Initialize();
        }
        public int InterstitialCount = 0;
        public bool IsInterstitialAdAvailable()
        {
            List<IAdGetDetail> tempList = new List<IAdGetDetail>();
            foreach (var item in adGetDetails)
            {
                if (item.IsAddAvailable(AdType.Interstitial))
                {
                    tempList.Add(item);
                }
            }
            return tempList.Count > 0;
        }
        public void ShowInterstitial()
        {
            List<IAdGetDetail> tempList = new List<IAdGetDetail>();
            foreach (var item in adGetDetails)
            {
                if (item.IsAddAvailable(AdType.Interstitial))
                {
                    tempList.Add(item);
                }
            }
            if (tempList.Count > 0)
            {
                InterstitialCount++;
                if (InterstitialCount >= tempList.Count)
                {
                    InterstitialCount = 0;
                }
                tempList[InterstitialCount].ShowInstestitial(tempList[InterstitialCount].GetAdId());
            }
        }
        public int BannerCount = 0;
        public void ShowBanner()
        {
            foreach (var item in adGetDetails)
            {
                if (item.IsAddAvailable(AdType.Reward))
                {
                    item.ShowBanner(item.GetAdId());
                    break;
                }
            }
        }
        public int RewardCount = 0;
        public bool IsRewardAdAvailable()
        {
            List<IAdGetDetail> tempList = new List<IAdGetDetail>();
            foreach (var item in adGetDetails)
            {
                if (item.IsAddAvailable(AdType.Reward))
                {
                    tempList.Add(item);
                }
            }
            return tempList.Count > 0;
        }
        public void ShowReward()
        {
            List<IAdGetDetail> tempList = new List<IAdGetDetail>();
            foreach (var item in adGetDetails)
            {
                if (item.IsAddAvailable(AdType.Reward))
                {
                    tempList.Add(item);
                }
            }
            if (tempList.Count > 0)
            {
                RewardCount++;
                if (RewardCount >= tempList.Count)
                {
                    RewardCount = 0;
                }
                tempList[RewardCount].ShowRewardAds(tempList[RewardCount].GetAdId(),()=>{ });
            }
        }
        public void ShowReward(Action onComplete)
        {
            List<IAdGetDetail> tempList = new List<IAdGetDetail>();
            foreach (var item in adGetDetails)
            {
                if (item.IsAddAvailable(AdType.Reward))
                {
                    tempList.Add(item);
                }
            }
            if (tempList.Count > 0)
            {
                RewardCount++;
                if (RewardCount >= tempList.Count)
                {
                    RewardCount = 0;
                }
                tempList[RewardCount].ShowRewardAds(tempList[RewardCount].GetAdId(), onComplete);
            }
        }
    }
}