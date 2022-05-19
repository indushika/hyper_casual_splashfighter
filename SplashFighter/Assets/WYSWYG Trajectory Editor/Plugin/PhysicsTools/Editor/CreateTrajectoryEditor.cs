using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
namespace PhysicsTools{
	[UnityEditor.CustomEditor(typeof(CreateTrajectory)), CanEditMultipleObjects]
	public class CreateTrajectoryEditor : UnityEditor.Editor {

		private SerializedObject so;
		private SerializedProperty type;
		private SerializedProperty numWayPts;
		//line
		private SerializedProperty startPt;
		private SerializedProperty endPt;
		//circle/ellipse/square/rectangle
		private SerializedProperty center;
		private SerializedProperty axis;
		//circle
		private SerializedProperty radius;
		//ellipse
		private SerializedProperty majorAxis;
		private SerializedProperty minorAxis;
		//square
		private SerializedProperty sideLength;
		//rectangle
		private SerializedProperty length;
		private SerializedProperty width;
		//helix
		private SerializedProperty pitch;
		private SerializedProperty numRounds;

		private SerializedProperty fraction;

		private bool showTypeFoldout = false;
		// Use this for initialization
		void OnEnable()
		{
			so = new SerializedObject (target);

			type = so.FindProperty ("type");
			numWayPts = so.FindProperty ("numWayPts");
			//line
			startPt = so.FindProperty ("startPt");
			endPt = so.FindProperty ("endPt");
			//circle/ellipse/square/rectangle
			center = so.FindProperty ("center");
			axis = so.FindProperty ("axis");
			//circle
			radius = so.FindProperty ("radius");
			//ellipse
			majorAxis = so.FindProperty ("majorAxis");
			minorAxis = so.FindProperty ("minorAxis");
			//square
			sideLength = so.FindProperty ("sideLength");
			//rectangle
			length = so.FindProperty ("length");
			width = so.FindProperty ("width");
			//helix
			pitch = so.FindProperty("pitch");
			numRounds = so.FindProperty ("numRounds");
			fraction = so.FindProperty ("fraction");
		}

		// Update is called once per frame
		public override void OnInspectorGUI()
		{
			CreateTrajectory tar = (CreateTrajectory)target;
			bool changed = false;
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (numWayPts);
			changed |= EditorGUI.EndChangeCheck ();
			showTypeFoldout = EditorGUILayout.Foldout(showTypeFoldout, "Select Type");
			if (showTypeFoldout) {
				EditorGUI.BeginChangeCheck ();
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField (type);
				CreateTrajectory.Type tType = (CreateTrajectory.Type)type.enumValueIndex;
				if (tType == CreateTrajectory.Type.CIRCLE || tType == CreateTrajectory.Type.ELLIPTICAL)
					EditorGUILayout.Slider (fraction, 0, 1);
				if (tType != CreateTrajectory.Type.LINE) {
					EditorGUILayout.PropertyField (center);
					EditorGUILayout.PropertyField (axis);
					if (tType == CreateTrajectory.Type.CIRCLE) {
						EditorGUILayout.PropertyField (radius);
					} else if (tType == CreateTrajectory.Type.ELLIPTICAL) {
						EditorGUILayout.PropertyField (majorAxis);
						EditorGUILayout.PropertyField (minorAxis);
					} else if (tType == CreateTrajectory.Type.SQUARE) {
						EditorGUILayout.PropertyField (sideLength);
					} else if (tType == CreateTrajectory.Type.RECTANGULAR) {
						EditorGUILayout.PropertyField (length);
						EditorGUILayout.PropertyField (width);
					} else if (tType == CreateTrajectory.Type.HELICAL) {
						EditorGUILayout.PropertyField (radius);
						EditorGUILayout.PropertyField (pitch);
						EditorGUILayout.PropertyField (numRounds);
					}
				} else {
					EditorGUILayout.PropertyField (startPt);
					EditorGUILayout.PropertyField (endPt);
				}
				changed |= EditorGUI.EndChangeCheck ();
				EditorGUI.indentLevel--;
			}
			so.ApplyModifiedProperties ();
			if (tar.numWayPts < 3) {
				tar.numWayPts = 3;
			}

			if (changed) {
                ((CreateTrajectory)so.targetObject).generateTrajectory ();
			}
		}
	}
}
#endif
