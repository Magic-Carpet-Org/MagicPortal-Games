using System.Collections.Generic;
using UnityEngine;
namespace TechJuego.GetOut
{
    public class LevelHandler : MonoBehaviour
    {
        GameObject m_CurrentLevel;
        public List<GameObject> m_Levels = new List<GameObject>();
        public float speed;
        private void OnEnable()
        {
            GameEvents.OnLevelUpdate += GameEvents_OnLevelUpdate;
            GameEvents.OnLoadLevel += GameEvents_OnLoadLevel;
        }
        private void OnDisable()
        {
            GameEvents.OnLevelUpdate -= GameEvents_OnLevelUpdate;
            GameEvents.OnLoadLevel -= GameEvents_OnLoadLevel;
        }
        private void GameEvents_OnLoadLevel()
        {
            m_CurrentLevel = m_Levels[Random.Range(0, m_Levels.Count)];
            m_CurrentLevel.SetActive(true);
            DataHandler.Instance.gameState = GameState.Playing;
        }
        private void GameEvents_OnLevelUpdate()
        {
            if (GameManager.Instance.Score % 5 == 0)
            {
                if (m_CurrentLevel != null)
                {
                    m_CurrentLevel.SetActive(false);
                }
                m_CurrentLevel = m_Levels[Random.Range(0, m_Levels.Count)];
                m_CurrentLevel.SetActive(true);
            }
        }
        private void Update()
        {
            transform.Rotate(0, 0, Time.deltaTime * (speed + GameManager.Instance.Score));
        }
    }
}