using UnityEngine;
namespace TechJuego.GetOut.Sound
{
    public class SoundSetting
    {
        public static readonly string SoundVariable = "SOUND";
        public static bool GetSound()
        {
            return PlayerPrefs.GetInt(SoundVariable, 1) > 0;
        }
        public static void SetSound(bool value)
        {
            PlayerPrefs.SetInt(SoundVariable, value ? 1 : 0);
        }
    }
}