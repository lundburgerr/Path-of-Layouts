using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace fireMCG.PathOfLayouts.Gameplay
{
    public sealed class FogOfWar : MonoBehaviour
    {
        private static readonly int _maskAId = Shader.PropertyToID("_PrevMask");
        private static readonly int _playerUvId = Shader.PropertyToID("_PlayerUv");
        private static readonly int _hardRadiusUvId = Shader.PropertyToID("_HardRadiusUv");
        private static readonly int _softRadiusUvId = Shader.PropertyToID("_SoftRadiusUv");
        private static readonly int _aspectId = Shader.PropertyToID("_Aspect");
        private static readonly int _fogMaskId = Shader.PropertyToID("_FogMask");

        [SerializeField] private RawImage _fogImage;
        [SerializeField] private RectTransform _fogTransform;

        [SerializeField] private Material _fogMaterial;
        [SerializeField] private Material _fogStampMaterial;

        [SerializeField] private int _hardBrushRadius = 60;
        [SerializeField] private int _softBrushRadius = 100;

        private int _width;
        private int _height;

        private RenderTexture _maskA;
        private RenderTexture _maskB;

        private void Awake()
        {
            Assert.IsNotNull(_fogImage);
            Assert.IsNotNull(_fogTransform);
            Assert.IsNotNull(_fogMaterial);
            Assert.IsNotNull(_fogStampMaterial);

            _fogImage.material = _fogMaterial;
        }

        private void OnDestroy()
        {
            Clear();
        }

        public void Build(int width, int height)
        {
            Clear();

            _fogTransform.sizeDelta = new Vector2(width, height);

            _width = width;
            _height = height;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_fogTransform);

            _maskA = CreateMaskTexture(width, height, "FogMaskA");
            _maskB = CreateMaskTexture(width, height, "FogMaskB");

            ClearMaskTexture(_maskA);
            ClearMaskTexture(_maskB);
        }

        private void Clear()
        {
            ReleaseMaskTexture(ref _maskA);
            ReleaseMaskTexture(ref _maskB);
        }

        public void RevealAt(Vector2Int pixelCoordinate)
        {
            Vector2 uvCoordinate = new Vector2(
                Mathf.Clamp01((float)pixelCoordinate.x / _width),
                Mathf.Clamp01((float)pixelCoordinate.y / _height));

            float aspect = (float)_width / _height;
            float hardRadiusUv = (float)_hardBrushRadius / _width;
            float softRadiusUv = (float)_softBrushRadius / _width;

            _fogStampMaterial.SetTexture(_maskAId, _maskA);
            _fogStampMaterial.SetVector(_playerUvId, uvCoordinate);
            _fogStampMaterial.SetFloat(_hardRadiusUvId, hardRadiusUv);
            _fogStampMaterial.SetFloat(_softRadiusUvId, softRadiusUv);
            _fogStampMaterial.SetFloat(_aspectId, aspect);

            CommandBuffer cmd = CommandBufferPool.Get("FogOfWarStamp");
            cmd.Blit(_maskA, _maskB, _fogStampMaterial, 0);
            Graphics.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            (_maskA, _maskB) = (_maskB, _maskA);

            _fogMaterial.SetTexture(_fogMaskId, _maskA);
        }

        private static RenderTexture CreateMaskTexture(int width, int height, string name)
        {
            RenderTexture texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                useMipMap = false,
                autoGenerateMips = false
            };

            texture.Create();

            return texture;
        }

        private static void ClearMaskTexture(RenderTexture texture)
        {
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = texture;
            GL.Clear(false, true, new Color(1, 0, 0, 1));
            RenderTexture.active = previousActive;
        }

        private static void ReleaseMaskTexture(ref RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            texture.Release();
            Destroy(texture);
            texture = null;
        }
    }
}