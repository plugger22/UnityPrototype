using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Condition Cures. Name of SO is the name of the cure, eg. "Medic"
/// </summary>
[CreateAssetMenu(menuName = "Actor / Cure")]
public class Cure : ScriptableObject
{
    [Tooltip("Condition for which this is a cure")]
    public Condition condition;
    [Tooltip("In game name")]
    public string cureName;


    [HideInInspector] public int cureID;                         //assigned a zero based ID at time of import. Max ID num is LoadManager.cs -> arrayOfCures.Length - 1


}
