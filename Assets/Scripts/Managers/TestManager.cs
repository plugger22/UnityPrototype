using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Handles all Tests
/// NOTE: Debug calls are tricky because of conflicts with C# diagnostic API's. Use 'UnityEngine.Debug.LogFormat' format and expect strange behaviour
/// </summary>
public class TestManager : MonoBehaviour
{
    [Header("Authority Player AutoRun tests")]
    [Tooltip("Specify a turn (within autorun) where the Stressed condition will be given to the Authority player")]
    public int stressTurnAuthority = -1;

    [Header("Resistance Player AutoRun tests")]
    [Tooltip("Specify a turn (within autorun) where the Stressed condition will be given to the Resistance player")]
    public int stressTurnResistance = -1;


    Stopwatch timer;

    private long totalElapsedTime;                      //start tally with 'TimerTallyStart', finish with 'TimerTallyStop'
    private bool isTimerTallyActive;                    //timers add to tally only if true (set by 'TimerTallyStart'

    public void Initialise()
    {
        timer = new Stopwatch();
        isTimerTallyActive = false;
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


    //
    // - - - Autorun tests
    //




}
