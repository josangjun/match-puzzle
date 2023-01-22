using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Studio.Game
{
    public class StageCheat : MonoBehaviour
    {
        public void SetGemType(int type)
        {
            var screenPt = Input.mousePosition;
            var go = GameObject.FindWithTag("UICamera");
            var camera = go.GetComponent<Camera>();
            var worldPt = camera.ScreenToWorldPoint(screenPt);
            
            worldPt.z = Stage.Instance.transform.position.z;
            Debug.Log(worldPt);

            for (var i = 0; i < Stage.COL; ++i)
            {
                for (var j = 0; j < Stage.ROW; ++j)
                {
                    var g = Stage.Instance[i, j];
                    var lpt = g.transform.InverseTransformPoint(worldPt);
                    if (g.rect.rect.Contains(lpt))
                    {
                        g.type = (GemType)type;
                        Debug.Log($"{g}, {lpt}");
                        break;
                    }
                }
            }
        }
    }
}