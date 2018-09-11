namespace gameAPI
{
    /// <summary>
    /// Enums -> only for use in code, anything that is in an SO requires an SO enum approach
    /// </summary>

    //
    // - - - Game Manager - - -
    //
    public enum WinState { None, Authority, Resistance }                                                                //none indicates nobody has yet won
    public enum GameState { Normal, ModalUI}                                                                            //main game states
    public enum ResistanceState { Normal }                                                                              //specific Resistance states (Player or AI)
    public enum AuthoritySecurityState { Normal, APB, SecurityAlert, SurveillanceCrackdown }                            //specific Authority Security states (Player or AI)   
    public enum SideState { None, AI, Player }                                                                          //who's running the side
    public enum MetaLevel { None, City, State, Nation }
    public enum AuthorityTitle { Superintendent, Minister, Secretary }                                          //different names for authority actors depending on MetaLevel

    //
    //  - - - UI - - -
    //
    public enum ModalState { None, Outcome, GenericPicker, ActionMenu, Inventory, TeamPicker, DiceRoller, InfoDisplay } //ModalUI sub game states
    public enum ModalInfoSubState { None, CityInfo, AIInfo, MainInfo }                                                                    //if ModalUI.InfoDisplay -> what type of info
    public enum ModalGenericPickerSubState { None, Normal, CompromisedGear }                                                     //if ModalUI.GenericPicker -> what type of picker
    public enum InventoryState { None, Gear, ReservePool}
    public enum UIPosition { None, Left, Middle, Right }
    public enum ActionMenuType { None, Node, NodeGear, Gear, Actor, Player, Move, Reserve }
    //public enum MainInfoTab { Main, HQ, People, Random, Summary, Help};                       //tabs for RHS of MainInfoUI. Order important (ties in with array indexes)
    public enum AlertType {
        None,
        SomethingWrong,
        PlayerStatus, SideStatus,
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
    public enum NodeInfo { Number, TargetsAll, TargetsLive, TargetsActive, Count }    //DataManager arrayOfNodes index
    public enum NodeType { Normal, Highlight, Active, Player, Count }
    public enum NodeData { Stability, Support, Security, Target, Probe, Spider, Erasure};    //NOTE: keep first 3 in order as they are used in tooltipNode.cs -> GetStatColour (matches node tooltip stat display seq)
    public enum NodeUI
    {                                                            //parameter for NodeDisplay event
        None,
        Reset, Redraw, Move,
        ShowTargets, ShowSpiders, ShowTracers, ShowTeams,
        NodeArc0, NodeArc1, NodeArc2, NodeArc3, NodeArc4, NodeArc5, NodeArc6, NodeArc7,
        MostConnected, Centre, NearNeighbours, DecisionNodes, CrisisNodes
    };

    //
    // - - - Activity - - -
    //
    public enum ActivityUI { None, Time, Count}

    //
    // - - - Decisions and Targets - - -
    //
    public enum Status { Dormant, Active, Live, Completed, Contained }
    public enum Activation { Low, Medium, High }                                     //chance of becoming 'Live' once 'Active'

    //
    // - - - ItemData - - -
    //
    public enum ItemPriority { Low, Medium, High, Count }                                    //DataManager.cs -> arrayOfItemsByDataPriority is keyed off ItemPriority & ItemTab 'Count'
    public enum ItemTab { ALERTS, Request, Meeting, Effects, Traits, Random, Count}            //PackageManager.cs -> MainInfoData array keyed off this

    //
    // - - - Actors - - -
    //
    public enum ActorStatus { Active, Inactive, Reserve, RecruitPool, Captured, Dismissed, Promoted, Killed, Resigned }
    public enum ActorInactive { None, LieLow, Breakdown}                             //reason actor is inactive
    public enum ActorList { None, Reserve, Promoted, Dismissed, Disposed}           //used as a parameter only to access lists (DataManager.cs -> GetActorList)
    public enum ActorTooltip { None, Breakdown, LieLow, Captured}                        //actor sprite shows a relevant tooltip if other than 'None'

    //
    // - - - AI - - -
    //
    public enum Priority { Low, Medium, High, Critical }
    public enum AIType { None, Team, Decision }
    public enum AIDebugData { None, Task, Node, Spider, Erasure, Decision}                             //used for toggling debugGUI.cs  AI data
    public enum HackingStatus { Offline, Initialising, Rebooting, InsufficientRenown, Indisposed, Possible}     //determines what happens when player clicks AISideTabUI

    //
    // - - - Dice - - -
    //
    public enum DiceOutcome { None, Ignore, Auto, Roll }
    public enum DiceType { None, Move, Gear}                                //reason dice is being rolled

    //
    // - - - Messages - - -
    //
    public enum MessageCategory { None, Current, Pending, Archive, AI }
    public enum MessageType { None, PLAYER, TEAM, AI, GEAR, ACTOR, TARGET, ACTIVE, ONGOING, DECISION, FACTION, CITY, NODE, GENERAL }
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
        Actor_Condition,
        Actor_Complains,
        Actor_Blackmail,
        Actor_Secret,
        Actor_Reassured,
        Actor_Conflict,
        Actor_Trait,
        //Target
        Target_Attempt,
        Target_Contained,
        //Ongoing
        Ongoing_Created,
        Ongoing_Current,
        Ongoing_Expired,
        //Active
        Active_Effect,
        //Node
        Node_Crisis,
        //City
        City_Loyalty,
        //Faction
        Faction_Support,
        Faction_Approval

    }

    //
    // - - - Teams - - -
    //
    public enum TeamInfo { Total, Reserve, OnMap, InTransit, Count }           //DataManager arrayOfTeams  index
    public enum TeamPool { Reserve, OnMap, InTransit, Count }                  //Different pools that teams move between
    public enum TeamDebug { None, Pools, Roster, Actors}
    public enum NATO
    {                                                        //used for sequentially naming teams, eg. 'Control Team Bravo'
        Alpha, Bravo, Charlie, Delta, Echo, Foxtrot, Golf, Hotel, India, Juliett, Kilo, Lima, Mike,
        November, Oscar, Papa, Quebec, Romeo, Sierra, Tango, Uniform, Victor, Whiskey, Xray, Yankee, Zulu,
        Count
    }



}
