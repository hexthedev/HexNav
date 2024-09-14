#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpaceNavigatorDriver
{
    [InitializeOnLoad]
    [Serializable]
    internal class ViewportController
    {
        // Damping
        static Vector3 _oldTranslation;
        static Vector3 _oldRotation;

        // Snapping
        static Dictionary<Transform, Quaternion> _unsnappedRotations = new();
        static Dictionary<Transform, Vector3> _unsnappedTranslations = new();
        static bool _wasIdle;

        // Rig components
        static GameObject _pivotGO, _cameraGO;
        static Transform _pivot, _camera;
        const string PivotName = "Scene camera pivot dummy";
        const string CameraName = "Scene camera dummy";

        static bool _wasHorizonLocked;
        const float _saveInterval = 30;
        static float _lastSaveTime;

        static double _lastRefreshTime;
        static double _deltaTime;
        static float _deltaTimeFactor = 400f;
        static bool _hadFocus;

        static ushort _lastButtonValue;

        static ViewportController()
        {
            // Set up callbacks.
            EditorApplication.update += Update;

            // Initialize.
            Settings.Load();
            
            InitCameraRig();
            StoreSelectionTransforms();
        }

        public static void OnApplicationQuit()
        {
            Settings.Save();
            DisposeCameraRig();
        }


        static void Update()
        {
            // Autosave settings.
            if (!Application.isPlaying && DateTime.Now.Second - _lastSaveTime > _saveInterval)
            {
                Settings.Save();
                _lastSaveTime = DateTime.Now.Second;
            }

            // Only act if scene view is open
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (!sceneView) return;

            // Align with dummy
            SyncRigWithScene();

            // Straighten horizon if that's locked
            if (Settings.Locks.Horizon && !_wasHorizonLocked)
                StraightenHorizon();
            _wasHorizonLocked = Settings.Locks.Horizon;

            // Settings.TranslationDrift ??= SpaceNavigatorHID.current.Translation.ReadValue();
            // Settings.RotationDrift ??= SpaceNavigatorHID.current.Rotation.ReadValue();

            // TODO: What if this wasn't DIY
            _deltaTime = EditorApplication.timeSinceStartup - _lastRefreshTime;
            _lastRefreshTime = EditorApplication.timeSinceStartup;

            ReadInput(Settings.Mode, out Vector3 translation, out Vector3 rotation, out ushort buttons);

            if (_lastButtonValue != buttons)
            {
                switch (buttons)
                {
                    case 1:
                        Settings.Profile.GearIndex = (Settings.Profile.GearIndex + 1) % Settings.Profile.Gears.Count;
                        Debug.Log(Settings.Profile.GearIndex);
                        SpaceNavigatorToolbar.Instance.TriggerRefresh();
                        break;
                    case 2:
                        Settings.ProfileIndex = (Settings.ProfileIndex + 1) % Settings.Profiles.Count;
                        Debug.Log(Settings.ProfileIndex);
                        SpaceNavigatorToolbar.Instance.TriggerRefresh();
                        break;
                }
                
                _lastButtonValue = buttons;
            }
            
            // Return if device is idle.
            if (ApproximatelyEqual(translation, Vector3.zero) &&
                ApproximatelyEqual(rotation, Vector3.zero))
            {
                _wasIdle = true;
                return;
            }

            switch (Settings.Mode)
            {
                case OperationMode.Fly:
                    Fly(sceneView, translation, rotation);
                    break;
                case OperationMode.Orbit:
                    Orbit(sceneView, translation, rotation);
                    break;
                case OperationMode.Telekinesis:
                    // Manipulate the object free from the camera.
                    Telekinesis(sceneView, translation, rotation);
                    break;
                case OperationMode.GrabMove:
                    // Manipulate the object together with the camera.
                    GrabMove(sceneView, translation, rotation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _wasIdle = false;
        }


        static void Fly(SceneView sceneView, Vector3 translation, Vector3 rotation)
        {
            SyncRigWithScene();

            _camera.Translate(translation, Space.Self);
            if (sceneView.orthographic)
                sceneView.size -= translation.z;
            else
            {
                if (Settings.Locks.Horizon)
                {
                    // Perform yaw in world coordinates.
                    _camera.Rotate(Vector3.up, rotation.y, Space.World);
                    // Perform pitch in local coordinates.
                    _camera.Rotate(Vector3.right, rotation.x, Space.Self);
                }
                else
                {
                    // Default rotation method, applies the whole quaternion to the camera.
                    _camera.Rotate(rotation);
                }
            }

            // Update sceneview pivot and repaint view.
            sceneView.pivot = _pivot.position;
            sceneView.rotation = _pivot.rotation;
            sceneView.Repaint();
        }

        static void Orbit(SceneView sceneView, Vector3 translation, Vector3 rotation)
        {
            // If no object is selected don't orbit, fly instead.
            if (Selection.gameObjects.Length == 0)
            {
                Fly(sceneView, translation, rotation);
                return;
            }

            SyncRigWithScene();

            _camera.Translate(translation, Space.Self);

            if (Settings.Locks.Horizon)
            {
                _camera.RotateAround(Tools.handlePosition, Vector3.up, rotation.y);
                _camera.RotateAround(Tools.handlePosition, _camera.right, rotation.x);
            }
            else
            {
                _camera.RotateAround(Tools.handlePosition, _camera.up, rotation.y);
                _camera.RotateAround(Tools.handlePosition, _camera.right, rotation.x);
                _camera.RotateAround(Tools.handlePosition, _camera.forward, rotation.z);
            }

            // Update sceneview pivot and repaint view.
            sceneView.pivot = _pivot.position;
            sceneView.rotation = _pivot.rotation;
            sceneView.Repaint();
        }

        static void Telekinesis(SceneView sceneView, Vector3 translation, Vector3 rotation)
        {
            if (_wasIdle)
                Undo.IncrementCurrentGroup();
            Transform[] selection = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);
            Undo.SetCurrentGroupName("Telekinesis");
            Undo.RecordObjects(selection, "Telekinesis");

            Quaternion euler = Quaternion.Euler(rotation);

            // Store the selection's transforms because the user could have edited them since we last used them via the inspector.
            if (_wasIdle)
                StoreSelectionTransforms();

            foreach (Transform transform in selection)
            {
                if (!_unsnappedRotations.ContainsKey(transform)) continue;

                Transform reference;
                switch (Settings.System)
                {
                    case CoordinateSystem.Camera:
                        reference = sceneView.camera.transform;
                        break;
                    case CoordinateSystem.World:
                        reference = null;
                        break;
                    case CoordinateSystem.Parent:
                        reference = transform.parent;
                        break;
                    case CoordinateSystem.Local:
                        reference = transform;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (reference == null)
                {
                    // Move the object in world coordinates.
                    _unsnappedTranslations[transform] += translation;
                    _unsnappedRotations[transform] = euler * _unsnappedRotations[transform];
                }
                else
                {
                    // Move the object in the reference coordinate system.
                    Vector3 worldTranslation = reference.TransformPoint(translation) -
                                               reference.position;
                    _unsnappedTranslations[transform] += worldTranslation;
                    _unsnappedRotations[transform] =
                        (reference.rotation * euler * Quaternion.Inverse(reference.rotation)) *
                        _unsnappedRotations[transform];
                }

                // Perform rotation with or without snapping.
                transform.rotation = _unsnappedRotations[transform];
                transform.position = _unsnappedTranslations[transform];
            }
        }

        static void GrabMove(SceneView sceneView, Vector3 translation, Vector3 rotation)
        {
            if (_wasIdle)
                Undo.IncrementCurrentGroup();
            Transform[] selection = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);
            Undo.SetCurrentGroupName("GrabMove");
            Undo.RecordObjects(selection, "GrabMove");

            // Store the selection's transforms because the user could have edited them since we last used them via the inspector.
            if (_wasIdle)
                StoreSelectionTransforms();

            foreach (Transform transform in selection)
            {
                if (!_unsnappedRotations.ContainsKey(transform)) continue;

                // Initialize transform to unsnapped state.
                transform.rotation = _unsnappedRotations[transform];
                transform.position = _unsnappedTranslations[transform];
                Vector3 oldPos = transform.position;

                // Rotate with horizon lock.
                transform.RotateAround(_camera.position, Vector3.up, rotation.y);
                transform.RotateAround(_camera.position, _camera.right, rotation.x);

                // Interpret SpaceNavigator input in camera space, calculate the effect in world space.
                Vector3 worldTranslation = sceneView.camera.transform.TransformPoint(translation) -
                                           sceneView.camera.transform.position;
                transform.position += worldTranslation;

                // Store new unsnapped state.
                _unsnappedRotations[transform] = transform.rotation;
                _unsnappedTranslations[transform] +=
                    transform.position -
                    oldPos; // The rotation also added translation, so calculate the translation delta.

                // Perform snapping.
                transform.position = _unsnappedTranslations[transform];
                transform.rotation = _unsnappedRotations[transform];
            }

            // Move the scene camera.
            Fly(sceneView, translation, rotation);
        }

        static void ReadInput(OperationMode mode, out Vector3 translation, out Vector3 rotation, out ushort buttons)
        {
            // Read data from device
            UTSpaceMouse.GetSpaceMouseVectors(out translation, out rotation, out buttons);

            // Make navigation framerate independent
            translation *= (float)_deltaTime * _deltaTimeFactor;
            rotation *= (float)_deltaTime * _deltaTimeFactor;

            // Apply sensitivity
            translation *= Settings.Gear.Translation.Value;
            rotation *= Settings.Gear.Rotation.Value;

            // Apply locks
            translation.Scale(Settings.GetLocks(DoF.Translation));
            rotation.Scale(Settings.GetLocks(DoF.Rotation));
        }

        public static void StraightenHorizon()
        {
            _camera.rotation = Quaternion.Euler(_camera.rotation.eulerAngles.x, _camera.rotation.eulerAngles.y, 0);

            // Update sceneview pivot and repaint view.
            SceneView.lastActiveSceneView.pivot = _pivot.position;
            SceneView.lastActiveSceneView.rotation = _pivot.rotation;
            SceneView.lastActiveSceneView.Repaint();
        }

        /// <summary>
        /// Sets up a dummy camera rig like the scene camera.
        /// We can't move the camera, only the SceneView's pivot & rotation.
        /// For some reason the camera does not always have the same position offset to the pivot.
        /// This offset is unpredictable, so we have to update our dummy rig each time before using it.
        /// </summary>
        static void InitCameraRig()
        {
            _cameraGO = GameObject.Find(CameraName);
            _pivotGO = GameObject.Find(PivotName);
            // Create camera rig if one is not already present.
            if (!_pivotGO)
            {
                _cameraGO = new GameObject(CameraName) { hideFlags = HideFlags.HideAndDontSave };
                _pivotGO = new GameObject(PivotName) { hideFlags = HideFlags.HideAndDontSave };
            }

            // Reassign these variables, they get destroyed when entering play mode.
            _camera = _cameraGO.transform;
            _pivot = _pivotGO.transform;
            _pivot.parent = _camera;

            SyncRigWithScene();
        }

        /// <summary>
        /// Position the dummy camera rig like the scene view camera.
        /// </summary>
        static void SyncRigWithScene()
        {
            if (SceneView.lastActiveSceneView)
            {
                _camera.position =
                    SceneView.lastActiveSceneView.camera.transform.position; // <- this value changes w.r.t. pivot !
                _camera.rotation = SceneView.lastActiveSceneView.camera.transform.rotation;
                _pivot.position = SceneView.lastActiveSceneView.pivot;
                _pivot.rotation = SceneView.lastActiveSceneView.rotation;
            }
        }

        static void DisposeCameraRig()
        {
            Object.DestroyImmediate(_cameraGO);
            Object.DestroyImmediate(_pivotGO);
        }

        public static void StoreSelectionTransforms()
        {
            _unsnappedRotations.Clear();
            _unsnappedTranslations.Clear();
            foreach (Transform transform in Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable))
            {
                _unsnappedRotations.Add(transform, transform.rotation);
                _unsnappedTranslations.Add(transform, transform.position);
            }
        }

        static Quaternion SnapOnRotation(Quaternion q, float snap)
        {
            Vector3 euler = q.eulerAngles;
            return Quaternion.Euler(
                Mathf.RoundToInt(euler.x / snap) * snap,
                Mathf.RoundToInt(euler.y / snap) * snap,
                Mathf.RoundToInt(euler.z / snap) * snap);
        }

        static Vector3 SnapOnTranslation(Vector3 v, float snap)
        {
            return new Vector3(
                Mathf.RoundToInt(v.x / snap) * snap,
                Mathf.RoundToInt(v.y / snap) * snap,
                Mathf.RoundToInt(v.z / snap) * snap);
        }

        static bool ApproximatelyEqual(Vector3 lhs, Vector3 rhs)
        {
            float num = lhs.x - rhs.x;
            float num2 = lhs.y - rhs.y;
            float num3 = lhs.z - rhs.z;
            float num4 = num * num + num2 * num2 + num3 * num3;
            
            return num4 < float.Epsilon;
        }
    }
}
#endif