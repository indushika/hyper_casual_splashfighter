using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
namespace PhysicsTools{
    [UnityEditor.CustomEditor(typeof(TrajectoryDrawer)), CanEditMultipleObjects]
    public class TrajectoryDrawerEditor : UnityEditor.Editor
    {

        private SerializedObject so;
        private SerializedProperty radius;
        private SerializedProperty smoothness;

        // Use this for initialization
        void OnEnable()
        {
            so = new SerializedObject(target);
            radius = so.FindProperty("radius");
            smoothness = so.FindProperty("smoothness");
        }

        // Update is called once per frame
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(radius);
            EditorGUILayout.PropertyField(smoothness);
            bool bChanged = EditorGUI.EndChangeCheck();
            if (bChanged)
            {
                so.ApplyModifiedProperties();
                if (((TrajectoryDrawer)target).thisTraj != null)
                    ((TrajectoryDrawer)target).thisTraj.refresh();
            }
        }
    }
}
#endif