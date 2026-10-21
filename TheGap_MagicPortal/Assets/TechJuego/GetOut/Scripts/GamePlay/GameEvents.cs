using System;

namespace TechJuego.GetOut
{
    public class GameEvents
    {
        public delegate void OnAction();
        public static OnAction OnLoadLevel;
        public static OnAction OnLevelFail;
        public static OnAction OnShowRateUs;
        public static OnAction OnLevelUpdate;
        public static OnAction OnShowContinueGame;
        public static OnAction OnContinueGame;
    }
}