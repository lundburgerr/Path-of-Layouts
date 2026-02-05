using UnityEditor;
using UnityEditor.UI;

namespace fireMCG.PathOfLayouts.Ui.Components
{
    [CustomEditor(typeof(ButtonImageTransition))]
    [CanEditMultipleObjects]
    public sealed class ButtonImageTransitionEditor : ButtonEditor
    {
        private SerializedProperty _hoverOffIcon;
        private SerializedProperty _hoverOnIcon;

        protected override void OnEnable()
        {
            base.OnEnable();

            _hoverOffIcon = serializedObject.FindProperty("_hoverOffIcon");
            _hoverOnIcon = serializedObject.FindProperty("_hoverOnIcon");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Icon Transition", EditorStyles.boldLabel);

            serializedObject.Update();

            EditorGUILayout.PropertyField(_hoverOffIcon);
            EditorGUILayout.PropertyField(_hoverOnIcon);

            serializedObject.ApplyModifiedProperties();
        }
    }
}