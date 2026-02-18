using UnityEngine;
using System.Collections.Generic;

namespace Rolice.Particle
{
    public class RcParticleEffectFactory
    {
        private Dictionary<string, Queue<RcParticleEffect>> pool = 
            new Dictionary<string, Queue<RcParticleEffect>>();

        public RcParticleEffect Get(string key, GameObject prefab)
        {
            if (!pool.ContainsKey(key))
                pool[key] = new Queue<RcParticleEffect>();

            RcParticleEffect effect;

            if (pool[key].Count > 0)
            {
                effect = pool[key].Dequeue();
                effect.gameObject.SetActive(true);
            }
            else
            {
                effect = Object.Instantiate(prefab).GetComponent<RcParticleEffect>();
                if (effect == null)
                    Debug.LogError($"[RcParticleEffectFactory] {prefab.name}에 RcParticleEffect 컴포넌트가 없습니다");
            }

            return effect;
        }

        public void Return(string key, RcParticleEffect effect)
        {
            if (effect == null) return;

            effect.Stop();
            effect.ClearEvents();
            effect.gameObject.SetActive(false);

            if (!pool.ContainsKey(key))
                pool[key] = new Queue<RcParticleEffect>();

            pool[key].Enqueue(effect);
        }

        public void Clear()
        {
            foreach (var queue in pool.Values)
            {
                while (queue.Count > 0)
                {
                    var effect = queue.Dequeue();
                    if (effect != null)
                        Object.Destroy(effect.gameObject);
                }
            }
            pool.Clear();
        }
    }
}
