using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Studio.Framework
{
    [System.Flags]
    public enum Reorder
    {
        None = 0,
        Draggable = 1,
        Header = 2,
        Add = 4,
        Remove = 8,

        Simple = Draggable|Add|Remove,
        All = Draggable|Header|Add|Remove
    }

    public class ReorderableObject : ScriptableObject {}

    [AttributeUsage(AttributeTargets.Field)]
    public class ReorderableAttribute : PropertyAttribute
    {
        public System.Type elementType { get; set; }
        public bool draggable { get; private set; }
        public bool displayHeader { get; private set; }
        public bool displayAddButton { get; private set; }
        public bool displayRemoveButton { get; private set; }
        public float[] columns { get; private set; }

        public ReorderableAttribute(Reorder flag, float[] columns = null)
        {
            this.draggable = flag.HasFlag(Reorder.Draggable);
            this.displayHeader = flag.HasFlag(Reorder.Header);
            this.displayAddButton = flag.HasFlag(Reorder.Add);
            this.displayRemoveButton = flag.HasFlag(Reorder.Remove);
            this.columns = columns ?? new float[0];
        }
    }
}