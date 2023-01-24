using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Studio.Game
{
    public class StageCheat : MonoBehaviour
    {
        private Gem GetFocusedGem()
        {
            if (Stage.Instance.focusedGem.val)
            {
                var g = Stage.Instance.focusedGem.val;
                Stage.Instance.focusedGem.val = null;
                return g;
            }

            var screenPt = Input.mousePosition;
            var go = GameObject.FindWithTag("UICamera");
            var camera = go.GetComponent<Camera>();
            var worldPt = camera.ScreenToWorldPoint(screenPt);
            var root = Stage.Instance.transform;
            worldPt.z = root.position.z;
            var localPt = root.InverseTransformPoint(worldPt);
            return (Gem)(Point)localPt;
        }

        public void SetGemType(string strType)
        {
            System.Enum.TryParse<GemType>(strType, out var type);

            var g = GetFocusedGem();
            if (g)
                g.type = type;
        }

        public void Explode()
        {
            var g = GetFocusedGem();
            if (g)
                g.Explode();
        }

        public void Fall()
        {
            var g = GetFocusedGem();
            if (g)
                g.Fall();
        }
    }
}