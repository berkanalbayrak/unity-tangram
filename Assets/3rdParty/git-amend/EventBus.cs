using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _3rdParty.git_amend
{
    public static class EventBus<T> where T : IEvent
    {
        private static readonly HashSet<IEventBinding<T>> bindings = new();

        public static void Register(EventBinding<T> binding)
        {
            bindings.Add(binding);
        }

        public static void Deregister(EventBinding<T> binding)
        {
            bindings.Remove(binding);
        }

        public static void Raise(T @event)
        {
            var bindingsCopy = bindings.ToList();

            foreach (var binding in bindingsCopy)
            {
                binding.OnEvent.Invoke(@event);
                binding.OnEventNoArgs.Invoke();
            }
        }

        private static void Clear()
        {
            Debug.Log($"Clearing {typeof(T).Name} bindings");
            bindings.Clear();
        }
    }
}