using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;

namespace QuickSwitch
{
    [InitializeOnLoad]
    public class QuickSwitch
    {
        public static bool auto_minimize = true;

        static EditorWindow focusedWindow;

        static GameObject activeObject;

        static HashSet<EditorWindow> minimizedWindows = new HashSet<EditorWindow>();

        static bool needToResort;

        public static Vector2 panelPos;

        static int tabWidth = 100;
        static int tabHeight = 26;

        public static int MinimizedWindowsCount
        {
            get
            {
                return minimizedWindows.Count;
            }
        }

        public static HandlerWindow HandlerWindow;

        static QuickSwitch()
        {
            AssemblyReloadEvents.beforeAssemblyReload += beforeAssemblyReload;

            EditorApplication.update += Update;
        }

        static void Update()
        {
            if (minimizedWindows.Count > 0)
            {
                if (HandlerWindow == null)
                {
                    //Debug.Log("handlerWindow created");
                    HandlerWindow = ScriptableObject.CreateInstance<HandlerWindow>();
                    var handlerRect = new Rect(panelPos, new Vector2(tabHeight, tabHeight));
                    HandlerWindow.minSize = handlerRect.size;
                    HandlerWindow.position = handlerRect;
                    HandlerWindow.ShowPopup();
                }
            }
            else
            {
                if (HandlerWindow != null)
                    HandlerWindow.Close();
            }

            if (HandlerWindow != null)
            {
                if (panelPos != HandlerWindow.position.position && HandlerWindow.position.position != Vector2.zero)
                {
                    panelPos = HandlerWindow.position.position;
                    needToResort = true;
                }
            }

            if (activeObject != Selection.activeGameObject)
            {
                activeObject = Selection.activeGameObject;

                if (activeObject != null)
                {
                    if (inspectorWindowMinimized != null)
                        inspectorWindowMinimized.RestoreWindow();
                }
                else
                {
                    if (inspectorWindow != null && !CheckIsDocked(inspectorWindow))
                        MinimizedWindow.MinimizeWindow(inspectorWindow);
                }
            }

            if (auto_minimize && focusedWindow != EditorWindow.focusedWindow)
            {
                if (EditorWindow.focusedWindow != null)
                {
                    if (focusedWindow != null)
                    {
                        //Debug.Log("Switch from" + focusedWindow + " to" + EditorWindow.focusedWindow);
                        if (!CheckIsDocked(focusedWindow))
                        {
                            MinimizedWindow.MinimizeWindow(focusedWindow);
                        }
                    }

                    focusedWindow = EditorWindow.focusedWindow;
                }
            }

            if (needToResort)
            {
                bool vertical = false;
                if (panelPos.x + tabWidth * minimizedWindows.Count > Screen.currentResolution.width)
                {
                    vertical = true;
                }

                int i = 0;
                foreach (var w in minimizedWindows)
                {
                    //Debug.Log(w.titleContent.text+" sort "+i);

                    w.minSize = new Vector2(tabWidth, tabHeight);

                    //new Rect(Screen.currentResolution.width - width, Screen.currentResolution.height - height / 2 - 100 - i * (height), width, height);
                    Rect r =
                        vertical ?
                        new Rect(panelPos.x + tabHeight - tabWidth, panelPos.y + tabHeight + i * tabHeight, tabWidth, tabHeight) :
                        new Rect(panelPos.x + tabHeight + tabWidth * i, panelPos.y, tabWidth, tabHeight);

                    if (w.position.position != r.position)
                    {
                        //Debug.LogWarning(w.position.position.y + " != " + r.position.y);
                        try
                        {
                            w.position = r;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.Message);
                        }
                    }

                    i++;
                }
                needToResort = false;
            }
        }

        public static MinimizedWindow inspectorWindowMinimized;
        public static EditorWindow inspectorWindow;

        public static void OnWindowAdded(EditorWindow window)
        {
            RegisterMinimizedWindow(window);
        }

        public static void RegisterMinimizedWindow(EditorWindow window)
        {
            if (!minimizedWindows.Contains(window))
            {
                minimizedWindows.Add(window);

                //Debug.Log(window.titleContent.text + " registered");

                needToResort = true;
            }
            else
            {
                //Debug.Log(window.titleContent.text + " already registered");
            }

            if (window.titleContent.text == "Inspector")
            {
                inspectorWindowMinimized = window as MinimizedWindow;
            }
        }

        public static void OnWindowClosed(EditorWindow window)
        {
            //Debug.Log(window.titleContent.text + " removed"); 

            if (minimizedWindows.Contains(window))
                minimizedWindows.Remove(window);
        }

        public static void RestoreWindow(EditorWindow currentWindow)
        {
            var minimized = currentWindow as MinimizedWindow;
            if (minimized != null)
            {
                minimized.RestoreWindow();
            }
        }

        static bool CheckIsDocked(EditorWindow currentWindow)
        {
            BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

            MethodInfo isDockedMethod = typeof(EditorWindow).GetProperty("docked", fullBinding).GetGetMethod(true);

            return (bool)isDockedMethod.Invoke(currentWindow, null);
        }

        private static void beforeAssemblyReload()
        {
            //if (handlerWindow != null)
            //handlerWindow.Close();
        }

        /*private static void OnSceneGUI(SceneView sceneView)
        {
            // Do your general-purpose scene gui stuff here...
            // Applies to all scene views regardless of selection!

            // You'll need a control id to avoid messing with other tools!
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            if (Event.current.GetTypeForControl(controlID) == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Keypad1)
                {
                    Debug.Log("1 pressed!");
                    // Causes repaint & accepts event has been handled
                    Event.current.Use();
                }
            }
        }*/

        [MenuItem("QuickSwitch/Minimize EditorWindow &PGDN")]
        public static void MinimizeWindowMenu()
        {
            var currentWindow = EditorWindow.focusedWindow;

            //if (!CheckIsDocked(currentWindow))
            //{
                MinimizedWindow.MinimizeWindow(currentWindow);
            //}
            //else
            //{
            //    Debug.LogWarning("Can't minimize docked window");
            //}
        }

        [MenuItem("QuickSwitch/Dock To SceneView &PGUP")]
        public static void DockWindowMenu()
        {
            var currentWindow = EditorWindow.focusedWindow;

            if ((currentWindow as PopupWindow) != null)
            {
                Debug.LogWarning(currentWindow.titleContent.text + " PopupWindow can't be docked");
                return;
            }

            var GetWindowNextToMethodInfo = Make_GetWindowNextTo_Method(currentWindow.GetType());

            EditorWindow window = (EditorWindow)GetWindowNextToMethodInfo.Invoke(null, new object[] { new Type[] { typeof(QuickSwitchWindow) } });
            
            window.Close();

            window = (EditorWindow)GetWindowNextToMethodInfo.Invoke(null, new object[] { new Type[] { typeof(QuickSwitchWindow) } });
        }

        static MethodInfo Make_GetWindowNextTo_Method(Type currentWindowType)
        {
            var originalMethodInfo = typeof(EditorWindow).GetMethods()
                             .Where(m => m.Name == "GetWindow")
                             .Select(m => new {
                                 Method = m,
                                 Params = m.GetParameters(),
                                 Args = m.GetGenericArguments()
                             })
                             .Where(x => x.Params.Length >= 1
                                         && x.Args.Length >= 1
                                         && x.Params[0].ParameterType == typeof(Type[]))
                             .Select(x => x.Method)
                             .First();

            return originalMethodInfo.MakeGenericMethod(new[] { currentWindowType });
        }
    }
}