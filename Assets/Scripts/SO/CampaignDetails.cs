using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles specific org / megaCorp / special gear details for a campaign
/// </summary>
[CreateAssetMenu(menuName = "Game / Campaign / Campaign Details")]
public class CampaignDetails : ScriptableObject
{
    [Header("Side")]
    [Tooltip("Which side")]
    public GlobalSide side;

    [Header("Organisations")]
    [Tooltip("Org that provides Cures for any condition that needs them")]
    public Organisation orgCure;
    [Tooltip("Org that provides Elimination Contracts to permanently remove your subordinates")]
    public Organisation orgContract;
    [Tooltip("Org that provides special HQ services such as shutting down investigations")]
    public Organisation orgHQ;
    [Tooltip("Org that provides Emergency services such as get out of jail and safe houses during a crackdown")]
    public Organisation orgEmergency;
    [Tooltip("Org that provides Information services such as location of Nemesis and the cleansing of secret evidence")]
    public Organisation orgInfo;

    [Header("MegaCorporations")]
    [Tooltip("Corresponds to enum.MegaCorpType -> MegaCorpOne")]
    public MegaCorp megaCorpOne;
    [Tooltip("Corresponds to enum.MegaCorpType -> MegaCorpTwo")]
    public MegaCorp megaCorpTwo;
    [Tooltip("Corresponds to enum.MegaCorpType -> MegaCorpThree")]
    public MegaCorp megaCorpThree;
    [Tooltip("Corresponds to enum.MegaCorpType -> MegaCorpFour")]
    public MegaCorp megaCorpFour;
    [Tooltip("Corresponds to enum.MegaCorpType -> MegaCorpFive")]
    public MegaCorp megaCorpFive;

    [Header("Special Gear")]
    [Tooltip("Special gear that is available during MetaGame if Player's relationship with HQ Boss is 2+")]
    public Gear specialBossGear;
    [Tooltip("Special gear that is available during MetaGame if Player's relationship with HQ SubBoss1 is 2+")]
    public Gear specialSubBoss1Gear;
    [Tooltip("Special gear that is available during MetaGame if Player's relationship with HQ SubBoss2 is 2+")]
    public Gear specialSubBoss2Gear;
    [Tooltip("Special gear that is available during MetaGame if Player's relationship with HQ SubBoss3 is 2+")]
    public Gear specialSubBoss3Gear;


    public void OnEnable()
    {
        //Asserts for Resistance side only (Debug measure -> remove once Authority is active)
        if (side.level == 2)
        {
            Debug.Assert(orgCure != null, "Invalid orgCure (Null)");
            Debug.Assert(orgContract != null, "Invalid orgContract (Null)");
            Debug.Assert(orgHQ != null, "Invalid orgHQ (Null)");
            Debug.Assert(orgEmergency != null, "Invalid orgEmergency (Null)");
            Debug.Assert(orgInfo != null, "Invalid orgInfo (Null)");

            Debug.Assert(specialBossGear != null, "Invalid specialGearBoss (Null)");
            Debug.Assert(specialSubBoss1Gear != null, "Invalid specialSubBoss1Gear (Null)");
            Debug.Assert(specialSubBoss2Gear != null, "Invalid specialSubBoss2Gear (Null)");
            Debug.Assert(specialSubBoss3Gear != null, "Invalid specialSubBoss3Gear (Null)");

            Debug.AssertFormat(megaCorpOne != null, "Invalid megaCorpOne (Null) for {0}", this);
            Debug.AssertFormat(megaCorpTwo != null, "Invalid megaCorpTwo (Null) for {0}", this);
            Debug.AssertFormat(megaCorpThree != null, "Invalid megaCorpThree (Null) for {0}", this);
            Debug.AssertFormat(megaCorpFour != null, "Invalid megaCorpFour (Null) for {0}", this);
            Debug.AssertFormat(megaCorpFive != null, "Invalid megaCorpFive (Null) for {0}", this);
        }
    }
}
