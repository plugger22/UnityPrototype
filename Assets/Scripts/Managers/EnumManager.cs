﻿namespace gameAPI
{
    /// <summary>
    /// Enums -> only for use in code, anything that is in an SO requires an SO enum approach
    /// </summary>

    //
    // - - - Game Manager - - -
    //
    public enum GameState {
        None, MainMenu, StartUp, Options, NewGame, NewGameOptions, NewInitialisation, FollowOnInitialisation, NewCampaign, SaveGame, LoadGame, LoadAtStart,
        PlayGame, MetaGame, ExitLevel, ExitCampaign, ExitGame}  //overall game state
    public enum WinState { None, Authority, Resistance }                                                                //none indicates nobody has yet won level
    public enum WinReason { None, CityLoyaltyMin, CityLoyaltyMax, FactionSupportMin, DoomTimerMin, MissionTimerMin, ObjectivesCompleted } //reason for Win State (from POV of winner)
    /*public enum ResistanceState { Normal }                                                                              //specific Resistance states (Player or AI)*/
    public enum AuthoritySecurityState { Normal, APB, SecurityAlert, SurveillanceCrackdown }                            //specific Authority Security states (Player or AI)   
    public enum SideState { None, AI, Human }                                                                           //who's running the side
    public enum MetaLevel { None, City, State, Nation }
    public enum AuthorityTitle { Superintendent, Minister, Secretary }                                                  //different names for authority actors depending on MetaLevel

    //
    //  - - - UI - - -
    //
    public enum ModalState { Normal, ModalUI}                                                                            //main modal state
    public enum ModalSubState { None, Outcome, GenericPicker, ActionMenu, MainMenu, Inventory, TeamPicker, DiceRoller, InfoDisplay, ShowMe }          //ModalUI sub game states
    public enum ModalInfoSubState { None, CityInfo, AIInfo, MainInfo }                                                   //if ModalUI.InfoDisplay -> what type of info
    public enum ModalGenericPickerSubState { None, Normal, CompromisedGear }                                                     //if ModalUI.GenericPicker -> what type of picker
    public enum InventoryState { None, Gear, ReservePool}
    public enum AlignHorizontal { None, Left, Centre, Right }
    public enum Background { None, Start, NewGame, NewGameOptions, SaveGame, LoadGame, Options, EndLevel, MetaGame, NewCampaign, EndCampaign }             //full screen backgrounds
    public enum ActionMenuType { None, Node, NodeGear, Gear, Actor, Player, Move, Reserve }
    public enum DebugRegister { None, Ongoing, Actions }
    public enum MsgPipelineType { None, CompromisedGear, Nemesis, ReleasePlayer, WinLose }            //start of turn message pipeline (used for determining order messages are displayed ->  shown in enum order)
    //public enum MainInfoTab { Main, HQ, People, Random, Summary, Help};                       //tabs for RHS of MainInfoUI. Order important (ties in with array indexes)
    public enum AlertType {
        None,
        MainMenuUnavailable,
        SomethingWrong,
        PlayerStatus, SideStatus, ActorStatus,
        DebugAI, DebugPlayer,
        HackingRebootInProgress, HackingInsufficientRenown, HackingInitialising, HackingIndisposed, HackingOffline
    }

   
    //
    // - - - Connections - - - 
    //
    public enum ConnectionType { None, HIGH, MEDIUM, LOW, Active, Count }   //NOTE: keep first four entries as is (none -> Low)

    //
    // - - - Nodes - - -
    // 
    public enum NodeInfo { Number, TargetsAll, TargetsLive, TargetsActive, Count }          //DataManager arrayOfNodes index
    public enum NodeArcTally { Current, Minimum, Count };                                   //used for indexing of arrayOfNodeArcTotals
    public enum NodeType { Normal, Highlight, Active, Player, Nemesis, Count }
    public enum NodeData { Stability, Support, Security, Target, Probe, Spider, Erasure};    //NOTE: keep first 3 in order as they are used in tooltipNode.cs -> GetStatColour (matches node tooltip stat display seq)
    /*public enum NodeDijkstra { Path, UnWeighted, Weighted, Count};   //array index for dictOfDijkstra Values*/
    public enum NodeUI
    {                                                            //parameter for NodeDisplay event
        None,
        Reset, Redraw, Move,
        ShowTargets, ShowSpiders, ShowTracers, ShowTeams,
        NodeArc0, NodeArc1, NodeArc2, NodeArc3, NodeArc4, NodeArc5, NodeArc6,
        MostConnected, Centre, NearNeighbours, DecisionNodes, CrisisNodes, LoiterNodes, CureNodes
    };

    //
    // - - - Activity - - -
    //
    public enum ActivityUI { None, Time, Count}

    //
    // - - - Decisions and Targets - - -
    //
    public enum Status { Dormant, Active, Live, Outstanding, Done }
    public enum Activation { Low, Medium, High }                                     //chance of becoming 'Live' once 'Active'

    //
    // - - - Actors - - -
    //
    public enum ActorStatus { Active, Inactive, Reserve, RecruitPool, Captured, Dismissed, Promoted, Killed, Resigned }
    public enum ActorInactive { None, LieLow, Breakdown, StressLeave}                             //reason actor is inactive
    public enum ActorList { None, Reserve, Promoted, Dismissed, Disposed}           //used as a parameter only to access lists (DataManager.cs -> GetActorList)
    public enum ActorTooltip { None, Breakdown, LieLow, Captured, Leave}                        //actor sprite shows a relevant tooltip if other than 'None'
    public enum ActorDebugData { None, Pools, Lists, Dict }                             //used for toggling debugGUI.cs  AI data
    public enum ActorDatapoint { Datapoint0, Influence0, Connections0, Datapoint1, Motivation1, Ability2, Invisibility2, Datapoint2}    //interchangeable. Use whichever one is appropriate

    //
    // - - - Gear - - -
    //
    public enum GearRemoved { Lost, Taken, Compromised }                        //Actor.cs -> RemoveGear, reason why

    //
    // - - - Targets - - -
    //
    public enum TargetFactors { TargetIntel, NodeSupport, ActorAndGear, NodeSecurity, TargetLevel, Teams } //Sequence is order of factor display

    //
    // - - - Contacts - - -
    //
    public enum ContactStatus { Active, Inactive, ContactPool }

    //
    // - - - AI - - -
    //
    public enum Priority { None, Low, Medium, High, Critical }
    public enum AITaskType { None, Team, Decision, Move, LieLow, StressLeave, Idle, ActorArc, Target, Cure, Dismiss, Faction, Count }  //used for both AI Authority and Rebel sides. Not all options apply to each side.
    public enum AIDebugData { None, Task, Node, Spider, Erasure, Decision}                             //used for toggling debugGUI.cs  AI data
    public enum AINodeCriteria { None }                                                     //AIResistanceManager.cs -> ActorArc node task (find node with this criteria)
    public enum HackingStatus { Offline, Initialising, Rebooting, InsufficientRenown, Indisposed, Possible}     //determines what happens when player clicks AISideTabUI
    public enum NemesisMode { Inactive, NORMAL, HUNT }
    public enum NemesisGoal { IDLE, MOVE, LOITER, AMBUSH, SEARCH}

    //
    // - - - Secrets - - -
    //
    public enum SecretStatus {  Inactive, Active, Revealed, Deleted }

    //
    // - - - Dice - - -
    //
    public enum DiceOutcome { None, Ignore, Auto, Roll }
    public enum DiceType { None, Move, Gear}                                //reason dice is being rolled

    //
    // - - - Teams - - -
    //
    public enum TeamInfo { Total, Reserve, OnMap, InTransit, Count }           //DataManager arrayOfTeams  index
    public enum TeamPool { Reserve, OnMap, InTransit, Count }                  //Different pools that teams move between
    public enum TeamDebug { None, Pools, Roster, Actors }
    public enum NATO
    {                                                        //used for sequentially naming teams, eg. 'Control Team Bravo'
        Alpha, Bravo, Charlie, Delta, Echo, Foxtrot, Golf, Hotel, India, Juliett, Kilo, Lima, Mike,
        November, Oscar, Papa, Quebec, Romeo, Sierra, Tango, Uniform, Victor, Whiskey, Xray, Yankee, Zulu,
        Count
    }

    //
    // - - - Statistics - - - 
    //
    public enum StatType                                //NOTE -> No 'None' or 'Count' (code loops enum and can't handle either)
    {
        StressLeaveResistance, StressLeaveAuthority,  
        PlayerBreakdown, PlayerLieLow, PlayerCaptured, PlayerBetrayed,
        TargetAttempts, TargetSuccesses,
        ActorsResignedAuthority, ActorsResignedResistance, ActorResistanceTraitors, ActorsRecruited, ActorConflicts, ActorLearntSecret,
        NodeCrisis, NodeCrisisExplodes
    }

    //
    // - - - ItemData - - -
    //
    public enum ItemPriority { Low, Medium, High, Count }                                    //DataManager.cs -> arrayOfItemsByDataPriority is keyed off ItemPriority & ItemTab 'Count'
    public enum ItemTab { ALERTS, Request, Meeting, Effects, Traits, Random, Count }            //PackageManager.cs -> MainInfoData array keyed off this

    //
    // - - - Messages - - -
    //
    public enum MessageCategory { None, Current, Pending, Archive, AI, Nemesis }
    public enum MessageType { None, PLAYER, TEAM, AI, GEAR, ACTOR, TARGET, ACTIVE, ONGOING, DECISION, FACTION, CITY, NODE, GENERAL, CONTACT, OBJECTIVE }
    public enum MessageSubType
    {
        None,
        //General
        General_Warning,
        General_Info,
        General_Random,
        //Player
        Plyr_Move,
        Plyr_Renown,
        Plyr_Secret,
        Plyr_Damage,
        Plyr_Recognised,
        Plyr_Betrayed,
        //Team
        Team_Add,
        Team_Deploy,
        Team_Withdraw,
        Team_AutoRecall,
        Team_Neutralise,
        Team_Effect,
        //AI
        AI_Connection,
        AI_Node,
        AI_Immediate,
        AI_Capture,
        AI_Release,
        AI_Hacked,
        AI_Reboot,
        AI_Countermeasure,
        AI_Alert,
        AI_Detected,
        AI_Nemesis,
        //Decision
        Decision_Global,
        Decision_Connection,                    //increaes connection security level
        Decision_Resources,                     //additional resources request
        Decision_Team,                          //additional team request
        //Gear
        Gear_Comprised,
        Gear_Used,
        Gear_Lost,
        Gear_Available,
        Gear_Obtained,
        Gear_Given,
        //Actor
        Actor_Recruited,
        Actor_Action,
        Actor_Status,
        Actor_StressLeave,
        Actor_Condition,
        Actor_Complains,
        Actor_Blackmail,
        Actor_Secret,
        Actor_Reassured,
        Actor_Conflict,
        Actor_Trait,
        Actor_Captured,
        Actor_Compatibility,
        //Contact
        Contact_Change,
        Contact_Target_Rumour,
        Contact_Nemesis_Spotted,
        Contact_Team_Spotted,
        Tracer_Nemesis_Spotted,
        Tracer_Team_Spotted,
        //Target
        Target_New,
        Target_Expired,
        Target_ExpiredWarning,
        Target_Attempt,
        Target_Contained,
        //Ongoing
        Ongoing_Created,
        Ongoing_Current,
        Ongoing_Expired,
        Ongoing_Nemesis,
        Ongoing_Decision,
        Ongoing_Warning,
        Ongoing_Node,
        Ongoing_LieLow,
        //Active
        Active_Effect,
        //Node
        Node_Crisis,
        //City
        City_Loyalty,
        //Faction
        Faction_Support,
        Faction_Approval,
        //Objective
        Objective_Progress

    }




}
