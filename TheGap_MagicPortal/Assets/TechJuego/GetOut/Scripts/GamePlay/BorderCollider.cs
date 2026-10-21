using UnityEngine;
namespace TechJuego.GetOut
{
    public class BorderCollider : MonoBehaviour
    {
        void Awake()
        {
            SetCollider();
        }

        public Collider2D left, right, up, down;

        private void OnValidate()
        {
            SetCollider();
        }
        void SetCollider()
        {
            float horizontal = Camera.main.aspect * Camera.main.orthographicSize;
            left.transform.position = new Vector3(-horizontal, 0, 0);
            right.transform.position = new Vector3(horizontal, 0, 0);
            up.transform.position = new Vector3(0, Camera.main.orthographicSize, 0);
            down.transform.position = new Vector3(0, -Camera.main.orthographicSize, 0);
            left.transform.localScale = new Vector3(1, Camera.main.orthographicSize * 2, 1);
            right.transform.localScale = new Vector3(1, Camera.main.orthographicSize * 2, 1);
            up.transform.localScale = new Vector3(horizontal * 2, 1, 1);
            down.transform.localScale = new Vector3(horizontal * 2, 1, 1);
        }
    }
}