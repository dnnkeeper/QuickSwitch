using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace QuickSwitch
{

    public class MinimizedWindow : PopupWindow
    {

        public Vector2 fullWindowPosition;

        public Vector2 fullWindowSize;

        public string windowTypeName;

        public override void OnGUI()
        {
            base.OnGUI();

            if (GUILayout.Button(titleContent, GUILayout.MinHeight(20), GUILayout.MinWidth(92), GUILayout.MaxWidth(92)))
            {
                RestoreWindow();
            }

            if (windowTypeName == null)
            {
                //Debug.Log("windowTypeName was null");
                Close();
            }
        }

        public EditorWindow RestoreWindow()
        {
            EditorWindow fullWindow = null;
            if (windowTypeName != null)
            {
                Type windowType = Type.GetType(windowTypeName); //+", UnityEditor" 

                //Debug.Log("RestoreWindow '" + windowTypeName + "' type: " + windowType);

                if (windowType != null)
                {
                    fullWindow = EditorWindow.GetWindow(windowType);
                    fullWindow.position = new Rect(fullWindowPosition, fullWindowSize);
                    fullWindow.titleContent = titleContent;
                }

                OnFullWindowCreated(fullWindow);

                Close();
            }

            return fullWindow;
        }

        void OnFullWindowCreated(EditorWindow fullWindow)
        {
            if (fullWindow != null && fullWindow.titleContent.text == "Inspector")
            {
                //Debug.Log("Inspector Full window restored");
                QuickSwitch.inspectorWindow = fullWindow;
            }
        }

        public static void MinimizeWindow(EditorWindow currentWindow)
        {
            if (currentWindow == null)
            {
                Debug.LogWarning("MinimizeWindow null");
                return;
            }

            if ((currentWindow as PopupWindow) != null)
            {
                Debug.LogWarning(currentWindow.titleContent.text + " PopupWindow can't be minimized");
                return;
            }

            string WindowTypeName = currentWindow.GetType().ToString();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            string assemblyName = "UnityEditor";
            foreach (var assembly in assemblies)
            {
                var textType = assembly.GetType(WindowTypeName);
                if (textType != null)
                {
                    assemblyName = assembly.FullName;
                    Debug.Log("found assemblyName "+ assemblyName + " for "+ WindowTypeName);
                    break;
                }
            }

            WindowTypeName = WindowTypeName + ", " + assemblyName;
            /*
            if (WindowTypeName.Split('.').Length > 1)
            {
                WindowTypeName = WindowTypeName + ", " + WindowTypeName.Substring(0, WindowTypeName.LastIndexOf('.'));
            }*/

            if (Type.GetType(WindowTypeName) == null)
            {
                //Can't serialize type of this window as string
                Debug.LogWarning("Can't serialize type of this window as string "+ WindowTypeName);
                return;
            }

            if (Selection.activeGameObject != null && currentWindow.titleContent.text == "Inspector")
            {
                return;
            }

            var minimizedWindow = ScriptableObject.CreateInstance<MinimizedWindow>();

            if (minimizedWindow != null)
            {

                minimizedWindow.titleContent = currentWindow.titleContent;

                minimizedWindow.windowTypeName = WindowTypeName;

                minimizedWindow.fullWindowPosition = currentWindow.position.position;

                minimizedWindow.fullWindowSize = currentWindow.position.size;

                minimizedWindow.ShowPopup();

                minimizedWindow.initialized = true;

                QuickSwitch.RegisterMinimizedWindow(minimizedWindow);
            }

            currentWindow.Close();
        }

        IEnumerator RestoreWindowRoutine(EditorWindow fullWindow, float t)
        {
            float period = 1 / 60f;
            float timer = 0f;
            while (timer < t)
            {
                timer += period;
                yield return new WaitForSecondsRealtime(period);
                float progress = t / timer;
                fullWindow.position = new Rect(Vector2.Lerp(position.position, fullWindowPosition, progress), Vector2.Lerp(position.size, fullWindowSize, progress));
            }
            fullWindow.position = new Rect(fullWindowPosition, fullWindowSize);
            Close();
        }

        [NonSerialized]
        bool initialized;

        private void OnFocus()
        {
            if (initialized)
            {
                RestoreWindow();
            }
        }
        
        private void OnEnable()
        {
            //Debug.Log(titleContent.text+" OnEnable");
            AssemblyReloadEvents.beforeAssemblyReload += beforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += afterAssemblyReload;
            QuickSwitch.OnWindowAdded(this);
        }

        private void beforeAssemblyReload()
        {
            initialized = false;
        }

        private void afterAssemblyReload()
        {
            initialized = true;
        }

        private void OnDisable()
        {
            //Debug.Log(titleContent.text+" OnDisable"); 
            AssemblyReloadEvents.beforeAssemblyReload -= beforeAssemblyReload;
            QuickSwitch.OnWindowClosed(this);
        }
    }
}