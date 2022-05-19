using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
namespace PhysicsTools{
	[UnityEditor.CustomEditor(typeof(Trajectory)), CanEditMultipleObjects]
	public class TrajectoryEditor : UnityEditor.Editor {

		private bool showWP = false;
		private SerializedObject so;
		private SerializedProperty loop;
		private SerializedProperty showWayPts;
		private SerializedProperty lstWayPts;
		Trajectory traj;
		// Use this for initialization
		void OnEnable()
		{
			so = new SerializedObject (target);
			traj = (Trajectory)target;
			loop = so.FindProperty ("loop");
			showWayPts = so.FindProperty ("ShowWayPoints");
			lstWayPts = so.FindProperty ("lstWayPts");
		}
		
		// Update is called once per frame
		public override void OnInspectorGUI()
		{
			if (traj.lstWayPts != null && traj.lstWayPts.Count > 0) {
				EditorGUI.BeginChangeCheck ();
				showWP = EditorGUILayout.Foldout (showWP, "Way points");
				if (showWP) {
					EditorGUI.indentLevel++;
					string[] wayPtsTxt = new string[traj.lstWayPts.Count];
					for (int i = 0; i < wayPtsTxt.Length; ++i) {
						string name = traj.lstWayPts [i].Name;
						if (name == null)
							name = "";
						wayPtsTxt [i] = "Waypoint " + i + ("" != name ? "(" + name + ")" : "");
					}
					if (traj.selectedWP > wayPtsTxt.Length)
						traj.selectedWP = wayPtsTxt.Length - 1;
					traj.selectedWP = EditorGUILayout.Popup ("Select WP", traj.selectedWP, wayPtsTxt);
					traj.lstWayPts [traj.selectedWP].Name = EditorGUILayout.TextField("Name", traj.lstWayPts [traj.selectedWP].Name);
					traj.lstWayPts [traj.selectedWP].position = EditorGUILayout.Vector3Field ("Pos", traj.lstWayPts [traj.selectedWP].position);
					traj.lstWayPts [traj.selectedWP].speed = EditorGUILayout.FloatField ("Speed", traj.lstWayPts [traj.selectedWP].speed);
					traj.lstWayPts [traj.selectedWP].twistValue = EditorGUILayout.FloatField ("Twist", traj.lstWayPts [traj.selectedWP].twistValue);
                    EditorGUI.indentLevel--;
				}
                traj.loop = EditorGUILayout.Toggle("Loop", traj.loop);
                traj.autoUpdate = EditorGUILayout.Toggle("AutoUpdate", traj.autoUpdate);
                traj.ShowWayPoints = EditorGUILayout.Toggle("ShowWayPts", traj.ShowWayPoints);
                traj.showWaypoints(traj.ShowWayPoints);
                bool changed = EditorGUI.EndChangeCheck();
                if (changed)
                {
                    so.ApplyModifiedProperties();
                    ((Trajectory)target).refresh();
                }
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add WP"))
            {
                ((Trajectory)so.targetObject).addWayPt();
            }
            if (GUILayout.Button("Insert WP"))
            {
                ((Trajectory)so.targetObject).insertWayPt(traj.selectedWP);
            }
            if (GUILayout.Button("Remove WP"))
            {
                int index = traj.selectedWP;
                if (traj.selectedWP == traj.lstWayPts.Count - 1)
                    traj.selectedWP--;
                if (traj.lstWayPts.Count > 2)
                {
                    ((Trajectory)so.targetObject).removeWayPt(index);
                }
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Cleanup"))
            {
                ((Trajectory)so.targetObject).cleanUp();
            }
            if (GUILayout.Button("Refresh Trajectory"))
            {
                ((Trajectory)so.targetObject).refresh();
            }
        }
    }
}
#endif