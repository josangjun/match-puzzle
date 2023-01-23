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

            worldPt.z = Stage.Instance.transform.position.z;

            for (var i = 0; i < Stage.COL; ++i)
            {
                for (var j = 0; j < Stage.ROW; ++j)
                {
                    var g = Stage.Instance[i, j];
                    if (g == null)
                        continue;
                    var lpt = g.transform.InverseTransformPoint(worldPt);
                    if (g.rectTransform.rect.Contains(lpt))
                    {
                        Debug.Log($"{g}, {lpt}");
                        return g;
                    }
                }
            }
            return null;
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