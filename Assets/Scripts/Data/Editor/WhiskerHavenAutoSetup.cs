#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WhiskerHaven.Audio;
using WhiskerHaven.Core;
using WhiskerHaven.Data;
using WhiskerHaven.Gameplay;
using WhiskerHaven.UI;

/// <summary>
/// Runs automatically when Unity opens the project.
/// Creates data assets, builds Bootstrap.unity, and configures build settings.
/// User only needs to press Play.
/// </summary>
[InitializeOnLoad]
public static class WhiskerHavenAutoSetup
{
    const string SCENE_PATH  = "Assets/Scenes/Bootstrap.unity";
    const string CONFIG_PATH = "Assets/Resources/Config/GameConfig.asset";
    const string SESSION_KEY = "WH_AutoSetup_v3";

    static WhiskerHavenAutoSetup()
    {
        EditorApplication.delayCall += TrySetup;
    }

    [MenuItem("WhiskerHaven/🔧 Run Full Setup (re-run)")]
    public static void ForceSetup()
    {
        SessionState.SetBool(SESSION_KEY, false);
        TrySetup();
    }

    static void TrySetup()
    {
        if (SessionState.GetBool(SESSION_KEY, false)) return;

        bool sceneExists  = File.Exists(SCENE_PATH);
        bool configExists = AssetDatabase.LoadAssetAtPath<GameConfig>(CONFIG_PATH) != null;

        if (sceneExists && configExists)
        {
            SessionState.SetBool(SESSION_KEY, true);
            AddToBuildSettings(SCENE_PATH);
            return;
        }

        Debug.Log("[WhiskerHaven] 🐱 Auto-setup starting...");

        try
        {
            EditorUtility.DisplayProgressBar("Whisker Haven Setup", "Creating data assets...", 0.1f);
            WhiskerHavenDataCreator.CreateAll();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar("Whisker Haven Setup", "Building Bootstrap scene...", 0.5f);
            BuildScene();

            EditorUtility.DisplayProgressBar("Whisker Haven Setup", "Configuring build settings...", 0.9f);
            AddToBuildSettings(SCENE_PATH);
            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();
            SessionState.SetBool(SESSION_KEY, true);

            Debug.Log("[WhiskerHaven] ✅ Setup complete! Press ▶ Play to start.");
            EditorUtility.DisplayDialog(
                "Whisker Haven Ready! 🐱",
                "Setup complete!\n\n" +
                "• 15 cats created\n• 6 habitats created\n• 20 achievements created\n• Bootstrap scene built\n\n" +
                "Press ▶ Play to start the game.",
                "Let's go! 🐾");
        }
        catch (Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError("[WhiskerHaven] ❌ Setup failed: " + e);
            EditorUtility.DisplayDialog("Setup Failed", $"{e.Message}\n\nCheck the Console for details.", "OK");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    static void BuildScene()
    {
        // Create/open a new empty scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Directional light
        var lightGo = new GameObject("Directional Light");
        var light   = lightGo.AddComponent<Light>();
        light.type      = LightType.Directional;
        light.intensity = 1f;
        lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);

        // ── Main Camera
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic      = true;
        cam.orthographicSize  = 5;
        cam.clearFlags        = CameraClearFlags.SolidColor;
        cam.backgroundColor   = new Color(0.17f, 0.09f, 0.06f);
        cam.depth             = -1;
        camGo.AddComponent<AudioListener>();

        // ── Systems root
        var systems = new GameObject("_Systems");

        void Mgr<T>(string name) where T : MonoBehaviour
        {
            var go = new GameObject(name); go.transform.SetParent(systems.transform);
            go.AddComponent<T>();
        }

        Mgr<BootstrapScene>("_Bootstrap");
        var gmGo = new GameObject("GameManager"); gmGo.transform.SetParent(systems.transform);
        var gm   = gmGo.AddComponent<GameManager>();

        // Wire GameConfig
        var config = AssetDatabase.LoadAssetAtPath<GameConfig>(CONFIG_PATH);
        if (config != null)
        {
            var so = new SerializedObject(gm);
            so.FindProperty("gameConfig").objectReferenceValue = config;
            so.ApplyModifiedProperties();
        }

        Mgr<SaveSystem>("SaveSystem");
        Mgr<ResourceManager>("ResourceManager");
        Mgr<AudioManager>("AudioManager");
        Mgr<CatManager>("CatManager");
        Mgr<HabitatManager>("HabitatManager");
        Mgr<VolunteerSystem>("VolunteerSystem");
        Mgr<PurrPowerSystem>("PurrPowerSystem");
        Mgr<MissionSystem>("MissionSystem");
        Mgr<AchievementSystem>("AchievementSystem");
        Mgr<IdleManager>("IdleManager");

        // ── Canvas
        var canvasGo = new GameObject("Canvas");
        var canvas   = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();
        var uiMgr = canvasGo.AddComponent<UIManager>();

        // Wire UIManager into GameManager
        {
            var so = new SerializedObject(gm);
            so.FindProperty("uiManager").objectReferenceValue = uiMgr;
            so.ApplyModifiedProperties();
        }

        // ── EventSystem
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<StandaloneInputModule>();

        // ── UI Panels (attach scripts; views build themselves in Awake)
        Panel<MainHUDView>(canvasGo.transform,       "HUD",              true);
        Panel<CatCollectionView>(canvasGo.transform, "CatCollection",    false);
        Panel<HabitatView>(canvasGo.transform,       "HabitatPanel",     false);
        Panel<MissionView>(canvasGo.transform,       "MissionPanel",     false);
        Panel<AchievementView>(canvasGo.transform,   "AchievementPanel", false);

        // Modals (no BaseView — own Awake)
        AddPanel(canvasGo.transform, "WelcomeBack", false).AddComponent<WelcomeBackView>();
        AddPanel(canvasGo.transform, "Tutorial",    false).AddComponent<TutorialView>();

        // Settings
        Panel<SettingsView>(canvasGo.transform, "Settings", false);

        // Notification banner
        var bannerGo = new GameObject("NotificationBanner");
        bannerGo.transform.SetParent(canvasGo.transform, false);
        bannerGo.AddComponent<RectTransform>();
        bannerGo.AddComponent<NotificationBanner>();

        // Floating numbers
        var floatGo = new GameObject("FloatingNumbers");
        floatGo.transform.SetParent(canvasGo.transform, false);
        floatGo.AddComponent<RectTransform>();
        floatGo.AddComponent<FloatingNumberPool>();

        // ── Save scene
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        Debug.Log("[WhiskerHaven] Scene saved to " + SCENE_PATH);
    }

    // Helper: add a BaseView panel (full-screen stretch)
    static T Panel<T>(Transform parent, string name, bool activeByDefault) where T : MonoBehaviour
    {
        var go = AddPanel(parent, name, activeByDefault);
        return go.AddComponent<T>();
    }

    static GameObject AddPanel(Transform parent, string name, bool activeByDefault)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.AddComponent<CanvasGroup>();
        go.SetActive(activeByDefault);
        return go;
    }

    // ── Build Settings ────────────────────────────────────────────────────────
    static void AddToBuildSettings(string scenePath)
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool found = false;
        foreach (var s in scenes) if (s.path == scenePath) { found = true; break; }
        if (!found)
        {
            scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[WhiskerHaven] Scene added to Build Settings.");
        }
    }
}
#endif
