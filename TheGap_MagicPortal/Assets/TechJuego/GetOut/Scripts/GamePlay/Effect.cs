using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TechJuego.GetOut
{
    public class Effect : MonoBehaviour
    {
        private float time = .5f;
        float scale = 1;
        SpriteRenderer spriteRenderer;
        private void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        private void Update()
        {
            time -= Time.deltaTime;
            scale += Time.deltaTime * 10;
            transform.localScale = Vector3.one * scale;
            spriteRenderer.color = new Color(1, 1, 1, 1 * time);
            if (time <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}