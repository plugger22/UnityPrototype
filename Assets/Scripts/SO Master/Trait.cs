using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Actor trait
/// </summary>
[CreateAssetMenu(menuName = "Trait")]
public class Trait : ScriptableObject
{
    public int TraitID { get; set; }                //unique #, zero based -> assigned automatically by DataManager.Initialise
    public Side side;                               //which side does the trait apply to
    public bool isGood = false;

}
