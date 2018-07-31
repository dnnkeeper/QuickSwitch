using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuickSwitch
{
    public class QuickSwitchWindow : EditorWindow
    {
        [MenuItem("Window/QuickSwitch/Settings")]
        public static void Init()
        {
            GetWindow<QuickSwitchWindow>("QuickSwitch");
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();

            QuickSwitch.Auto_minimize = GUILayout.Toggle(QuickSwitch.Auto_minimize, "Auto minimize");

            QuickSwitch.Auto_inspector = GUILayout.Toggle(QuickSwitch.Auto_inspector, "Inspector auto-expand");

            GUILayout.EndHorizontal();

            GUILayout.Label("Minimized:");

            //foreach(var min in minimizedWindows)
            //{
            //    GUILayout.Label(min.titleContent.text);
            //}

            //GUILayout.Label("activeObject: " + QuickSwitch.activeObjectName);

            //GUILayout.Label("count: " + QuickSwitch.MinimizedWindowsCount);

            //GUILayout.Label("activeGameObject: " + Selection.activeGameObject);

            //GUILayout.Label("inspectorWindow: " + ((QuickSwitch.inspectorWindowMinimized != null) ? "Yes" : "No"));

            //GUILayout.Space(20);

            //if (EditorWindow.focusedWindow != null)
            //    GUILayout.Label("Focused: " + EditorWindow.focusedWindow.ToString());

            //GUILayout.Space(20);

            if (QuickSwitch.handlerWindow != null && GUILayout.Button("Reset Panel Position"))
            {
                QuickSwitch.ResetPanelPos();
            }
        }
    }
}