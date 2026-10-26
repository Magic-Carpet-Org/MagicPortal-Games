using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TechJuego.GetOut.Utils;
using System.Collections;
using UnityEngine.SceneManagement;
using Kilimanjaro.Portals;

namespace TechJuego.GetOut
{
    public class SplashScreen : MonoBehaviour
    {
        // Time To wait After Scene is Loaded
        [SerializeField] private float time;

        private AsyncOperation scene;

        // Loading Progress Text
        [SerializeField] private TextMeshProUGUI m_LoadingText;
        [SerializeField] private Slider m_LoadinImage;

        private float progrss;
        private bool apiFinished = false;
        private bool apiSuccess = false;

        IEnumerator Start()
        {
            // --- 1. WAIT FOR THE PORTAL (PortalKit) ---
            // PortalKit reads the session from the URL, detects MagicGames or
            // Lagos Life, and checks the MagicGames session with the backend.
            // ok is false only if MagicGames rejected the session.
            PortalManager.WhenReady(ok =>
            {
                apiSuccess = ok;
                apiFinished = true;

                if (ok)
                    Debug.Log($"[SplashScreen] Portal ready: {PortalManager.Current.Name}");
                else
                    Debug.LogError("[SplashScreen] Portal Error: session was rejected.");
            });

            // --- 2. START BACKGROUND SCENE LOAD ---
            scene = SceneManager.LoadSceneAsync("Game");
            scene.allowSceneActivation = false;

            while (!scene.isDone)
            {
                if (scene.progress >= .9f)
                {
                    bool visualSweepDone = false;

                    StartCoroutine(Utility.ChangeValue(0, 100, time, (value) =>
                    {
                        progrss = value;
                        if (value >= 100)
                        {
                            visualSweepDone = true;
                        }
                    }));

                    while (!visualSweepDone || !apiFinished)
                    {
                        yield return null;
                    }

                    if (apiSuccess)
                    {
                        scene.allowSceneActivation = true;
                    }
                    else
                    {
                        m_LoadingText.text = "Network Error!";
                        Debug.LogError("[SplashScreen] Cannot start game. API Initialization Failed.");
                    }
                    break;
                }
                yield return null;
            }
        }

        private void Update()
        {
            if (m_LoadingText.text != "Network Error!")
            {
                m_LoadingText.text = (int)progrss + "%";
            }

            m_LoadinImage.value = (progrss / 100);
        }
    }
}