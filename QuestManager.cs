using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    class QuestManager
    {
        List<Quest> quests;
        Map map;

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
            foreach (Quest q in quests)
            {
                result += q.Name.PadRight(longest + 2) + q.Progress + "\n";
            }

            //Remove last \n
            return result.Substring(0, result.Length-1);
        }

        public void Update()
        {
            foreach (Quest q in quests)
                q.CheckCompletion();
        }

    }
}

/*
1. hrac musi presne vedet co se po nem chce
2. TYP - zabit oznacena NPCKA
3. TYP - najit a donest item
4. TYP - dostat se nekam nahoru?
5. TYP - zavod?
*/