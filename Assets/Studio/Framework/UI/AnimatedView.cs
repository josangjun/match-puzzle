using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Studio.Framework
{
    [RequireComponent(typeof(Animator))]
    public class AnimatedView : View
    {
        [SerializeField]
        protected string _openTrigger = "Start";
        protected int _openHash;
        [SerializeField]
        protected string _closeTrigger = "Close";
        protected int _closeHash;

        private Animator _ani;

        public virtual Animator animator
        {
            get
            {
                if (_ani == null)
                    _ani = GetComponent<Animator>();
                return _ani;
            }
        }

        protected virtual void Awake()
        {
            _openHash = Animator.StringToHash(_openTrigger);
            _closeHash = Animator.StringToHash(_closeTrigger);
        }

        protected override async Task FadeIn()
        {
            if (animator.runtimeAnimatorController == null)
            {
                await base.FadeIn();
                return;
            }

            alpha = 1f;
            if (_openHash != 0)
                animator.SetTrigger(_openHash);
            await Task.Delay(System.TimeSpan.FromSeconds(inTime));
        }

        protected override async Task FadeOut(System.Action onComplete = null)
        {
            if (animator.runtimeAnimatorController == null)
            {
                await base.FadeOut(onComplete);
                return;
            }
            animator.SetTrigger(_closeHash);
            _canvasGroup.blocksRaycasts = false;
            
            for (var e = 0f; e < outTime; e += Time.deltaTime)
                await Task.Yield();
            
            if (!this)
                return;
            _canvasGroup.blocksRaycasts = true;
            onComplete?.Invoke();
        }
    }
}