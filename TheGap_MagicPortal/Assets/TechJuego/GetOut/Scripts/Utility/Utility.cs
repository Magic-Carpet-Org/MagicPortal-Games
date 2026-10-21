using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;

namespace TechJuego.GetOut.Utils
{
    public class Utility
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
        /// <summary>
        /// Add Action on button
        /// </summary>
        public static void SetButton(Button button, Action action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { action?.Invoke(); });
        }
        public static IEnumerator Updatelayout(VerticalLayoutGroup group)
        {
            yield return new WaitForEndOfFrame();
            group.spacing += 0.1f;
            group.spacing -= 0.1f;
        }
        public static IEnumerator Updatelayout(HorizontalLayoutGroup group)
        {
            yield return new WaitForEndOfFrame();
            group.spacing += 0.1f;
            group.spacing -= 0.1f;
        }
        public static IEnumerator Updatelayout(GridLayoutGroup group)
        {
            yield return new WaitForEndOfFrame();
        }
        public static IEnumerator ChangeValue(float startValue, float endValue, float seconds, Action<float> action = null)
        {
            float elapsedTime = 0; float startingPos = startValue;
            while (elapsedTime < seconds)
            {
                action.Invoke(Mathf.Lerp(startingPos, endValue, (elapsedTime / seconds)));
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            action.Invoke(endValue);
        }
        public static IEnumerator WaitAndCallback(float time, Action action)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke();
        }
        public static async void DelayCall(int time, Action onComplete)
        {
            await System.Threading.Tasks.Task.Delay(time);
            onComplete?.Invoke();
        }
    }
    public static class ExtensionMethods
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        public static void DestroyChildren(this GameObject parent)
        {
            Transform[] children = new Transform[parent.transform.childCount];
            for (int i = 0; i < parent.transform.childCount; i++)
                children[i] = parent.transform.GetChild(i);
            for (int i = 0; i < children.Length; i++)
                GameObject.Destroy(children[i].gameObject);
        }
    }
  
}
