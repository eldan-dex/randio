﻿using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    class QuestManager
    {
        #region Public variables
        public int Count { get { return Quests.Count; } }
        public Texture2D Background;
        public List<Quest> Quests { get; private set; }
        #endregion

        #region Private variables
        Map map;
        #endregion

        #region Public methods
        public QuestManager(Map map)
        { 
            Quests = new List<Quest>();
            this.map = map;
        }

        public void AddQuest(Quest q)
        {
            if (!Quests.Contains(q))
                Quests.Add(q);
        }

        public string QuestsStatus()
        {
            string result = "";
            int longest = 0;

            //Iterate for the first time, get max length
            foreach (Quest q in Quests)
            {
                if (q.Name.Length > longest)
                    longest = q.Name.Length;
            }

            //Iterate for the second time, get names and perpare layout
            int completed = 0;
            foreach (Quest q in Quests)
            {
                result += q.Name.PadRight(longest + 2) + q.Progress + "\n";
                if (q.Completed)
                    ++completed;
            }

            if (completed > map.Player.Stats.QuestsCompleted)
                map.Player.Stats.QuestsCompleted = completed;

            if (completed == Quests.Count)
                map.ReachedExit = true;

            //Remove last \n
            return result.Substring(0, result.Length-1);
        }

        public void Update()
        {
            foreach (Quest q in Quests)
                q.CheckCompletion();
        }
        #endregion
    }
}