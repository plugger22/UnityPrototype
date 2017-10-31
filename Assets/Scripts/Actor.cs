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
        public int Datapoint0 { get; set; }                //higher the number (1 to 3), see DM: arrayOfQualities for string tags
        public int Datapoint1 { get; set; }                 //higher the better (1 to 3)
        public int Datapoint2 { get; set; }               //higher the better (1 to 3)

        public int SlotID { get; set; }                     //actor slot ID (eg, 0 to 3)
        public int Renown { get; set; }                     //starts at '0' and goes up (no limit)
        public string Name { get; set; }
        public Trait trait { get; set; }
        public bool isLive { get; set; }                    //actor can 'go silent' and be unavailable on occasion

        [HideInInspector] public ActorArc arc;

        public Actor()
        { }


    }
}
