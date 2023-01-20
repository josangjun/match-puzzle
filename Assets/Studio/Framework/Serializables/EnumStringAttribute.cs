using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Studio.Framework
{
    public enum EnumStringOption { Sub, Add }
    public class EnumStringAttribute : PropertyAttribute
    {
        public System.Type enumType { get; private set; }

        public EnumStringAttribute(System.Type type)
        {
            Assert.IsTrue(type.IsEnum);
            enumType = type;
        }

        

        public EnumStringAttribute(System.Type type, params string[] args)
            :this(type, EnumStringOption.Sub, args)
        {

        }

        public EnumStringAttribute(System.Type type, EnumStringOption opt, params string[] args)
        {
            Assert.IsTrue(type.IsEnum);
            enumType = type;
            if (opt == EnumStringOption.Sub)
                this.filters = args;
            else this.extras = args;
        }

        public bool hasFilter
        {
            get { return filters != null && filters.Length > 0; }
        }

        public string[] filters { get; } = new string[] {};
        public string[] extras { get; } = new string[] {};
    }
}