using System;
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
    [Tooltip("If there is a topic pool specified here then a topic will be randomly chosen from that pool overriding any normally selected topic. Ignored for autoRuns")]
    public TopicPool debugTopicPool;

    [Header("Authority Player AutoRun tests")]
    [Tooltip("Specify a turn (within autorun) where the indicated Condition will be given to the Authority player")]
    public int conditionTurnAuthority = -1;
    [Tooltip("Who gets the Condition? use 0/1/2/3  (slotID's) for authority actors and 999 for Player")]
    public int conditionWhoAuthority = 999;
    [Tooltip("Condition to be applied")]
    public Condition condtionAuthority;

    [Header("Resistance Player AutoRun tests")]
    [Tooltip("Specify a turn (within autorun) where the Condition will be given to the Resistance player")]
    public int conditionTurnResistance = -1;
    [Tooltip("Who gets the Condition? use 0/1/2/3  (slotID's) for resistance actors and 999 for Player")]
    public int conditionWhoResistance = 999;
    [Tooltip("Condition to be applied")]
    public Condition conditionResistance;


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
        GameManager.instance.campaignScript.campaign = campaign;
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
