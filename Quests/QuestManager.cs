using System.Collections.Generic;

namespace Randio_2
{
    class QuestManager
    {
        #region Private variables
        List<Quest> quests;
        Map map;
        #endregion

        #region Public methods
        public QuestManager(Map map)
        { 
            quests = new List<Quest>();
            this.map = map;
        }

        public void AddQuest(Quest q)
        {
            if (!quests.Contains(q))
                quests.Add(q);
        }

        public string QuestsStatus()
        {
            string result = "";
            int longest = 0;

            //Iterate for the first time, get max length
            foreach (Quest q in quests)
            {
                if (q.Name.Length > longest)
                    longest = q.Name.Length;
            }

            //Iterate for the second time, get names and perpare layout
            bool allCompleted = false;

            int completed = 0;
            foreach (Quest q in quests)
            {
                result += q.Name.PadRight(longest + 2) + q.Progress + "\n";
                allCompleted = q.Completed;
                if (q.Completed)
                    ++completed;
            }

            map.Player.Stats.QuestsCompleted = completed;

            if (allCompleted)
                result += "All quests are completed, press G to win.\n";

            //Remove last \n
            return result.Substring(0, result.Length-1);
        }

        public void Update()
        {
            foreach (Quest q in quests)
                q.CheckCompletion();
        }
        #endregion
    }
}