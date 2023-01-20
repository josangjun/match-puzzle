
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Studio.Framework.Pool
{
	/*
    public class MessageCreator : ISealedCreator<ProtoBuf.IExtensible>
    {
        T ISealedCreator<ProtoBuf.IExtensible>.New<T>()
        {
            return System.Activator.CreateInstance<T>();
        }

        private readonly IDictionary<System.Type, IList<PropertyInfo>> _protoCache =
            new Dictionary<System.Type, IList<PropertyInfo>>();

        private IEnumerable<PropertyInfo> GetProperties(System.Type type)
        {
            var props = type.GetProperties(
				BindingFlags.Public|
				BindingFlags.Instance);
            IList<PropertyInfo> protoMembers;
            if (!_protoCache.TryGetValue(type, out protoMembers) || protoMembers == null)
            {    
                protoMembers = new List<PropertyInfo>();
                _protoCache[type] = protoMembers;
                foreach (var prop in props)
                {
                    if (prop.IsDefined(typeof(ProtoBuf.ProtoMemberAttribute)))
                        protoMembers.Add(prop);
                }
            }
			return protoMembers;

        }

        public void Reset(ProtoBuf.IExtensible target)
        {
			var type = target.GetType();
			var props = GetProperties(type);
			foreach (var prop in props)
			{
				var propType = prop.PropertyType;
				if (propType == typeof(bool))
				{
					prop.SetValue(target, false);
				}
				else if (propType.IsPrimitive)
				{
					prop.SetValue(target, 0);
				}
				else if (propType.IsEnum)
				{
					prop.SetValue(target, 0);
				}
				else if (propType == typeof(string))
				{
					prop.SetValue(target, string.Empty);
				}
				else if (typeof(IList).IsAssignableFrom(propType))
				{
					var list = prop.GetValue(target) as IList;
					if (list != null) list.Clear();
				}
				else if (typeof(ProtoBuf.IExtensible).IsAssignableFrom(propType))
				{
					var subMsg = prop.GetValue(target) as ProtoBuf.IExtensible;
					if (subMsg != null)
						Reset(subMsg);
				}
				else
				{
					Debug.LogWarning("[failed to re-initialize] property type:" + prop.PropertyType);
				}
            }
        }
    }
	*/
}