using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Gear SO. Name of SO is the name of the Gear, eg. "Safe House"
/// </summary>
[CreateAssetMenu(menuName = "Gear / Gear")]
public class Gear : ScriptableObject
{
    public string description;

    [Tooltip("Only select an option here if the Gear is restricted to a particular metaLevel, otherwise leave as None (null)")]
    public GlobalMeta metaLevel;  //local / state / national
    public GearRarity rarity;       //common / rare / unique
    public GearType type;

    [Tooltip("Multipurpose datapoint that depends on gear category")]
    public int data;               

    public Sprite sprite;

    [Header("Special Cases")]
    [Tooltip("Any effects for when gear is Used by the player within Inventory. Ignore if none")]
    public List<Effect> listOfPersonalEffects;
    [Tooltip("Any effect for when gear is used by the Player while hacking AI. Ignore if none")]
    public Effect aiHackingEffect;

    [HideInInspector] public int gearID;
    [HideInInspector] public int timesUsed = 0;                 //# of times used in any given turn (reset to zero each turn)
    [HideInInspector] public bool isCompromised = false;        //tested at end of turn (GearManager.cs -> CheckForCompromisedGear) reset if renown spent to retain
    [HideInInspector] public string reasonUsed;                 //tag showing reason gear used (reset each turn), set by GearManager.cs -> SetGearUsed
    [HideInInspector] public int chanceOfCompromise;            //set at time of use, cleared with a new turn

    //stats
    [HideInInspector] public int statTurnObtained = 0;              //turn gear was first obtained
    [HideInInspector] public int statTurnLost = 0;                  //turn gear was lost
    [HideInInspector] public int statTimesUsed = 0;                 //times used (total)
    [HideInInspector] public int statTimesGiven = 0;                //times given to another actor
    [HideInInspector] public int statTimesCompromised = 0;          //times compromised
    [HideInInspector] public int statTimesSaved = 0;                //times compromised gear is Saved
    [HideInInspector] public int statRenownSpent = 0;               //total renown spent on gear


}
