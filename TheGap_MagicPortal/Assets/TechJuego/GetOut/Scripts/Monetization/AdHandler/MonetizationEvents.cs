using UnityEngine;
namespace TechJuego.GetOut.Monetization
{
    public class MonetizationEvents
    {
        public static void SetConcents(bool value)
        {
            PlayerPrefs.SetString("Concent", value ? "true" : "false");
        }
        public static bool GetConcents()
        {
            return PlayerPrefs.GetString("Concent") == "true" ? true : false;
        }
        public static bool HasConcentSet()
        {
            bool isSet = false;
            if (PlayerPrefs.GetString("Concent") == "true")
                isSet = true;
            else if (PlayerPrefs.GetString("Concent") == "false")
                isSet = true;
            else
                isSet = false;
            return isSet;
        }
        public static void SetHideShowAds()
        {
            PlayerPrefs.SetInt("NoAds", 1);
        }
        public static bool CanShowAds()
        {
            return PlayerPrefs.GetInt("NoAds") < 1;
        }
    }
}