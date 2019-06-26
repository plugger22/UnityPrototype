using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles all personality related matters
/// </summary>
public class PersonalityManager : MonoBehaviour
{

    [Tooltip("Number of personality factors present (combined total of Five Factor Model and Dark Triad factors)")]
    [Range(8, 8)] public int numOfFactors = 8;
	
}
