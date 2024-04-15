using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FullscreenPlayMode
{
    static FullscreenPlayMode()
    {
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
    }

    private static void PlayModeStateChanged(PlayModeStateChange playModeStateChange)
    {
        switch (playModeStateChange)
        {
            case PlayModeStateChange.EnteredPlayMode:
                new GameObject("PlayModeFullscreen", typeof(PlayModeFullscreenMonoBehaviour));
                break;
        }
    }
}

public class PlayModeFullscreenMonoBehaviour : MonoBehaviour
{
    private List<EditorWindow> dummyViews;
    private EditorWindow fullscreenGameView;

    private void Awake()
    {
        // Hide this game object
        gameObject.hideFlags = HideFlags.HideInHierarchy;
        GameObject.DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Time.frameCount == 3)
        {
            // Create dummy views
            dummyViews = new List<EditorWindow>();

            Type dockAreaType = Type.GetType("UnityEditor.DockArea,UnityEditor");
            UnityEngine.Object[] dockAreas = Resources.FindObjectsOfTypeAll(dockAreaType);

            MethodInfo addTabMethod = dockAreaType.GetMethod("AddTab", new Type[] { typeof(EditorWindow), typeof(bool) });
            foreach (UnityEngine.Object dockArea in dockAreas)
            {
                EditorWindow dummyView = ScriptableObject.CreateInstance<EditorWindow>();
                dummyView.titleContent = new GUIContent("Dummy");
                dummyViews.Add(dummyView);

                addTabMethod.Invoke(dockArea, new object[] { dummyView, true });
            }

            // Create fullscreen game view
            Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
            fullscreenGameView = (EditorWindow)ScriptableObject.CreateInstance(gameViewType);

            PropertyInfo showToolbarProperty = gameViewType.GetProperty("showToolbar", BindingFlags.NonPublic | BindingFlags.Instance);
            showToolbarProperty.SetValue(fullscreenGameView, false);

            fullscreenGameView.ShowPopup();
            fullscreenGameView.position = new Rect(new Vector2(0, 0), new Vector2(Screen.currentResolution.width, Screen.currentResolution.height));
            fullscreenGameView.Focus();
        }

        // Check for the Escape key to stop the game play
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopPlayMode();
        }
    }

    private void OnDestroy()
    {
        StopPlayMode();
    }

    private void StopPlayMode()
    {
        // Exit play mode when the Escape key is pressed
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }

        // Destroy fullscreen game view
        if (fullscreenGameView != null)
        {
            fullscreenGameView.Close();
            fullscreenGameView = null;
        }

        // Destroy dummy views
        if (dummyViews != null)
        {
            foreach (EditorWindow dummyView in dummyViews)
            {
                dummyView.Close();
            }
            dummyViews.Clear();
            dummyViews = null;
        }
    }
}