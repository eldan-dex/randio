using System;
using System.Collections.Generic;

namespace Randio_2
{
    class EventManager<T> {
        List<TimedEvent<T>> events;
        TimedEvent<T> next;

        public EventManager()
        {
            events = new List<TimedEvent<T>>();
            next = null;
        }

        public void AddEvent(TimedEvent<T> action) {
            events.Add(action);
            PrepareNextEvent();
        }

        public void PrepareNextEvent() //find next scheuduled event
        {
            TimedEvent<T> nearestEvent = null;
            DateTime eventTime = DateTime.Now.AddYears(1); //this can cause issues if an event is set to happen more than 1 year in the future. but why would anybody want to do that?

            foreach (TimedEvent<T> e in events)
            {
                if (e.fireTime < eventTime)
                {
                    eventTime = e.fireTime;
                    nearestEvent = e;
                }
            }

            next = nearestEvent;
        }

        public void Update()
        {
            if (next != null && next.fireTime <= DateTime.Now)
            {
                next.FireEvent();
                events.Remove(next);
                PrepareNextEvent();
            }
        }

    }

    class TimedEvent<T>
    {
        public DateTime fireTime;
        Action<T> action;
        T parameter;

        public TimedEvent(int fireTimeMiliseconds, Action<T> action, T parameter)
        {
            fireTime = DateTime.Now.AddMilliseconds(fireTimeMiliseconds);
            this.action = action;
            this.parameter = parameter;
        }

        public void FireEvent()
        {
            action(parameter);
        }
    }
}
