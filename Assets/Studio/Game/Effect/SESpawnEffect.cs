using Studio.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Studio.Game
{
    public class SESpawnEffect : SubEffect
    {
        public GoEffect prefab;

        public int count;

        public Vector2 interval;

        private UniRx.Subject<Component> onSpawn = new UniRx.Subject<Component>();

        public System.IObservable<Component> OnSpawnAsObservable() => onSpawn;

        public override async void Play(GameObject owner)
        {
            await Task.Yield();

            for (var i = 0; i < count; ++i)
            {
                var effect = Service.GoPool.Rent(prefab, owner.transform);

                onSpawn.OnNext(effect);

                effect.Play(owner);

                if(string.IsNullOrEmpty(sortLayerName) == false)
                {
                    Renderer[] renderers = effect.gameObject.GetComponentsInChildren<Renderer>();
                    foreach (var renderer in renderers)
                        renderer.sortingLayerID = SortingLayer.NameToID(sortLayerName);
                }

                await Task.Delay(System.TimeSpan.FromSeconds(Random.Range(interval.x, interval.y)));
            }
        }
    }
}