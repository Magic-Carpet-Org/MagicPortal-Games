using System;
using UnityEditor;
using UnityEngine;
using TechJuego.GetOut.Sound;
using TechJuego.GetOut.Rateus;
using TechJuego.GetOut.Monetization;
using UnityEditor.SceneManagement;

namespace TechJuego.GetOut
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public class GameEditor : EditorWindow
    {
        private Vector2 scrollViewVector;
        private static int selected;
        private string[] toolbarStrings = { "Sound","Ads",  "Rate us", "About" };
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
            if (toolbarStrings[selected] == "Ads")
            {
                ShowMonetization();
            }
            if (toolbarStrings[selected] == "Rate us")
            {
                ShowRateUs();
            }
            if (toolbarStrings[selected] == "About")
            {
                ShowAbout();
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

        #region Monetization

        private AdManager m_AdManager;
        void ShowMonetization()
        {
            if (m_AdManager == null)
            {
                m_AdManager = Resources.Load("Monetization/AdManager") as AdManager;
                if (m_AdManager == null)
                {
                    CreateMonetizationSettings();
                    m_AdManager = Resources.Load("Monetization/AdManager") as AdManager;
                }
            }
            if (m_AdManager != null)
            {
                ShowProviderToAdd();
                ShowAdProviderAppID();
                MonitizationID();
                ShowAdCallEvent();
            }
        }
        /// <summary>
        /// Displays ad provider selection UI.
        /// Adds or removes the provider from the list based on selection.
        /// </summary>
        private void ShowProviderToAdd()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Select Ad Provider Which you want to Use");
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("||", GUILayout.Width(20));
            GUILayout.Label("Unity", GUILayout.Width(40));
            m_AdManager.isUnityPresent = EditorGUILayout.Toggle(m_AdManager.isUnityPresent, GUILayout.Width(40));
            if (m_AdManager.isUnityPresent)
            {
                if (!m_AdManager.providerAdded.Contains("Unity"))
                {
                    m_AdManager.providerAdded.Add("Unity");
                }
                EditorUtility.SetDirty(m_AdManager);
            }
            else
            {
                if (m_AdManager.providerAdded.Contains("Unity"))
                {
                    m_AdManager.providerAdded.Remove("Unity");
                }
                EditorUtility.SetDirty(m_AdManager);
            }
            if (GUILayout.Button(new GUIContent("Add  Symbol"), GUILayout.Width(100)))
            {
                AddDefineSymbolToGroup(BuildTargetGroup.Android, "UnityAds");
                AddDefineSymbolToGroup(BuildTargetGroup.iOS, "UnityAds");
                AddDefineSymbolToGroup(BuildTargetGroup.Standalone, "UnityAds");
            }
            if (GUILayout.Button(new GUIContent("Remove Symbol"), GUILayout.Width(110)))
            {
                RemoveDefineSymbolToGroup(BuildTargetGroup.Android, "UnityAds");
                RemoveDefineSymbolToGroup(BuildTargetGroup.iOS, "UnityAds");
                RemoveDefineSymbolToGroup(BuildTargetGroup.Standalone, "UnityAds");
            }
            GUILayout.Label("||", GUILayout.Width(20));
            GUILayout.Label("Admob", GUILayout.Width(50));
            m_AdManager.isAdmobPresent = EditorGUILayout.Toggle(m_AdManager.isAdmobPresent, GUILayout.Width(40));
            if (m_AdManager.isAdmobPresent)
            {
                if (!m_AdManager.providerAdded.Contains("Admob"))
                {
                    m_AdManager.providerAdded.Add("Admob");
                }

                EditorUtility.SetDirty(m_AdManager);
            }
            else
            {
                if (m_AdManager.providerAdded.Contains("Admob"))
                {
                    m_AdManager.providerAdded.Remove("Admob");
                }
                EditorUtility.SetDirty(m_AdManager);
            }
            if (GUILayout.Button(new GUIContent("Add  Symbol"), GUILayout.Width(130)))
            {
                AddDefineSymbolToGroup(BuildTargetGroup.Android, "ADMOB");
                AddDefineSymbolToGroup(BuildTargetGroup.iOS, "ADMOB");
                AddDefineSymbolToGroup(BuildTargetGroup.Standalone, "ADMOB");
            }
            if (GUILayout.Button(new GUIContent("Remove Symbol"), GUILayout.Width(130)))
            {
                RemoveDefineSymbolToGroup(BuildTargetGroup.Android, "ADMOB");
                RemoveDefineSymbolToGroup(BuildTargetGroup.iOS, "ADMOB");
                RemoveDefineSymbolToGroup(BuildTargetGroup.Standalone, "ADMOB");
            }
            GUILayout.Label("||", GUILayout.Width(50));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        /// <summary>
        /// Displays monetization ID input fields.
        /// </summary>
        private void ShowAdProviderAppID()
        {
            if (m_AdManager.providerAdded.Count > 0)
            {
                GUILayout.Label("=============================================================================================================");
                if (m_AdManager.isAdmobPresent)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Admob App ID:--", GUILayout.Width(100));
                    GUILayout.Label("Android", GUILayout.Width(60));
                    m_AdManager.AdmobAppID_Android = EditorGUILayout.TextField(m_AdManager.AdmobAppID_Android, GUILayout.Width(200));
                    GUILayout.Space(30);
                    GUILayout.Label("IOS", GUILayout.Width(30));
                    m_AdManager.AdmobAppID_IOS = EditorGUILayout.TextField(m_AdManager.AdmobAppID_IOS, GUILayout.Width(200));
                    if (GUILayout.Button(new GUIContent("Get Admob Ad App ID"), GUILayout.Width(150)))
                    {
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(10);
                if (m_AdManager.isUnityPresent)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Unity App ID:--", GUILayout.Width(100));
                    GUILayout.Label("Android", GUILayout.Width(60));
                    m_AdManager.UnityAppID_Android = EditorGUILayout.TextField(m_AdManager.UnityAppID_Android, GUILayout.Width(200));
                    GUILayout.Space(30);
                    GUILayout.Label("IOS", GUILayout.Width(30));
                    m_AdManager.UnityAppID_IOS = EditorGUILayout.TextField(m_AdManager.UnityAppID_IOS, GUILayout.Width(200));
                    if (GUILayout.Button(new GUIContent("Get Unity Ad App ID"), GUILayout.Width(150)))
                    {
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
        private void MonitizationID()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Space(20);
                if (m_AdManager.providerAdded.Count > 0)
                {
                    GUILayout.Label("=============================================================================================================");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(29));
                    GUILayout.Label("Ad Provider", GUILayout.Width(100));
                    GUILayout.Label("Ad Type", GUILayout.Width(100));
                    GUILayout.Label("Android Ad ID", GUILayout.Width(280));
                    GUILayout.Label("iOS Ad ID", GUILayout.Width(280));
                    GUILayout.EndHorizontal();
                    for (int i = 0; i < m_AdManager.monitizationAds.Count; i++)
                    {
                        int no = i;
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("X"), GUILayout.Width(30)))
                        {
                            m_AdManager.monitizationAds.RemoveAt(no);
                            EditorUtility.SetDirty(m_AdManager);
                            AssetDatabase.SaveAssets();
                        }
                        if (no < m_AdManager.monitizationAds.Count)
                        {
                            string[] provider = m_AdManager.providerAdded.ToArray();
                            int selectedIndex = Mathf.Max(0, Array.IndexOf(provider, m_AdManager.monitizationAds[i].providers));
                            selectedIndex = EditorGUILayout.Popup(selectedIndex, provider, GUILayout.Width(100));
                            m_AdManager.monitizationAds[i].providers = provider[selectedIndex];
                            m_AdManager.monitizationAds[no].AdType = (AdType)EditorGUILayout.EnumPopup(m_AdManager.monitizationAds[no].AdType, GUILayout.Width(100));
                            m_AdManager.monitizationAds[no].Android_ID = EditorGUILayout.TextField(m_AdManager.monitizationAds[no].Android_ID, GUILayout.Width(280));
                            m_AdManager.monitizationAds[no].IOS_ID = EditorGUILayout.TextField(m_AdManager.monitizationAds[no].IOS_ID, GUILayout.Width(280));
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("Add Ad ID"), GUILayout.Width(80)))
                    {
                        m_AdManager.monitizationAds.Add(new() { AdType = AdType.Interstitial, Android_ID = "", IOS_ID = "" });
                        EditorUtility.SetDirty(m_AdManager);
                        AssetDatabase.SaveAssets();
                    }
                    if (GUILayout.Button(new GUIContent("Save"), GUILayout.Width(80)))
                    {
                        EditorUtility.SetDirty(m_AdManager);
                        AssetDatabase.SaveAssets();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.Label("=============================================================================================================");
                GUILayout.EndVertical();
            }
        }
        private void ShowAdCallEvent()
        {
            if (m_AdManager.providerAdded.Count > 0)
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Label("Add Call Events ============================================================================================");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(" ", GUILayout.Width(30));
                    GUILayout.Label("When To Call", GUILayout.Width(100));
                    GUILayout.Label("Call Every Level", GUILayout.Width(100));
                    GUILayout.EndHorizontal();
                    if (m_AdManager.monitizationAds.Count > 0)
                    {
                        for (int i = 0; i < m_AdManager.adsEvents.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            if (GUILayout.Button(new GUIContent("X"), GUILayout.Width(30)))
                            {
                                m_AdManager.adsEvents.RemoveAt(i);
                                EditorUtility.SetDirty(m_AdManager);
                                AssetDatabase.SaveAssets();
                            }
                            if (i < m_AdManager.adsEvents.Count)
                            {
                                m_AdManager.adsEvents[i].gameEvent = (GameState)EditorGUILayout.EnumPopup(m_AdManager.adsEvents[i].gameEvent, GUILayout.Width(100));
                                if (m_AdManager.adsEvents[i].AddToCall == AdType.Banner)
                                {
                                    m_AdManager.adsEvents[i].AddToCall = AdType.Interstitial;
                                }
                                m_AdManager.adsEvents[i].AddToCall = (AdType)EditorGUILayout.EnumPopup(m_AdManager.adsEvents[i].AddToCall, GUILayout.Width(100));
                                m_AdManager.adsEvents[i].everyLevel = EditorGUILayout.IntPopup(m_AdManager.adsEvents[i].everyLevel, new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" }, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, GUILayout.Width(70));
                            }
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("Add Call Event"), GUILayout.Width(100)))
                        {
                            m_AdManager.adsEvents.Add(new() { everyLevel = 1, gameEvent = GameState.None });
                            EditorUtility.SetDirty(m_AdManager);
                            AssetDatabase.SaveAssets();
                        }
                        if (GUILayout.Button(new GUIContent("Save"), GUILayout.Width(80)))
                        {
                            EditorUtility.SetDirty(m_AdManager);
                            AssetDatabase.SaveAssets();
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.Label("========================================================================================================");
                }
                GUILayout.EndVertical();
            }
        }
        private static void AddDefineSymbolToGroup(BuildTargetGroup targetGroup, string symbol)
        {
            // Get existing symbols
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            // Check if symbol already exists
            if (!defines.Contains(symbol))
            {
                // Add new symbol
                defines += $";{symbol}";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
            }
        }
        private static void RemoveDefineSymbolToGroup(BuildTargetGroup targetGroup, string symbol)
        {
            // Get existing symbols
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            // Check if the symbol exists
            if (defines.Contains(symbol))
            {
                // Remove the symbol safely
                string updatedDefines = defines.Replace(symbol, "").Replace(";;", ";").Trim(';');
                // Apply the updated define symbols
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, updatedDefines);
                Debug.Log($"Removed '{symbol}' from {targetGroup}");
            }
        }
        #endregion

        private RateUsData rateSettings;
        void ShowRateUs()
        {
            rateSettings = Resources.Load<RateUsData>("Rateus/RateUsSetting");
            if (rateSettings == null)
            {
                CreateAdSettings();
                rateSettings = Resources.Load<RateUsData>("Rateus/RateUsSetting");
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Your App IDs:", EditorStyles.boldLabel);
            rateSettings.iosAppID = EditorGUILayout.TextField("iOS App ID", rateSettings.iosAppID, GUILayout.Width(500));
            rateSettings.googlePlayBundleID = EditorGUILayout.TextField("Google Play bundle ID", rateSettings.googlePlayBundleID, GUILayout.Width(500));
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("When To Show Popup", GUILayout.Width(150));
            rateSettings.WhenToShow = (GameState)EditorGUILayout.EnumPopup(rateSettings.WhenToShow, GUILayout.Width(140));
            EditorGUILayout.LabelField("||", GUILayout.Width(20));
            EditorGUILayout.LabelField("Call on every", GUILayout.Width(100));
            rateSettings.CallOnEvery = EditorGUILayout.IntPopup(rateSettings.CallOnEvery, new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" }, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, GUILayout.Width(70));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button(new GUIContent("Save"), GUILayout.Width(80)))
            {
                EditorUtility.SetDirty(rateSettings);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
   
        private static Texture WebsiteIcon;
        private static Texture YoutubeIcon;
        private static Texture DiscordIcon;
        private static Texture TwitterIcon;
        private static Texture FacebookIcon;
        private static Texture InstagramIcon;
        private static Texture techjuegoIcon;
        static void LoadIcons()
        {
            if (techjuegoIcon == null)
            {
                techjuegoIcon = Resources.Load("Graphics/techjuegoIcon") as Texture;
            }
            if (WebsiteIcon == null)
            {
                WebsiteIcon = Resources.Load("Graphics/WebsiteIcon") as Texture;
            }
            if (YoutubeIcon == null)
            {
                YoutubeIcon = Resources.Load("Graphics/YoutubeIcon") as Texture;
            }
            if (DiscordIcon == null)
            {
                DiscordIcon = Resources.Load("Graphics/DiscordIcon") as Texture;
            }
            if (TwitterIcon == null)
            {
                TwitterIcon = Resources.Load("Graphics/TwitterIcon") as Texture;
            }
            if (FacebookIcon == null)
            {
                FacebookIcon = Resources.Load("Graphics/FacebookIcon") as Texture;
            }
            if (InstagramIcon == null)
            {
                InstagramIcon = Resources.Load("Graphics/InstagramIcon") as Texture;
            }
            if (techjuegoIcon == null)
            {
                techjuegoIcon = Resources.Load("Graphics/techjuegoIcon") as Texture;
            }
        }
        private void ShowAbout()
        {
            LoadIcons();
            GUILayout.Label(techjuegoIcon);

            GUILayout.Label("Connect with us:");
            EditorGUILayout.SelectableLabel("techjuego@gmail.com");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(" Website", WebsiteIcon), GUILayout.Width(200)))
            {
                Application.OpenURL("techjuego.com");
            }
            if (GUILayout.Button(new GUIContent(" Youtube", YoutubeIcon), GUILayout.Width(200)))
            {
                Socialmedia.SubscribeOnYoutube();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(" Discord", DiscordIcon), GUILayout.Width(200)))
            {
                Socialmedia.ConnectOnDiscord();
            }
            if (GUILayout.Button(new GUIContent(" Twitter", TwitterIcon), GUILayout.Width(200)))
            {
                Socialmedia.FollowOnTweeter();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(" Facebook", FacebookIcon), GUILayout.Width(200)))
            {
                Socialmedia.FollowOnFacebook();
            }
            if (GUILayout.Button(new GUIContent(" Instagram", InstagramIcon), GUILayout.Width(200)))
            {
                Socialmedia.FollowOnInstagram();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Open Asset Store Publisher Page", GUILayout.Width(400)))
            {
                Application.OpenURL("https://assetstore.unity.com/publishers/46402");
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
        private string basePath = "Assets/TechJuego/GetOut/Resources";
        private void CreateAdSettings()
        {
            RateUsData asset = ScriptableObject.CreateInstance<RateUsData>();
            if (!AssetDatabase.IsValidFolder(basePath + "/Rateus/"))
            {
                AssetDatabase.CreateFolder(basePath, "Rateus");
                AssetDatabase.Refresh();
            }
            AssetDatabase.CreateAsset(asset, basePath + "/Rateus/RateUsSetting.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
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
        private void CreateMonetizationSettings()
        {
            AdManager asset = ScriptableObject.CreateInstance<AdManager>();
            if (!AssetDatabase.IsValidFolder(basePath + "/Monetization/"))
            {
                AssetDatabase.CreateFolder(basePath, "Monetization");
                AssetDatabase.Refresh();
            }
            AssetDatabase.CreateAsset(asset, basePath +"/Monetization/AdManager.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
#endif
}