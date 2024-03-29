﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Handles all Tests. Note: AutoRun tests implemented in AIManager.cs / AIRebelManager.cs -> DebugTest
/// NOTE: Debug calls are tricky because of conflicts with C# diagnostic API's. Use 'UnityEngine.Debug.LogFormat' format and expect strange behaviour
/// </summary>
public class TestManager : MonoBehaviour
{
    [Header("Test Game State")]
    [Tooltip("Test personality for player. Leave as 'None' if you want a random personality")]
    public TestPersonality testPersonality;
    [Tooltip("Test campaign to run")]
    public Campaign campaign;

    [Header("Topics")]
    [Tooltip("If there is a topic pool specified here then a topic will be randomly chosen from that pool overriding any normally selected topic. Ignored for autoRuns")]
    public TopicPool debugTopicPool;

    [Header("Gear")]
    [Tooltip("Number of gear items in inventory at end of autorun (default -1 for normal random assignment, specify number from 0 to 3 otherwise to override)")]
    public int numOfGearItems = -1;

    [Header("HQ")]
    [Tooltip("How many worker, at campaign start, are populated in HQ pool (default -1 for normal assignment). Allows number between 0 and HQManager.cs -> maxNumOfWorkers")]
    [Range(0, 8)] public int numOfWorkers = -1;

    [Header("MetaGame")]
    [Tooltip("Bonus Power given to player at start of every MetaGame, default 0")]
    [Range(0, 10)] public int bonusPower = 0;

    [Header("Nemesis")]
    [Tooltip("Nemesis will be shown on map at all times if true, otherwise according to standard rules (shown only at start until it moves")]
    public bool showNemesis;

    [Header("Authority Player AutoRun tests")]
    [Tooltip("Specify a turn (LESS THAN autorun period, ignored otherwise) where the indicated Condition will be given to the Authority player")]
    public int conditionTurnAuthority = -1;
    [Tooltip("Who gets the Condition? use 0/1/2/3  (slotID's) for authority actors and 999 for Player")]
    public int conditionWhoAuthority = 999;
    [Tooltip("Condition to be applied")]
    public Condition condtionAuthority;

    [Header("Resistance Player AutoRun tests")]
    [Tooltip("Specify a turn (LESS THAN autorun period, ignored otherwise) where the Condition will be given to the Resistance player")]
    public int conditionTurnResistance = -1;
    [Tooltip("Who gets the Condition? use 0/1/2/3  (slotID's) for resistance actors and 999 for Player")]
    public int conditionWhoResistance = 999;
    [Tooltip("Condition to be applied")]
    public Condition conditionResistance;
    [Tooltip("Invisibility at end of AutoRun (-1 default allows it to be whatever it would normally be) Resistance Player only")]
    [Range(-1, 3)] public int playerInvisibility = -1;

    [Header("Actor -> First Scenario Only")]
    [Tooltip("slotID (0 -3) of actor you want to specify. Leave as '-1' (default) for no effect. All changes made at start")]
    [Range(-1, 3)] public int actorSlotID = -1;
    [Tooltip("The type of actor you want them to be, leave as 'None' for random. If type not present existing actor will be placed in Reserves and a new actor of required type will replace them")]
    public ActorArc actorType;
    [Tooltip("Value you want for actor's datapoint0 -> Influence/Connections, leave as '-1' for random, normal, value")]
    [Range(-1, 3)] public int actorDatapoint0 = -1;
    [Tooltip("Value you want for actor's datapoint1 -> Motivation, leave as '-1' for random, normal, value")]
    [Range(-1, 3)] public int actorDatapoint1 = -1;
    [Tooltip("Value you want for actor's datapoint2 -> Ability/Invisibility, leave as '-1' for random, normal, value")]
    [Range(-1, 3)] public int actorDatapoint2 = -1;

    [Header("Organisations")]
    [Tooltip("Default starting Organisation Reputation (Player's rep with Org)")]
    public int orgReputation = 2;
    [Tooltip("Default starting Organisation Freedom (Player's freedom from debt)")]
    public int orgFreedom = 3;

