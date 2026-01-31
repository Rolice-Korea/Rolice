using System;
using System.Collections.Generic;

namespace Engine
{
    public class RcEventHub<TTrigger, TValue>
    {
        private readonly Dictionary<TTrigger, List<Action<TValue>>> _events = new();

        public void Subscribe(TTrigger trigger, Action<TValue> action)
        {
            if (!_events.ContainsKey(trigger))
                _events[trigger] = new List<Action<TValue>>();

            _events[trigger].Add(action);
        }

        public void Unsubscribe(TTrigger trigger, Action<TValue> action)
        {
            if (!_events.TryGetValue(trigger, out var eventHandler)) return;
            
            eventHandler.Remove(action);
            
            if (eventHandler.Count == 0)
                _events.Remove(trigger);
        }

        public void Publish(TTrigger trigger, TValue value)
        {
            if (!_events.TryGetValue(trigger, out var handlers)) return;

            foreach (var handler in handlers)
                handler(value);
        }
    }
    
    public class EventHub<TTrigger>
    {
        private readonly Dictionary<TTrigger, List<Action>> _events = new();

        public void Subscribe(TTrigger trigger, Action action)
        {
            if (!_events.ContainsKey(trigger))
                _events[trigger] = new List<Action>();

            _events[trigger].Add(action);
        }

        public void Unsubscribe(TTrigger trigger, Action action)
        {
            if (!_events.TryGetValue(trigger, out var eventHandler)) return;
            
            eventHandler.Remove(action);
            
            if (eventHandler.Count == 0)
                _events.Remove(trigger);
        }

        public void Publish(TTrigger trigger)
        {
            if (!_events.TryGetValue(trigger, out var handlers)) return;

            foreach (var handler in handlers)
                handler?.Invoke();
        }
    }
}