using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Studio.Game
{
    [System.Serializable]
    public class Lane
    {
        private int x;

        public Lane() { }
        public Lane(int x)
        {
            this.x = x;
        }
        [SerializeField]
        private List<Gem> _list = new();

        public void Add(Gem g) => _list.Add(g);

        public int Count => _list.Count;

        public void Update()
        {
            if (_list.Count == 0)
                return;

            var top = Stage.Instance[x, 0];
            if (top)
            {
                if (top.IsFloor())
                {
                    _list.Remove(top);
                    return;
                }
            }

            for (var j = 0; j < _list.Count; ++j)
            {
                var g = _list[j];
                if (g.y >= 0)
                {
                    _list.RemoveAt(j--);
                    continue;
                }
                if (g.IsFalling())
                    continue;
                g.SetPosition(x, -1 - j);
                g.Fall();
            }
        }
    }

    [System.Serializable]
    public class Lanes
    {
        [SerializeField]
        private Lane[] _lanes = new Lane[Stage.COL];

        public Lanes()
        {
            for (var i = 0; i < _lanes.Length; ++i)
                _lanes[i] = new Lane(i);
        }

        public Lane this[int i] => _lanes[i];

        public void Update()
        {
            for (var i = 0; i < _lanes.Length; ++i)
            {
                var lane = _lanes[i];

                lane.Update();
            }
        }
    }
}