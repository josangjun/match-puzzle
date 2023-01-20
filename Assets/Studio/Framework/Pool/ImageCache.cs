using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Studio.Framework
{
    public class ImageCache : MonoBehaviour
    {
        public Coroutine Load(List<string> urls)
        {
            for (var i = urls.Count - 1; i >= 0; --i)
            {
                if (Davinci.IsCached(urls[i]))
                    urls.RemoveAt(i);
            }

            if (urls.Count == 0)
            {
                return null;
            }

            IEnumerator StartOperation()
            {
                Coroutine[] coroutines = new Coroutine[urls.Count];
                for (var i = 0; i < urls.Count; ++i)
                    coroutines[i] = Load(urls[i]);
                foreach (var arg in coroutines)
                    yield return arg;
            }
            return StartCoroutine(StartOperation());
        }

        public Coroutine Load(string url)
        {
            return StartCoroutine(LoadAsync(url));
        }

        public IEnumerator LoadAsync(string url)
        {
            var isDone = false;
            var cache = Davinci.get();
            cache.setCached(true)
                .load(url)
                .withEndAction(() => isDone = true)
                .start();
            while (isDone == false)
                yield return null;
        }
    }
}