using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Topic Profile with all timer, delay and repeat data
/// </summary>
[CreateAssetMenu(menuName = "Topic / TopicProfile")]
public class TopicProfile : ScriptableObject
{
    [Tooltip("Descriptor only, not used in-game")]
    public string descriptor;

    [Header("Delay Factors")]
    [Tooltip("Multiplier to global Minimum interval, eg. 2x, 3x, etc. for number of turns that must elapse before it can be chosen again, default 0")]
    [Range(0, 10)] public int delayRepeatFactor = 0;
    [Tooltip("Multiplier to global Minimum interval, eg. 2x, 3x, etc. for number of turns that must elapse before it can be selected for the first time (at start), default 0")]
    [Range(0, 10)] public int delayStartFactor = 0;  
    [Tooltip("Multiplier to global Minimum interval, eg. 2x, 3x, etc. for number of turns that topic remains Live before timing out, default 0")]
    [Range(0, 10)] public int liveWindowFactor = 0;
    
    //actual delays/timers in turns (initialised in TopicManager.cs -> SubInitialiseStartUp)
    [HideInInspector] public int delayRepeat;
    [HideInInspector] public int delayStart;
    [HideInInspector] public int timerWindow;
}
