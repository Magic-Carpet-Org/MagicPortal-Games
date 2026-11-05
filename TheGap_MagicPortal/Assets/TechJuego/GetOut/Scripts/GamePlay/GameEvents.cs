using System;

namespace TechJuego.GetOut
{
    public class GameEvents
    {
        public delegate void OnAction();
        public static OnAction OnLoadLevel;
        public static OnAction OnLevelFail;
        public static OnAction OnLevelUpdate;
        public static OnAction OnShowContinueGame; // Second Chance offer (Lagos Life purchase)
    }
}