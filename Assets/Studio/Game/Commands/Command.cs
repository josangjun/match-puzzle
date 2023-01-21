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
    public class PrepareBoard : ICommand
    {
        public void Run()
        {

        }
    }
}