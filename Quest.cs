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
            KillTargets, //only set Targets variable
            FetchIitems, //set RequiredItems and DestinationPoints[0] - items need to be placed somewhere
            ReachPoint //only set DestinationPoints
        }
        public const int QuestTypeCount = 3;

        public bool Completed { get; private set; }
        public QuestType Type { get; private set; }
        public string Name { get; private set; }
        public string Progress { get; private set; }
        public string Description { get; private set; } //todo:do we need this?

        //not necessary to initialize and use all the fields
        public List<Entity> Targets { get; private set; }
        public List<Item> RequiredItems { get; private set; }
        public List<Vector2> DestinationPoints { get; private set; }

        private Map map; //for references to Player and objects
        private List<Vector2> reachedPoints;

        //todo: do we need to have the Completed parameter?
        public Quest(Map map, QuestType type, string name, string description, List<Entity> targets = null, List<Item> items = null, List<Vector2> points = null, bool completed = false)
        {
            this.map = map;
            Type = type;
            Name = name;
            Description = description;

            Targets = targets;
            RequiredItems = items;
            DestinationPoints = points;

            CheckProperInitialization(); //debug only

            //only initialize this if we need it
            if (DestinationPoints != null)
                reachedPoints = new List<Vector2>();

            Completed = completed;
        }

        void CheckProperInitialization()
        {
            if ((Type == QuestType.KillTargets && Targets.Count == 0) || (Type == QuestType.FetchIitems && (RequiredItems.Count == 0 || DestinationPoints.Count == 0)) || (Type == QuestType.ReachPoint && DestinationPoints == null))
                throw new ArgumentNullException("Cannot initialize quest: required argument is null");
        }

        public void CheckCompletion()
        {
            int percent = 0;

            if (Type == QuestType.KillTargets)
            {
                int deadCount = 0;
                //it doesn't matter how they die. If they're dead we got 'em.
                foreach (Entity e in Targets)
                {
                    if (!e.Alive)
                        ++deadCount;
                }

                percent = (int)((float)deadCount / Targets.Count * 100);    
            }

            else if (Type == QuestType.FetchIitems)
            {
                //hackish way of detecting whether items are placed where they need to be
                int finishedItems = 0;
                foreach (Item i in RequiredItems)
                {
                    //check whether the item is where it's supposed to be
                    int distance = GeometryHelper.VectorDistance(i.Position, DestinationPoints[0]);
                    if (distance <= i.Width*3 && i.IsPlaced) //don't account for held items //todo: balance distance
                        ++finishedItems;
                }

                percent = (int)((float)finishedItems / RequiredItems.Count * 100);
            }

            else if (Type == QuestType.ReachPoint)
            {
                //for each point, check whether we've reached it
                foreach (Vector2 point in DestinationPoints)
                {
                    if (map == null)
                        return;
                    int distance = GeometryHelper.VectorDistance(map.Player.Position, point); //todo: map null exception on init?

                    //We only need to reach it once for it to count towards reachedPoints
                    if (distance <= map.Player.Width*2) //todo: balance distance
                        if (!reachedPoints.Contains(point))
                            reachedPoints.Add(point);
                }

                percent = (int)((float)reachedPoints.Count / DestinationPoints.Count * 100);
            }

            //Set the display value
            if (percent == 100)
                Progress = "Done";

            else
                Progress = percent.ToString() + "%";
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