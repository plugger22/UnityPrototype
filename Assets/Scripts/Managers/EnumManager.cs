using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gameAPI
{
    //Game Manager
    public enum Side { Authority, Rebel }
    public enum GameState { Normal, ModalOutcome, ModalActionMenu }
    public enum MetaLevel { City, State, Nation }
    public enum NodeInfo { Number, TargetsAll, TargetsLive, TargetsActive, Count}    //DataManager arrayOfNodes index
    

    //Level Manager
    public enum ConnectionType { Neutral, HighSec, MedSec, LowSec, Count }
    public enum NodeType { Normal, Highlight, Active, Player, Count }

    //Events and Targets
    public enum Status {Dormant, Active, Live }
    public enum Activation { Low, Medium, High}                           //chance of becoming 'Live' once 'Active'

    //Action Manager
    public enum Comparison { None, LessThan, GreaterThan, EqualTo }
    public enum Result { None, Add, Subtract, EqualTo }

    //Effects
    public enum EffectCriteria {
        None,
        NodeStability, NodeSecurity, NodeSupport,
        NumRecruits, NumTeams, NumTracers, NumGear,
        TargetInfo,
        RebelCause
    }     
    public enum EffectOutcome {
        None,
        NodeStability, NodeSecurity, NodeSupport,
        Recruit, NeutraliseTeam, AddTracer, GetGear, GetTargetInfo, SpreadInstability,
        Renown,
        RebelCause
    }





}
