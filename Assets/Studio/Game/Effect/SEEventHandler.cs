using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

namespace Studio.Framework
{
    public class SEEventHandler : SubEffect
    {
        public GameObject owner { get; private set; }

        public override void Play(GameObject owner)
        {
            this.owner = owner;
        }

        public void NewEffect(AnimationEvent evt)
        {
            var obj = evt.objectReferenceParameter as GameObject;
            var effect = obj.GetComponent<GoEffect>();
            var instance = (GoEffect)Service.GoPool.Rent(effect);
            instance.Play(gameObject);
        }
    }
}