using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace gameAPI
{
    /// <summary>
    /// Actor class -> supporting minion for player
    /// </summary>
    public class Actor
    {
        public int Connections { get; set; }                //higher the number (1 to 3), the greater the likelihood of them having more active nodes on the map (probability based)
        public int Motivation { get; set; }                 //higher the better (1 to 3), Motivation to the cause
        public int Invisibility { get; set; }               //higher the better (1 to 3)
        public int SlotID { get; set; }                     //actor slot ID (eg, 0 to 3)
        public string Name { get; set; }
        public Trait trait { get; set; }

        [HideInInspector] public ActorArc arc;
        [HideInInspector] Sprite sprite;

        public Actor()
        { }


    }
}
