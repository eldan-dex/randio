using System;

namespace Randio_2
{
    //Timed event (executes a given action at a given time). Managed by EventManager
    class Event<T>
    {
        #region Public variables
        public DateTime fireTime;
        #endregion

        #region Private variables
        private Action<T> action;
        private T parameter;
        #endregion

        #region Public variables
        //Default ctor
        public Event(int fireTimeMiliseconds, Action<T> action, T parameter)
        {
            fireTime = DateTime.Now.AddMilliseconds(fireTimeMiliseconds);
            this.action = action;
            this.parameter = parameter;
        }

        //Executes the action
        public void FireEvent()
        {
            action(parameter);
        }
        #endregion
    }
}
