using UnityEngine;

namespace TechJuego.GetOut
{
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static bool m_ShuttingDown = false;
		private static object m_Lock = new object();
		private static T m_Instance;

		public static T Instance
		{
			get
			{
				if (m_ShuttingDown)
				{
					Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
						"' already destroyed. Returning null.");
					return null;
				}

				lock (m_Lock)
				{
					if (m_Instance == null)
					{
						m_Instance = (T)FindObjectOfType(typeof(T));

						if (m_Instance == null)
						{
							var singletonObject = new GameObject();
							m_Instance = singletonObject.AddComponent<T>();
							singletonObject.name = typeof(T).ToString() + " (Singleton)";
							DontDestroyOnLoad(singletonObject);
						}
						else
						{
							// FIX: If we found one manually placed in the scene, 
							// we must still tell Unity not to destroy it!
							DontDestroyOnLoad(m_Instance.gameObject);
						}
					}

					return m_Instance;
				}
			}
		}

		// FIX: Reset the shutting down flag when the game actually starts up.
		// This fixes a bug where stopping/starting the Unity Editor keeps the static flag as true
		// (specifically when "Reload Domain" is disabled in Editor > Project Settings > Editor).
		//
		// This only matters in the Editor. A real build has no domain-reload concept - every
		// static field already starts at its plain default (false) on every fresh load - so
		// this method is unnecessary in a build. It also happens to sit on a generic class,
		// and [RuntimeInitializeOnLoadMethod] on a generic method is unreliable under IL2CPP:
		// the AOT compiler can't always resolve which closed generic types (Singleton<ApiManager>,
		// Singleton<GameManager>, etc.) need code generated for it via that special early-bootstrap
		// invocation path, which throws an ExecutionEngineException at boot in WebGL/IL2CPP builds.
		// Restricting it to the Editor removes it from the build entirely, so IL2CPP never has to
		// deal with it.
#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void ResetStatics()
		{
			m_ShuttingDown = false;
		}
#endif

		private void OnApplicationQuit()
		{
			m_ShuttingDown = true;
		}

		private void OnDestroy()
		{
			// Only flag as shutting down if the application is actually quitting.
			// This prevents scene changes from breaking the singleton pattern.
			m_ShuttingDown = true; 
		}
	}
}