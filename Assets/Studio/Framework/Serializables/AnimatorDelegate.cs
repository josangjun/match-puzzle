using System;
using UnityEngine;

namespace Studio.Framework
{
    [System.Serializable]
    public class AnimatorParameterValue {
        public bool boolValue;
        public int intValue;
        public float floatValue;
    }
    [System.Serializable]
    public class AnimatorParameterInfo 
    #if UNITY_EDITOR
    : ISerializationCallbackReceiver {
        void ISerializationCallbackReceiver.OnAfterDeserialize() {}
        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            OnValidate();
        }
    #else
    {
    #endif
        public string name;
        public bool updateImmediate;
        [HideInInspector]
        public int hash;
        public AnimatorParameterValue parameterValue;
        public AnimatorControllerParameterType type = AnimatorControllerParameterType.Trigger;

        public void Apply(Animator animator)
        {
            switch (type)
            {
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(hash, parameterValue.boolValue);
                    break;
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(hash, parameterValue.intValue);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    animator.SetTrigger(hash);
                    break;
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(hash, parameterValue.floatValue);
                    break;
                default: break;
            }
            if (updateImmediate)
                animator.Update(0);
        }

        public void OnValidate()
        {
            hash = Animator.StringToHash(name);
        }
    }

    [System.Serializable]
    public class AnimatorDelegate
    {
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private bool _debug;

        public void SetTrigger(int id)
        {
            #if UNITY_EDITOR
            if (_debug)
                Debug.Log($"[AnimatorDelegate.SetTrigger] {_animator.name}:{FieldId.ToString(id)}");
            #endif
            _animator.SetTrigger(id);
        }

        public void SetBool(int id, bool val)
        {
            #if UNITY_EDITOR
            if (_debug)
                Debug.Log($"[AnimatorDelegate.SetBool] {_animator.name}:{FieldId.ToString(id)}, {val}");
            #endif
            _animator.SetBool(id, val);
        }

        public void SetInteger(int id, int val)
        {
            #if UNITY_EDITOR
            if (_debug)
                Debug.Log($"[AnimatorDelegate.SetInteger] {_animator.name}:{FieldId.ToString(id)}, {val}");
            #endif
            _animator.SetInteger(id, val);
        }

        public int GetInteger(int id)
        {
            return _animator.GetInteger(id);
        }

        public bool GetBool(int id)
        {
            return _animator.GetBool(id);
        }
    }
}