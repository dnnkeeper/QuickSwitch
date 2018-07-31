using UnityEditor;
using UnityEngine;

namespace QuickSwitch
{

    public class HandlerWindow : PopupWindow
    {
        private GUISkin skin;

        private void OnEnable()
        {
            if (QuickSwitch.handlerWindow == null)
            {
                //Debug.Log("QuickSwitch.handlerWindow = "+this); 
                QuickSwitch.handlerWindow = this;
            }
            else
            {
                if (QuickSwitch.handlerWindow != this)
                {
                    Debug.LogWarning("Handler already exist. Close()");
                    Close();
                    return;
                }
            }

            //Debug.Log("Handler OnEnable");

            skin = ScriptableObject.CreateInstance<GUISkin>();

            var style = new GUIStyle();

            style.alignment = TextAnchor.MiddleCenter;

            style.margin = new RectOffset(3, 3, 3, 3);

            skin.button = style;

            skin.hideFlags = HideFlags.DontSave;

            AssemblyReloadEvents.afterAssemblyReload += afterAssemblyReload;
        }

        private void afterAssemblyReload()
        {
            QuickSwitch.panelPos = position.position;

            //It is needed to re-create EditorWindow to keep it after Editor restarts
            Close();
        }

        private void OnDisable()
        {
            if (QuickSwitch.handlerWindow == this)
                QuickSwitch.handlerWindow = null;
        }

        public static HandlerWindow Create(Vector2 panelPos, Vector2 handlerSize)
        {
            HandlerWindow handlerWindow = ScriptableObject.CreateInstance<HandlerWindow>();
            var handlerRect = new Rect(panelPos, handlerSize);
            handlerWindow.minSize = handlerRect.size;
            handlerWindow.position = handlerRect;
            handlerWindow.ShowPopup();
            return handlerWindow;
        }

        public bool vertical;

        //bool drag;

        bool click;

        public override void OnGUI()
        {
            base.OnGUI();

            GUILayout.BeginHorizontal();

            GUI.skin = skin;

            //GUILayout.Label("⋮⋮⋮", GUILayout.MinHeight(20));

            var e = Event.current;

            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                //drag = false;
                click = true;
            }
            else if (e.type == EventType.MouseDrag)
            {
                //drag = true;
                click = false;
            }

            if (e.button == 0 && e.type == EventType.MouseUp && click)
            {
                vertical = !vertical;

                QuickSwitch.Resort();
            }

            if (GUILayout.Button(vertical ? "⋮" : "∙∙∙", GUILayout.MinHeight(20), GUILayout.MinWidth(20)))
            {
               
            }

            GUILayout.EndHorizontal();

            GUI.skin = null;

            //pos = position.position;
            //size = position.size;
            //QuickSwitch.panelPos = position.position;
            //if (GUILayout.Button("*"))
            //{
            //    Close();
            //}
        }
    }
}
