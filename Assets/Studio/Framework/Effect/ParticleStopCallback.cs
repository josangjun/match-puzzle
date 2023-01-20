using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Studio.Framework
{
    
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleStopCallback : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem _particleSystem;

        [SerializeField]
        private UnityEvent<GameObject> _onStopped;

        [SerializeField]
        private ParticleSystemStopAction _stopAction = ParticleSystemStopAction.Callback;

        public void Start()
        {
            var main = _particleSystem.main;
            main.stopAction = _stopAction;
        }

        private void OnParticleSystemStopped()
        {
            if (_stopAction == ParticleSystemStopAction.Callback)
                _onStopped.Invoke(gameObject);
        }

        protected void OnDisable()
        {
            if (_stopAction == ParticleSystemStopAction.Disable)
                _onStopped.Invoke(gameObject);
        }

        protected void OnDestroy()
        {
            if (_stopAction == ParticleSystemStopAction.Destroy)
                _onStopped.Invoke(gameObject);
        }
    }
}