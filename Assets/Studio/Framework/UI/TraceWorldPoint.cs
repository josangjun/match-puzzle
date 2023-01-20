using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Studio.Framework
{
    public class TraceWorldPoint : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _offset;

        public Vector3 offset => _offset;
        
        [SerializeField]
        private Vector3 _localOffset;
        public Vector3 LocalOffset
        {
            get => _localOffset;
            set => _localOffset = value;
        }

        private Camera _mainCamera;

        public Camera mainCamera
        {
            get 
            {
                if (!_mainCamera)
                    _mainCamera = Camera.main;
                return _mainCamera;
            }
        }

        private Camera _uiCamera;
        private Camera uiCamera 
        {
            get 
            {
                if (!_uiCamera)
                {
                    var canvas = transform.root.GetComponent<Canvas>();
                    _uiCamera = canvas.worldCamera;
                }
                return _uiCamera;
            }
        }

        [SerializeField]
        private Transform _target;
        public Transform target => _target;

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            UpdatePosition();
        }

        [ContextMenu("UpdatePosition")]
        public void UpdatePosition()
        {
            if (!_target || !mainCamera)
                return;

            var point = _target.transform.position;
            var screenPoint = mainCamera.WorldToScreenPoint(point);
            var uiPoint = uiCamera.ScreenToWorldPoint(screenPoint) + offset;
            uiPoint.z = 0f;
            transform.position = uiPoint;
            transform.localPosition += LocalOffset;
        }
    }
}