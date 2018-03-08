using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;




namespace gameAPI
{
    /// <summary>
    /// Enums -> only for use in code, anything that is in an SO requires an SO enum approach
    /// </summary>

    //
    // - - - Game Manager - - -
    //
    //public enum Side { None, Authority, Resistance, Count }                                   //Sides available in game
    public enum GameState { Normal, ModalOutcome, ModalActionMenu, ModalPicker, ModalDice }
    public enum ResistanceState { Normal, Captured }                                    //player as Resistance
    public enum AuthorityState { Normal }                                               //player as Authority
    public enum MetaLevel { None, City, State, Nation }
    public enum AuthorityActor { Superintendent, Minister, Secretary }              //different names for authority actors depending on MetaLevel

    //
    // - - - Level Manager
    //
    public enum ConnectionType { None, HIGH, MEDIUM, LOW, Count }

    //
    // - - - Node Manager - - 
    // 
    public enum NodeInfo { Number, TargetsAll, TargetsLive, TargetsActive, Count }    //DataManager arrayOfNodes index
    public enum NodeType { Normal, Highlight, Active, Player, Count }
    public enum NodeUI
    {                                                            //parameter for NodeDisplay event
        None,
        Reset, Redraw, Move,
        ShowTargets, ShowSpiders, ShowTracers, ShowTeams,
        NodeArc0, NodeArc1, NodeArc2, NodeArc3, NodeArc4, NodeArc5, NodeArc6, NodeArc7, NodeArc8, NodeArc9
    };

    //
    // - - - Decisions and Targets - - -
    //
    public enum Status { Dormant, Active, Live, Completed, Contained }
    public enum Activation { Low, Medium, High }                                     //chance of becoming 'Live' once 'Active'

    //
    // - - - Actors - - -
    //
    public enum ActorStatus { Active, Inactive, Reserve, Pool, Captured, Dismissed, Promoted, Killed }

    //
    // - - - Action Menu - - -
    //
    public enum ActionMenuType { None, Node, Actor, Gear, Move}

    //
    // - - - Dice - - -
    //
    public enum DiceOutcome { None, Ignore, Auto, Roll }
    public enum DiceType { None, Move, Gear}                                //reason dice is being rolled

    //
    // - - - Messages - - -
    //
    public enum MessageCategory { None, Current, Pending, Archive }
    public enum MessageType { None, PLAYER, TEAM, AI, GEAR, ACTOR, TARGET, EFFECT }
    public enum MessageSubType
    {
        None,
        //Player
        Plyr_Move,
        Plyr_Renown,
        //Team
        Team_Deploy,
        Team_Withdraw,
        Team_AutoRecall,
        Team_Neutralise,
        Team_Effect,
        //AI
        AI_SpotMove,
        AI_Capture,
        AI_Release,
        //Gear
        Gear_Comprised,
        Gear_Used,
        Gear_Obtained,
        Gear_Given,
        //Actor
        Actor_Recruited,
        Actor_Action,
        Actor_Status,
        Actor_Condition,
        //Target
        Target_Attempt,
        Target_Contained,
        //Effect
        Ongoing_Created,
        Ongoing_Expired

    }

    //
    // - - - Teams - - -
    //
    public enum TeamInfo { Total, Reserve, OnMap, InTransit, Count }           //DataManager arrayOfTeams  index
    public enum TeamPool { Reserve, OnMap, InTransit, Count }                  //Different pools that teams move between
    public enum NATO
    {                                                        //used for sequentially naming teams, eg. 'Control Team Bravo'
        Alpha, Bravo, Charlie, Delta, Echo, Foxtrot, Golf, Hotel, India, Juliett, Kilo, Lima, Mike,
        November, Oscar, Papa, Quebec, Romeo, Sierra, Tango, Uniform, Victor, Whiskey, Xray, Yankee, Zulu,
        Count
    }



}
