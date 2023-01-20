using Studio.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Studio.Game
{
    public class JewelFactory : MonoBehaviour
    {
        [SerializeField]
        private Stage _stage;

        // Start is called before the first frame update
        void Start()
        {

        }

        private CountingLock _lock = new CountingLock();

        public async Task Prepare()
        {
            var j = await Service.GoPool.RentAsync<Jewel>("Assets/Prefabs/Base/Jewel.prefab", transform);
            j.Return();
        }

        public readonly UniRx.Subject<Jewel> onInit = new UniRx.Subject<Jewel>();

        public Jewel Produce()
        {
            using var h = _lock.Lock();
            var j = Service.GoPool.Rent<Jewel>("Assets/Prefabs/Base/Jewel.prefab", transform);
            onInit.OnNext(j);
            return j;
        }

        public bool IsProcessing => _lock.count > 0;
    }
}