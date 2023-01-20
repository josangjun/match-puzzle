using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

namespace Studio.Framework
{
    public class SEActivate : SubEffect
    {
        [SerializeField]
        private GameObject _target;

        [SerializeField]
        private float _delay;

        public override void Play(GameObject owner)
        {
            if (_target)
                Timing.CallDelayed(_delay, () => _target.gameObject.SetActive(true), gameObject);
        }

        public void OnDisable()
        {
            if (_target)
                _target.gameObject.SetActive(false);
        }
    }
}