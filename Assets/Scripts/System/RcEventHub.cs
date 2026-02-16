using System;
using System.Collections.Generic;

namespace Engine
{
    public class RcEventHub<TTrigger, TValue>
    {
        private readonly Dictionary<TTrigger, List<Action<TValue>>> events = new();

        public void Subscribe(TTrigger trigger, Action<TValue> action)
        {
            if (!events.ContainsKey(trigger))
                events[trigger] = new List<Action<TValue>>();

            events[trigger].Add(action);
        }

        public void Unsubscribe(TTrigger trigger, Action<TValue> action)
        {
            if (!events.TryGetValue(trigger, out var eventHandler)) return;
            
            eventHandler.Remove(action);
            
            if (eventHandler.Count == 0)
                events.Remove(trigger);
        }

        public void Publish(TTrigger trigger, TValue value)
        {
            if (!events.TryGetValue(trigger, out var handlers)) return;

            foreach (var handler in handlers)
                handler(value);
        }
    }
    
    public class RcEventHub<TTrigger>
    {
        private readonly Dictionary<TTrigger, List<Action>> events = new();

        public void Subscribe(TTrigger trigger, Action action)
        {
            if (!events.ContainsKey(trigger))
                events[trigger] = new List<Action>();

            events[trigger].Add(action);
        }

        public void Unsubscribe(TTrigger trigger, Action action)
        {
            if (!events.TryGetValue(trigger, out var eventHandler)) return;
            
            eventHandler.Remove(action);
            
            if (eventHandler.Count == 0)
                events.Remove(trigger);
        }

        public void Publish(TTrigger trigger)
        {
            if (!events.TryGetValue(trigger, out var handlers)) return;

            foreach (var handler in handlers)
                handler?.Invoke();
        }
    }
}