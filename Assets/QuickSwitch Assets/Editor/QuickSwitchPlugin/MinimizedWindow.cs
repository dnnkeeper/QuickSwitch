using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace QuickSwitch
{

    public class MinimizedWindow : PopupWindow
    {
        [NonSerialized]
        protected bool initialized;

        public Vector2 fullWindowPosition;

        public Vector2 fullWindowSize;

        public string windowTypeName;

        EditorWindow fullWindow;

        static List<EditorWindow> excludedWindows = new List<EditorWindow>();

        public override void OnGUI()
        {
            base.OnGUI();

            var e = Event.current;

            if (e.type == EventType.DragExited)
            {
                if (fullWindow != null)
                {
                    //Debug.LogWarning(titleContent.text + " DragExited Close: fullWindow = " + fullWindow);
                    Close();
                }
                else
                {
                    //Debug.LogWarning(titleContent.text + " DragExited, don't close: fullWindow == null");
                }
            }

            if (fullWindow != null && !IsDragAndDrop)
            {
                //Debug.Log("fullWindow != null && DragAndDrop.objectReferences.Length == 0");

                Close();
            }

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
            if (fullWindow == null)
            {
                if (windowTypeName != null)
                {
                    Type windowType = Type.GetType(windowTypeName);

                    //Debug.Log("RestoreWindow '" + windowTypeName + "' type: " + windowType);

                    if (windowType != null)
                    {
                        fullWindow = EditorWindow.GetWindow(windowType);

                        if (fullWindow.GetType().ToString().StartsWith("UnityEditor.InspectorWindow"))
                        {
                            if (CheckInspectorIsLocked(fullWindow))
                            {
                                //Debug.LogWarning("locked inspector - create a new one!");

                                fullWindow = ScriptableObject.CreateInstance(Type.GetType("UnityEditor.InspectorWindow, UnityEditor")) as EditorWindow;

                                fullWindow.Show();
                            }
                        }
                       
                        fullWindow.position = new Rect(fullWindowPosition, fullWindowSize);
                        fullWindow.titleContent = titleContent;
                    }

                    /*if ( DragAndDrop.objectReferences.Length != 0)
                    {
                        Debug.Log("Excluded window "+fullWindow);
                        excludedWindows.Add(fullWindow);
                    }
                    else
                    {
                        excludedWindows.Clear();
                    }*/

                    OnFullWindowCreated(fullWindow);

                    //if (DragAndDrop.objectReferences.Length == 0)
                        Close();
                }
            }

            return fullWindow;
        }

        static bool CheckInspectorIsLocked(EditorWindow fullWindow)
        {
            Type InspectorWindowType = Type.GetType("UnityEditor.InspectorWindow, UnityEditor");
            if (fullWindow.GetType() != InspectorWindowType)
                return false;
            MethodInfo isLockedMethod = InspectorWindowType.GetProperty("isLocked", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).GetGetMethod(true);
            bool isLocked = (bool)isLockedMethod.Invoke(fullWindow, null);
            return isLocked;
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

            if (excludedWindows.Contains(currentWindow))
            {
                Debug.LogWarning(currentWindow.titleContent.text + " is excluded from minimization");
                return;
            }

            if (IsDragAndDrop)
            {
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
                    //Debug.Log("found assemblyName "+ assemblyName + " for "+ WindowTypeName);
                    break;
                }
            }

            WindowTypeName = WindowTypeName + ", " + assemblyName;

            if (Type.GetType(WindowTypeName) == null)
            {
                //Can't serialize type of this window as string
                Debug.LogWarning("Can't serialize type of this window as string "+ WindowTypeName);
                return;
            }

            if ( currentWindow.GetType().ToString().StartsWith("UnityEditor.InspectorWindow")) //currentWindow.titleContent.text == "Inspector")
            {
                //Debug.LogWarning("Inspector type = "+ WindowTypeName);
                if ( (QuickSwitch.Auto_inspector && Selection.activeGameObject != null) || CheckInspectorIsLocked(currentWindow))
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

        private void OnFocus()
        {
            if (initialized)
            {
                RestoreWindow();
            }
        }

        static bool IsDragAndDrop
        {
            get { return DragAndDrop.paths.Length != 0 || DragAndDrop.objectReferences.Length != 0; }
        }

        private void OnLostFocus()
        {

            if (fullWindow != null)
            {
                //Debug.LogWarning("OnLostFocus don't close - no full window");
                return;
            }
            else if (IsDragAndDrop)
            {
                //Debug.Log("OnLostFocus don't close - Drag");
            }
            else
            {
                if (initialized)
                {
                    //Debug.LogWarning("OnLostFocus Close");
                    Close();
                }
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