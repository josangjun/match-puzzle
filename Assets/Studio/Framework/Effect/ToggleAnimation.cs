using System.Collections;
using UnityEngine;

namespace Studio.Framework.Animations
{
    [RequireComponent(typeof(Animation))]
    public class ToggleAnimation : MonoBehaviour
    {
        [SerializeField]
        private Animation _animation;
        public string trueAnimation;
        public string falseAnimation;

        public StopAction stopAction;

        public Animation GetAnimation() => _animation;

        public void Toggle(bool val)
        {
            if (val)
            {
                _animation.Play(trueAnimation);
            }
            else
            {
                _animation.Play(falseAnimation);
                TriggerStopAction();
            }
        }

        public bool IsPlaying()
        {
            return _animation.isPlaying;
        }

        private void TriggerStopAction()
        {
            if (stopAction == StopAction.Nothing)
                return;
            
            if (stopActionEn != null)
                StopCoroutine(stopActionEn);
            StartCoroutine(stopActionEn = WaitStopAction());
        }

        private void OnDisable()
        {
            stopActionEn = null;
        }

        private IEnumerator stopActionEn;

        private IEnumerator WaitStopAction()
        {
            yield return null;
            while (this)
            {
                if (!_animation.isPlaying)
                    break;
                yield return null;
                continue;
            }
            if (stopAction == StopAction.Disable)
                gameObject.SetActive(false);
            else if (stopAction == StopAction.Destroy)
                Destroy(gameObject);
            stopActionEn = null;
        }
    }
}