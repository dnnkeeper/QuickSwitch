﻿using System.Collections.Generic;
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
        static EditorWindow focusedWindow;

        static String activeObjectName;

        static HashSet<EditorWindow> minimizedWindows = new HashSet<EditorWindow>();

        static bool needToResort;

        public static Vector2 panelPos;

        static int tabWidth = 100;

        static int tabHeight = 26;

        static int handleWidth = 30;

        public static int MinimizedWindowsCount
        {
            get
            {
                return minimizedWindows.Count;
            }
        }

        public static bool Auto_minimize
        {
            get
            {
                return EditorPrefs.GetBool("QuickSwitch.auto_minimize");
            }

            set
            {
                EditorPrefs.SetBool("QuickSwitch.auto_minimize", value);
            }
        }

        public static bool Auto_inspector
        {
            get
            {
                return EditorPrefs.GetBool("QuickSwitch.auto_inspector");
            }

            set
            {
                EditorPrefs.SetBool("QuickSwitch.auto_inspector", value);
            }
        }

        public static HandlerWindow handlerWindow;

        public static MinimizedWindow inspectorWindowMinimized;

        public static EditorWindow inspectorWindow;

        static QuickSwitch()
        {
            EditorApplication.update += Update;

            ResetPanelPos(false);
        }

        public static void ResetPanelPos(bool force = true)
        {
            if (force || handlerWindow == null || handlerWindow.position.position == Vector2.zero)
                panelPos = new Vector2(Screen.currentResolution.width - 45, Screen.currentResolution.height/2);

            if (handlerWindow != null)
            {
                handlerWindow.vertical = true;

                handlerWindow.position = new Rect(panelPos, handlerWindow.position.size);
            }
            
            Resort();
        }

        [InitializeOnLoadMethod]
        static void EditorInit()
        {
            FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler", BindingFlags.Static | BindingFlags.NonPublic);

            EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue(null);

            value += EditorGlobalKeyPress;

            info.SetValue(null, value);
        }

        public static bool ctrl;

        static void EditorGlobalKeyPress()
        {
            if (Event.current.control)
            {
                //Debug.Log("ctrl true");

                ctrl = true;
            }
            else
            {
                if (ctrl)
                {
                    MinimizedWindow min = EditorWindow.focusedWindow as MinimizedWindow;

                    if (min != null)
                    {
                        min.RestoreWindow();
                    }

                    //Debug.Log("ctrl true->false");

                    ctrl = false;
                }
            }
        }

        static void Update()
        {
            if (minimizedWindows.Count > 0)
            {
                if (handlerWindow == null)
                {
                    //Debug.Log("handlerWindow created at "+ panelPos); 

                    handlerWindow = HandlerWindow.Create(panelPos, new Vector2(handleWidth, tabHeight));

                    Resort();
                }
            }
            else
            {
                if (handlerWindow != null)
                {
                    //Debug.Log("No minimizedWindows. Close handler");

                    handlerWindow.Close(); 
                }
            }

            if (handlerWindow != null)
            {
                if (panelPos != handlerWindow.position.position && handlerWindow.position.position != Vector2.zero)
                {
                    panelPos = handlerWindow.position.position;

                    needToResort = true;
                }
            }

            string newActiveObjectName = (Selection.activeObject != null ? Selection.activeObject.ToString() : "");

            if (Auto_inspector && activeObjectName != newActiveObjectName)
            {
                activeObjectName = newActiveObjectName;

                if (!String.IsNullOrEmpty(activeObjectName))
                {
                    //Debug.Log("activeObject: " + activeObjectName); 

                    if (inspectorWindowMinimized != null)
                    {
                        inspectorWindowMinimized.RestoreWindow();

                        if (focusedWindow != null)
                            EditorWindow.FocusWindowIfItsOpen(focusedWindow.GetType());
                    }
                }
                else
                {
                    //Debug.Log("activeObject: null");
                    if (inspectorWindow != null && !CheckIsDocked(inspectorWindow))
                    {
                        bool isLocked = CheckInspectorIsLocked(inspectorWindow);

                        if (isLocked)
                        {
                            //Debug.LogWarning("locked inspector does not minimize");
                        }
                        else
                        {
                            //Debug.Log("Minimize Inspector");

                            MinimizedWindow.MinimizeWindow(inspectorWindow);
                        }
                    }
                }
            }

            if (Auto_minimize && focusedWindow != EditorWindow.focusedWindow)
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
                if (handlerWindow != null && panelPos.x + tabWidth * minimizedWindows.Count > Screen.currentResolution.width)
                {
                    handlerWindow.vertical = true;
                }

                int i = 0;

                foreach (var w in minimizedWindows)
                {
                    //Debug.Log(w.titleContent.text+" sort "+i);

                    w.minSize = new Vector2(tabWidth, tabHeight);

                    //new Rect(Screen.currentResolution.width - width, Screen.currentResolution.height - height / 2 - 100 - i * (height), width, height);
                    Rect r =
                        (handlerWindow == null || handlerWindow.vertical) ?
                        new Rect(panelPos.x + handleWidth - tabWidth, panelPos.y + tabHeight + i * tabHeight, tabWidth, tabHeight) :
                        new Rect(panelPos.x + handleWidth + tabWidth * i, panelPos.y, tabWidth, tabHeight);

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

        public static void OnWindowAdded(EditorWindow window)
        {
            RegisterMinimizedWindow(window);
        }

        public static void Resort()
        {
            needToResort = true;
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

            var typeName = (window as MinimizedWindow).windowTypeName;

            if (typeName != null && typeName.StartsWith("UnityEditor.InspectorWindow"))
            {
                //Debug.Log("Inspector minimized registered");

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

        public static bool CheckInspectorIsLocked(EditorWindow fullWindow)
        {
            Type InspectorWindowType = Type.GetType("UnityEditor.InspectorWindow, UnityEditor");

            if (fullWindow.GetType() != InspectorWindowType)
                return false;

            MethodInfo isLockedMethod = InspectorWindowType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(true);

            bool isLocked = (bool)isLockedMethod.Invoke(fullWindow, null);

            return isLocked;
        }

        static bool CheckIsDocked(EditorWindow currentWindow)
        {
            BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

            MethodInfo isDockedMethod = typeof(EditorWindow).GetProperty("docked", fullBinding).GetGetMethod(true);

            return (bool)isDockedMethod.Invoke(currentWindow, null);
        }

        [MenuItem("Window/QuickSwitch/Dock To QuickSwitch &PGUP")]
        public static void DockWindowMenu()
        {
            var currentWindow = EditorWindow.focusedWindow;

            if ((currentWindow as PopupWindow) != null || (currentWindow as QuickSwitchWindow) != null)
            {
                Debug.LogWarning(currentWindow.titleContent.text + " PopupWindow can't be docked");
                return;
            }

            var GetWindowNextToMethodInfo = Make_GetWindowNextTo_Method(currentWindow.GetType());

            EditorWindow window = (EditorWindow)GetWindowNextToMethodInfo.Invoke(null, new object[] { new Type[] { typeof(QuickSwitchWindow) } });
            
            window.Close();

            //To dock window next to other we have to call this method second time in case this window was docked
            window = (EditorWindow)GetWindowNextToMethodInfo.Invoke(null, new object[] { new Type[] { typeof(QuickSwitchWindow) } });
        }

        [MenuItem("Window/QuickSwitch/Minimize EditorWindow &PGDN")]
        public static void MinimizeWindowMenu()
        {
            var currentWindow = EditorWindow.focusedWindow;

            MinimizedWindow.MinimizeWindow(currentWindow);
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