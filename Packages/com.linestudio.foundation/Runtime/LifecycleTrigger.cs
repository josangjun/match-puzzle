using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Studio.Common
{
    public class LifecycleTrigger : MonoBehaviour
    {
        public enum Type
        {
            Awake, Start, Enable, Disable, Destroy
        }

        public Type type;
        public float delay;
        public UnityEvent trigger;

        private void Awake()
        {
            if (type == Type.Awake)
            {
                StartCoroutine(Trigger());
            }
        }

        private void Start()
        {
            if (type == Type.Start)
            {
                StartCoroutine(Trigger());
            }
        }

        private void OnEnable()
        {
            if (type == Type.Enable)
            {
                StartCoroutine(Trigger());
            }
        }

        private void OnDisable()
        {
            if (type == Type.Disable)
            {
                StartCoroutine(Trigger());
            }
        }

        private void OnDestroy()
        {
            if (type == Type.Destroy)
            {
                StartCoroutine(Trigger());
            }
        }

        private IEnumerator Trigger()
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            trigger.Invoke();
        }
    }
}
