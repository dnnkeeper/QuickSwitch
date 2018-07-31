using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuickSwitch
{

    public class MinimizedWindow : PopupWindow
    {
        private bool _initialized;
        
        protected bool initialized
        {
            get { return _initialized;  }
            set {
                _initialized = value;
                if (value)
                {
                    OnInitialized();
                }
            }
        }

        public Vector2 fullWindowPosition;

        public Vector2 fullWindowSize;

        public string windowTypeName;

        protected EditorWindow fullWindow;

        static List<EditorWindow> excludedWindows = new List<EditorWindow>();

        [SerializeField]
        string titleContentText;

        public override void OnGUI()
        {
            base.OnGUI();

            var e = Event.current;

            if (e.type == EventType.DragExited)
            {
                if (fullWindow != null)
                {
                    //Debug.LogWarning(titleContent.text + " DragExited Close(): fullWindow = " + fullWindow);

                    Close();
                }
                else
                {
                    //Debug.LogWarning(titleContent.text + " DragExited, don't close: fullWindow == null");
                }
            }

            if (fullWindow != null && !IsDragAndDrop)
            {
                //Debug.Log("fullWindow != null && !IsDragAndDrop - Close()");

                Close();
            }

            if (GUILayout.Button(titleContent, GUILayout.MinHeight(20), GUILayout.MinWidth(92), GUILayout.MaxWidth(92)))
            {
                RestoreWindow();
            }

            if (windowTypeName == null)
            {
                //Debug.Log("windowTypeName was null - close()");

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

                    ///Debug.Log("RestoreWindow '" + windowTypeName + "' type: " + windowType);

                    if (windowType != null)
                    {
                        fullWindow = EditorWindow.GetWindow(windowType); //false, fullWindow.titleContent.text

                        if (fullWindow.GetType().ToString().StartsWith("UnityEditor.InspectorWindow"))
                        {
                            if (QuickSwitch.CheckInspectorIsLocked(fullWindow))
                            {
                                //Debug.LogWarning("locked inspector - create a new one!");

                                fullWindow = ScriptableObject.CreateInstance(Type.GetType("UnityEditor.InspectorWindow, UnityEditor")) as EditorWindow;

                                fullWindow.Show();
                            }
                        }
                       
                        fullWindow.position = new Rect(fullWindowPosition, fullWindowSize);

                        fullWindow.titleContent = titleContent;
                    }

                    if (fullWindow != null)
                    {
                        OnFullWindowCreated(fullWindow);

                        //Debug.LogWarning("RestoreWindow. Close()");

                        Close();
                    }
                }
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

        public static void MinimizeWindow(EditorWindow window)
        {
            if (window == null)
            {
                Debug.LogWarning("MinimizeWindow null");

                return;
            }

            //if ((currentWindow as QuickSwitchWindow) != null)
            //{
                //return;
            //}

            if ((window as PopupWindow) != null)
            {
                Debug.LogWarning(window.titleContent.text + " PopupWindow can't be minimized");

                return;
            }

            if (excludedWindows.Contains(window))
            {
                Debug.LogWarning(window.titleContent.text + " is excluded from minimization");

                return;
            }

            if (IsDragAndDrop)
            {
                return;
            }
            
            string WindowTypeName = window.GetType().ToString();

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

            if ( window.GetType().ToString().StartsWith("UnityEditor.InspectorWindow")) //currentWindow.titleContent.text == "Inspector")
            {
                //Debug.LogWarning("Inspector type = "+ WindowTypeName);

                if ( (QuickSwitch.Auto_inspector && Selection.activeGameObject != null) || QuickSwitch.CheckInspectorIsLocked(window))
                    return;
            }

            var minimized = createMinimizedWindow(window.titleContent, WindowTypeName, window.position);

            window.Close();

            //minimizedWindow.fullWindow = null;
            //currentWindow.minSize = Vector2.zero;
            //currentWindow.position = new Rect(new Vector2(Screen.currentResolution.width, Screen.currentResolution.height), Vector2.zero); //currentWindow.position.size
        }

        static MinimizedWindow createMinimizedWindow(GUIContent titleContent, string WindowTypeName, Rect rect)
        {
            var minimizedWindow = ScriptableObject.CreateInstance<MinimizedWindow>();

            if (minimizedWindow != null)
            {

                minimizedWindow.titleContent = titleContent;

                minimizedWindow.windowTypeName = WindowTypeName;

                minimizedWindow.fullWindowPosition = rect.position;

                minimizedWindow.fullWindowSize = rect.size;

                minimizedWindow.ShowPopup();

                minimizedWindow.initialized = true;
            }

            return minimizedWindow;
        }
        
        private void OnFocus()
        {
            if (initialized)
            {
                if (!QuickSwitch.ctrl)
                {
                    RestoreWindow();
                }
                else
                {
                    if (String.IsNullOrEmpty(titleContentText))
                        titleContentText = titleContent.text;

                    titleContent.text = "[" + titleContent.text + "]";
                }
            }
        }
         
        static bool IsDragAndDrop
        {
            get { return DragAndDrop.paths.Length != 0 || DragAndDrop.objectReferences.Length != 0; }
        }

        private void OnLostFocus()
        {
            if (!String.IsNullOrEmpty(titleContentText))
                titleContent.text = titleContentText;

            if (fullWindow == null)
            {
                //Debug.LogWarning("OnLostFocus don't close - no full window");
                return;
            }
            else if (IsDragAndDrop)
            {
                //Debug.Log("OnLostFocus don't close - Drag");
                return;
            }
            else
            {
                if (initialized)
                {
                    //Debug.LogWarning("OnLostFocus Close");
                    if (QuickSwitch.ctrl)
                    {
                        return;
                    }
                    else
                    {
                        //Debug.Log(this+ " OnLostFocus Close()");
                        Close();
                    }
                }
            }
        }

        private void OnEnable()
        {
            //Debug.Log(titleContent.text + " OnEnable");

            AssemblyReloadEvents.beforeAssemblyReload += beforeAssemblyReload;

            AssemblyReloadEvents.afterAssemblyReload += afterAssemblyReload;

            QuickSwitch.OnWindowAdded(this);
        }

        void OnInitialized()
        {
            QuickSwitch.RegisterMinimizedWindow(this);
        }

        private void beforeAssemblyReload()
        {
            initialized = false; 
        }

        private void afterAssemblyReload()
        {
            //EditorWindow is being created after UnityEditor restart but after next restart it somehow not creating again. 
            //So next editor restart leads to minimized tab not being created if it wasn't created via ScriptableObject.CreateInstance.
            //So we need to force ScriptableObject.CreateInstance again to save this tab.
            var minimized = createMinimizedWindow(titleContent, windowTypeName, new Rect(fullWindowPosition, fullWindowSize));

            Close();
        }

        private void OnDisable()
        {
            //Debug.Log(titleContent.text+" OnDisable"); 

            AssemblyReloadEvents.beforeAssemblyReload -= beforeAssemblyReload;

            QuickSwitch.OnWindowClosed(this);
        }
    }
}