using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Gear SO. Name of SO is the name of the Gear without spaces, eg. "SafeHouse", use tag for in-game name, eg. "Safe House"
/// </summary>
[CreateAssetMenu(menuName = "Gear / Gear")]
public class Gear : ScriptableObject
{
    public string description;
    [Tooltip("Used for in-Game")]
    public string tag;

    [Tooltip("Only select an option here if the Gear is restricted to a particular metaLevel, otherwise leave as None (null)")]
    public GearRarity rarity;       //common / rare / unique
    public GearType type;

    [Tooltip("Multipurpose static datapoint that depends on gear category")]
    public int data;               

    public Sprite sprite;

    [Header("Personal Use effects")]
    [Tooltip("Any effects for when gear is Used by the player within Inventory. Ignore if none. Max ONE Ongoing effect")]
    public List<Effect> listOfPersonalEffects;

    [Header("Hacking AI effects")]
    [Tooltip("Any effect for when gear is used by the Player while hacking AI. Ignore if none")]
    public Effect aiHackingEffect;


    #region Save Data Compatible
    [HideInInspector] public int timesUsed;                     //# of times used in any given turn (reset to zero each turn)
    [HideInInspector] public bool isCompromised;                //tested at end of turn (GearManager.cs -> CheckForCompromisedGear) reset if Power spent to retain
    [HideInInspector] public string reasonUsed;                 //tag showing reason gear used (reset each turn), set by GearManager.cs -> SetGearUsed
    [HideInInspector] public int chanceOfCompromise;            //set at time of use, cleared with a new turn
    [HideInInspector] public int statTurnObtained;              //turn gear was first obtained
    [HideInInspector] public int statTurnLost;                  //turn gear was lost
    [HideInInspector] public int statTimesUsed;                 //times used (total)
    [HideInInspector] public int statTimesGiven;                //times given to another actor
    [HideInInspector] public int statTimesCompromised;          //times compromised
    [HideInInspector] public int statTimesSaved;                //times compromised gear is Saved
    [HideInInspector] public int statPowerSpent;                //total power spent on gear
    #endregion

    /// <summary>
    /// called by GearManager.cs -> Initialise to zero all values as SO's carry values over between sessions
    /// </summary>
    public void ResetStats()
    {
        timesUsed = 0;
        isCompromised = false;
        statTurnObtained = 0;
        statTurnLost = 0;
        statTimesUsed = 0;
        statTimesGiven = 0;
        statTimesCompromised = 0;
        statTimesSaved = 0;
        statPowerSpent = 0;
    }


    public void OnEnable()
    {
        Debug.AssertFormat(sprite != null, "Invalid sprite (Null) for {0}", name);
        Debug.AssertFormat(rarity != null, "Invalid rarity (Null) for {0}", name);
        Debug.AssertFormat(type != null, "Invalid type (Null) for {0}", name);

    }

}
