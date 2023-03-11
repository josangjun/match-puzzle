using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MEC;
using UniRx;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEngine.ResourceManagement.AsyncOperations;
using Studio.Framework.SceneBuilding;
using UnityEngine.SceneManagement;

namespace Studio.Framework
{
    public static class CoroutineBridge
    {
        public static Coroutine CallDelayed(this MonoBehaviour script, float seconds, System.Action call)
        {
            return script.StartCoroutine(WaitForSeconds(seconds).OnComplete(call));
        }

        public static IEnumerator WaitForSeconds(float seconds)
        {
            for (var elapsed = 0f; elapsed < seconds; elapsed += Time.deltaTime)
                yield return null;
        }

        public static IEnumerator WaitInflation(Scene scene) => PrefabDeflater.Wait(scene);

        public static IEnumerator OnComplete(this Coroutine coroutine, System.Action action)
        {
            yield return coroutine;
            action?.Invoke();
        }

        public static float WaitUntilDone<T>(this System.IObservable<T> obs)
        {
            IEnumerator<float> Tick_()
            {
                var e = obs.ToYieldInstruction();
                while (e.IsDone == false)
                    yield return Timing.WaitForOneFrame;
            }
            return Tick_().WaitUntilDone();
        }

        public static System.IObservable<UniRx.Unit> ToObservable(this Coroutine coroutine)
        {
            return new YieldWhile(coroutine).ToObservable();
        }

        public static IEnumerator OnComplete(this IEnumerator en, System.Action action)
        {
            yield return en;
            action?.Invoke();
        }

        public static YieldWhile GetAwaiter(this IEnumerator en)
        {
            return new YieldWhile(en);
        }

        public static IEnumerator<float> Append(this CoroutineHandle handle, System.Action action)
        {
            yield return Timing.WaitUntilDone(handle);
            action?.Invoke();
        }

        public static async Task Delay(float seconds, float timeOut = float.MaxValue)
        {
            seconds = Mathf.Clamp(seconds, 0f, timeOut);
            for (var e = 0f; e < seconds; e += Time.deltaTime)
                await Task.Yield();
        }

        public static IEnumerator Delay(this IEnumerator en, float seconds)
        {
            yield return WaitForSeconds(seconds);
            yield return en;
        }

        public static IEnumerator<float> AsMEC(this IEnumerator en)
        {
            var finished = false;
            Service.Instance.StartCoroutine(en.OnComplete(() => finished = true));
            yield return Timing.WaitUntilTrue(() => finished);
        }

        public static IEnumerator<float> AsMEC(this Coroutine coroutine)
        {
            var finished = false;
            Service.Instance.StartCoroutine(coroutine.OnComplete(() => finished = true));
            yield return Timing.WaitUntilTrue(() => finished);
        }

        public static IEnumerator WaitWhile(System.Func<bool> predicate)
        {
            while (predicate())
                yield return null;
        }

        public static IEnumerator WaitUntil(System.Func<bool> predicate)
        {
            while (!predicate())
                yield return null;
        }

        public static IEnumerator AsEnum(this CoroutineHandle handle)
        {
            while (handle.IsRunning)
                yield return null;
        }

        public static IEnumerator AsEnum(this IEnumerator<float> en)
        {
            var h = Timing.RunCoroutine(en);
            while (h.IsRunning)
                yield return null;
        }

        public static IEnumerator<float> Sequence(params ParticleSystem[] particles)
        {
            for (var i = 0; i < particles.Length; ++i)
            {
                var ps = particles[i];
                if (!ps) yield break;
                ps.gameObject.SetActive(true);
                ps.time = 0f;
                ps.Play(true);
                
                if (i == particles.Length-1)
                    break;
                var main = ps.main;
                main.stopAction = ParticleSystemStopAction.Disable;
                while (ps && ps.IsAlive())
                    yield return 0;
            }
        }

        public static IEnumerator<float> PlayAsync(this Animation animation, string clip)
        {
            animation.Play(clip);
            do {
                yield return 0f;
            } while (animation && animation.IsPlaying(clip));
        }
    }

    public class YieldWhile : CustomYieldInstruction, INotifyCompletion
    {
        public YieldWhile(IEnumerator en)
        {
            Service.Instance.StartCoroutine(en.OnComplete(Stop));
        }

        public YieldWhile(IEnumerator<float> en)
        {
            en.Append(Stop).RunCoroutine();
        }

        public YieldWhile(CoroutineHandle handle)
        {
            handle.Append(Stop).RunCoroutine();
        }

        public YieldWhile(Coroutine coroutine)
        {
            Service.Instance.StartCoroutine(coroutine.OnComplete(Stop));
        }

        public YieldWhile(Task task)
        {
            Wait(task);
        }

        private async void Wait(Task task)
        {
            await task;
            Stop();
        }

        private void Stop()
        {
            _keepWaiting = false;
            _continuation?.Invoke();
        }

        public YieldWhile GetAwaiter()
        {
            return this;
        }

        System.Action _continuation;
        public void OnCompleted(System.Action continuation)
        {
            this._continuation = continuation;
        }

        private bool _keepWaiting = true;

        public override bool keepWaiting => _keepWaiting;

        public bool IsCompleted { get => !_keepWaiting; }

        public void GetResult()
        {
            if (!IsCompleted) throw new System.InvalidOperationException("coroutine not yet completed");
        }
    }
}