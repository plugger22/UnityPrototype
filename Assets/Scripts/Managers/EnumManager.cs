namespace gameAPI
{
    /// <summary>
    /// Enums -> only for use in code, anything that is in an SO requires an SO enum approach
    /// </summary>

    //
    // - - - Game Manager - - -
    //
    public enum GameState {
        None, MainMenu, StartUp, Options, NewGame, NewGameOptions, NewInitialisation, FollowOnInitialisation, NewCampaign, SaveGame, SaveAndExit, SaveAndMain, LoadGame, LoadAtStart,
        PlayGame, MetaGame, TutorialOptions, Tutorial, ExitLevel, ExitCampaign, ExitGame}  //overall game state, NOTE: don't change order (save file)
    public enum WinStateLevel { None, Authority, Resistance }                                                                                   //none indicates nobody has yet won level
    public enum WinReasonLevel { None, CityLoyaltyMin, CityLoyaltyMax, HqSupportMin, MissionTimerMin, ObjectivesCompleted, Investigation, CampaignResult } //reason for Level Win State (from POV of winner)
    public enum WinStateCampaign { None, Authority, Resistance }
    public enum WinReasonCampaign { None, DoomTimerMin, Commendations, BlackMarks, MainGoal, Innocence}
    public enum AuthoritySecurityState { Normal, APB, SecurityAlert, SurveillanceCrackdown }                            //specific Authority Security states (Player or AI)   
    public enum SideState { None, AI, Human }                                                                           //who's running the side
    public enum MetaLevel { None, City, State, Nation }
    public enum AuthorityTitle { Superintendent, Minister, Secretary }                                                  //different names for authority actors depending on MetaLevel

    //
    //  - - - UI - - -
    //
    public enum ModalState { Normal, ModalUI}                                                                                //main modal state
    public enum ModalSubState { None, Outcome, Confirm, GenericPicker, ActionMenu, MainMenu, Inventory, TeamPicker, DiceRoller, Topic, InfoDisplay, ShowMe, Review, Transition, MetaGame, Billboard, Advert,
         GameHelp, Debug } //ModalUI sub game states
    public enum ModalInfoSubState { None, CityInfo, AIInfo, MainInfo, TabbedUI, MasterHelp }                                                                  //if ModalUI.InfoDisplay -> what type of info
    public enum ModalGenericPickerSubState { None, Normal, CompromisedGear }                                                            //if ModalUI.GenericPicker -> what type of picker
    public enum ModalMetaSubState { None, PlayerOptions, OptionsConfirm, EndScreen }                                                    //if ModalUI.MetaGame -> what type of meta UI
    public enum ModalReviewSubState { None, Open, Review, Close }                                                                       //if ModalUI.Review -> what type of Review
    public enum ModalInventorySubState { None, Gear, ReservePool, HQ, CaptureTool}                                                      //if ModalUI.Inventory -> what type of Inventory
    public enum ModalTransitionSubState { None, EndLevel, HQ, PlayerStatus, BriefingOne, BriefingTwo}                                   //if ModalUI.Transition -> what type of Transition
    public enum AlignHorizontal { None, Left, Centre, Right }
    public enum Background { None, Start, NewGame, NewGameOptions, SaveGame, LoadGame, Options, TutorialOptions, EndLevel, MetaGame, NewCampaign, EndCampaign }             //full screen backgrounds
    public enum ActionMenuType { None, Node, NodeGear, Gear, Actor, Player, Move, Reserve }
    public enum DebugRegister { None, Ongoing, Actions }
    public enum GenericTooltipType { Any, ActorInfo }                                           //ability to specify tooltip types that must be closed and to ignore the rest. Create group types as required.
    public enum MajorUI { None, MainInfoApp, MetaGameUI }                                       //used for Item prefab attached scripts to determine which UI they are working within
    public enum PopUpPosition { ActorSlot0, ActorSlot1, ActorSlot2, ActorSlot3, Player, TopBarLeft, TopBarRight, TopCentre, Count}  //used for fixed UI PopUps
    public enum EndlLevelMedal { DeadDuck, Bronze, Silver, Gold }                               //used by EndLevel data to determine medal to award during TransitionUI / MetaGame
    public enum Pulsing { Fading, Growing, Constant }                     //used for UI elements, eg. text, that 'pulse' (grown/shrink in size). Constant is for maintaining constant size 
    public enum MainMenuType { Main, Game, Tutorial }                              //configs for custom (main) menus
    public enum AlertType {
        None,
        MainMenuUnavailable,
        GearDisabled, RecruitingDisabled,
        TutorialGoal, TutorialMenuUnavailable, TutorialDossierUnavailable,
        SomethingWrong,
        PlayerStatus, SideStatus, ActorStatus,
        DebugAI, DebugPlayer,
        HackingRebootInProgress, HackingInsufficientPower, HackingInitialising, HackingIndisposed, HackingOffline
    }

    //
    // - - - File Operations
    //
    public enum SaveType { None, PlayerSave, AutoSave, Tutorial}                                       //type of save file. Don't change order (save file)
    public enum RestorePoint { None, MetaTransition, MetaOptions, MetaComplete }                       //when save and exit, chance to return to game. This specifies the point of return

    //
    // - - - Animation - - -
    //
    public enum CarType { Normal, Police, Rogue, Bus, Surveil }

    //
    // - - - ItemData / MetaData - - -
    //
    public enum ItemPriority { Low, Medium, High, Count }                                           //DataManager.cs -> arrayOfItemsByDataPriority is keyed off ItemPriority & ItemTab 'Count'
    public enum ItemTab { ALERTS, Request, Meeting, Effects, Traits, Random, Count }                //PackageManager.cs -> MainInfoData array keyed off this
    public enum MetaTabSide { Boss, SubBoss1, SubBoss2, SubBoss3, Count }                           //MetaGameUI side tabs (HQ actors). NOTE: order needs to correspond to UI (arrays keyed off this)
    public enum MetaTabTop { Status, Selected, Count }                                              //MetaGameUI top tabs. NOTE: order needs to correspond to UI (arrays keyed off this)
    public enum MetaPriority { Low, Medium, High, Extreme, Count }

    //
    // - - - TabbedUI (Dossiers)
    //
    public enum TabbedUISide { Tab0, Tab1, Tab2, Tab3, Count}                                       //ModalTabbedUI side tabs. NOTE order needs to correspond to UI (arrays keyed off this)
    public enum TabbedUIWho { Subordinates, Player, HQ, Reserves, Count }                           //who the tabbedUI is to be setUp for. NOTE order needs to correspond to UI (code keyed off this)
    public enum TabbedUITop { Tab0, Tab1, Tab2, Tab3, Tab4, Tab5, Tab6, Tab7, Tab8, Count}          //ModalTabbedUI top tabs. NOTE order needs to correspond to UI (arrays keyed off this)
    public enum TabbedPage { Main, Personality, History, Contacts, Secrets, Investigations, Beliefs, Gear, Stats, Organisations, Count } //list of all possible pages (which ones vary by actorSet)
    public enum TabbedHistory { Events, Emotions, MegaCorps, Moves }                                //Events is HistoryActor, emotions is HistoryMood/HistoryOption depending on Player/Actor
    public enum TabbedGear { Normal, Capture }                                                      //Normal Gear page or Capture gear page
    public enum TabbedOrg { Organisation, MegaCorp }                                                //Organisation page -> illegal organisation or MegaCorp

    //
    // - - - Connections - - - 
    //
    public enum ConnectionType { None, HIGH, MEDIUM, LOW, Active, Count }   //NOTE: keep first four entries as is (none -> Low)

    //
    // - - - Effects - - -
    //
    public enum EffectSource { None, Topic, Gear, ReserveActor, ManageAction, NodeAction, Target, TargetAI, Mission, Team, MetaGame }   //source of effects being passed onto EffectManager.cs

    //
    // - - - Nodes - - -
    // 
    public enum NodeInfo { Number, TargetsAll, TargetsLive, TargetsActive, Count }          //DataManager arrayOfNodes index
    public enum NodeArcTally { Current, Minimum, Count };                                   //used for indexing of arrayOfNodeArcTotals
    public enum NodeColour { TowerDark, TowerLight, TowerActive, Highlight, Active, Player, Nemesis, Background, Invisible, Transparent, Normal, Count } //colour of node cylinder or district component (usually the base)
    public enum NodeComponent { Cylinder, Base, Towers, BaseAndTowers, TowerRear, TowerLeftRight }      //Cylinder is the node cylinder, others are for base and/or towers
    public enum NodeType { Node, District }                                                 //Type of node display, cylindrical nodes or prefab tower districts
    public enum NodeData { Stability, Support, Security, Target, Probe, Spider, Erasure};   //NOTE: keep first 3 in order as they are used in tooltipNode.cs -> GetStatColour (matches node tooltip stat display seq)
    public enum NodeText { None, ID, Icon, Contact, ActivityCount, ActivityTime }           //What to display on node/district faceText
    public enum NodeAction
    {                                                           //NodeActionData package
        None,
        ActorBlowStuffUp, ActorGainTargetInfo, ActorHackSecurity, ActorInsertTracer, ActorNeutraliseTeam, ActorObtainGear,
        ActorRecruitActor, ActorSpreadFakeNews, ActorCreateRiots, ActorDeployTeam, ActorRecallTeam,
        PlayerBlowStuffUp, PlayerGainTargetInfo, PlayerHackSecurity, PlayerInsertTracer, PlayerNeutraliseTeam, PlayerObtainGear,
        PlayerRecruitActor, PlayerSpreadFakeNews, PlayerCreateRiots, PlayerDeployTeam, PlayerRecallTeam
    } 
    public enum NodeUI
    {                                                            //parameter for NodeDisplay event
        None,
        Reset, Redraw, Move,
        ShowTargets, ShowSpiders, ShowTracers, ShowTeams, ShowContacts,
        NodeArc0, NodeArc1, NodeArc2, NodeArc3, NodeArc4, NodeArc5, NodeArc6, PlayerKnown,
        MostConnected, Centre, NearNeighbours, DecisionNodes, CrisisNodes, LoiterNodes, CureNodes
    };

    //
    // - - - Activity - - -
    //
    public enum ActivityUI { None, Time, Count}

    //
    // - - - Topics and Targets - - -
    //
    public enum TopicGlobal { None, Decision, Review, Advert }                      //what type of topic will be generated this turn
    public enum TopicDecisionType { Normal, Letter, Comms, Tutorial, Count }                 //type of decision topic
    public enum TopicBase { Normal, Other, Count }                      //used for base topicUI. Order matters (array index)
    public enum Status { Dormant, Active, Live, Outstanding, Done }         //target or topic status (Outstanding applies only targets)
    public enum Activation { Low, Medium, High }                            //chance of Target becoming 'Live' once 'Active'
    public enum GroupType { VeryBad, Bad, Neutral, Good }                   //Topic group type (NOTE: DO NOT CHANGE -> order specific) 'VeryBad' is another version of 'Bad'. Maps to actor Motivation (0 -> 3)
    public enum CampaignOutcome { Inconclusive, Commendation, Blackmark }   //anytime a campaign outcome occurs, eg. Review topic, fail a level, etc.

    //
    // - - - Story - - - 
    //
    public enum StoryType { None, Alpha, Bravo, Charlie, Count }            //Story module type
    public enum MegaCorpType { None, MegaCorpOne, MegaCorpTwo, MegaCorpThree, MegaCorpFour, MegaCorpFive, Count }  //correspond to GlobalManager.cs -> tagMegaCorpOne...

    //
    // - - - Actors - - -
    //
    public enum ActorSex { None, Male, Female}
    public enum ActorHQ { None, Boss, SubBoss1, SubBoss2, SubBoss3, Worker, LeftHQ, Count}      //determines size of DataManager.cs -> arrayOfActorsHQ. Change enum add/remove/order and you'll need to...  
                                                                                                //change code in DataManager -> InitialiseActorArrays
                                                                                                //change code in HQManager.cs -> DebugDisplayHQActors
                                                                                                //change ValidationManager.cs -> CheckActorData
                                                                                                //change HQManager.cs -> GetRandomHQPosition
                                                                                                //change ActorManager.cs -> InitialiseHqHierarchyInventory
                                                                                                //change TopicManager.cs -> GetHQSubTopics
                                                                                                //change MetaManager.cs -> InitialiseHQ

    public enum ActorStatus { Active, Inactive, Reserve, RecruitPool, Captured, Dismissed, Promoted, Killed, Resigned, HQ }
    public enum ActorInactive { None, LieLow, Breakdown, StressLeave, Dormant}                               //reason actor is inactive
    public enum ActorList { None, Reserve, Promoted, Dismissed, Disposed, Resigned, HQ}             //used as a parameter only to access lists (DataManager.cs -> GetActorList)
    public enum ActorTooltip { None, Breakdown, LieLow, Captured, Leave, Dormant}                            //actor sprite shows a relevant tooltip if other than 'None'
    public enum ActorDatapoint { Datapoint0, Influence0, Contacts0, Datapoint1, Opinion1, Ability2, Invisibility2, Datapoint2}    //interchangeable. Use whichever one is appropriate
    public enum ActorCheck {      //DataManager.cs -> CheckNumOfActiveActorsSpecial
        None, CompatibilityNOTZero, NodeActionsNOTZero, TeamActionsNOTZero,
        PersonalGearYes, PersonalGearNo,
        ActorContactMin, ActorContactNOTMax, ActorConflictNOTZero,
        PowerMore, PowerLess, KnowsSecret, KnowsNothing
    }
    public enum ActorRelationship { None, Friend, Enemy }                                       //relationships between subordinate actors, NOT with the player

    //
    // - - - Gear - - -
    //
    public enum GearRemoved { Lost, Taken, Compromised, Decision, RecruitPool }                        //Actor.cs -> RemoveGear, reason why

    //
    // - - - Targets - - -
    //
    public enum TargetFactors { TargetIntel, NodeSupport, ActorAndGear, NodeSecurity, TargetLevel, Teams } //Sequence is order of factor display

    //
    // - - - Contacts - - -
    //
    public enum ContactStatus { Active, Inactive, ContactPool }

    //
    // - - - Organisations - - -
    //
    public enum OrganisationType { None, Cure, Contract, Emergency, HQ, Info} //Maps to OrgType.SO
    public enum OrgInfoType { Nemesis, ErasureTeams, Npc, Count } //targets of orgInfo hacks (index for DataManager.cs -> arrayOfOrgInfo)

    //
    // - - - AI - - -
    //
    public enum Priority { None, Low, Medium, High, Critical }
    public enum AITaskType { None, Team, Decision, Move, LieLow, StressLeave, Idle, ActorArc, Target, Cure, Dismiss, HQ, Count }  //used for both AI Authority and Rebel sides. Not all options apply to each side.
    public enum AIDebugData { None, Task, Node, Spider, Erasure, Decision}                                      //used for toggling debugGUI.cs  AI data
    public enum AINodeCriteria { None }                                                                         //AIResistanceManager.cs -> ActorArc node task (find node with this criteria)
    public enum HackingStatus { Offline, Initialising, Rebooting, InsufficientPower, Indisposed, Possible}     //determines what happens when player clicks AISideTabUI
    public enum NemesisMode { Inactive, NORMAL, HUNT }
    public enum NemesisGoal { IDLE, MOVE, LOITER, AMBUSH, SEARCH}
    public enum NpcStatus { Standby, Active, Departed }
     
    //
    // - - - Secrets - - -
    //
    public enum SecretStatus {  Inactive, Active, Revealed, Deleted }

    //
    // - - - Investigations - - -
    //
    public enum InvestStatus { None, Ongoing, Resolution, Completed }           //status of investigation
    public enum InvestOutcome { None, Guilty, Innocent, Dropped}                //outcome of a completed investigation

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
    public enum TeamAction { None, TeamCivil, TeamControl, TeamDamage, TeamErasure, TeamMedia, TeamProbe, TeamSpider }      //TeamActionData package
    public enum NATO
    {                                                        //used for sequentially naming teams, eg. 'Control Team Bravo'
        Alpha, Bravo, Charlie, Delta, Echo, Foxtrot, Golf, Hotel, India, Juliett, Kilo, Lima, Mike,
        November, Oscar, Papa, Quebec, Romeo, Sierra, Tango, Uniform, Victor, Whiskey, Xray, Yankee, Zulu,
        Count
    }

    //
    // - - - Player - - -  
    //
    public enum MoodType {
        ReservePromise, ReserveNoPromise, ReserveRest,
        DismissIncompetent, DismissUnsuited, DismissPromote,
        DisposeCorrupt, DisposeLoyalty, DisposeHabit,
        ReserveLetGo, ReserveFire, ReserveBully, ReserveReassure,
        GiveGear, TakeGear
    }
    public enum PlayerCheck { None, NodeActionsNOTZero }

    //
    // - - - Goals
    //
    public enum GoalType                        //TutorialGoal.SO equivalents
    {
        None,
        Move, MoveSecurity,
        PlayerInvisibility, PlayerNodeActions, PlayerRecruit,
        SubordinateRecruit, SubordinateNodeActions
    }                 

    //
    // - - - Statistics - - - 
    //
    public enum StatType                                //NOTE -> No 'None' or 'Count' (code loops enum and can't handle either)
    {
        StressLeaveResistance, StressLeaveAuthority,  
        PlayerBreakdown, PlayerLieLowTimes, PlayerLieLowDays, PlayerCaptured, PlayerCapturedDays, PlayerBetrayed, PlayerTimesStressed, PlayerStressedDays, PlayerSuperStressed, PlayerDoNothing,
        PlayerNodeActions, PlayerManageActions, PlayerMoveActions, PlayerTargetAttempts, PlayerGiveGear, PlayerAddictedDays, PlayerTimesCured, PlayerDaysOnJob, PlayerInvisibilityLost,
        PlayerMoveSecureConnections,
        SubordinateNodeActions,
        LieLowDaysTotal, GearTotal,
        TargetAttempts, TargetSuccesses,
        ActorsResignedAuthority, ActorsResignedResistance, ActorResistanceTraitors, ActorsRecruited, ActorConflicts, ActorLearntSecret, ActorCompatibilityGood, ActorCompatibilityBad,
        NodeCrisis, NodeCrisisExplodes, NodeActionsResistance,
        TopicsGood, TopicsBad, TopicsIgnored,
        InvestigationsLaunched, InvestigationsCompleted,
        OrgCures, OrgContractHits, OrgInfoHacks, OrgHQDropped, OrgEscapes,
        ReviewsTotal, ReviewCommendations, ReviewBlackmarks, ReviewInconclusive,
        DaysAPB, DaysAlert, DaysCrackdown, DaysErasureTeam,
        ObjectivesComplete,
        HQRelocations
    }


    //
    // - - - Help - - -
    //
    public enum HelpType {                                                                  //used to give correct help tooltip tags to certain messages with multiple causes, eg. ActorCondition 
        None,
        PlayerBreakdown, StressLeave, LieLow, BadCondition
    }

    //
    // - - - Messages - - -
    //
    //start of turn message pipeline (used for determining order messages are displayed ->  shown in enum order, make sure Compromised Gear is kept first as it's interactive)
    public enum MsgPipelineType { None, CompromisedGear, Npc, ReleasePlayer, Nemesis, CapturePlayer, CaptureActor, SecretRevealed, InvestigationLaunched, InvestigationCompleted, WinLoseLevel, WinLoseCampaign, DebugTopic} //see notes above
    public enum MessageCategory { None, Current, Pending, Archive, AI, Nemesis }
    public enum MessageType { None, PLAYER, TEAM, AI, GEAR, ACTOR, TARGET, ACTIVE, ONGOING, DECISION, HQ, CITY, NODE, GENERAL, CONTACT, OBJECTIVE, TOPIC, ORGANISATION, NPC, INVESTIGATION }
    public enum MessageSubType
    {
        None,
        //General
        General_Warning,
        General_Info,
        General_Random,
        //Player
        Plyr_Move,
        Plyr_Power,
        Plyr_Secret,
        Plyr_Damage,
        Plyr_Escapes,
        Plyr_Recognised,
        Plyr_Betrayed,
        Plyr_Mood,
        Plyr_Immune,
        Plyr_Addicted,
        Plyr_Cure,
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
        Gear_Special,
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
        Contact_Npc_Spotted,
        Contact_Inactive,
        Contact_Active,
        Tracer_Nemesis_Spotted,
        Tracer_Team_Spotted,
        Tracer_Npc_Spotted,
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
        Ongoing_Npc,
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
        //HQ
        HQ_Support,
        HQ_SupportUnavailable,
        HQ_Approval,
        HQ_Relocates,
        //Objective
        Objective_Progress,
        //Topic
        Topic_Decision,
        Topic_Review,
        //Organisation
        Org_Secret,
        Org_Reputation,
        Org_Nemesis,
        Org_Erasure,
        Org_Npc,
        //Npc
        Npc_Arrival,
        Npc_Interact,
        Npc_Depart,
        //Investigations
        Invest_New,
        Invest_Ongoing,
        Invest_Evidence,
        Invest_Resolution,
        Invest_Completed,
        Invest_Dropped
    }



}
