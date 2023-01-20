using System;
using UnityEngine;
using UniRx;

namespace Studio.Framework
{
    [System.Serializable]
    public class RangedProperty<T> : ReactiveProperty<T> where T : System.IComparable
    {

        private T _min, _max;

        public ref T min => ref _min;
        public ref T max => ref _max;
        public RangedProperty(T val, T min, T max) : base(Clamp(val, min, max))
        {
            _min = min; _max = max;
        }
        public new T Value {
            get => base.Value;
            set => base.Value = Clamp(value, min, max);
        }

        private static T Clamp(T val, T min,  T max)
        {
            var x = val;
            if (x.CompareTo(min) < 0)
                x = min;
            else if (x.CompareTo(max) > 0)
                x = max;
            return x;
        }
    }
}