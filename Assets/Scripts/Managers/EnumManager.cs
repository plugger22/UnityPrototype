using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gameAPI
{
    //
    // - - - Game Manager - - -
    //
    public enum Side { None, Authority, Resistance, Count }                                   //Sides available in game
    public enum GameState { Normal, ModalOutcome, ModalActionMenu, ModalPicker, ModalDice }
    public enum ResistanceState { Normal, Captured }                                    //player as Resistance
    public enum AuthorityState { Normal }                                               //player as Authority
    public enum MetaLevel { None, City, State, Nation }
    public enum AuthorityActor { None, Superintendent, Minister, Secretary}              //different names for authority actors depending on MetaLevel
    
    //
    // - - - Level Manager
    //
    public enum ConnectionType { None, HIGH, MEDIUM, LOW, Count }

    //
    // - - - Node Manager - - 
    // 
    public enum NodeInfo { Number, TargetsAll, TargetsLive, TargetsActive, Count}    //DataManager arrayOfNodes index
    public enum NodeType { Normal, Highlight, Active, Player, Count }
    public enum NodeUI {                                                            //parameter for NodeDisplay event
        None,
        Reset, Redraw, Move,
        ShowTargets, ShowSpiders, ShowTracers, ShowTeams,
        NodeArc0, NodeArc1, NodeArc2, NodeArc3, NodeArc4, NodeArc5, NodeArc6, NodeArc7, NodeArc8, NodeArc9
        };             

    //
    // - - - Decisions and Targets - - -
    //
    public enum Status {Dormant, Active, Live, Completed, Contained }
    public enum Activation { Low, Medium, High}                                     //chance of becoming 'Live' once 'Active'

    //
    // - - - Actors - - -
    //
    public enum TraitType { Good, Neutral, Bad}
    public enum ActorStatus { Active, Inactive, Reserve, Pool, Captured}

    //
    // - - - Action Manager - - -
    //
    public enum Comparison { None, LessThan, GreaterThan, EqualTo }
    //public enum Result { None, Add, Subtract }
    public enum ActionType { None, Node, NeutraliseTeam, Gear, Recruit}

    //
    // - - - Gear - - -
    //
    public enum GearLevel { Common, Rare, Unique, Count}
    public enum GearType { None, Infiltration, Kinetic, Hacking, Stealth, Invisibility, Movement, Recovery}

    //
    // - - - Dice - - -
    //
    public enum DiceOutcome { None, Ignore, Auto, Roll}

    //
    // - - - Messages - - -
    //
    public enum MessageCategory { None, Current, Pending, Archive}
    public enum MessageType { None, PLAYER, TEAM, AI, GEAR, ACTOR, TARGET}
    public enum MessageSubType {
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
        //Actor
        Actor_Recruited,
        Actor_Action,
        //Target
        Target_Attempt,
        Target_Contained

    }

    //
    // - - - Effects - - -       
    //

    //* * * WARNING: changing / deleting entries can mess up the Effect SO's * * * 

    public enum EffectTypeEnum { None, Good, Neutral, Bad}
    public enum EffectDurationEnum { Single, Ongoing}
    public enum EffectCategory { Normal, AuthorityTeam, Target }
    /*public enum EffectApplyEnum {
        None,
        //Node
        NodeCurrent,
        NodeNeighbours,
        NodeSameArc,
        NodeAll,
        //Connections
        ConnectionCurrent,
        ConnectionNeighbours,
        ConnectionSameArc,
        ConnectionAll
    }*/
    public enum EffectCriteria {
        None,
        //resistance
        NodeStability, NodeSecurity, NodeSupport,
        NumRecruits, NumTeams, NumTracers, NumGear, GearAvailability,
        TargetInfo, TargetPresent,
        RebelCause,
        //authority
        ActorAbility, TeamIdentical, TeamPreferred, TeamAny
    }     
    /*public enum EffectOutcomeEnum {
        None,
        //resistance
        Stability, Security, Support,
        Recruit, NeutraliseTeam, Tracer, Gear, TargetInfo, SpreadInstability,
        RebelCause,
        Invisibility,
        //both
        Renown,
        //authority
        AnyTeam, CivilTeam, DamageTeam, ProbeTeam, MediaTeam, ControlTeam, ErasureTeam, SpiderTeam,
        //assorted
        RevealSpiders, RevealTeams, RevealTracers, RevealActors,
        ConnectionSecurity
    }*/

    //
    // - - - Teams - - -
    //
    //public enum TeamType { None, Civil, Damage, Probe, Media, Control, Erasure, Spider} -> not needed
    public enum TeamInfo { Total, Reserve, OnMap, InTransit, Count}           //DataManager arrayOfTeams  index
    public enum TeamPool { Reserve, OnMap, InTransit, Count}                  //Different pools that teams move between
    public enum NATO {                                                        //used for sequentially naming teams, eg. 'Control Team Bravo'
        Alpha, Bravo, Charlie, Delta, Echo, Foxtrot, Golf, Hotel, India, Juliett, Kilo, Lima, Mike,
        November, Oscar, Papa, Quebec, Romeo, Sierra, Tango, Uniform, Victor, Whiskey, Xray, Yankee, Zulu,
        Count }



}
