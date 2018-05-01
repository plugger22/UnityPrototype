namespace gameAPI
{
    /// <summary>
    /// Enums -> only for use in code, anything that is in an SO requires an SO enum approach
    /// </summary>

    //
    // - - - Game Manager - - -
    //
    //public enum Side { None, Authority, Resistance, Count }                                   //Sides available in game
    public enum GameState { Normal, ModalUI}                                                    //main game states
    public enum ModalState { None, Outcome, GenericPicker, ActionMenu, Inventory, TeamPicker, DiceRoller} //ModalUI sub game states
    public enum ResistanceState { Normal }                                              //player as Resistance (not used at present)
    public enum AuthorityState { Normal }                                               //player as Authority (not used at present)
    public enum SideState { None, AI, Player }                                          //who's running the side
    public enum MetaLevel { None, City, State, Nation }
    public enum AuthorityActor { Superintendent, Minister, Secretary }              //different names for authority actors depending on MetaLevel

    //
    //  - - - UI - - -
    //
    public enum InventoryState { None, Gear, ReservePool}
    public enum AlertType { None, PlayerSatus, SideStatus, DebugAI }

    //
    // - - - Connections - - - 
    //
    public enum ConnectionType { None, HIGH, MEDIUM, LOW, Count }

    //
    // - - - Nodes - - -
    // 
    public enum NodeInfo { Number, TargetsAll, TargetsLive, TargetsActive, Count }    //DataManager arrayOfNodes index
    public enum NodeType { Normal, Highlight, Active, Player, Count }
    public enum NodeData { Stability, Support, Security, Target, Probe, Spider};    //NOTE: keep first 3 in order as they are used in tooltipNode.cs -> GetStatColour (matches node tooltip stat display seq)
    public enum NodeUI
    {                                                            //parameter for NodeDisplay event
        None,
        Reset, Redraw, Move,
        ShowTargets, ShowSpiders, ShowTracers, ShowTeams,
        NodeArc0, NodeArc1, NodeArc2, NodeArc3, NodeArc4, NodeArc5, NodeArc6, NodeArc7,
        MostConnected, Centre, NearNeighbours
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
    // - - - Actors - - -
    //
    public enum ActorStatus { Active, Inactive, ReservePool, RecruitPool, Captured, Dismissed, Promoted, Killed }
    public enum ActorInactive { None, LieLow, Breakdown}                             //reason actor is inactive
    public enum ActorList { None, Reserve, Promoted, Dismissed, Disposed}           //used as a parameter only to access lists (DataManager.cs -> GetActorList)
    public enum ActorTooltip { None, Breakdown, LieLow, Talk}                        //actor sprite shows a relevant tooltip if other than 'None'

    //
    // - - - Action Menu - - -
    //
    public enum ActionMenuType { None, Node, NodeGear, Gear, Actor, Move, Reserve}

    //
    // - - - AI - - -
    //
    public enum Priority { Low, Medium, High, Critical }
    public enum AIType { None, Team, Connection, Decision }
    public enum AIDebugData { None, Task, Node, Spider, Erasure}                             //used for toggling debugGUI.cs  AI data

    //
    // - - - Dice - - -
    //
    public enum DiceOutcome { None, Ignore, Auto, Roll }
    public enum DiceType { None, Move, Gear}                                //reason dice is being rolled

    //
    // - - - Messages - - -
    //
    public enum MessageCategory { None, Current, Pending, Archive, AI }
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
        AI_Connection,
        AI_Node,
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
        Actor_Reassured,
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
