using TechJuego.GetOut.Sound;
using UnityEngine;
namespace TechJuego.GetOut
{
    public class Ball : MonoBehaviour
    {
        Rigidbody2D m_rigidbody;
        private void OnEnable()
        {
            m_rigidbody = GetComponent<Rigidbody2D>();
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.transform.tag.Contains("Enemy"))
            {
                GameManager.Instance.isGameEnded = true;
                SoundEvents.OnPlaySingleShotSound?.Invoke("Loose");
                Destroy(this.gameObject);
                Instantiate(GameManager.Instance.m_LooseEffect, transform.position, Quaternion.identity);
                TechJuego.GetOut.TechTween.DelayCall(1, () =>
                {
                    DataHandler.Instance.gameState = GameState.Contine;
                });

            }

            if (collision.transform.tag.Contains("Wall"))
            {
                SoundEvents.OnPlaySingleShotSound?.Invoke("Point");
                GameEvents.OnLevelUpdate?.Invoke();
                Instantiate(GameManager.Instance.m_Effect, transform.position, Quaternion.identity);
                Destroy(this.gameObject);
            }
        }
        public void Shoot(Vector3 direction)
        {
            m_rigidbody.velocity = direction * 10;
        }
    }
}