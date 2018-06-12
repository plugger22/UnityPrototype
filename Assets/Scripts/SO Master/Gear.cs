﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Gear SO. Name of SO is the name of the Gear, eg. "Safe House"
/// </summary>
[CreateAssetMenu(menuName = "Gear / Gear")]
public class Gear : ScriptableObject
{
    [HideInInspector] public int gearID;

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
    [Tooltip("Any effects for when gear is used by the Player while hacking AI. Ignore if none")]
    public List<Effect> listOfAIEffects;
    


}
