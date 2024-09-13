#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace SpaceNavigatorDriver
{
    public partial class SpaceNavigatorToolbar
    {
        [EditorToolbarElement(ID, typeof(SceneView))]
        class ShowSettings : EditorToolbarButton
        {
            public const string ID = "SpaceNavigator/ShowSettings";

            public ShowSettings()
            {
                icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath + "Show Settings.psd");
                tooltip = "Show Settings";
                clicked += OnClick;
            }

            void OnClick()
            {
                EditorWindow.GetWindow(typeof(SpaceNavigatorWindow));
            }
        }
    }
}
#endif