using UnityEngine;
namespace TechJuego.GetOut.Rateus
{
    public class RateUsSetting
    {
        public static bool IsRatingDone()
        {
            return PlayerPrefs.HasKey("RATE");
        }
        public static void SetRateDone()
        {
            PlayerPrefs.SetInt("RATE", 1);
        }
    }
}