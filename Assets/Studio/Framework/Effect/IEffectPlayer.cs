using UnityEngine;

namespace Studio.Framework
{
    public interface IEffectPlayer
    {
        GameObject gameObject { get; }

        Transform transform { get; }

        GameObject target { get; }

        void PlayEffect(GoEffect asset);
    }
}