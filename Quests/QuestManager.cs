using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    class QuestManager
    {
        #region Public variables
        public int Count { get { return quests.Count; } }
        public Texture2D Background;
        #endregion

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
            int completed = 0;
            foreach (Quest q in quests)
            {
                result += q.Name.PadRight(longest + 2) + q.Progress + "\n";
                if (q.Completed)
                    ++completed;
            }

            if (completed > map.Player.Stats.QuestsCompleted)
                map.Player.Stats.QuestsCompleted = completed;

            if (completed == quests.Count)
                map.ReachedExit = true;

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