    [Header("Player Ratios")]
    [Tooltip("Any value other than zero will override calculated ratio each turn")]
    [Range(0, 1)] public float testRatioPlayNodeAct = 0;
    [Tooltip("Any value other than zero will override calculated ratio each turn")]
    [Range(0, 1)] public float testRatioPlayTargetAtt = 0;
    [Tooltip("Any value other than zero will override calculated ratio each turn")]
    [Range(0, 1)] public float testRatioPlayMoveAct = 0;
    [Tooltip("Any value other than zero will override calculated ratio each turn")]
    [Range(0, 1)] public float testRatioPlayLieLow = 0;
    [Tooltip("Any value other than zero will override calculated ratio each turn")]
    [Range(0, 1)] public float testRatioPlayGiveGear = 0;
    [Tooltip("Any value other than zero will override calculated ratio each turn")]
    [Range(0, 1)] public float testRatioPlayManageAct = 0;
    [Tooltip("Any value other than zero will override calculated ratio each turn")]
    [Range(0, 1)] public float testRatioDoNothing = 0;
    [Tooltip("Any value other than zero will override calculated ratio each turn")]
    [Range(0, 1)] public float testRatioAddictedDays = 0;

    [Header("[Tst] Logging")]
    [Tooltip("Toggle ON to record [Tst] messages for the TopicManager.cs class")]
    public bool isTopicManager;
    [Tooltip("Toggle ON to record [Tst] messages for the MetaGame (MetaManager.cs / ActorManager.cs / HQ Manager.cs")]
    public bool isMetaGame;
    [Tooltip("Toggle ON to record [Tst] messages for the PopUpFixed.cs class")]
    public bool isPopUpFixed;
  

    /*[Header("MetaGame Options")]
    [Tooltip("Options only taken into account if this is True")]
    public bool isValidMetaOptions = false;
    [Tooltip("If true, actors who have previously been dismissed, will be included in the selection pool for the new level OnMap actors, if false, they'll be excluded")]
    public bool isDismissed = true;
    [Tooltip("If true, actors who have previously resigned, will be included in the selection pool for the new level OnMap actors, if false, they'll be excluded")]
    public bool isResigned = true;
    [Tooltip("If true, actors with all opinion values will be included, if false any with opinion Zero will be excluded")]
    public bool isLowMotivation = true;
    [Tooltip("If true, actors are included if they are traitors, if false then they are excluded")]
    public bool isTraitor = true;
    [Tooltip("If true, all actors in the selection pool will be level 2")]
    public bool isLevelTwo = false;
    [Tooltip("If true, all actors in the selection pool will be level 3")]
    public bool isLevelThree = false;*/


    Stopwatch timer;

    private long totalElapsedTime;                      //start tally with 'TimerTallyStart', finish with 'TimerTallyStop'
    private bool isTimerTallyActive;                    //timers add to tally only if true (set by 'TimerTallyStart'


    /// <summary>
    /// NOTE: Initialised in GameManager.cs -> InitialiseNewSession BEFORE the normal Startup Sequence (including Global and Game methods)
    /// </summary>
    public void Initialise()
    {
        //needed for performance monitoring
        timer = new Stopwatch();
        isTimerTallyActive = false;
        //campaign
        UnityEngine.Debug.Assert(campaign != null, "Invalid campaign (Null)");
        GameManager.i.campaignScript.campaign = campaign;
    }

    //
    // - - - Performance testing - - -
    //

    /// <summary>
    /// starts timer
    /// </summary>
    public void StartTimer()
    {
        timer.Restart();
    }

    /// <summary>
    /// Returns elapsed time in milliseconds
    /// </summary>
    /// <returns></returns>
    public long StopTimer()
    {
        timer.Stop();
        if (isTimerTallyActive == true)
        { totalElapsedTime += timer.ElapsedMilliseconds; }
        return timer.ElapsedMilliseconds;
    }

    /// <summary>
    /// resets timer tally to zero. All subsequent timers will add to tally until TimerTallyStop is called and result returned
    /// </summary>
    public void TimerTallyStart()
    {
        totalElapsedTime = 0;
        isTimerTallyActive = true;
    }

    /// <summary>
    /// returns timer tally, sets isTimerTallyActive to false
    /// </summary>
    /// <returns></returns>
    public long TimerTallyStop()
    {
        isTimerTallyActive = false;
        return totalElapsedTime;
    }


    


}
