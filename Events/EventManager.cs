using System;
using System.Collections.Generic;

namespace Randio_2
{
    class EventManager<T>
    {
        #region Private variables
        List<Event<T>> events;
        Event<T> next;
        #endregion

        #region Public variables
        public EventManager()
        {
            events = new List<Event<T>>();
            next = null;
        }

        public void AddEvent(Event<T> action)
        {
            events.Add(action);
            PrepareNextEvent();
        }

        public void PrepareNextEvent() //find next scheuduled event
        {
            Event<T> nearestEvent = null;
            DateTime eventTime = DateTime.Now.AddYears(1); //this can cause issues if an event is set to happen more than 1 year in the future. but why would anybody want to do that?

            foreach (Event<T> e in events)
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
        #endregion
    }
}
