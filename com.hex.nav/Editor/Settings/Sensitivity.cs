using System;
using UnityEditor;
using UnityEngine;

namespace SpaceNavigatorDriver
{
    [Serializable]
    public class Sensitivity
    {
        public float Value;
        public float Min;
        public float Max;

        public void RenderSensitivitySetter(string label)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(label, GUILayout.MaxWidth(UTSettingsWindow.TitleMaxWidth));
                Value = EditorGUILayout.FloatField(Value, GUILayout.MaxWidth(UTSettingsWindow.FloatMaxWidth));
                Min = EditorGUILayout.FloatField(Min, GUILayout.MaxWidth(UTSettingsWindow.FloatMaxWidth));
                Value = GUILayout.HorizontalSlider(Value, Min, Max, GUILayout.ExpandWidth(true));
                Max = EditorGUILayout.FloatField(Max, GUILayout.MaxWidth(UTSettingsWindow.FloatMaxWidth));
            }
            GUILayout.EndHorizontal();
        }
    }
}