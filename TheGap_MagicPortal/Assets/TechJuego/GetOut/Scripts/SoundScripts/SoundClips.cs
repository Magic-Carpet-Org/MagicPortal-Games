using UnityEngine;
using System;

namespace TechJuego.GetOut.Sound
{
    [Serializable]
    public class SoundClips
    {
        public string clipName;
        public AudioClip clip;
        [Range(0, 1)]
        public float volume = 1;
    }
}