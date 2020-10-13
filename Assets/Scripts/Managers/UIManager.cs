﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a unified colour scheme for UI elements
/// </summary>
public class UIManager : MonoBehaviour
{
    [Tooltip("Palette of colours from which all other colours derive (all UI elements must use a colour in the palette to ensure a consistent look)")]
    public List<Color> listOfColours;

    
    [Header("Topic UI")]
    [Tooltip("Topic UI, main background, for all normal topics")]
    public Color TopicNormal;
    [Tooltip("Topic UI, main background, for Capture topics")]
    public Color TopicCapture;

    [Header("MainInfoApp UI")]
    [Tooltip("Main Info UI left and right backgrounds as well as colour of Active LHS tab")]
    public Color MainBackground;

    [Header("MetaGame UI")]
    [Tooltip("MetaGame main background colours")]
    public Color MetaBackground;

    [Header("Top Bar UI")]
    [Tooltip("Top Bar background")]
    public Color TopBarBackground;
    [Tooltip("Top Bar Icon Dormant colour")]
    public Color IconDormant;
    [Tooltip("Top Bar Icon Active Good colour")]
    public Color IconActiveGood;
    [Tooltip("Top Bar Icon Active Bad colour")]
    public Color IconActiveBad;

    [Header("Top Widget UI")]
    [Tooltip("Top Widget sprite colour")]
    public Color TopWidget;
    [Tooltip("Top Widget bar backgrounds colour")]
    public Color TopWidgetBarBacks;

    [Header("Transition UI")]
    [Tooltip("Transition background colour")]
    public Color TransitionBackground;
    [Tooltip("Transition Header and highlight text colour")]
    public Color TransitionHeader;
    [Tooltip("Transition background Text (that sits ontop of colourTransitionBackground")]
    public Color TransitionText;
}
