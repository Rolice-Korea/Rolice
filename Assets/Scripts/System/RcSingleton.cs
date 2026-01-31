using System;

namespace Engine
{
    public abstract class RcSingleton<T> where T : class, new()
    {
        private static readonly Lazy<T> StaticIns = new Lazy<T>(() =>
        {
            var inst = new T();

            return inst;
        });

        public static T Instance => StaticIns.Value;
    }
}