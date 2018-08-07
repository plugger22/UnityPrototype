using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all start game Player interactions
/// </summary>
public class StartManager : MonoBehaviour
{

    [Tooltip("If doing an AI vs. AI test run, how many turns do you want to process")]
    [Range(1, 100)] public int aiTestRun = 10;
}
