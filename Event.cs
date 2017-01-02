using System;

namespace Randio_2
{
    class Event<T>
    {
        public DateTime fireTime;
        Action<T> action;
        T parameter;

        public Event(int fireTimeMiliseconds, Action<T> action, T parameter)
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
