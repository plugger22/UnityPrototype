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

    [Tooltip("Number of turns before a topic repeats Ignored if Zero. Must be (if non Zero) GREATER THAN TopicManager.cs -> minIntervalGlobal")]
    public int delayRepeat;
    [Tooltip("Number of turns from start of level before topic goes Active. Ignored if Zero. Must be (if non Zero) GREATER THAN TopicManager.cs -> minIntervalGlobal")]
    public int delayStart;
    [Tooltip("Number of turns the topic, once Live, remains Live before timing out. Must be (if non Zero) GREATER THAN TopicManager.cs -> minIntervalGlobal")]
    public int timerWindow;
    

}
