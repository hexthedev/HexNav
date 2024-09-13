#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceNavigatorDriver
{
    [Overlay(typeof(SceneView), "SpaceNavigator")]
    [Icon(IconPath + "SpaceNavigator.png")]
    public partial class SpaceNavigatorToolbar : ToolbarOverlay, ICreateHorizontalToolbar, ICreateVerticalToolbar
    {
        public static SpaceNavigatorToolbar Instance;
        
        private const bool Debug = true;
        private const string IconPath = "Packages/com.hex.nav/Editor/Toolbar/Icons/";
        private static List<SpeedGearButton> m_speedGearButtons = new List<SpeedGearButton>();

        public static event EventHandler RefreshLayout;
        
        SpaceNavigatorToolbar()
        {
            Instance = this;
            RefreshLayout += (sender, args) =>
            {
                // Toggling the 'displayed' property causes the internal method 'RebuildContent' to be called
                displayed = false;
                displayed = true;
            };
        }

        private static OverlayToolbar CreateToolbar()
        {
            OverlayToolbar toolbar = new OverlayToolbar();
            toolbar.Add(new NavigationMode());
            toolbar.Add(new SpeedGearDropdown());

            if (Debug)
                toolbar.Add(new ShowSettings());
            EditorToolbarUtility.SetupChildrenAsButtonStrip(toolbar);
            return toolbar;
        }

        public override VisualElement CreatePanelContent()
        {
            return CreateToolbar();
        }

        public new OverlayToolbar CreateHorizontalToolbarContent()
        {
            return CreateToolbar();
        }

        public new OverlayToolbar CreateVerticalToolbarContent()
        {
            return CreateToolbar();
        }

        public void TriggerRefresh()
        {
            RefreshLayout?.Invoke(this, EventArgs.Empty);
        }
    }
}
#endif