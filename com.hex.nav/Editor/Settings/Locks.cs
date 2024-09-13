using System;
using UnityEngine;

namespace SpaceNavigatorDriver {

	[Serializable]
	public class Locks {
		public bool X, Y, Z, All;

		[SerializeField] string Name;

		public Locks(string name) {
			Name = name;
		}

		public void Write() {
			string prefix = Name + " lock ";
			PlayerPrefs.SetInt(prefix + "X", X ? 1 : 0);
			PlayerPrefs.SetInt(prefix + "Y", Y ? 1 : 0);
			PlayerPrefs.SetInt(prefix + "Z", Z ? 1 : 0);
			PlayerPrefs.SetInt(prefix + "All", All ? 1 : 0);
		}
		public void Read() {
			string prefix = Name + " lock ";
			X = PlayerPrefs.GetInt(prefix + "X", 0) == 1;
			Y = PlayerPrefs.GetInt(prefix + "Y", 0) == 1;
			Z = PlayerPrefs.GetInt(prefix + "Z", 0) == 1;
			All = PlayerPrefs.GetInt(prefix + "All", 0) == 1;
		}

		public void RenderLocksSetter()
		{
			GUILayout.BeginHorizontal();
			{
				All = GUILayout.Toggle(All, "Translation", GUILayout.Width(120));
				GUI.enabled = !All;
				X = GUILayout.Toggle(X, "X", GUILayout.Width(60));
				Y = GUILayout.Toggle(Y, "Y", GUILayout.Width(60));
				Z = GUILayout.Toggle(Z, "Z", GUILayout.Width(60));
				GUI.enabled = true;
			}
			GUILayout.EndHorizontal();
		}
	}
}