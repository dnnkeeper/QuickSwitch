using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace QuickSwitch
{

    public abstract class PopupWindow : EditorWindow
    {
        private Vector2 _offset;

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public static T GetDraggableWindow<T>() where T : PopupWindow
        {
            var array = Resources.FindObjectsOfTypeAll(typeof(T)) as T[];
            var t = (array.Length <= 0) ? null : array[0];

            return t ?? CreateInstance<T>();
        }


        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        /// <summary>
        /// Show draggable editor window with popup-style framing.
        /// </summary>
        public void Show<T>(Rect position, bool focus = true) where T : PopupWindow
        {
            this.minSize = position.size;
            this.position = position;

            if (focus) this.Focus();
            this.ShowPopup();
        }

        /// <summary>
        /// Callback for drawing GUI controls for the popup window.
        /// </summary>
        [SuppressMessage("ReSharper", "InvertIf")]
        public virtual void OnGUI()
        {
            var e = Event.current;

            // calculate offset for the mouse cursor when start dragging
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                _offset = position.position - GUIUtility.GUIToScreenPoint(e.mousePosition);
            }

            // drag window
            if (e.button == 0 && e.type == EventType.MouseDrag)
            {
                var mousePos = GUIUtility.GUIToScreenPoint(e.mousePosition);
                position = new Rect(mousePos + _offset, position.size);
            }
        }

    }
}