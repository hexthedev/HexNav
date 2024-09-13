#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceNavigatorDriver
{
    public partial class SpaceNavigatorToolbar
    {
        [EditorToolbarElement(ID, typeof(SceneView))]
        class NavigationMode : EditorToolbarDropdown
        {
            public const string ID = "SpaceNavigator/NavigationMode";

            Texture2D m_navModeFly;
            Texture2D m_navModeOrbit;
            Texture2D m_navModeTelekinesis;
            Texture2D m_navModeGrabMove;
            VisualElement m_coordinateSystemDropdown;

            public NavigationMode()
            {
                tooltip = "Navigation Mode";
                m_navModeFly = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath + "NavigationMode Fly.psd");
                m_navModeOrbit = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath + "NavigationMode Orbit.psd");
                m_navModeTelekinesis = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath + "NavigationMode Telekinesis.psd");
                m_navModeGrabMove = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath + "NavigationMode GrabMove.psd");
                switch (Settings.Mode)
                {
                    case OperationMode.Fly: icon = m_navModeFly; break;
                    case OperationMode.Orbit: icon = m_navModeOrbit; break;
                    case OperationMode.Telekinesis: icon = m_navModeTelekinesis; break;
                    case OperationMode.GrabMove: icon = m_navModeGrabMove; break;
                }
                clicked += ShowDropdown;
            }

            void ShowDropdown()
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Fly"), Settings.Mode == OperationMode.Fly, () =>
                {
                    icon = m_navModeFly;
                    Settings.Profile.Mode = OperationMode.Fly;
                    RefreshLayout.Invoke(this, EventArgs.Empty);
                });
                menu.AddItem(new GUIContent("Orbit"), Settings.Mode == OperationMode.Orbit, () =>
                {
                    icon = m_navModeOrbit;
                    Settings.Profile.Mode = OperationMode.Orbit;
                    RefreshLayout.Invoke(this, EventArgs.Empty);
                });
                menu.AddItem(new GUIContent("Telekinesis"), Settings.Mode == OperationMode.Telekinesis, () =>
                {
                    icon = m_navModeTelekinesis;
                    Settings.Profile.Mode = OperationMode.Telekinesis;
                    RefreshLayout.Invoke(this, EventArgs.Empty);
                });
                menu.AddItem(new GUIContent("Grab Move"), Settings.Mode == OperationMode.GrabMove, () =>
                {
                    icon = m_navModeGrabMove;
                    Settings.Profile.Mode = OperationMode.GrabMove;
                    RefreshLayout.Invoke(this, EventArgs.Empty);
                });
                menu.ShowAsContext();
            }
        }
    }
}
#endif