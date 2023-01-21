using UnityEngine;
using System.Collections.Generic;
using Studio.Framework;
using System.Collections;
using System.Linq;

namespace Studio.Game
{
    using KeyType = System.Int32;

    public enum JobPriority
    {
        
    }

    public class JobInstruction : IEnumerator
    {
        public KeyType priority;
        private System.Func<IEnumerator> func;
        private IEnumerator enumerator = null;
        public bool persistent = true;
        
        public JobInstruction(KeyType priority, System.Func<IEnumerator> job, bool persistent)
        {
            Init(priority, job, persistent);
        }

        public void Init(KeyType priority, System.Func<IEnumerator> job, bool persistent)
        {
            this.priority = priority;
            this.func = job;
            this.enumerator = null;
            this.persistent = persistent;
        }

        public bool isEmpty => func == null;

        public object Current => enumerator == null ? null : enumerator.Current;

        public void Reset()
        {
            enumerator = null;
            if (!persistent)
                func = null;
        }

        public override string ToString()
        {
            return $"[JobInstruction]{(JobPriority)priority}";
        }

        public bool MoveNext()
        {
            if (enumerator == null)
            {
                if (func == null)
                    return false;
                enumerator = func.Invoke();
                if (enumerator == null)
                    return false;
            }

            try
            {
                return enumerator.MoveNext();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message + "\n" + e.StackTrace);
                return false;
            }
        }
    }

    internal class DescendingOrder : IComparer<KeyType>
    {
        public int Compare(KeyType x, KeyType y)
        {
            return y.CompareTo(x);
        }
    }

    public class BatchJob
    {
        private SortedList<KeyType, JobInstruction> dict = new SortedList<KeyType, JobInstruction>(new DescendingOrder());

        public BatchJob() {} 

        public void Add(JobPriority priority, System.Func<IEnumerator> func, bool persistent = false)
        {
            Add((KeyType)priority, func, persistent);
        }

        public void Add(KeyType key, System.Func<IEnumerator> func, bool persistent)
        {
            dict.TryGetValue(key, out var job);

            if (IsProcessing)
                MakeDirty();
            
            if (job == null)
            {
                dict[key] = new JobInstruction(key, func, persistent);
            }
            else
            {
                job.Init(key, func, persistent);
            }
        }

        [System.NonSerialized]
        private List<KeyType> ignoredJobs;

        public void SetIgnoredJobs(params string[] jobs)
        {
            ignoredJobs = jobs.Select(arg =>
                {
                    System.Enum.TryParse<JobPriority>(arg, out var en);
                    return (KeyType)en;
                }).ToList();
        }

        public BatchJob Add(JobInstruction instruction)
        {
            dict[instruction.priority] = instruction;
            if (IsProcessing)
                MakeDirty();
            return this;
        }

        public int Count => dict.Count;

        public bool IsProcessing { get; private set; }

        public KeyType current { get; private set; }

        public System.Func<bool> CanMoveNext;

        private bool noFurtherProcessing;

        public bool WillStopProcessing() => noFurtherProcessing;

        public void StopProcessing(bool enabled)
        {
            noFurtherProcessing = enabled;
        }

        private bool isDirty = false;

        public void MakeDirty()
        {
            isDirty = true;
        }

        public IEnumerator Process()
        {
            if (IsProcessing)
                yield break;

            isDirty = false;
            IsProcessing = true;
            using var scope = new ScopeExit(() =>
            {
                IsProcessing = false;
            });

            for (var i = dict.Count - 1; i >= 0; --i)
            {
                var instruction = dict.Values[i];
                if (instruction == null || instruction.isEmpty)
                    continue;
                #if UNITY_EDITOR
                if (ignoredJobs?.Contains(instruction.priority) ?? false)
                    continue;
                #endif
                current = dict.Keys[i];
                yield return instruction;
                instruction.Reset();
                if (!instruction.persistent)
                {
                    var index = dict.IndexOfKey(instruction.priority);
                    if (index < 0)
                        continue;
                    dict.RemoveAt(index);
                    i = index;
                    isDirty = false;
                }
                else if (isDirty)
                {
                    var index = dict.IndexOfKey(instruction.priority);
                    if (index >= 0)
                        i = index;
                    isDirty = false;
                }
                if (noFurtherProcessing)
                {
                    noFurtherProcessing = false;
                    break;
                }
                while (CanMoveNext != null && !CanMoveNext.Invoke())
                    yield return null;
            }
            current = int.MinValue;
        }

        public void RemoveAt(int index)
        {
            dict.RemoveAt(index);
            if (IsProcessing)
                MakeDirty();
        }

        public bool Remove(JobPriority priority) => Remove((KeyType)priority);

        public bool Remove(KeyType key)
        {
            if (IsProcessing)
                MakeDirty();
            return dict.Remove(key);
        }

        public void Clear()
        {
            dict.Clear();
            IsProcessing = false;
            isDirty = false;
            CanMoveNext = null;
            noFurtherProcessing = false;
            current = int.MinValue;
        }

        public IList<KeyType> Keys => dict.Keys;

        public IList<JobInstruction> Values => dict.Values;
    }
}