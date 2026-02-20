using UnityEngine;
using UnityEngine.UI;

namespace Engine.UI
{
    // HDR Material + Screen Space Camera 모드에서 Bloom 적용용
    [AddComponentMenu("UI/Rc UI HDR Image")]
    [RequireComponent(typeof(Graphic))]
    [ExecuteAlways]
    public class RcUIHDRImage : MonoBehaviour, IMaterialModifier
    {
        [Header("HDR")]
        [ColorUsage(false, true)]
        [SerializeField] private Color hdrColor = Color.white;

        [Tooltip("1.0 = 원본 밝기, 1.0 초과 시 Bloom 발생")]
        [SerializeField, Range(0f, 20f)] private float intensity = 1f;

        private static readonly int hdrColorId = Shader.PropertyToID("_HDRColor");
        private static readonly int intensityId = Shader.PropertyToID("_HDRIntensity");

        private Graphic graphic;
        private Material materialInstance;
        private Material baseMaterial;

        public Color HDRColor
        {
            get => hdrColor;
            set { hdrColor = value; MarkDirty(); }
        }

        public float Intensity
        {
            get => intensity;
            set { intensity = value; MarkDirty(); }
        }

        private void OnEnable()
        {
            graphic = GetComponent<Graphic>();
            MarkDirty();
        }

        private void OnDisable() => MarkDirty();

        private void OnDestroy() => DestroyMaterialInstance();

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
                materialInstance.name = $"{baseMaterial.name} (HDR {GetInstanceID()})";
                this.baseMaterial = baseMaterial;
            }

            ApplyMaterialProperties();
            return materialInstance;
        }

        private void ApplyMaterialProperties()
        {
            materialInstance.SetColor(hdrColorId, hdrColor);
            materialInstance.SetFloat(intensityId, intensity);
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
                graphic.SetMaterialDirty();
        }

#if UNITY_EDITOR
        private void OnValidate() => MarkDirty();
#endif
    }
}
