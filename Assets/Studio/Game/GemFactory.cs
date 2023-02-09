using Studio.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Studio.Game
{
    public class GemFactory : MonoBehaviour
    {
        [SerializeField]
        private Stage _stage;
        [SerializeField]
        private GemVisualizer _gemVisualizer;
        public GemVisualizer visualizer => _gemVisualizer;

        // Start is called before the first frame update
        void Start()
        {

        }

        private const string assetPath = "Assets/Prefabs/Base/Gem.prefab";

        public async Task Prepare()
        {
            var g = await Service.GoPool.RentAsync<Gem>(assetPath, transform);
            g.Return();
        }

        public readonly UniRx.Subject<Gem> onInit = new UniRx.Subject<Gem>();

        public GemType NewRandomGemType()
        {
            return (GemType)Random.Range(0, Stage.Instance.GemTypeCount) + 1;
        }

        public Gem Produce()
        {
            var t = NewRandomGemType();
            return Produce(t);
        }

        public Gem Produce(GemType type)
        {
            var g = Service.GoPool.Rent<Gem>(assetPath, transform);
            g.SetVisualizer(_gemVisualizer);
            g.type = type;
            onInit.OnNext(g);
            return g;
        }
    }
}