using System.Collections.Generic;
using TechJuego.GetOut;
using UnityEngine;
namespace TechJuego.GetOut.Monetization
{
    [CreateAssetMenu(fileName = "AdManager", menuName = "TechJuego/AdManager", order = 1)]
    public class AdManager : ScriptableObject
    {
        public bool isUnityPresent;
        public bool isAdmobPresent;
        public List<string> providerAdded = new List<string>();
        public List<AdEvents> adsEvents = new List<AdEvents>();
        public List<MonitizationAds> monitizationAds = new List<MonitizationAds>();

        public string AdmobAppID_Android;
        public string AdmobAppID_IOS;

        public string UnityAppID_Android;
        public string UnityAppID_IOS;

    }
}