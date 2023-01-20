using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Threading.Tasks;
using System.Linq;
using Studio.Framework;

namespace Studio.Game
{
    public class Stage : MonoBehaviour
    {
        public const int COL = 8;
        public const int ROW = 8;

        [SerializeField]
        private JewelFactory _jewelFactory;

        private List<Jewel> jewels = new List<Jewel>();

        // Start is called before the first frame update
        IEnumerator Start()
        {
            if (!Service.ready)
                yield return Service.Init();

            _jewelFactory.onInit.Subscribe(OnInit).AddTo(this);
            yield return _jewelFactory.Prepare().ToObservable().ToYieldInstruction();

            var count = COL * ROW;
            for (var i = 0; i < count; ++i)
            {
                _jewelFactory.Produce();
            }
            
            ArrangePosition();
        }

        private void ArrangePosition()
        {
            var j = jewels[0];
            var size = j.rect.sizeDelta;
            var board = (RectTransform)_jewelFactory.transform;
            var offset = board.sizeDelta * 0.5f;
            offset.x -= size.x * 0.5f;
            offset.y -= size.y * 0.5f;

            Jewel.Init(size, offset);
            
            int i = 0;
            for (var x = 0; x < COL; ++x)
            {
                for (var y = 0; y < ROW; ++y)
                {
                    j = jewels[i];
                    j.transform.SetSiblingIndex(i);
                    j.SetPosition(x, y);
                    ++i;
                }
            }
        }

        private class PressdedJewel
        {
            private Jewel _j;

            public Jewel val
            {
                get => _j;
                private set
                {
                    if (_j)
                        _j.image.SetAlpha(1f);
                    _j = value;
                    if (_j)
                        _j.image.SetAlpha(0.7f);
                }
            }

            public void Set(Jewel j)
            {
                val = j;
            }
        }

        private PressdedJewel pressedJewel = new PressdedJewel();

        private void OnInit(Jewel j)
        {
            j.onPress.TakeUntilDisable(j).Subscribe(OnPress).AddTo(this);
            jewels.Add(j);
        }

        private void OnPress(Jewel j)
        {
            SLogger.Log(j);
            if (pressedJewel.val == null)
            {
                pressedJewel.Set(j);
                j.image.SetAlpha(0.7f);
                return;
            }

            var lhs = pressedJewel.val;
            var rhs = j;
            var tf = lhs.IsAdjacency(rhs);
            pressedJewel.Set(null);
            SLogger.Log($"adjacency:{lhs}, {rhs}, {tf}");
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}