using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

namespace TechJuego.GetOut
{
    public class SwitchButton : MonoBehaviour
    {
        private Button m_Button;
        private Image m_BGImage;
        [SerializeField] private RectTransform m_MovingImage;
        public UnityEvent<bool> OnClicEvent;
        private Color m_DisableColor = new Color(149f / 255f, 149f / 255f, 149f / 255f, 1f);
        private Color m_EnableColor = new Color(71f / 255f, 133f / 255f, 1f, 1f);
        private float time = 0.2f;
        public string gameSetting;
        public IEnumerator RunTween(float start, float end, float time, Action<float> update, Action onComplete)
        {
            yield return new WaitForEndOfFrame();
            float elapsedTime = 0;
            while (elapsedTime < time)
            {
                float newVal = Mathf.Lerp(start, end, (elapsedTime / time));
                elapsedTime += Time.deltaTime;
                update?.Invoke(newVal);
                yield return new WaitForEndOfFrame();
            }
            onComplete?.Invoke();
        }
        public void Initialize(string types)
        {
            gameSetting = types;
            m_Button = GetComponent<Button>();
            m_BGImage = GetComponent<Image>();
            Debug.Log(PlayerPrefs.GetInt(gameSetting, 1));
            if (PlayerPrefs.GetInt(gameSetting, 1) > 0)
            {
                m_MovingImage.pivot = new Vector2(0, 0.5f);
                m_MovingImage.anchorMin = new Vector2(0, 0.5f);
                m_MovingImage.anchorMax = new Vector2(0, 0.5f);
                m_BGImage.color = new Color(149f / 255f, 149f / 255f, 149f / 255f, 1f);
      
            }
            else
            {
                m_MovingImage.pivot = new Vector2(1, 0.5f);
                m_MovingImage.anchorMin = new Vector2(1, 0.5f);
                m_MovingImage.anchorMax = new Vector2(1, 0.5f);
                m_BGImage.color = new Color(71f / 255f, 133f / 255f, 1f, 1f);
            }
            m_MovingImage.anchoredPosition = Vector2.zero;
            m_Button.onClick.RemoveAllListeners();
            m_Button.onClick.AddListener(()=> 
            {
                m_MovingImage.anchoredPosition = Vector2.zero;
                if (PlayerPrefs.GetInt(gameSetting, 1) > 0)
                {
                    PlayerPrefs.SetInt(gameSetting, 0);
                    StartCoroutine(RunTween(0,1,time,(value)=>
                    {
                        m_MovingImage.pivot = new Vector2(value, 0.5f);
                        m_MovingImage.anchorMin = new Vector2(value, 0.5f);
                        m_MovingImage.anchorMax = new Vector2(value, 0.5f);
                        m_BGImage.color = Color.Lerp(m_BGImage.color, m_EnableColor, value);
                    },()=> {  OnClicEvent?.Invoke(true); }));
                }
                else
                {
                    PlayerPrefs.SetInt(gameSetting, 1);
                    StartCoroutine(RunTween(1, 0, time, (value) => 
                    {
                        m_MovingImage.pivot = new Vector2(value, 0.5f);
                        m_MovingImage.anchorMin = new Vector2(value, 0.5f);
                        m_MovingImage.anchorMax = new Vector2(value, 0.5f);
                        m_BGImage.color = Color.Lerp(m_BGImage.color, m_DisableColor, (1 - value));
                    }, () => { OnClicEvent?.Invoke(false); }));
                }

            });
        }
    }
}