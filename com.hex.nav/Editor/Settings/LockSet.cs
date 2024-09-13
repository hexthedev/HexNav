using System;
using UnityEngine;

namespace SpaceNavigatorDriver
{
    [Serializable]
    public class LockSet
    {
        public bool Horizon = true;
        public Locks Translation = new Locks("Translation");
        public Locks Rotation = new Locks("Rotation");

        public void RenderSensitivitySetter()
        {
            Horizon = GUILayout.Toggle(Horizon, "Horizon", GUILayout.Width(120));
            Translation.RenderLocksSetter();
            Rotation.RenderLocksSetter();
        }
    }
}