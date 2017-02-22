using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Randio_2
{
    class Quest
    {
        #region Public variables
        public enum QuestType
        {
            KillTargets, //only set Targets variable
            FetchIitems, //set RequiredItems and DestinationBlocks[0] - items need to be placed somewhere
            ReachBlock //only set DestinationBlocks
        }
        public const int QuestTypeCount = 3;

        public bool Completed { get; private set; }
        public QuestType Type { get; private set; }
        public string Name { get; private set; }
        public string BaseName { get; private set; }
        public string Progress { get; private set; }

        //not necessary to initialize and use all the fields
        public List<Entity> Targets { get; private set; }
        public List<Item> RequiredItems { get; private set; }
        public List<Zone> DestinationBlocks { get; private set; }
        public int TargetTileIndex { get; private set; }
        #endregion

        #region Private variables
        private Map map; //for references to Player and objects
        private List<Rectangle> reachedBlocks;
        #endregion

        #region Public methods
        public Quest(Map map, QuestType type, string name, List<Entity> targets = null, List<Item> items = null, List<Zone> blocks = null, bool completed = false)
        {
            this.map = map;
            Type = type;
            Name = name;
            BaseName = name;

            Targets = targets;
            RequiredItems = items;
            DestinationBlocks = blocks;

            //only initialize this if we need it
            if (DestinationBlocks != null)
                reachedBlocks = new List<Rectangle>();

            Completed = completed;
        }

        public void CheckCompletion()
        {
            int percent = 0;

            if (Type == QuestType.KillTargets)
            {
                int deadCount = 0;
                //it doesn't matter how they die. If they're dead we got 'em.
                Name = BaseName;
                foreach (Entity e in Targets)
                {
                    if (!e.Alive)
                        ++deadCount;

                    Name += e.Name + (!e.Alive ? "*" : "") + ", ";
                }
                Name = Name.Remove(Name.Length - 2);

                percent = (int)((float)deadCount / Targets.Count * 100);    
            }

            else if (Type == QuestType.FetchIitems)
            {
                //hackish way of detecting whether items are placed where they need to be
                int finishedItems = 0;
                Name = BaseName;

                foreach (Item i in RequiredItems)
                {
                    //check whether the item is where it's supposed to be
                    var dest = DestinationBlocks[0].Coords;
                    var pos = i.Position;
                    bool ok = false;

                    if (i.IsPlaced && pos.X >= dest.Left && pos.Y >= dest.Top && pos.X + i.Width <= dest.Right && pos.Y + i.Height <= dest.Bottom)
                    {
                        ++finishedItems;
                        ok = true;
                    }


                    Name += i.Properties.Name + (ok ? "*" : "") + ", ";
                }
                Name = Name.Remove(Name.Length - 2);
                Name = (Name + " to a green area in tile " + map.GetTileForX(DestinationBlocks[0].Coords.X).Index.ToString());

                percent = (int)((float)finishedItems / RequiredItems.Count * 100);
            }

            else if (Type == QuestType.ReachBlock)
            {
                Name = BaseName;
                //for each point, check whether we've reached it
                foreach (Zone zone in DestinationBlocks)
                { 
                    bool ok = false;

                    var block = zone.Coords;
                    var newPlayerRect = GeometryHelper.TileToGlobalCoordinates(map.Player.BoundingRectangle, map.GetTileByIndex(map.Player.CurrentTile));
                    if (GeometryHelper.GetIntersectionDepth(block, newPlayerRect) != Vector2.Zero)
                        if (!reachedBlocks.Contains(block)) //We only need to reach it once for it to count towards reachedPoints
                        {
                            reachedBlocks.Add(block);
                            zone.Deactivate();
                        }

                    if (reachedBlocks.Contains(zone.Coords))
                        ok = true;

                    Name += map.GetTileForX(zone.Coords.X).Index + (ok ? "*" : "") + ", ";
                }
                Name = Name.Remove(Name.Length - 2);

                percent = (int)((float)reachedBlocks.Count / DestinationBlocks.Count * 100);
            }

            //Set the display value
            if (percent == 100)
            {
                Progress = "Done";
                Completed = true;
            }

            else
                Progress = percent.ToString() + "%";
        }
        #endregion
    }
}

/*
1. hrac musi presne vedet co se po nem chce
2. TYP - zabit oznacena NPCKA
3. TYP - najit a donest item
4. TYP - dostat se nekam nahoru?
5. TYP - zavod?
*/