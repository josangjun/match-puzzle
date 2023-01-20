
using System.Collections;
using UnityEngine;
using UniRx;

namespace Studio.Framework.Animations
{
    public enum StopAction { Nothing, Disable, Destroy, Custom };
    
    public class ToggleAnimator : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;

        public Animator animator => _animator;

        public static implicit operator Animator(ToggleAnimator arg)
        {
            return arg.animator;
        }

        [SerializeField]
        private AnimatorParameterInfo _trueParameter;
        [SerializeField]
        private AnimatorParameterInfo _falseParameter;

        public StopAction stopAction;

        private readonly UniRx.Subject<GameObject> _onStop = new UniRx.Subject<GameObject>();

        public System.IObservable<GameObject> OnStopAsObservable() => _onStop.AsObservable().TakeUntilDisable(this);

        public void Toggle(bool val)
        {
            if (val)
            {
                if (gameObject.activeSelf == val)
                    return;
                gameObject.SetActive(val);
                if (stopActionEn != null)
                    StopCoroutine(stopActionEn);
                _trueParameter.Apply(_animator);
            }
            else
            {
                _falseParameter.Apply(_animator);
                TriggerStopAction();
            }
        }

        private void TriggerStopAction()
        {
            if (!isActiveAndEnabled || stopAction == StopAction.Nothing)
                return;
            
            if (stopActionEn != null)
                StopCoroutine(stopActionEn);
            StartCoroutine(stopActionEn = WaitFinish(_onStop));
        }

        private void OnDisable()
        {
            if (stopActionEn != null)
            {
                StopCoroutine(stopActionEn);    
            }
            stopActionEn = null;
        }

        private IEnumerator stopActionEn;

        private IEnumerator WaitFinish(System.IObserver<GameObject> observer)
        {
            yield return null;
            try
            {
                while (this)
                {
                    var state = _animator.GetCurrentAnimatorStateInfo(0);
                    if (1f <= state.normalizedTime)
                        break;
                    yield return null;
                    continue;
                }
            }
            finally
            {
                observer.OnNext(gameObject);
            }
            if (stopAction == StopAction.Disable)
                gameObject.SetActive(false);
            else if (stopAction == StopAction.Destroy)
                Destroy(gameObject);
            stopActionEn = null;
        }

        private void OnValidate()
        {
            _trueParameter.OnValidate();
            _falseParameter.OnValidate();
        }
    }
}