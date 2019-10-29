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
    [Tooltip("Optimum distance (in nodes) that the cure location should be from the Resistance player's current node at time of gaining the condition")]
    [Range(1, 10)] public int distance = 4;
    [Tooltip("Tooltip description for Player Action menu. Self contained")]
    [TextArea] public string tooltipText;
    [Tooltip("Outcome message details text")]
    [TextArea] public string outcomeText;

    #region Save Compatible Data
    [HideInInspector] public bool isActive;                     //cure only useable if active (can be present but not yet active, eg. awaiting HQ approval)
    [HideInInspector] public int timesCured;                    //number of times cure has been used
    [HideInInspector] public bool isOrgActivated;               //true if org activated cure, false other
    #endregion

    /*[HideInInspector] public int cureID;                        //assigned a zero based ID at time of import. Max ID num is LoadManager.cs -> arrayOfCures.Length - 1*/



    /// <summary>
    /// reset persistant data
    /// </summary>
    public void Awake()
    {
        Reset();
    }

    /// <summary>
    /// reset fields
    /// </summary>
    public void Reset()
    {
        isActive = false;
        timesCured = 0;
    }
}
