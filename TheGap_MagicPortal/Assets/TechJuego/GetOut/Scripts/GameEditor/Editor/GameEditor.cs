using System;
using UnityEditor;
using UnityEngine;
using TechJuego.GetOut.Sound;
using UnityEditor.SceneManagement;

namespace TechJuego.GetOut
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public class GameEditor : EditorWindow
    {
        private Vector2 scrollViewVector;
        private static int selected;
        private string[] toolbarStrings = { "Sound" };
        private static GameEditor window;
        [MenuItem("Tech Juego/Game editor and settings")]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            window = (GameEditor)GetWindow(typeof(GameEditor), false, "Game editor");
            window.Show();
        }
        private void OnGUI()
        {

            GUI.changed = false;
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            int oldSelected = selected;
            selected = GUILayout.Toolbar(selected, toolbarStrings, GUILayout.Width(500));
            GUILayout.EndHorizontal();
            scrollViewVector = GUI.BeginScrollView(new Rect(0, 45, position.width, position.height), scrollViewVector, new Rect(0, 0, 600, 1600));
            GUILayout.Space(-30);
            if (oldSelected != selected)
                scrollViewVector = Vector2.zero;
            if (toolbarStrings[selected] == "Sound")
            {
                ShowSound();
            }
            GUI.EndScrollView();
            if (GUI.changed && !EditorApplication.isPlaying)
                EditorSceneManager.MarkAllScenesDirty();
        }

        #region Sounds
        private SoundsHolder soundsHolder;
        private void ShowSound()
        {
            if (soundsHolder == null)
            {
                soundsHolder = Resources.Load("Sounds/SoundsHolder") as SoundsHolder;
                if (soundsHolder == null)
                {
                    CreateSoundSettings();
                    soundsHolder = Resources.Load("Sounds/SoundsHolder") as SoundsHolder;
                }
            }
            if (soundsHolder != null)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("SFX");
                for (int i = 0; i < soundsHolder.soundClips.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label((i + 1) + ".");
                    GUILayout.Label("Clip name");
                    soundsHolder.soundClips[i].clipName = EditorGUILayout.TextField("", soundsHolder.soundClips[i].clipName, GUILayout.Width(100));
                    GUILayout.Label("Clip");
                    soundsHolder.soundClips[i].clip = (AudioClip)EditorGUILayout.ObjectField("", soundsHolder.soundClips[i].clip, typeof(AudioClip), false, GUILayout.Width(100));
                    GUILayout.Label("Volume");
                    soundsHolder.soundClips[i].volume = EditorGUILayout.Slider(soundsHolder.soundClips[i].volume, 0, 1, GUILayout.Width(150));
                    if (GUILayout.Button(new GUIContent("X", "X"), GUILayout.Width(30)))
                    {
                        soundsHolder.soundClips.RemoveAt(i);
                        EditorUtility.SetDirty(soundsHolder);
                        AssetDatabase.SaveAssets();
                    }
                    GUILayout.Space(Screen.width);
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Add Sound", "Add Sound"), GUILayout.Width(80)))
                {
                    soundsHolder.soundClips.Add(new SoundClips());
                    EditorUtility.SetDirty(soundsHolder);
                    AssetDatabase.SaveAssets();
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Label("Music");
                for (int i = 0; i < soundsHolder.musicClip.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label((i + 1) + ".");
                    GUILayout.Label("Clip name");
                    soundsHolder.musicClip[i].clipName = EditorGUILayout.TextField("", soundsHolder.musicClip[i].clipName, GUILayout.Width(100));
                    GUILayout.Label("Clip");
                    soundsHolder.musicClip[i].clip = (AudioClip)EditorGUILayout.ObjectField("", soundsHolder.musicClip[i].clip, typeof(AudioClip), false, GUILayout.Width(100));
                    GUILayout.Label("Volume");
                    soundsHolder.musicClip[i].volume = EditorGUILayout.Slider(soundsHolder.musicClip[i].volume, 0, 1, GUILayout.Width(150));
                    if (GUILayout.Button(new GUIContent("X", "X"), GUILayout.Width(30)))
                    {
                        soundsHolder.musicClip.RemoveAt(i);
                        EditorUtility.SetDirty(soundsHolder);
                        AssetDatabase.SaveAssets();
                    }
                    GUILayout.Space(Screen.width);
                    GUILayout.EndHorizontal();
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Add Music", "Add Music"), GUILayout.Width(80)))
                {
                    soundsHolder.musicClip.Add(new SoundClips());
                    EditorUtility.SetDirty(soundsHolder);
                    AssetDatabase.SaveAssets();
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }
        #endregion

        private string basePath = "Assets/TechJuego/GetOut/Resources";
        private void CreateSoundSettings()
        {
            SoundsHolder asset = ScriptableObject.CreateInstance<SoundsHolder>();
            if (!AssetDatabase.IsValidFolder(basePath + "/Sounds/"))
            {
                AssetDatabase.CreateFolder(basePath, "Sounds");
                AssetDatabase.Refresh();
            }
            AssetDatabase.CreateAsset(asset, basePath + "/Sounds/SoundsHolder.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
#endif
}