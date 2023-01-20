using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using System.Linq;
using Studio.Common;

namespace Studio.Framework
{
    [ExecuteAlways]
    [RequireComponent(typeof(RawImage))]
    public class ScreenCapture : Singleton<ScreenCapture>
    {
        private Material _material;
        [SerializeField]
        private Shader _shader;
        [SerializeField]
        private float _blurSize;
        [SerializeField]
        private Color _color;

        [SerializeField, HideInInspector]
        private RawImage _image;
        
        private RenderTexture _target;

        private SortedList<int, CaptureImage> _imageDict = new SortedList<int, CaptureImage>();

        [SerializeField]
        private GraphicsFormat[] _candidateFormats = { GraphicsFormat.R5G6B5_UNormPack16, GraphicsFormat.R8G8B8_UNorm };
        
        private GraphicsFormat _format;

        [SerializeField][Range(0.1f, 2)]
        private float _scale = 1f;

        public void Start()
        {
            if (_image == null)
                _image = GetComponent<RawImage>();
            if (Application.isPlaying)
            {
                _image.enabled = false;
            }
        }

        private GraphicsFormat GetCompatibleFormat(IEnumerable<GraphicsFormat> candidates)
        {
            foreach (var candidate in candidates)
            {
                var fmt = SystemInfo.GetCompatibleFormat(candidate, FormatUsage.Render);
                if (fmt != GraphicsFormat.None)
                    return fmt;
            }
            return SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
        }



        [ContextMenu("Capture")]
        public RenderTexture Capture(bool takeScreenshotWider)
        {
            var rect = _image.rectTransform.rect;
            var w = Mathf.RoundToInt(rect.width * _scale);
            var h = Mathf.RoundToInt(rect.height * _scale);
            if (takeScreenshotWider)
            {
                w = Mathf.RoundToInt(h * 0.8f);
            }
            return Capture(w, h);
        }

        public RenderTexture Capture(int w, int h)
        {
            if (_material == null)
            {
                _material = new Material(_shader);
                _material.name = "GrabpassBlur";
            }
            if (_blurSize > 0f)
            {
                _material.SetFloat("_Blur", _blurSize);
                _material.SetColor("_Color", _color);
                _image.material = _material;
            }
            if (Application.isPlaying)
                _image.enabled = true;

            if (_format == GraphicsFormat.None)
                _format = GetCompatibleFormat(_candidateFormats);
            var desc = new RenderTextureDescriptor(w, h, _format, 24, 1);
//            var target = RenderTexture.GetTemporary(desc);
            var target = new RenderTexture(desc);

            SaveRT(target);
            if (Application.isPlaying)
                _image.enabled = false;
            
            _image.material = null;
            _image.texture = target;
            return target;
        }

        public void Add(CaptureImage image)
        {
            _imageDict[image.GetInstanceID()] = image;
        }

        public void Remove(CaptureImage image)
        {
            _imageDict.Remove(image.GetInstanceID());
        }


        private Camera[] _cameras = null;

        private List<CaptureImage> activeImages = new List<CaptureImage>();

        [SerializeField]
        private LayerMask _cullingMask = -1;

        private void SaveRT(RenderTexture rt)
        {
            activeImages.Clear();
            activeImages.AddRange(_imageDict.Values.Where(arg => arg.isActiveAndEnabled && arg.GetAlpha() > 0f));
            foreach (var img in activeImages)
                img.IsVisible = false;

            var count = Camera.allCamerasCount;
            if (_cameras == null || _cameras.Length != count)
                _cameras = new Camera[count];
            Camera.GetAllCameras(_cameras);
            
            for (var i = 0; i < count; ++i)
            {
                var c = _cameras[i];
                var layer = c.gameObject.layer;
                
                if (((1 << layer) & _cullingMask.value) == 0)
                    continue;

                var cullingMask = c.cullingMask;
                if (c.CompareTag("MainCamera"))
                    c.cullingMask |= 1;
                c.targetTexture = rt;
                c.Render();
                c.targetTexture = null;
                c.cullingMask = cullingMask;
            }

            foreach (var img in activeImages)
                img.IsVisible = true;
            activeImages.Clear();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (dirty > 0)
            {
                Capture(false);
                --dirty;
            }
        }
#endif

        private int dirty;
        private void OnValidate()
        {
            if (!_shader)
                _shader = Shader.Find("UI/GrabpassBlur");
            _blurSize = Mathf.Max(0, _blurSize);
            if (!Application.isPlaying && _image.enabled)
                ++dirty;
        }

        protected void OnDisable()
        {
            if (_material)
            {
                DestroyImmediate(_material);
                _material = null;
            }
        }
    }
}