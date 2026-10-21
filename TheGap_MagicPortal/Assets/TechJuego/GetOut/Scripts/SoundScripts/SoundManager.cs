using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechJuego.GetOut.Sound
{
   
        // SoundManager is responsible for managing and playing sound effects and background music
        public class SoundManager : MonoBehaviour
        {
            [SerializeField] private AudioSource m_BgMusic;  // AudioSource for background music
            private List<AudioSource> m_PoolList = new List<AudioSource>();  // Pool of audio sources for single-shot sounds
            [SerializeField] private int m_PoolCount;  // Maximum number of audio sources in the pool
            private static SoundManager Instance;  // Singleton instance of SoundManager
            [SerializeField] private SoundsHolder m_soundsHolder;  // Reference to the SoundsHolder which stores all sound clips

            private void Awake()
            {
                // If m_soundsHolder is not assigned, try loading it from Resources
                if (m_soundsHolder == null)
                {
                    m_soundsHolder = Resources.Load("Sounds/SoundsHolder") as SoundsHolder;
                }

                gameObject.name = GetType().Name; // Set the game object name to match the class name

                // Ensure only one instance of SoundManager exists
                if (Instance == null)
                {
                    Instance = this;
                    DontDestroyOnLoad(this.gameObject);  // Keep SoundManager across scene loads
                }
                else
                {
                    DestroyImmediate(this.gameObject);  // Destroy duplicate instance
                }
            }

            private void OnEnable()
            {
            AudioListener.volume = SoundSetting.GetSound() ? 1 : 0;
            // Subscribe to sound events
            SoundEvents.OnPlaySingleShotSound += PlaySingleShotSount;
                SoundEvents.OnPlayLoopSound += SoundEvents_OnPlayLoopSound;
                SoundEvents.OnStopLoopSound += SoundEvents_OnStopLoopSound;
            }

            // Event handler to stop the looping background music
            private void SoundEvents_OnStopLoopSound()
            {
                m_BgMusic.Stop();
            }

            // Event handler to play a looping background music clip
            private void SoundEvents_OnPlayLoopSound(string _ClipName)
            {

                if (m_soundsHolder == null)
                {
                    m_soundsHolder = Resources.Load("Sounds/SoundsHolder") as SoundsHolder;
                }

                // Find the requested clip from the background music list
                SoundClips sc = m_soundsHolder.musicClip.Find(F => F.clipName == _ClipName);
                if (sc != null)
                {
                    // Check if the background music is already playing the requested clip
                    if (sc.clip == m_BgMusic.clip)
                    {
                        if (!m_BgMusic.isPlaying)
                        {
                            m_BgMusic.loop = true;
                            m_BgMusic.Play(); // Start playing if it's not already playing
                        }
                    }
                    else
                    {
                        m_BgMusic.clip = sc.clip;
                        m_BgMusic.volume = sc.volume;
                        m_BgMusic.loop = true;
                        m_BgMusic.Play(); // Play the new background music
                    }
                }
            }

            // Method to play a single-shot sound effect
            private void PlaySingleShotSount(string _ClipName)
            {
                // Check if sound effects are enabled in the game settings
                if (!SoundSetting.GetSound())
                {
                    return;
                }

                // Check if the pool has available audio sources
                if (m_PoolList.Count > m_PoolCount)
                {
                    SoundClips sc = m_soundsHolder.soundClips.Find(F => F.clipName == _ClipName);
                    if (sc != null)
                    {
                        // Reuse an inactive audio source from the pool
                        var source = m_PoolList.Find(f => !f.gameObject.activeSelf);
                        source.gameObject.SetActive(true);
                        source.clip = sc.clip;
                        source.volume = sc.volume;
                        source.loop = false;
                        source.Play(); // Play the sound effect

                        // After the sound finishes, deactivate the source
                        StartCoroutine(DelayCall(sc.clip.length, () =>
                        {
                            source.gameObject.SetActive(false);
                        }));
                    }
                }
                else
                {
                    // If no available sources in the pool, create a new audio source
                    SoundClips sc = m_soundsHolder.soundClips.Find(F => F.clipName == _ClipName);
                    if (sc != null)
                    {
                        var source = new GameObject(_ClipName).AddComponent<AudioSource>();
                        source.transform.SetParent(transform);
                        source.clip = sc.clip;
                        source.volume = sc.volume;
                        source.loop = false;
                        source.Play(); // Play the sound effect
                        m_PoolList.Add(source);

                        // After the sound finishes, deactivate the source
                        StartCoroutine(DelayCall(sc.clip.length, () =>
                        {
                            source.gameObject.SetActive(false);
                        }));
                    }
                }
            }

            // Coroutine to delay the deactivation of audio source after the sound clip ends
            IEnumerator DelayCall(float duration, Action onComplete)
            {
                yield return new WaitForSeconds(duration);
                onComplete();
            }

            // Method to stop all single-shot sound effects
            public void StopAllSingleShotSound()
            {
                foreach (var item in m_PoolList)
                {
                    item.gameObject.SetActive(false); // Deactivate all pooled audio sources
                }
            }

            private void OnDestroy()
            {
                // Unsubscribe from sound events when the SoundManager is destroyed
                SoundEvents.OnPlaySingleShotSound -= PlaySingleShotSount;
                SoundEvents.OnStopLoopSound -= SoundEvents_OnStopLoopSound;
            }
        }
}
