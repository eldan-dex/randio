using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    class QuestManager
    {
        List<Quest> quests;

        public QuestManager()
        {
            quests = new List<Quest>();
        }

        //todo: allow some parameters?
        public void AddRandomQuest()
        {
            Quest quest;
            Quest.QuestType type = (Quest.QuestType)AlgorithmHelper.GetRandom(0, Quest.QuestTypeCount);

            //set quest parameters based on type
            //or leave this to a GenerateRandomQuest method in Quest.cs?

            quest = new Quest(type, "Testovaci", "Vitut. Vitut. Vitut. Vitut.");


            quests.Add(quest);
        }

        public void CreateRandomQuestSet()
        {
            int count = AlgorithmHelper.GetRandom(1, 5);
            for (int i = 0; i < count; ++i)
                AddRandomQuest();
        }

        public string GetQuestStatus()
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
                result += q.Name.PadRight(longest + 2) + (q.Completed ? "X" : "O") + "\n";
            }

            //Remove last \n
            return result.Substring(0, result.Length-1);
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