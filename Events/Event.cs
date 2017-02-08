using System;

namespace Randio_2
{
    class Event<T>
    {
        #region Public variables
        public DateTime fireTime;
        #endregion

        #region Private variables
        Action<T> action;
        T parameter;
        #endregion

        #region Public variables
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
        #endregion
    }
}
