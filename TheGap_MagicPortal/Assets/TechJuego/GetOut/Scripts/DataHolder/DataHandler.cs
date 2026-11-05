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
                        // Offer a paid Second Chance where the portal sells items (Lagos Life).
                        // Everywhere else - or if it's already been used this run - go
                        // straight to Game Over as before.
                        if (GameManager.Instance != null && GameManager.Instance.CanOfferSecondChance() && GameEvents.OnShowContinueGame != null)
                        {
                            GameEvents.OnShowContinueGame.Invoke();
                        }
                        else
                        {
                            gameState = GameState.LevelFail;
                        }
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