﻿using System;

namespace _3rdParty.git_amend
{
    public interface IEventBinding<T>
    {
        public Action<T> OnEvent { get; set; }
        public Action OnEventNoArgs { get; set; }
    }

    public class EventBinding<T> : IEventBinding<T> where T : IEvent
    {
        private Action<T> onEvent = _ => { };
        private Action onEventNoArgs = () => { };

        public EventBinding(Action<T> onEvent)
        {
            this.onEvent = onEvent;
        }

        public EventBinding(Action onEventNoArgs)
        {
            this.onEventNoArgs = onEventNoArgs;
        }

        Action<T> IEventBinding<T>.OnEvent
        {
            get => onEvent;
            set => onEvent = value;
        }

        Action IEventBinding<T>.OnEventNoArgs
        {
            get => onEventNoArgs;
            set => onEventNoArgs = value;
        }

        public void Add(Action onEvent)
        {
            onEventNoArgs += onEvent;
        }

        public void Remove(Action onEvent)
        {
            onEventNoArgs -= onEvent;
        }

        public void Add(Action<T> onEvent)
        {
            this.onEvent += onEvent;
        }

        public void Remove(Action<T> onEvent)
        {
            this.onEvent -= onEvent;
        }
    }
}