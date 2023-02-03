using UnityEngine;
using System.Collections.Generic;

namespace Studio.Game
{
    [System.Serializable]
    public class Automator 
    {
        Judge judge => Stage.Instance.judge;

        private List<Move> moves = new List<Move>();

        [SerializeField]
        private bool _enabled;
        public bool enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                    return;
                _enabled = value;
                onAuto.OnNext(value);
            }
        }
        
        [SerializeField]
        [Range(0.2f, 2f)]
        private float _interval = 1f;
        public float interval
        {
            get => _interval;
            set => _interval = Mathf.Max(0.3f, value);
        }
        private float elapsed = 0f;

        public UniRx.Subject<bool> onAuto = new UniRx.Subject<bool>();


        public void Update()
        {
            if (!enabled) 
                return;

            if (elapsed < interval)
            {
                elapsed += Time.deltaTime;
                return;
            }

            if (!Test())
            {
                enabled = false;
                return;
            }

            elapsed = 0f;
            
            judge.CollectMove(moves);
            if (moves.Count == 0)
            {
                if (!AllFree())
                {
                    elapsed = interval - Mathf.Epsilon;
                    return;
                }
                Stage.Instance.Shuffle();
                return;
            }

            var move = moves[Random.Range(0, moves.Count)];
            var lhs = (Gem)move.point;
            var rhs = lhs.GetNext(move.dir);
            Stage.Instance.Swap(lhs, rhs);
            moves.Clear();
        }

        public bool Test()
        {
            /*
            for (var i = 0; i < Stage.COL; ++i)
            {
                for (var j = 0; j < Stage.ROW; ++j)
                {
                    
                }
            }
            */
            return true;
        }

        public bool AllFree()
        {
            for (var i = 0; i < Stage.COL; ++i)
            {
                for (var j = 0; j < Stage.ROW; ++j)
                {
                    var g = (Gem)new Point(i, j);
                    if (g && g.IsLocked())
                        return false;
                }
            }
            return true;
        }
    }
}
