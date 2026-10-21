using System.Collections;
using System.Collections.Generic;
using TechJuego.GetOut.Sound;
using UnityEngine;
namespace TechJuego.GetOut
{
    public class Shooter : MonoBehaviour
    {
        public float speed;
        float lastTime;
        private void Update()
        {
            if (DataHandler.Instance.gameState != GameState.Playing)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                Ball ball = Instantiate(GameManager.Instance.m_Ball, transform.position, Quaternion.identity);
                ball.Shoot(transform.up);
                SoundEvents.OnPlaySingleShotSound?.Invoke("Click");
            }
            lastTime = Time.timeSinceLevelLoad;
            transform.Rotate(0, 0, Time.deltaTime * (speed + GameManager.Instance.Score));
        }
    }
}