using UnityEngine;
using TechJuego.GetOut.Sound;

namespace TechJuego.GetOut
{
    public class DataHandler : Singleton<DataHandler>
    {
        protected DataHandler()
        {

        }

        private GameState GameState;

        public GameState gameState
        {
            get { return GameState; }
            set
            {
                GameState = value;

                switch (value)
                {
                    case GameState.CreateLevel:
                        GameEvents.OnLoadLevel?.Invoke();
                        break;
                    case GameState.LevelFail:
                        GameEvents.OnLevelFail?.Invoke();
                        break;
                    case GameState.Contine:
                        // INSTANT ROUTE: If the game tries to continue, instantly force a Level Fail instead!
                        gameState = GameState.LevelFail;
                        break;
                }
            }
        }

        public int Highscore
        {
            get
            {
                return PlayerPrefs.GetInt("HIGHSCORE", 0);
            }
            set
            {
                PlayerPrefs.SetInt("HIGHSCORE", value);
            }
        }
    }
}