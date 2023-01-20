using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Studio.Framework
{
    public class PopupAttribute : PropertyAttribute, IEnumerable<string>, IEnumerable
    {
        private string[] _items;

        public System.Type type { get; }

        public int Count => Items.Length;

        public string[] Items { get => _items; set => _items = value; }

        public string this[int index] => Items[index];
        
        public int IndexOf(string val)
        {
            return Array.IndexOf(_items, val);
        }
        
        public PopupAttribute(params string[] items)
        {
            this.type = type;
            this.Items = items;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>)Items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)Items).GetEnumerator();
        }
    }
}