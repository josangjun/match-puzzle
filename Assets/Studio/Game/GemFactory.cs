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

        // Start is called before the first frame update
        void Start()
        {

        }

        private CountingLock _lock = new CountingLock();

        private const string assetPath = "Assets/Prefabs/Base/Gem.prefab";

        public async Task Prepare()
        {
            var g = await Service.GoPool.RentAsync<Gem>(assetPath, transform);
            g.Return();
        }

        public readonly UniRx.Subject<Gem> onInit = new UniRx.Subject<Gem>();

        public Gem Produce()
        {
            var t = (GemType)Random.Range(1, (int)GemType.White + 1);
            return Produce(t);
        }

        public Gem Produce(GemType type)
        {
            using var h = _lock.Lock();
            var g = Service.GoPool.Rent<Gem>(assetPath, transform);
            g.type = type;
            onInit.OnNext(g);
            return g;
        }

        public bool IsProcessing => _lock.count > 0;
    }
}