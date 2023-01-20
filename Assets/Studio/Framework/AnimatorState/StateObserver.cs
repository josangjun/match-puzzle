using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.Events;

namespace Studio.Framework.AnimatorState
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class StateObserver : MonoBehaviour, IStateObserver
    {
        public virtual void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public Animator animator { get; private set; }
        public readonly Subject<StateSubject> onStateEnter = new Subject<StateSubject>();
        public readonly Subject<StateSubject> onStateExit = new Subject<StateSubject>();

        [System.Serializable]
        private class PersistentCallback {
            public UnityEvent<StateSubject> onEnter = new UnityEvent<StateSubject>();
            public UnityEvent<StateSubject> onExit = new UnityEvent<StateSubject>();
        }
        [SerializeField]
        private PersistentCallback _persistentCallback = new PersistentCallback();

        public virtual void OnStateEnter(StateSubject subject)
        {
            this.onStateEnter.OnNext(subject);
            _persistentCallback.onEnter.Invoke(subject);
        }

        public virtual void OnStateExit(StateSubject subject)
        {
            this.onStateExit.OnNext(subject);
            _persistentCallback.onExit.Invoke(subject);
        }

        public readonly Subject<AnimationEvent> onCustomEvent = new Subject<AnimationEvent>();

        public void OnCustomEvent(AnimationEvent evt)
        {
            onCustomEvent.OnNext(evt);
        }
    }
}