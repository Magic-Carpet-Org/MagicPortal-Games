using UnityEngine;

namespace TechJuego.GetOut.Rateus
{
    /// <summary>
    /// properties from settings window
    /// </summary>
    public class RateUsData : ScriptableObject
    {
        public string iosAppID;
        public string googlePlayBundleID;
        public GameState WhenToShow = GameState.None;
        public int CallOnEvery = 5;
        public int CallCount;
    }
}