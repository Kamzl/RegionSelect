using TMPro;
using UnityEditor;
using UnityEngine;

namespace GrandMobile.Utilities
{
    [ExecuteAlways]
    public class WholeGradientTMP : MonoBehaviour
    {
        [SerializeField] private Gradient gradient;

        private TMP_Text _text;
        private bool _isEditor;

        protected void Awake()
        {
            _text = GetComponent<TMP_Text>();
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        }

        private void Start()
        {
            _isEditor = !Application.IsPlaying(gameObject);
        }

        private void OnTextChanged(Object obj)
        {
            if (_text == obj)
            {
                Apply(false);
            }
        }

        public void Update()
        {
            if (!_isEditor) return;

            Apply();
        }

        public void Set(Gradient grad)
        {
            gradient = grad;
        }

        public void Apply(bool meshUpdate = true)
        {
            ApplyGradientToTMP(_text, gradient, meshUpdate);
        }

        public static void ApplyGradientToTMP(TMP_Text text, Gradient gradient, bool meshUpdate = true)
        {
            if (!text) return;

            if (text.textInfo == null) return;

            if (meshUpdate) text.ForceMeshUpdate(true, true);
            var info = text.textInfo;
            if (info == null) return;
            var count = info.characterCount;

            if (count == 0) return;

            var minX = text.bounds.min.x;
            var maxX = text.bounds.max.x;

            var length = maxX - minX;

            for (var i = 0; i < count; i++)
            {
                if (!info.characterInfo[i].isVisible) continue;

                var leftColor = gradient.Evaluate((info.characterInfo[i].bottomLeft.x - minX) / length);
                var rightColor = gradient.Evaluate((info.characterInfo[i].topRight.x - minX) / length);

                var vertexGradient = new VertexGradient(leftColor, rightColor, leftColor, rightColor);
                var matIndex = info.characterInfo[i].materialReferenceIndex;
                var vertIndex = info.characterInfo[i].vertexIndex;
                info.meshInfo[matIndex].colors32[vertIndex + 0] = vertexGradient.bottomLeft;
                info.meshInfo[matIndex].colors32[vertIndex + 1] = vertexGradient.topLeft;
                info.meshInfo[matIndex].colors32[vertIndex + 2] = vertexGradient.bottomRight;
                info.meshInfo[matIndex].colors32[vertIndex + 3] = vertexGradient.topRight;
            }

            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WholeGradientTMP))]
    public class WholeGradientTMPEditor : Editor
    {
        public WholeGradientTMP Target { get; protected set; }
        SerializedProperty serializedProperty;

        private void OnEnable()
        {
            serializedProperty = serializedObject.FindProperty("gradient");
            Target = target as WholeGradientTMP;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedProperty, true);
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                Target?.Update();
            }
        }
    }
#endif
}
