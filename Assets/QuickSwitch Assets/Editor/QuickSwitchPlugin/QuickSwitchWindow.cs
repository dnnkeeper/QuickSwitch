using UnityEditor;
using UnityEngine;

namespace QuickSwitch
{
    public class QuickSwitchWindow : EditorWindow
    {
        //static QuickSwitchWindow thisWindow;

        [MenuItem("Window/QuickSwitch/Settings")]
        public static void Init()
        {
            GetWindow<QuickSwitchWindow>("QuickSwitch");
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();

            QuickSwitch.auto_minimize = GUILayout.Toggle(QuickSwitch.auto_minimize, "Auto minimize");

            QuickSwitch.auto_inspector = GUILayout.Toggle(QuickSwitch.auto_inspector, "Inspector auto-expand");

            GUILayout.EndHorizontal();

            //GUILayout.Label("count: " + QuickSwitch.MinimizedWindowsCount);

            //GUILayout.Label("activeGameObject: " + Selection.activeGameObject);

            //GUILayout.Label("inspectorWindow: " + ((QuickSwitch.inspectorWindowMinimized != null) ? "Yes" : "No"));

            //GUILayout.Space(20);

            //if (EditorWindow.focusedWindow != null)
            //    GUILayout.Label("Focused: " + EditorWindow.focusedWindow.ToString());

            //GUILayout.Space(20);

            if (QuickSwitch.handlerWindow != null && GUILayout.Button("Reset Handler"))
            {
                QuickSwitch.handlerWindow.position = new Rect(Vector2.zero, QuickSwitch.handlerWindow.position.size);
            }
        }
    }
}