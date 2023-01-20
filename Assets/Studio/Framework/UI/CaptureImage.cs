using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.Framework
{
    [RequireComponent(typeof(RawImage))]
    public class CaptureImage : RawImage
    {
        private RenderTexture _target;

        private ScreenCapture GetScreen()
        {
            var screen = ScreenCapture.Instance;
            if (screen == null)
            {
                screen = FindObjectOfType<ScreenCapture>();
                if (screen == null)
                    return null;
            }
            return screen;
        }

        [ContextMenu("Refresh")]
        private void Refresh_()
        {
            Refresh(false);
        }

        public void Refresh(bool takeScreenshotWider = false)
        {
            if (!this || !isActiveAndEnabled)
                return;

            var screen = GetScreen();
            if (screen)
            {
                base.texture = _target = screen.Capture(takeScreenshotWider);
            }
        }

        public void Release()
        {
            if (_target)
            {
                //RenderTexture.ReleaseTemporary(_target);
#if UNITY_EDITOR
                DestroyImmediate(_target);
#else
                Destroy(_target);
#endif
                base.texture = _target = null;
            }
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            var screen = GetScreen();
            if (screen)
            {
                screen.Add(this);
            }
            Refresh();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            var screen = GetScreen();
            if (screen)
                screen.Remove(this);

            Release();
        }

        public void RefreshCapture(bool takeScreenShotWider)
        {
            var screen = GetScreen();
            if (screen)
                screen.Remove(this);

            Release();
            
            if (screen)
            {
                screen.Add(this);
            }
            Refresh(takeScreenShotWider);
        }

        private float _alpha;

        public bool IsVisible
        {
            get => base.color.a > 0f && isActiveAndEnabled;
            set
            {
                if (IsVisible == value)
                    return;

                if (value)
                {
                    this.SetAlpha(_alpha);
                }
                else
                {
                    _alpha = this.GetAlpha();
                    this.SetAlpha(0f);
                }
            }
        }
    }
}