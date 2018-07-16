using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Handles all Performance Monitoring methods
/// NOTE: Debug calls are tricky because of conflicts with C# diagnostic API's. Use 'UnityEngine.Debug.LogFormat' format and expect strange behaviour
/// </summary>
public class TestManager : MonoBehaviour
{
    Stopwatch timer;

    private long totalElapsedTime;                      //start tally with 'TimerTallyStart', finish with 'TimerTallyStop'
    private bool isTimerTallyActive;                    //timers add to tally only if true (set by 'TimerTallyStart'

    public void Initialise()
    {
        timer = new Stopwatch();
        isTimerTallyActive = false;
    }


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


    /*/// <summary>
    /// test speed of iterating between the dictOfNodes and the listOfNode
    /// NOTE: Debug is doing funny things and the numbers it gives are nonsense. Do a breakpoint check to get numbers. 
    /// Averages around 352 ms for dict, 278 ms for list
    /// </summary>
    public void TestIterationSpeed()
    {
        Stopwatch timerDict = new Stopwatch();
        Stopwatch timerList = new Stopwatch();
        //iterate dictionary
        Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetDictOfNodes();
        List <Node> listOfNodes = GameManager.instance.dataScript.GetListOfAllNodes();
        timerDict.Start();
        foreach (var node in dictOfNodes)
        {
            if (node.Value.nodeID == 999)
            { UnityEngine.Debug.Log("WOW BABY!"); }
        }
        timerDict.Stop();
        UnityEngine.Debug.LogFormat("[Tst] TestManager.cs -> TestIterationSpeed: Dictionary {0} ms, {1} records{2}", timerDict.ElapsedMilliseconds, dictOfNodes.Count, "\n");
        //iterate list
        
        timerList.Start();
        foreach (Node node in listOfNodes)
        {
            if (node.nodeID == 999)
            { UnityEngine.Debug.Log("WOW BABY!"); }
        }
        timerList.Stop();
        UnityEngine.Debug.LogFormat("[Tst] TestManager.cs -> TestIterationSpeed: List {0} ms, {1} records{2}", timerList.ElapsedMilliseconds, listOfNodes.Count, "\n");
    }*/


}
