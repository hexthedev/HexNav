using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace SpaceNavigatorDriver
{
    public static class UTSettingsWindow
    {
        public static float TitleMaxWidth = 100;
        public static float FloatMaxWidth = 35;
        public static float Space = 10;
    }

    [Serializable]
    public static class Settings
    {
        public static List<Profile> Profiles = new();

        public static int ProfileIndex = 0;
        public static Profile Profile => Profiles[ProfileIndex];

        public static Gear Gear
        {
            get
            {
                if (Profile.Gears == null || Profile.Gears.Count == 0)
                    Profile.Gears = Gear.DefaultGears();

                return Profile.Gears[Profile.GearIndex];
            }
        }

        public static OperationMode Mode => Profiles[ProfileIndex].Mode;
        public static CoordinateSystem System => Profiles[ProfileIndex].System;

        public static LockSet Locks
        {
            get
            {
                if (Profile.Locks == null)
                    Profile.Locks = new();

                return Profile.Locks;
            }
        }

        public static bool OnGUI()
        {
            bool triggerToolbarRefresh = false;

            ProfileIndex = GUILayout.SelectionGrid(
                ProfileIndex,
                Profiles.Select(p => p.Name).ToArray(),
                Profiles.Count);

            EditorGUI.BeginChangeCheck();
            {
                _scrollPos = GUILayout.BeginScrollView(_scrollPos);
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Profile Name: ", GUILayout.MaxWidth(UTSettingsWindow.TitleMaxWidth));
                            Profile.Name = GUILayout.TextField(Profile.Name);
                        }
                        GUILayout.EndHorizontal();
                        
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("Add Profile"))
                            {
                                Profiles.Add(new Profile() { Name = "New Profile" });
                                ProfileIndex = Profiles.Count - 1;
                            }

                            if (GUILayout.Button("Remove Profile") && Profiles.Count > 1)
                            {
                                for (int i = ProfileIndex; i < Profiles.Count - 1; i++)
                                    Profiles[i] = Profiles[i + 1];
                                
                                Profiles.RemoveAt(Profiles.Count - 1);
                                
                                if (EditorPrefs.HasKey(GetProfileKey(Profiles.Count)))
                                    EditorPrefs.DeleteKey(GetProfileKey(Profiles.Count));

                                Save();

                                if (ProfileIndex >= Profiles.Count)
                                    ProfileIndex = Profiles.Count() - 1;
                                
                                triggerToolbarRefresh = true;
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    {
                        GUILayout.Space(UTSettingsWindow.Space);
                        GUILayout.Label("Mode");
                        GUILayout.BeginVertical();
                        {
                            Profile.Mode = (OperationMode)GUILayout.SelectionGrid(
                                (int)Mode,
                                Enum.GetNames(typeof(OperationMode)),
                                Enum.GetNames(typeof(OperationMode)).Length);
                        }
                        GUILayout.EndVertical();
                        GUILayout.Space(UTSettingsWindow.Space);
                    }
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    {
                        GUILayout.Space(UTSettingsWindow.Space);
                        GUILayout.Label("Sensitivity");
                        GUILayout.BeginVertical();
                        {
                            Profile.GearIndex = GUILayout.SelectionGrid(
                                Profile.GearIndex,
                                Profile.Gears.Select(g => g.Name).ToArray(),
                                Profile.Gears.Count);

                            Gear.Translation.RenderSensitivitySetter("Translation");
                            Gear.Rotation.RenderSensitivitySetter("Rotation");
                        }
                        GUILayout.EndVertical();
                        GUILayout.Space(UTSettingsWindow.Space);
                    }
                    EditorGUILayout.EndVertical();

                    GUILayout.BeginVertical(GUI.skin.box);
                    {
                        GUILayout.Space(UTSettingsWindow.Space);

                        GUILayout.Label("Lock");
                        Locks.RenderSensitivitySetter();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndScrollView();
            }
            
            if (EditorGUI.EndChangeCheck())
                Save();

            return triggerToolbarRefresh;
        }

        static Vector2 _scrollPos;

        static string GetProfileKey(int index) => $"HexNavProfile{index}";

        /// <summary>
        /// Write settings to PlayerPrefs.
        /// </summary>
        public static void Save()
        {
            for (var i = 0; i < Profiles.Count; i++)
            {
                Profile profile = Profiles[i];
                EditorPrefs.SetString(GetProfileKey(i), JsonUtility.ToJson(profile));
            }
        }

        /// <summary>
        /// Read settings from PlayerPrefs.
        /// </summary>
        public static void Load()
        {
            if (!EditorPrefs.HasKey(GetProfileKey(0)))
                EditorPrefs.SetString(GetProfileKey(0), JsonUtility.ToJson(Profile.DefaultProfile0));

            Profiles.Clear();
            for (int i = 0; i < 100; i++)
            {
                if (!EditorPrefs.HasKey(GetProfileKey(i)))
                    break;

                try
                {
                    Profiles.Add(JsonUtility.FromJson<Profile>(EditorPrefs.GetString(GetProfileKey(i))));
                }
                catch (Exception e)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Utility function to write axis inversions to PlayerPrefs.
        /// </summary>
        /// <param name="translation"></param>
        /// <param name="rotation"></param>
        /// <param name="baseName"></param>
        static void WriteAxisInversions(Vector3 translation, Vector3 rotation, string baseName)
        {
            PlayerPrefs.SetInt(baseName + " invert translation x", translation.x < 0 ? -1 : 1);
            PlayerPrefs.SetInt(baseName + " invert translation y", translation.y < 0 ? -1 : 1);
            PlayerPrefs.SetInt(baseName + " invert translation z", translation.z < 0 ? -1 : 1);
            PlayerPrefs.SetInt(baseName + " invert rotation x", rotation.x < 0 ? -1 : 1);
            PlayerPrefs.SetInt(baseName + " invert rotation y", rotation.y < 0 ? -1 : 1);
            PlayerPrefs.SetInt(baseName + " invert rotation z", rotation.z < 0 ? -1 : 1);
        }

        /// <summary>
        /// Utility function to read axis inversions from PlayerPrefs.
        /// </summary>
        /// <param name="translation"></param>
        /// <param name="rotation"></param>
        /// <param name="baseName"></param>
        static void ReadAxisInversions(ref Vector3 translation, ref Vector3 rotation, string baseName)
        {
            translation.x = PlayerPrefs.GetInt(baseName + " invert translation x", 1);
            translation.y = PlayerPrefs.GetInt(baseName + " invert translation y", 1);
            translation.z = PlayerPrefs.GetInt(baseName + " invert translation z", 1);
            rotation.x = PlayerPrefs.GetInt(baseName + " invert rotation x", 1);
            rotation.y = PlayerPrefs.GetInt(baseName + " invert rotation y", 1);
            rotation.z = PlayerPrefs.GetInt(baseName + " invert rotation z", 1);
        }

        /// <summary>
        /// Utility function to determine whether a specific axis is locked for the specified DoF.
        /// </summary>
        /// <param name="doF"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static bool GetLock(DoF doF, Axis axis)
        {
            Locks locks = doF == DoF.Translation ? Locks.Translation : Locks.Rotation;

            switch (axis)
            {
                case Axis.X:
                    return (locks.X || locks.All) && !Application.isPlaying;
                case Axis.Y:
                    return (locks.Y || locks.All) && !Application.isPlaying;
                case Axis.Z:
                    return (locks.Z || locks.All) && !Application.isPlaying;
                default:
                    throw new ArgumentOutOfRangeException("axis");
            }
        }

        /// <summary>
        /// Returns a vector which can be multiplied with an input vector to apply the current locks of the specified DoF. 
        /// </summary>
        /// <param name="doF"></param>
        /// <returns></returns>
        public static Vector3 GetLocks(DoF doF)
        {
            Locks locks = doF == DoF.Translation ? Locks.Translation : Locks.Rotation;

            return new Vector3(
                (locks.X || locks.All) && !Application.isPlaying ? 0 : 1,
                (locks.Y || locks.All) && !Application.isPlaying ? 0 : 1,
                (locks.Z || locks.All) && !Application.isPlaying ? 0 : 1);
        }
    }
}