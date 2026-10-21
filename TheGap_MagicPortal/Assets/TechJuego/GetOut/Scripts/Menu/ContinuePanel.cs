using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TechJuego.GetOut.Utils;
using TechJuego.GetOut.Monetization;
namespace TechJuego.GetOut
{
    public class ContinuePanel : MonoBehaviour
    {
        [SerializeField] Button _continueButton;
        [SerializeField] Button _skipButton;
        [Space]
        [SerializeField] TMP_Text _timerText;
        [SerializeField] Image _timerFill;
        bool isContinueClicked = false;
        private void OnEnable()
        {
            isContinueClicked = false;
            transform.SetAsLastSibling();

            TechTween.ValueTo( gameObject,5f,0,5f).GetFloatUpdate((val)=>
             {
                 _timerText.text = Mathf.RoundToInt(val).ToString();
                 _timerFill.fillAmount = val / 5f;
            }).GetOnCompleteCallback(()=> 
            {
                if(isContinueClicked)
                {
                    return;
                }
                DataHandler.Instance.gameState = GameState.LevelFail;
            });
        }

        private void Start()
        {
            Utility.SetButton(_continueButton, ContinueButtonPressed);
            Utility.SetButton(_skipButton, SkipButtonPressed);
        }

        private void SkipButtonPressed()
        {
            DataHandler.Instance.gameState = GameState.LevelFail;
            gameObject.SetActive(false);
        }

        private void ContinueButtonPressed()
        {
            isContinueClicked = true;
            TechTween.StopThisTween(gameObject);
            AdsHandler.Instance.ShowReward(()=> 
            {
                gameObject.SetActive(false);
               DataHandler.Instance.gameState = GameState.Playing;
            });
        }
    }
}