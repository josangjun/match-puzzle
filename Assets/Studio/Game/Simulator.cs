using UnityEngine;
using System.Collections.Generic;

namespace Studio.Game
{
    [System.Serializable]
    public class Simulator 
    {
        Judge judge => Stage.Instance.judge;

        private List<Move> moves = new List<Move>();

        public bool enabled;

        [SerializeField]
        private float interval = 1f;
        private float elapsed = 0f;

        public void Update()
        {
            if (!enabled) 
                return;

            if (elapsed < interval)
            {
                elapsed += Time.deltaTime;
                return;
            }

            elapsed = 0f;
            
            judge.CollectMove(moves);
            if (moves.Count == 0)
            {
                Stage.Instance.Shuffle();
                return;
            }

            var move = moves[Random.Range(0, moves.Count)];
            var lhs = (Gem)move.point;
            var rhs = lhs.GetNext(move.dir);
            Stage.Instance.Swap(lhs, rhs);
            moves.Clear();
        }
    }
}
