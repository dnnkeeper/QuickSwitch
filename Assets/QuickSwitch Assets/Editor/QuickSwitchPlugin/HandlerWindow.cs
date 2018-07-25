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
            QuickSwitch.HandlerWindow = this;
            //Debug.Log("Set Handler Window");
        }

        private void afterAssemblyReload()
        {
            if (QuickSwitch.HandlerWindow == null)
            {
                QuickSwitch.HandlerWindow = this;
            }
            else
            {
                if (QuickSwitch.HandlerWindow != this)
                {
                    Debug.Log("Handler already exist. Close()");
                    Close();
                }
            }
        }

        private void OnDisable()
        {
            if (QuickSwitch.HandlerWindow == this)
                QuickSwitch.HandlerWindow = null;
        }

        /*private void beforeAssemblyReload()
        {
            Close(); 
        }*/

        public override void OnGUI()
        {
            base.OnGUI();
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
