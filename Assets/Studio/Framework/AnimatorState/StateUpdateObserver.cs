using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Studio.Framework.AnimatorState
{
    public class StateUpdateObserver : MonoBehaviour, IStateUpdateObserver
    {
        public UnityEvent<StateSubject> onUpdate = new UnityEvent<StateSubject>();

        public void OnStateUpdate(StateSubject subject)
        {
            onUpdate.Invoke(subject);
        }
    }
}