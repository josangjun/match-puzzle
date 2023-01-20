using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Studio.Game
{
    public class Stage : MonoBehaviour
    {
        public const int COL = 9;
        public const int ROW = 9;

        [SerializeField]
        private RectTransform _board;
        
        // Start is called before the first frame update
        IEnumerator Start()
        {
            if (!Service.ready)
                yield return Service.Init();

            var count = COL * ROW;
            for (var i = 0; i < count; ++i)
            {
                var t = Service.GoPool.RentAsync<Jewel>("Assets/Prefabs/Base/Jewel.prefab", _board);
                yield return t.ToObservable().ToYieldInstruction();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}