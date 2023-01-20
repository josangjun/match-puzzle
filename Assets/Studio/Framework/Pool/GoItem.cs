using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using MEC;

namespace Studio.Framework
{
    [DisallowMultipleComponent]
    public class GoItem : MonoBehaviour
    {
        private string _itemKey;
        public string itemKey
        {
            get
            {
                if (string.IsNullOrEmpty(_itemKey))
                    _itemKey = $"@{name}:{GetInstanceID()}";
                return _itemKey;
            } 
            set
            {
                _itemKey = value;
            }
        }

        public bool IsFromAddress()
        {
            return string.IsNullOrEmpty(_itemKey) == false && _itemKey[0] != '@';
        }

        public void Destroy()
        {
            if (IsFromAddress())
            {
                if (Addressables.ReleaseInstance(gameObject))
                    return;
            }
            Destroy(gameObject);
        }
        
        public virtual void OnRent() {}

        public virtual void OnReturn()
        {
            
        }

        public void Return()
        {
            if (hReturn.IsRunning)
                Timing.KillCoroutines(hReturn);
            
            Service.GoPool.Return(this);
        }

        protected CoroutineHandle hReturn;

        public void Return(float seconds)
        {
            if (!this || !isActiveAndEnabled)
                return;
            
            hReturn = ReturnAsync(seconds).RunCoroutineSingleton(hReturn, SingletonBehavior.Overwrite);
        }

        public bool IsReturning() 
        {
            return hReturn.IsRunning;
        }

        private IEnumerator<float> ReturnAsync(float seconds)
        {
            for (var e = 0f; e < seconds; e += Time.deltaTime)
            {
                if (!this || !isActiveAndEnabled)
                    yield break;
                yield return Timing.WaitForOneFrame;
            }
            Service.GoPool.Return(this);
        }
    }
}