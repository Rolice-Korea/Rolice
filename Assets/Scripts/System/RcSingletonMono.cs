using System;
using UnityEngine;

namespace Engine
{
    public interface ISingletonMonoInterface
    {
        public Type GetSingletonType();
        public void InitializeSingleton();
        public void ReleaseSingleton();
    }

    public abstract class RcSingletonMono<T> : MonoBehaviour, ISingletonMonoInterface where T : RcSingletonMono<T>
    {
        private static T instance;

        private static bool HasInstance => instance != null;

        public static T Instance
        {
            get
            {
                if (HasInstance) return instance;

                instance = FindAnyObjectByType<T>();

                if (HasInstance) return instance;

                var go = new GameObject
                {
                    name = typeof(T).Name + " Auto-Generated",
                    hideFlags = HideFlags.DontSave
                };

                instance = go.AddComponent<T>();

                return instance;
            }
        }

        public Type GetSingletonType()
        {
            return typeof(T);
        }

        public virtual void InitializeSingleton()
        {
            if (!Application.isPlaying) return;

            // 이미 등록된 인스턴스가 있으면 자신을 파괴
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this as T;
        }

        public virtual void ReleaseSingleton()
        {
        }
    }
}
