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

    #region Outcome Panel...
    [Header("Outcome Panel")]
    [Tooltip("Panel Special background (should be 100% alpha)")]
    public Color outcomeSpecial;
    [Tooltip("Black bars that grow out behind panel Special")]
    public Color outcomeBlackBars;
    [Tooltip("Outcome special highlight colour -> Good outcomes, alpha at 75% (191)")]
    public Color outcomeGood;
    [Tooltip("Outcome special highlight colour -> Bad outcomes, alpha at 75% (191)")]
    public Color outcomeBad;
    #endregion

    #region PowerBlocTexts
    [Header("Power Bloc Texts")]
    [Tooltip("Americon")]
    public Color Bloc1;
    [Tooltip("Eurasia")]
    public Color Bloc2;
    [Tooltip("Chinock")]
    public Color Bloc3;
    #endregion

    #region Tutorial Buttons...
    [Header("Tutorial Button Colours")]
    [Tooltip("Dialogue button colour (should be 100% alpha)")]
    public Color tutDialogue;
    [Tooltip("Info button colour (should be 100% alpha)")]
    public Color tutInfo;
    [Tooltip("Question button colour (should be 100% alpha)")]
    public Color tutQuestion;
    [Tooltip("Goal button colour (should be 100% alpha)")]
    public Color tutGoal;
    [Tooltip("Goal button colour once goal Activated (should be 100% alpha)")]
    public Color tutActiveGoal;
    [Tooltip("Tutorial button completed (should be 100% alpha)")]
    public Color tutCompleted;
    #endregion

    #region TopicUI...
    [Header("Topic UI")]
    [Tooltip("Topic UI, main background, for all normal topics")]
    public Color TopicNormal;
    [Tooltip("Topic UI, main background, for Capture topics")]
    public Color TopicCapture;
    #endregion

    #region MainInfoAppUI...
    [Header("MainInfoApp UI")]
    [Tooltip("Main Background")]
    public Color MainBackground;
    [Tooltip("Main Info UI left and right inner backgrounds as well as colour of Active LHS tab")]
    public Color InnerBackground;
    #endregion

    #region MetaGameUI...
    [Header("MetaGame UI")]
    [Tooltip("MetaGame main background colours")]
    public Color MetaBackground;
    #endregion

    #region TopBarUI...
    [Header("Top Bar UI")]
    [Tooltip("Top Bar background")]
    public Color TopBarBackground;
    [Tooltip("Top Bar Icon Dormant colour")]
    public Color IconDormant;
    [Tooltip("Top Bar Icon Active Good colour")]
    public Color IconActiveGood;
    [Tooltip("Top Bar Icon Active Bad colour")]
    public Color IconActiveBad;
    #endregion

    #region TopWidgetUI...
    [Header("Top Widget UI")]
    [Tooltip("Top Widget sprite colour")]
    public Color TopWidget;
    [Tooltip("Top Widget bar backgrounds colour")]
    public Color TopWidgetBarBacks;
    #endregion

    #region TransitionUI...
    [Header("Transition UI")]
    [Tooltip("Transition background colour")]
    public Color TransitionBackground;
    [Tooltip("Transition Header and highlight text colour")]
    public Color TransitionHeader;
    [Tooltip("Transition background Text (that sits ontop of colourTransitionBackground")]
    public Color TransitionText;
    #endregion

    #region TabbedUI...
    [Header("Tabbed UI")]
    [Tooltip("Active side tab colour")]
    public Color TabbedSideTabActive;
    [Tooltip("Dormant side tab colour")]
    public Color TabbedSideTabDormant;
    [Tooltip("Active top tab colour (should be 100% alpha)")]
    public Color TabbedTopTabActive;
    [Tooltip("Dormant top tab colour")]
    public Color TabbedTopTabDormant;
    [Tooltip("Active Top and Controller text colour (should be 100% alpha)")]
    public Color TabbedTextActive;
    [Tooltip("Dormant Top and Controller text colour (should be 100% alpha)")]
    public Color TabbedTextDormant;
    [Tooltip("Inactive Option Text color -> should be 50% (126) alpha")]
    public Color TabbedTextInactive;
    [Tooltip("Sub header background sprite colour, eg. Status, Friends and Enemies, (should be 100% alpha)")]
    public Color TabbedSubHeader;
    [Tooltip("Sub header text colour (should be 100% alpha)")]
    public Color TabbedSubHeaderText;
    [Tooltip("Personality Matrix prefab middle colour (should be 100% alpha)")]
    public Color TabbedPersonMiddle;
    [Tooltip("Personality Matrix prefab middle colour alternate (should be 100% alpha)")]
    public Color TabbedPersonMiddleAlt;
    [Tooltip("Personality Matrix prefab middle colour Active  (should be 100% alpha)")]
    public Color TabbedPersonMiddleActive;
    [Tooltip("Personality Matrix prefab bottom colour (should be 100% alpha)")]
    public Color TabbedPersonBottom;
    [Tooltip("Personality Matrix prefab middle text dormant (should be 100% alpha)")]
    public Color TabbedPersonMiddleTextDormant;
    [Tooltip("Personality Matrix prefab middle text active (should be 100% alpha)")]
    public Color TabbedPersonMiddleTextActive;
    [Tooltip("Personality Matrix prefab bottom text (should be 100% alpha)")]
    public Color TabbedPersonBottomText;
    [Tooltip("History Option image/buttons Dormant colour")]
    public Color TabbedHistoryOptionDormant;
    [Tooltip("History Option image/buttons Active colour")]
    public Color TabbedHistoryOptionActive;
    [Tooltip("Contact prefab background, Active colour (should be 100% alpha)")]
    public Color TabbedContactActive;
    [Tooltip("Contact prefab background, Inactive colour (should be 100% alpha)")]
    public Color TabbedContactInactive;
    [Tooltip("Secret prefab background -> All (should be 100% alpha)")]
    public Color TabbedSecretAll;
    [Tooltip("Gear prefab background -> All (should be 100% alpha)")]
    public Color TabbedGearAll;
    [Tooltip("Capture Tool Devices prefab background -> All (should be 100% alpha)")]
    public Color TabbedDeviceAll;
    [Tooltip("Investigation prefab background -> All (should be 100% alpha)")]
    public Color TabbedInvestigationAll;
    [Tooltip("Organisation prefab background -> All (should be 100% alpha)")]
    public Color TabbedOrgAll;
    [Tooltip("MegaCorp prefab background -> All (should be 100% alpha)")]
    public Color TabbedCorpAll;
    [Tooltip("Statistics item background -> minimal alpha (25)")]
    public Color TabbedStatItem;
    [Tooltip("History Option Light color -> All (should be 100% alpha)")]
    public Color TabbedOptionLight;
    [Tooltip("History Option Dark color -> All (should be 100% alpha)")]
    public Color TabbedOptionDark;
    [Tooltip("History Option Inactive color -> 50% (126) alpha)")]
    public Color TabbedOptionInactive;
    #endregion

    #region MasterHelpUI...
    [Header("Master Help UI")]
    [Tooltip("Text colour (list of help item names) Inactive colour -> All (should be 100% alpha) ")]
    public Color MasterHelpInactive;
    [Tooltip("Text colour (list of help item names) Actve colour -> All (should be 100% alpha)")]
    public Color MasterHelpActive;
    #endregion

    #region FinderUI
    [Header("Finder UI")]
    [Tooltip("Default text colour for finder buttons")]
    public Color FinderDefault;
    [Tooltip("Highlight text colour for finder buttons")]
    public Color FinderHighlight;
    #endregion

}
