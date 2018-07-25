using UnityEditor;
using UnityEngine;

namespace QuickSwitch
{

    public class HandlerWindow : PopupWindow
    {
        //public Vector2 pos;
        //public Vector2 size;

        GUISkin skin;

        private void OnEnable()
        {
            skin = ScriptableObject.CreateInstance<GUISkin>();

            var style = new GUIStyle();

            style.alignment = TextAnchor.MiddleCenter;

            style.margin = new RectOffset(3, 3, 3, 3);

            skin.button = style;
            //AssemblyReloadEvents.beforeAssemblyReload += beforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += afterAssemblyReload;
            //position = new Rect(pos, size);
            QuickSwitch.handlerWindow = this;
            //Debug.Log("Set Handler Window");
        }

        private void afterAssemblyReload()
        {
            if (QuickSwitch.handlerWindow == null)
            {
                QuickSwitch.handlerWindow = this;
            }
            else
            {
                if (QuickSwitch.handlerWindow != this)
                {
                    Debug.Log("Handler already exist. Close()");
                    Close();
                }
            }
        }

        private void OnDisable()
        {
            if (QuickSwitch.handlerWindow == this)
                QuickSwitch.handlerWindow = null;
        }

        /*private void beforeAssemblyReload()
        {
            Close(); 
        }*/

        public bool vertical;

        bool drag;

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
                drag = false;
                click = true;
            }
            else if (e.type == EventType.MouseDrag)
            {
                drag = true;
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
