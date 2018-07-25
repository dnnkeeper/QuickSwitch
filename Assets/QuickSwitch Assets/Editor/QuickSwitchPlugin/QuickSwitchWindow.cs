using UnityEditor;
using UnityEngine;

namespace QuickSwitch
{
    public class QuickSwitchWindow : EditorWindow
    {
        //static QuickSwitchWindow thisWindow;

        [MenuItem("QuickSwitch/Settings")]
        public static void Init()
        {
            GetWindow<QuickSwitchWindow>("QuickSwitch");
        }

        void OnGUI()
        {
            QuickSwitch.auto_minimize = GUILayout.Toggle(QuickSwitch.auto_minimize, "Auto minimize");

            GUILayout.Label("count: " + QuickSwitch.MinimizedWindowsCount);

            GUILayout.Label("activeGameObject: " + Selection.activeGameObject);

            GUILayout.Label("inspectorWindow: " + ((QuickSwitch.inspectorWindowMinimized != null) ? "Yes" : "No"));

            GUILayout.Space(20);

            if (EditorWindow.focusedWindow != null)
                GUILayout.Label("Focused: " + EditorWindow.focusedWindow.ToString());

            GUILayout.Space(20);

            if (QuickSwitch.HandlerWindow != null && GUILayout.Button("Reset Handler"))
            {
                QuickSwitch.HandlerWindow.position = new Rect(Vector2.zero, QuickSwitch.HandlerWindow.position.size);
            }
        }
    }
}