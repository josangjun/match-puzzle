using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Studio.Framework
{
    [RequireComponent(typeof(Canvas))]
    public class WorldCanvas : MonoBehaviour
    {
        protected virtual void Start()
        {
            StartCoroutine(SetWorldCamera());
        }

        private IEnumerator SetWorldCamera()
        {
            var canvas = GetComponent<Canvas>();

            while (canvas.worldCamera == null)
            {
                yield return new WaitForEndOfFrame();
                canvas.worldCamera = Camera.main;
            }
        }
    }
}