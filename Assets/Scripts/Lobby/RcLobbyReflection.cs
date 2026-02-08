using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RcLobbyReflection : MonoBehaviour
{
    private static readonly Vector3 MirrorScale = new(1f, -1f, 1f);
    private const float FloorOffset = 0.001f;
    private const int GradientResolution = 128;
    private const string URPUnlitShader = "Universal Render Pipeline/Unlit";

    [Header("원본 다이스")]
    [SerializeField] private Transform _source;

    [Header("반사 설정")]
    [SerializeField] private float _floorHeight = -0.7f;
    [SerializeField, Range(0f, 1f)] private float _reflectionAlpha = 0.08f;
    [SerializeField, Range(0f, 1f)] private float _emissionMultiplier = 0.7f;

    [Header("페이드 설정")]
    [SerializeField] private bool _useFade = true;
    [SerializeField] private float _fadeSize = 30f;
    [SerializeField] private float _fadeRadius = 2.5f;

    private Material[] _reflectionMaterials;
    private GameObject _fadeQuad;
    private Material _fadeMaterial;
    private Texture2D _fadeTexture;

    private void Start()
    {
        transform.localScale = MirrorScale;
        SetupReflectionMaterials();
        if (_useFade) SetupFadeQuad();
    }

    private void LateUpdate()
    {
        if (_source == null) return;

        var srcPos = _source.position;
        transform.position = new Vector3(srcPos.x, 2f * _floorHeight - srcPos.y, srcPos.z);

        // 스케일 반전(1,-1,1) + 쿼터니언 미러로 임의 회전 바닥 반사
        var q = _source.rotation;
        transform.rotation = new Quaternion(-q.x, q.y, -q.z, q.w);

        if (_fadeQuad != null)
            _fadeQuad.transform.position = new Vector3(srcPos.x, _floorHeight + FloorOffset, srcPos.z);
    }

    private void OnDestroy()
    {
        DestroyMaterials();
        if (_fadeMaterial != null) Destroy(_fadeMaterial);
        if (_fadeTexture != null) Destroy(_fadeTexture);
    }

    private void SetupReflectionMaterials()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        var matList = new List<Material>();

        foreach (var renderer in renderers)
        {
            var mats = renderer.materials;

            foreach (var t in mats)
            {
                ConvertToTransparent(t);
                matList.Add(t);
            }

            renderer.materials = mats;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        _reflectionMaterials = matList.ToArray();
    }

    private void ConvertToTransparent(Material mat)
    {
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 1);
        mat.renderQueue = (int)RenderQueue.Transparent;
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.SetFloat("_Cull", (float)CullMode.Back);

        if (mat.HasProperty("_BaseColor"))
        {
            var color = mat.GetColor("_BaseColor");
            color.a = _reflectionAlpha;
            mat.SetColor("_BaseColor", color);
        }

        if (mat.HasProperty("_EmissionColor"))
        {
            var emission = mat.GetColor("_EmissionColor");
            mat.SetColor("_EmissionColor", emission * _emissionMultiplier);
        }
    }

    private void DestroyMaterials()
    {
        if (_reflectionMaterials == null) return;

        foreach (var mat in _reflectionMaterials)
            if (mat != null) Destroy(mat);
    }

    private void SetupFadeQuad()
    {
        _fadeQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _fadeQuad.name = "ReflectionFade";
        Destroy(_fadeQuad.GetComponent<Collider>());

        var fadeTransform = _fadeQuad.transform;
        fadeTransform.position = new Vector3(0f, _floorHeight + FloorOffset, 0f);
        fadeTransform.rotation = Quaternion.Euler(90f, 0f, 0f);
        fadeTransform.localScale = new Vector3(_fadeSize, _fadeSize, 1f);

        _fadeTexture = CreateRadialFadeGradient();
        _fadeMaterial = CreateFadeMaterial(_fadeTexture);

        var mr = _fadeQuad.GetComponent<MeshRenderer>();
        mr.material = _fadeMaterial;
        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.receiveShadows = false;
    }

    private Material CreateFadeMaterial(Texture2D texture)
    {
        var mat = new Material(Shader.Find(URPUnlitShader));

        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)RenderQueue.Transparent + 1;

        mat.SetTexture("_BaseMap", texture);
        mat.SetColor("_BaseColor", Color.white);

        return mat;
    }

    private Texture2D CreateRadialFadeGradient()
    {
        var tex = new Texture2D(GradientResolution, GradientResolution, TextureFormat.RGBA32, false);
        var pixels = new Color32[GradientResolution * GradientResolution];

        float center = GradientResolution * 0.5f;
        float fadeRatio = Mathf.Clamp01(_fadeRadius / (_fadeSize * 0.5f));
        float invFadeRatio = 1f / Mathf.Max(fadeRatio, 0.01f);

        for (int y = 0; y < GradientResolution; y++)
        {
            float dy = y - center;
            for (int x = 0; x < GradientResolution; x++)
            {
                float dx = x - center;
                float t = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) / center);
                float remapped = Mathf.Clamp01(t * invFadeRatio);
                float smooth = remapped * remapped * (3f - 2f * remapped);
                pixels[y * GradientResolution + x] = new Color32(0, 0, 0, (byte)(smooth * 255f));
            }
        }

        tex.SetPixels32(pixels);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.Apply();
        return tex;
    }
}
