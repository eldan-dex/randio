using System;
using System.Collections.Generic;

namespace Randio_2
{
    //Manages events, updating and timing
    class EventManager<T>
    {
        #region Private variables
        List<Event<T>> events;
        Event<T> next;
        #endregion

        #region Public variables
        //Default ctor
        public EventManager()
        {
            events = new List<Event<T>>();
            next = null;
        }

        //Adds an event to the manager
        public void AddEvent(Event<T> action)
        {
            events.Add(action);
            PrepareNextEvent();
        }

        //Goes through all events, finds the nearest one and prepares it for execution
        public void PrepareNextEvent()
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

        //Update the manager (check times and possibly fire an event). Called in the main update loop
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
