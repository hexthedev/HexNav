#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace SpaceNavigatorDriver
{
    public partial class SpaceNavigatorToolbar
    {
        [EditorToolbarElement(ID, typeof(SceneView))]
        class CoordinateSystem : EditorToolbarDropdown
        {
            public const string ID = "SpaceNavigator/CoordinateSystem";

            public CoordinateSystem()
            {
                switch (Settings.System)
                {
                    case SpaceNavigatorDriver.CoordinateSystem.Camera: text = "Camera"; break;
                    case SpaceNavigatorDriver.CoordinateSystem.World: text = "World"; break;
                    case SpaceNavigatorDriver.CoordinateSystem.Parent: text = "Parent"; break;
                    case SpaceNavigatorDriver.CoordinateSystem.Local: text = "Local"; break;
                    default: throw new ArgumentOutOfRangeException();
                }
                clicked += ShowDropdown;
            }

            void ShowDropdown()
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Camera"), Settings.System == SpaceNavigatorDriver.CoordinateSystem.Camera, () =>
                {
                    text = "Camera";
                    Settings.Profile.System = SpaceNavigatorDriver.CoordinateSystem.Camera;
                });
                menu.AddItem(new GUIContent("World"), Settings.System == SpaceNavigatorDriver.CoordinateSystem.World, () =>
                {
                    text = "World";
                    Settings.Profile.System = SpaceNavigatorDriver.CoordinateSystem.World;
                });
                menu.AddItem(new GUIContent("Parent"), Settings.System == SpaceNavigatorDriver.CoordinateSystem.Parent, () =>
                {
                    text = "Parent";
                    Settings.Profile.System = SpaceNavigatorDriver.CoordinateSystem.Parent;
                });
                menu.AddItem(new GUIContent("Local"), Settings.System == SpaceNavigatorDriver.CoordinateSystem.Local, () =>
                {
                    text = "Local";
                    Settings.Profile.System = SpaceNavigatorDriver.CoordinateSystem.Local;
                });
                menu.ShowAsContext();
            }
        }
    }
}
#endif