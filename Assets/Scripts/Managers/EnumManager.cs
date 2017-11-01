using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gameAPI
{
    //
    // - - - Game Manager - - -
    //
    public enum Side { Authority, Resistance, Count }
    public enum GameState { Normal, ModalOutcome, ModalActionMenu }
    public enum MetaLevel { None, City, State, Nation }
    public enum NodeInfo { Number, TargetsAll, TargetsLive, TargetsActive, Count}    //DataManager arrayOfNodes index
    
    //
    // - - - Level Manager
    //
    public enum ConnectionType { Neutral, HighSec, MedSec, LowSec, Count }

    //
    // - - - Node Manager - - 
    //
    public enum NodeType { Normal, Highlight, Active, Player, Count }
    public enum NodeUI {                                                            //parameter for NodeDisplay event
        None,
        Reset, Redraw,
        ShowTargets,
        NodeArc0, NodeArc1, NodeArc2, NodeArc3, NodeArc4, NodeArc5, NodeArc6, NodeArc7, NodeArc8, NodeArc9};             

    //
    // - - - Events and Targets
    //
    public enum Status {Dormant, Active, Live }
    public enum Activation { Low, Medium, High}                                     //chance of becoming 'Live' once 'Active'

    //
    // - - - Action Manager - - -
    //
    public enum Comparison { None, LessThan, GreaterThan, EqualTo }
    public enum Result { None, Add, Subtract, EqualTo }

    //
    // - - - Effects - - -
    //
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
        RebelCause,
        //authority
        AnyTeam,
        CivilTeam, DamageTeam, ProbeTeam, MediaTeam, ControlTeam, ErasureTeam, SpiderTeam
    }

    //
    // - - - Teams - - -
    //
    public enum TeamInfo { Total, Reserve, OnMap, InTransit, Count}           //DataManager arrayOfTeams  index
    public enum TeamPool { Reserve, OnMap, InTransit, Count}                  //Different pools that teams move between
    public enum NATO {                                                        //used for sequentially naming teams, eg. 'Control Team Bravo'
        Alpha, Bravo, Charlie, Delta, Echo, Foxtrot, Golf, Hotel, India, Juliett, Kilo, Lima, Mike,
        November, Oscar, Papa, Quebec, Romeo, Sierra, Tango, Uniform, Victor, Whiskey, Xray, Yankee, Zulu,
        Count }



}
