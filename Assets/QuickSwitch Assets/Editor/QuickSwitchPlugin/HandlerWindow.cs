using UnityEditor;
using UnityEngine;

namespace QuickSwitch
{

    public class HandlerWindow : PopupWindow
    {

        //public Vector2 pos;
        //public Vector2 size;

        private void OnEnable()
        {
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

        public override void OnGUI()
        {
            base.OnGUI();

            if ( GUILayout.Button(vertical? "⁞" : "…", GUILayout.MinHeight(20), GUILayout.MinWidth(20)))
            {
                vertical = !vertical;
                QuickSwitch.Resort();
            }

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
