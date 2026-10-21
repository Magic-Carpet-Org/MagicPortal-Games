using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TechJuego.GetOut
{
    public class TechTweenPlayer : MonoBehaviour
    {
        public TweenDetail tween;
        public void RunTween()
        {
           
                StartCoroutine(tween.RunTween(() =>
                {
                    if (tween.type == TweenAction.ValueTo)
                    {
                        Destroy(this.gameObject);
                    }
                    else
                    {
                        Destroy(this);
                    }
                }));
        }
        public void RunDelayTween(float time, Action onComplete)
        {
            StartCoroutine(RunTweens(time, onComplete));
        }
        public IEnumerator RunTweens(float time, Action onComplete)
        {
            yield return new WaitForSeconds(time);
            onComplete?.Invoke();
            Destroy(this.gameObject);
        }
    }
    public class TechTween
    {
        public enum EaseType
        {
            Linear,
            EaseOut,
            EaseIn,
            SmoothStep,
            SmootherStep
        }
        public static IEnumerator RotateZ(GameObject obj, Vector3 destination, float timeToMove, EaseType easeType)
        {
            Vector3 startRotation = obj.transform.eulerAngles;
            bool reachedDestination = false;
            float elapsedTime = 0f;
            //m_isMoving = true;
            while (!reachedDestination)
            {
                // if we are close enough to destination
                if (Vector3.Distance(obj.transform.eulerAngles, destination) < 0.01f)
                {
                    obj.transform.eulerAngles = destination;
                    reachedDestination = true;
                    break;
                }
                // track the total running time
                elapsedTime += Time.deltaTime;
                // calculate the Lerp value
                float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);

                switch (easeType)
                {
                    case EaseType.Linear:
                        break;
                    case EaseType.EaseOut:
                        t = Mathf.Sin(t * Mathf.PI * 0.5f);
                        break;
                    case EaseType.EaseIn:
                        t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
                        break;
                    case EaseType.SmoothStep:
                        t = t * t * (3 - 2 * t);
                        break;
                    case EaseType.SmootherStep:
                        t = t * t * t * (t * (t * 6 - 15) + 10);
                        break;

                }
                // move the game piece
                obj.transform.eulerAngles = Vector3.Lerp(startRotation, destination, t);
                // wait until next frame
                yield return null;
            }
            //m_isMoving = false;

        }
        public static IEnumerator Alpha(SpriteRenderer spr, float alpha, float timeToMove, EaseType easeType)
        {
            float startalpha = spr.color.a;
            bool reachedDestination = false;
            float elapsedTime = 0f;
            //m_isMoving = true;
            while (!reachedDestination)
            {
                // if we are close enough to destination
                if (Mathf.Abs(spr.color.a - alpha) < 0.01f)
                {
                    spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, alpha);
                    reachedDestination = true;
                    break;
                }
                // track the total running time
                elapsedTime += Time.deltaTime;
                // calculate the Lerp value
                float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);

                switch (easeType)
                {
                    case EaseType.Linear:
                        break;
                    case EaseType.EaseOut:
                        t = Mathf.Sin(t * Mathf.PI * 0.5f);
                        break;
                    case EaseType.EaseIn:
                        t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
                        break;
                    case EaseType.SmoothStep:
                        t = t * t * (3 - 2 * t);
                        break;
                    case EaseType.SmootherStep:
                        t = t * t * t * (t * (t * 6 - 15) + 10);
                        break;

                }
                spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, Mathf.Lerp(startalpha, alpha, t));
                // wait until next frame
                yield return null;
            }
            //m_isMoving = false;

        }
        public static void SetAngleX(GameObject obj, float angle)
        {
            obj.transform.eulerAngles = new Vector3(angle, obj.transform.eulerAngles.y, obj.transform.eulerAngles.z);
        }
        public static void SetAngleY(GameObject obj, float angle)
        {
            obj.transform.eulerAngles = new Vector3(obj.transform.eulerAngles.x, angle, obj.transform.eulerAngles.z);
        }

        public static void SetAngleZ(GameObject obj, float angle)
        {
            obj.transform.eulerAngles = new Vector3(obj.transform.eulerAngles.x, obj.transform.eulerAngles.y, angle);
        }
        public static void SetPosition(GameObject _target, Vector3 _postion)
        {
            _target.transform.position = _postion;
        }
        public static void SetPosition(Transform _target, Vector3 _postion)
        {
            _target.position = _postion;
        }
        public static void  StopThisTween(GameObject gameObject)
        {
            TechTweenPlayer techTweenPlayer = gameObject.GetComponent<TechTweenPlayer>();
            MonoBehaviour.Destroy(techTweenPlayer);
        }
        public static TweenDetail MoveTo(GameObject gameObject, Vector3 to, float time)
        {
            TweenDetail tween = new TweenDetail();
            tween.reset();
            tween.trans = gameObject.transform;
            tween.to = to;
            tween.time = time;
            TechTweenPlayer tweenPlayer = gameObject.AddComponent<TechTweenPlayer>();
            gameObject.GetComponent<TechTweenPlayer>().tween = tween;
            tween.SetMoveTo();
            tweenPlayer.RunTween();
            return tween;
        }
        public static TweenDetail ValueTo(GameObject gameObject, float start, float end, float time)
        {
            TweenDetail tween = new TweenDetail();
            tween.reset();
            tween.trans = gameObject.transform;
            tween.from = new Vector3(start, 0,0);
            tween.to = new Vector3(end,0,0);
            tween.time = time;
            TechTweenPlayer tweenPlayer = gameObject.AddComponent<TechTweenPlayer>();
            gameObject.GetComponent<TechTweenPlayer>().tween = tween;
            tween.SetValueTo();
            tweenPlayer.RunTween();
            return tween;
        }
        public static TweenDetail ValueTo(int start, int end, float time)
        {
            GameObject obj = new GameObject("ValueTo", typeof(TechTween));
            TweenDetail tween = new TweenDetail();
            tween.reset();
            tween.trans = obj.transform;
            tween.from = new Vector3(start, 0, 0);
            tween.to = new Vector3(end, 0, 0);
            tween.time = time;
            TechTweenPlayer tweenPlayer = obj.AddComponent<TechTweenPlayer>();
            obj.GetComponent<TechTweenPlayer>().tween = tween;
            tween.SetValueTo();
            tweenPlayer.RunTween();
            return tween;
        }
        public static void DelayCall(float time, Action onComplete)
        {
            GameObject tempObj = new GameObject();
            TechTweenPlayer techTween = tempObj.AddComponent<TechTweenPlayer>();
            techTween.RunDelayTween(time, onComplete);
        }
    }
    public class TechTweenUpdates
    {
        public Action<Vector3> onUpdateVector3;
        public Action<float> onUpdatefloat;
        public Action<int> onUpdateint;
        public Action OnTweenStart;
        public Action OnTweenComplete;
        public void Reset()
        {
            OnTweenStart = null;
            onUpdateVector3 = null;
            OnTweenComplete = null;
            onUpdatefloat = null;
            onUpdateint = null;
        }
    }
    public enum TweenAction { MoveTo,ValueTo  }
    [Serializable]
    public class TweenDetail
    {
        public static Vector3 newVect;
        public bool hasInitiliazed;
        public float delay;
        public float time;
        public Transform trans;
        public Vector3 from;
        public Vector3 to;
        public TweenAction type;
        public TechTweenUpdates tweenUpdates = new TechTweenUpdates();
        public TweenDetail() { }
        public void reset()
        {
            trans = null;
            delay = 0.0f;
            hasInitiliazed = false;
            from = to = Vector3.zero;
            tweenUpdates.Reset();
        }
        public TweenDetail SetMoveTo()
        {
            type = TweenAction.MoveTo;
            return this;
        }
        public TweenDetail SetValueTo()
        {
            type = TweenAction.ValueTo;
            return this;
        }
        public IEnumerator RunTween(Action onComplete)
        {
            yield return new WaitForEndOfFrame();
            tweenUpdates.OnTweenStart?.Invoke();
            yield return new WaitForSeconds(delay);
            float elapsedTime = 0;
            while (elapsedTime < time)
            {
                newVect = Vector3.Lerp(from, to, (elapsedTime / time));
                elapsedTime += Time.deltaTime;
                 switch (type)
            {
                case TweenAction.MoveTo:
                        trans.transform.position = newVect;
                        break;
                case TweenAction.ValueTo:
                        tweenUpdates.onUpdatefloat?.Invoke(newVect.x);
                        tweenUpdates.onUpdateint?.Invoke(Mathf.RoundToInt(newVect.x));
                        break;
            }
                tweenUpdates.onUpdateVector3?.Invoke(newVect);
           
                yield return new WaitForEndOfFrame();
            }
            switch (type)
            {
                case TweenAction.MoveTo:
                    trans.transform.position = to;
                    break;
                case TweenAction.ValueTo:
                    tweenUpdates.onUpdatefloat?.Invoke(to.x);
                    tweenUpdates.onUpdateint?.Invoke(Mathf.RoundToInt(newVect.x));
                    break;
            }
            tweenUpdates.onUpdateVector3?.Invoke(to);
            tweenUpdates.OnTweenComplete?.Invoke();
            onComplete?.Invoke();
        }
        public TweenDetail AddDelay(float _delay)
        {
            delay = _delay;
            return this;
        }
        public TweenDetail SetOnTweenStart(Action onComplete)
        {
            tweenUpdates.OnTweenStart = onComplete;
            return this;
        }
        public TweenDetail GetOnCompleteCallback(Action onComplete)
        {
            tweenUpdates.OnTweenComplete = onComplete;
            return this;
        }
        public TweenDetail GetVector3Update(Action<Vector3> onUpdate)
        {
            tweenUpdates.onUpdateVector3 = onUpdate;
            return this;
        }
        public TweenDetail GetFloatUpdate(Action<float> onUpdate)
        {
            tweenUpdates.onUpdatefloat = onUpdate;
            return this;
        }
        public TweenDetail GetIntUpdate(Action<int> onUpdate)
        {
            tweenUpdates.onUpdateint = onUpdate;
            return this;
        }
    }
}