using UnityEngine;
using System.Runtime.InteropServices;

public static class UTSpaceMouse
{
    const float TranslationModifier = 10f;
    const float RotationModifier = 10f;
    
    [DllImport("SpaceMousePlugin")]
    public static extern void GetSpaceMouseState(out short tx, out short ty, out short tz, 
        out short rx, out short ry, out short rz, 
        out ushort buttons);

    public static void GetSpaceMouseVectors(out Vector3 translation, out Vector3 rotation, out ushort buttons)
    {
        GetSpaceMouseState(out short tx, out short ty, out short tz,
            out short rx, out short ry, out short rz,
            out buttons);
        
        // Make the inputs reasonable at source so that scaling inputs by 1 on translation and
        // rotation moves the camera in fly mode at reasonable speeds relative to a 1x1 cube
        translation = new Vector3(tx, -tz, -ty) / 32767f * TranslationModifier;
        rotation = new Vector3(-rx, rz,ry) / 32767f * RotationModifier;
    }
}