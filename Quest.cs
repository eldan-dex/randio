using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    class Quest
    {
        public enum QuestType
        {
            KillTargets,
            FetchIitems,
            ReachPoint
        }

        public bool Completed { get; private set; }
        public QuestType Type { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; } //todo:do we need this?

        //not necessary to initialize and use all the fields
        public List<Entity> Targets { get; private set; }
        public List<Item> RequiredItems { get; private set; }
        public Vector2 PointToReach { get; private set; }

        //todo: do we need to have the Completed parameter?
        public Quest(QuestType type, string name, string description, List<Entity> targets = null, List<Item> required = null, Vector2 point = default(Vector2), bool completed = false)
        {
            Type = type;
            Name = name;
            Description = description;

            Targets = targets;
            RequiredItems = required;
            PointToReach = point;

            Completed = completed;
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