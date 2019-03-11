using System.Collections.Generic;

namespace MogglesClient
{
    internal static class MogglesContainer
    {
        private static readonly Dictionary<string, object> Container = new Dictionary<string, object>();

        internal static object Resolve<T>()
        {
            if (Container.TryGetValue(typeof(T).Name, out object value))
            {
                return value;
            }

            return null;
        }

        internal static void Register<T>(T instance)
        {
            if (!Container.ContainsKey(typeof(T).Name))
            {
                Container.Add(typeof(T).Name, instance);
            }     
        }

        internal static void RemoveRegisteredComponents()
        {
            Container.Clear();
        }
    }
}
