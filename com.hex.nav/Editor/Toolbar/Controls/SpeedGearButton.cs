#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace SpaceNavigatorDriver
{
    public partial class SpaceNavigatorToolbar
    {
        public class SpeedGearButton : EditorToolbarToggle
        {
            public readonly string ID;
            public int Gear;
            List<SpeedGearButton> m_buttons;

            public SpeedGearButton(int _gear, string _icon, string _tooltip, List<SpeedGearButton> _buttons)
            {
                Gear = _gear;
                m_buttons = _buttons;
                ID = "SpaceNavigator/SpeedGearButton" + _gear;
                icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath + _icon);
                tooltip = _tooltip;
                value = _gear == Settings.Profile.GearIndex;
            }
        }
    }
}
#endif