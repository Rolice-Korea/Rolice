using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI
{
    [AddComponentMenu("UI/Rc UI Gradient")]
    [RequireComponent(typeof(Graphic))]
    [ExecuteAlways]
    public class RcUIGradient : MonoBehaviour, IMaterialModifier, IMeshModifier
    {
        [Header("Base")]
        [SerializeField] private Color baseColor = new Color(0.15f, 0.15f, 0.15f, 1f);

        [Header("Corner Colors (HDR)")]
        [ColorUsage(false, true)]
        [SerializeField] private Color colorA = new Color(1f, 0.4f, 0.2f);
        [ColorUsage(false, true)]
        [SerializeField] private Color colorB = new Color(0.3f, 1f, 0.8f);

        [Header("Glow")]
        [Tooltip("블룸 밝기 (1.0 이하 = 블룸 없음)")]
        [SerializeField, Range(0f, 10f)] private float intensity = 2f;
        [Tooltip("코너에서 색상이 퍼지는 범위")]
        [SerializeField, Range(0.05f, 1.5f)] private float size = 0.5f;
        [Tooltip("색상 경계의 부드러움 (0=날카롭게, 1=부드럽게)")]
        [SerializeField, Range(0f, 1f)] private float softness = 0.5f;
        [Tooltip("블룸 발광 범위 (Size와 독립)")]
        [SerializeField, Range(0.05f, 1.5f)] private float bloomSpread = 0.3f;
        [Tooltip("블룸 감쇠 커브 (낮을수록 부드럽게 퍼짐, 높을수록 코너에 집중)")]
        [SerializeField, Range(0.1f, 3f)] private float bloomFalloff = 0.4f;

        private static readonly int baseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int colorAId = Shader.PropertyToID("_GradientColorA");
        private static readonly int colorBId = Shader.PropertyToID("_GradientColorB");
        private static readonly int intensityId = Shader.PropertyToID("_GlowIntensity");
        private static readonly int sizeId = Shader.PropertyToID("_GlowSize");
        private static readonly int softnessId = Shader.PropertyToID("_GlowSoftness");
        private static readonly int bloomSpreadId = Shader.PropertyToID("_BloomSpread");
        private static readonly int bloomFalloffId = Shader.PropertyToID("_BloomFalloff");

        private Graphic graphic;
        private RectTransform rectTransform;
        private Material materialInstance;
        private Material baseMaterial;

        public Color BaseColor
        {
            get => baseColor;
            set { baseColor = value; MarkDirty(); }
        }

        public Color ColorA
        {
            get => colorA;
            set { colorA = value; MarkDirty(); }
        }

        public Color ColorB
        {
            get => colorB;
            set { colorB = value; MarkDirty(); }
        }

        public float Intensity
        {
            get => intensity;
            set { intensity = value; MarkDirty(); }
        }

        public float Size
        {
            get => size;
            set { size = value; MarkDirty(); }
        }

        public float Softness
        {
            get => softness;
            set { softness = value; MarkDirty(); }
        }

        public float BloomSpread
        {
            get => bloomSpread;
            set { bloomSpread = value; MarkDirty(); }
        }

        public float BloomFalloff
        {
            get => bloomFalloff;
            set { bloomFalloff = value; MarkDirty(); }
        }

        private void OnEnable()
        {
            graphic = GetComponent<Graphic>();
            rectTransform = GetComponent<RectTransform>();
            MarkDirty();
        }

        private void OnDisable() => MarkDirty();

        private void OnDestroy() => DestroyMaterialInstance();

        public void ModifyMesh(VertexHelper vh)
        {
            if (!enabled || rectTransform == null) return;

            var rect = rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f) return;

            UIVertex vert = default;
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vert, i);
                vert.uv1 = new Vector2(
                    (vert.position.x - rect.xMin) / rect.width,
                    (vert.position.y - rect.yMin) / rect.height
                );
                vh.SetUIVertex(vert, i);
            }
        }

        public void ModifyMesh(Mesh mesh) { }

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!enabled)
                return baseMaterial;

            if (materialInstance != null && this.baseMaterial != baseMaterial)
                DestroyMaterialInstance();

            if (materialInstance == null)
            {
                materialInstance = new Material(baseMaterial);
                materialInstance.hideFlags = HideFlags.HideAndDontSave;
                materialInstance.name = $"{baseMaterial.name} (Gradient {GetInstanceID()})";
                this.baseMaterial = baseMaterial;
            }

            ApplyMaterialProperties();
            return materialInstance;
        }

        private void ApplyMaterialProperties()
        {
            materialInstance.SetColor(baseColorId, baseColor);
            materialInstance.SetColor(colorAId, colorA);
            materialInstance.SetColor(colorBId, colorB);
            materialInstance.SetFloat(intensityId, intensity);
            materialInstance.SetFloat(sizeId, size);
            materialInstance.SetFloat(softnessId, softness);
            materialInstance.SetFloat(bloomSpreadId, bloomSpread);
            materialInstance.SetFloat(bloomFalloffId, bloomFalloff);
        }

        private void DestroyMaterialInstance()
        {
            if (materialInstance == null) return;

            if (Application.isPlaying)
                Destroy(materialInstance);
            else
                DestroyImmediate(materialInstance);

            materialInstance = null;
            baseMaterial = null;
        }

        private void MarkDirty()
        {
            if (graphic == null)
                graphic = GetComponent<Graphic>();

            if (graphic != null)
            {
                graphic.SetMaterialDirty();
                graphic.SetVerticesDirty();
            }
        }

#if UNITY_EDITOR
        private void OnValidate() => MarkDirty();
#endif
    }
}
