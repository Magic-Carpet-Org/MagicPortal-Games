using System;

namespace TechJuego.GetOut.Monetization
{ 
    public interface IAdGetDetail
    {
        bool IsAddAvailable(AdType adType);
        string GetAdId();
        void ShowInstestitial(string id);
        void ShowRewardAds(string id,Action OnComplete);
        void ShowBanner(string id);
        void HideBanner();
    }
}