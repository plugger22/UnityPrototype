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

    [Tooltip("True if topic repeats after use")]
    public bool isRepeat;
    [Tooltip("Number of turns before a topic repeats (assuming that isRepeat is True, ignored otherwise)")]
    public int delayRepeat;
    [Tooltip("Number of turns from start of level before topic goes Active. Ignored if Zero")]
    public int delayStart;
    [Tooltip("Number of turns the topic, once Live, remains Live before timing out")]
    public int timerWindow;
    

}
