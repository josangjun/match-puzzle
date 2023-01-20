using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Studio.Framework.AnimatorState
{
    public interface IStateObserver
    {
        void OnStateEnter(StateSubject subject);
        void OnStateExit(StateSubject subject);
    }

    public interface IStateUpdateObserver {
        void OnStateUpdate(StateSubject subject);
    }

    public interface IStateMoveObserver {
        void OnStateMove(StateSubject subject);
    }
}