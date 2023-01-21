using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;

namespace Studio.Game.Commands
{
    [System.Serializable]
    public class ProduceGem : ICommand
    {
        public GemType type;
        public int x, y;
        
        public void Run()
        {
            var g = Stage.Instance.gemFactory.Produce(type);
            g.SetPosition(x, y);
        }
    }

    [System.Serializable]
    public class SwapGem : ICommand
    {
        public int x, y, z, w;

        public async void Run()
        {
            var lhs = Stage.Instance[x, y];
            var rhs = Stage.Instance[z, w];
            if (lhs.IsLocked() || rhs.IsLocked())
                return;

            using var hh = lhs.Lock();
            using var hg = rhs.Lock();

            var s = lhs.MovePosition(z, w);
            var t = rhs.MovePosition(x, y);
            while (s.IsActive() || t.IsActive())
                await Task.Yield();
            var ss = lhs.MovePosition(x, y);
            var tt = rhs.MovePosition(z, w);
        }
    }

    [System.Serializable]
    public class PrepareBoard : ICommand
    {
        public void Run()
        {

        }
    }
}