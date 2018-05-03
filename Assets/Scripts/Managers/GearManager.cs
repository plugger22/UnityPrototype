using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using modalAPI;
using gameAPI;
using packageAPI;

/// <summary>
/// handles all gear related matters (Only the Resistance has gear)
/// </summary>
public class GearManager : MonoBehaviour
{
    [Range(0, 4)] public int maxNumOfGear = 3;
    [Tooltip("Whenever a random selection of gear is provided there is a chance * actor Ability of it being a Rare item, otherwise it's the standard Common")]
    [Range(1, 10)] public int chanceOfRareGear = 5;
    [Tooltip("Chance gear will be compromised and be no longer of any benefit after each use")]
    [Range(25, 75)] public int chanceOfCompromise = 50;
    [Tooltip("Benefit obtained in Motivation and Renown Transfer from gifting Common gear to an Actor")]
    [Range(1, 3)] public int gearBenefitCommon = 1;
    [Tooltip("Benefit obtained in Motivation and Renown Transfer from gifting Rare gear to an Actor")]
    [Range(1, 5)] public int gearBenefitRare = 2;
    [Tooltip("Benefit obtained in Motivation and Renown Transfer from gifting Unique gear to an Actor")]
    [Range(1, 10)] public int gearBenefitUnique = 3;

    //used for quick reference  -> rarity
    [HideInInspector] public GearRarity gearCommon;
    [HideInInspector] public GearRarity gearRare;
    [HideInInspector] public GearRarity gearUnique;
    //quick reference -> type
    [HideInInspector] public GearType typeHacking;
    [HideInInspector] public GearType typeInfiltration;
    [HideInInspector] public GearType typeInvisibility;
    [HideInInspector] public GearType typeKinetic;
    [HideInInspector] public GearType typeMovement;
    [HideInInspector] public GearType typeRecovery;
    [HideInInspector] public GearType typePersuasion;

    private string colourEffectGood;
    private string colourEffectNeutral;
    private string colourEffectBad;
    private string colourSide;
    private string colourGear;
    private string colourDefault;
    private string colourGrey;
    private string colourNormal;
    private string colourGood;
    private string colourActor;
    private string colourAlert;
    private string colourBad;
    private string colourEnd;


    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        //initialise fast access variables -> rarity
        List<GearRarity> listOfGearRarity = GameManager.instance.dataScript.GetListOfGearRarity();
        if (listOfGearRarity != null)
        {
            foreach(GearRarity rarity in listOfGearRarity)
            {
                //pick out and assign the ones required for fast acess, ignore the rest. 
                //Also dynamically assign gearRarity.level values (0/1/2). Do so here as lots of gear calc's depend on these and they need to be correct
                switch (rarity.name)
                {
                    case "Common":
                        gearCommon = rarity;
                        rarity.level = 0;
                        break;
                    case "Rare":
                        gearRare = rarity;
                        rarity.level = 1;
                        break;
                    case "Unique":
                        gearUnique = rarity;
                        rarity.level = 2;
                        break;
                }
            }
            //error check
            if (gearCommon == null) { Debug.LogError("Invalid gearCommon (Null)"); }
            if (gearRare == null) { Debug.LogError("Invalid gearRare (Null)"); }
            if (gearUnique == null) { Debug.LogError("Invalid gearUnique (Null)"); }
        }
        else { Debug.LogError("Invalid listOfGearRarity (Null)"); }

        //initialise fast access variables -> type
        List<GearType> listOfGearType = GameManager.instance.dataScript.GetListOfGearType();
        if (listOfGearType != null)
        {
            foreach (GearType gearType in listOfGearType)
            {
                //pick out and assign the ones required for fast acess, ignore the rest. 
                switch (gearType.name)
                {
                    case "Hacking":
                        typeHacking = gearType;
                        break;
                    case "Infiltration":
                        typeInfiltration = gearType;
                        break;
                    case "Invisibility":
                        typeInvisibility = gearType;
                        break;
                    case "Kinetic":
                        typeKinetic = gearType;
                        break;
                    case "Movement":
                        typeMovement = gearType;
                        break;
                    case "Recovery":
                        typeRecovery = gearType;
                        break;
                    case "Persuasion":
                        typePersuasion = gearType;
                        break;
                }
            }
            //error check
            if (typeHacking == null) { Debug.LogError("Invalid typeHacking (Null)"); }
            if (typeInfiltration == null) { Debug.LogError("Invalid typeInfiltration (Null)"); }
            if (typeInvisibility == null) { Debug.LogError("Invalid typeInvisibility (Null)"); }
            if (typeKinetic == null) { Debug.LogError("Invalid typeKinetic (Null)"); }
            if (typeMovement == null) { Debug.LogError("Invalid typeMovement (Null)"); }
            if (typeRecovery == null) { Debug.LogError("Invalid typeRecovery (Null)"); }
            if (typePersuasion == null) { Debug.LogError("Invalid typePersuasion (Null)"); }
        }
        else { Debug.LogError("Invalid listOfGearType (Null)"); }

        //initialise gear lists
        Dictionary<int, Gear> dictOfGear = GameManager.instance.dataScript.GetAllGear();
        if (dictOfGear != null)
        {
            int gameLevel = GameManager.instance.metaScript.metaLevel.level;
            //set up an array of Lists, with index corresponding to GearLevel enum, eg. Common / Rare / Unique
            List<int>[] arrayOfGearLists = new List<int>[(int)GameManager.instance.dataScript.GetNumOfGearRarity()];
            for (int i = 0; i < arrayOfGearLists.Length; i++)
            { arrayOfGearLists[i] = new List<int>(); }
            //loop dict and allocate gear to various lists
            foreach (var gearEntry in dictOfGear)
            {
                //check appropriate for metaLevel (same level or 'none', both are acceptable)
                if (gearEntry.Value.metaLevel == null || gearEntry.Value.metaLevel.level == gameLevel)
                {
                    //assign to a list based on rarity
                    int index = gearEntry.Value.rarity.level;
                    arrayOfGearLists[index].Add(gearEntry.Key);
                }
            }
            //initialise dataManager lists with local lists
            for (int i = 0; i < arrayOfGearLists.Length; i++)
            {
                GearRarity indexRarity = GameManager.instance.dataScript.GetGearRarity(i);
                if (indexRarity != null)
                { GameManager.instance.dataScript.SetGearList(arrayOfGearLists[i], indexRarity); }
                else { Debug.LogError("Invalid indexRarity (Null)"); }
            }
        }
        else { Debug.LogError("Invalid dictOfGear (Null) -> Gear not initialised"); }
        //event Listeners
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        EventManager.instance.AddListener(EventType.GearAction, OnEvent);
        EventManager.instance.AddListener(EventType.GenericGearChoice, OnEvent);
        EventManager.instance.AddListener(EventType.InventorySetGear, OnEvent);
    }


    /// <summary>
    /// Called when an event happens
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect Event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.GearAction:
                ModalActionDetails details = Param as ModalActionDetails;
                InitialiseGenericPickerGear(details);
                break;
            case EventType.GenericGearChoice:
                GenericReturnData returnDataGear = Param as GenericReturnData;
                ProcessGearChoice(returnDataGear);
                break;
            case EventType.InventorySetGear:
                InitialiseGearInventoryDisplay();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourEffectGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourEffectNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourEffectBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourSide = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourGrey = GameManager.instance.colourScript.GetColour(ColourType.greyText);
        colourGear = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourActor = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }





    /// <summary>
    /// Choose Gear (Resistance): sets up ModalGenericPicker class and triggers event: ModalGenericEvent.cs -> SetGenericPicker()
    /// </summary>
    /// <param name="details"></param>
    private void InitialiseGenericPickerGear(ModalActionDetails details)
    {
        bool errorFlag = false;
        int gearID, index;
        GlobalSide globalResistance = GameManager.instance.globalScript.sideResistance;
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Node node = GameManager.instance.dataScript.GetNode(details.nodeID);
        if (node != null)
        {
            //check for player/actor being captured
            int actorID = 999;
            if (node.nodeID != GameManager.instance.nodeScript.nodePlayer)
            {
                Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, globalResistance);
                if (actor != null)
                { actorID = actor.actorID; }
                else { Debug.LogError(string.Format("Invalid actor (Null) fro details.ActorSlotID {0}", details.actorDataID)); errorFlag = true; }
            }
            //check capture provided no errors
            if (errorFlag == false)
            {
                CaptureDetails captureDetails = GameManager.instance.captureScript.CheckCaptured(node.nodeID, actorID);
                if (captureDetails != null)
                {
                    //capture happened, abort recruitment
                    captureDetails.effects = string.Format("{0}The contact wasn't there. Nor was the gear.{1}", colourEffectNeutral, colourEnd);
                    EventManager.instance.PostNotification(EventType.Capture, this, captureDetails);
                    return;
                }
            }
            else
            {
                //reset flag to the default state prior to recruitments
                errorFlag = false;
            }
            //Obtain Gear
            genericDetails.returnEvent = EventType.GenericGearChoice;
            genericDetails.side = globalResistance;
            genericDetails.nodeID = details.nodeID;
            genericDetails.actorSlotID = details.actorDataID;
            //picker text
            genericDetails.textTop = string.Format("{0}Gear{1} {2}available{3}", colourEffectNeutral, colourEnd, colourNormal, colourEnd);
            genericDetails.textMiddle = string.Format("{0}Gear will be placed in your inventory{1}",
                colourNormal, colourEnd);
            genericDetails.textBottom = "Click on an item to Select. Press CONFIRM to obtain gear. Mouseover gear for more information.";
            //
            //generate temp list of gear to choose from
            //
            List<int> tempCommonGear = new List<int>(GameManager.instance.dataScript.GetListOfGear(gearCommon));
            List<int> tempRareGear = new List<int>(GameManager.instance.dataScript.GetListOfGear(gearRare));
            //remove from lists any gear that the player currently has
            List<int> tempPlayerGear = new List<int>(GameManager.instance.playerScript.GetListOfGear());
            if (tempPlayerGear.Count > 0)
            {

                for (int i = 0; i < tempPlayerGear.Count; i++)
                {
                    gearID = tempPlayerGear[i];
                    if (tempCommonGear.Exists(id => id == gearID) == true)
                    { tempCommonGear.Remove(gearID); }
                    else if (tempRareGear.Exists(id => id == gearID) == true)
                    { tempRareGear.Remove(gearID); }
                }
            }
            //
            //select two items of gear for the picker
            //
            int[] arrayOfGear = new int[2];
            int countOfGear = 0;
            for (int i = 0; i < arrayOfGear.Length; i++)
            {
                gearID = -1;
                //any rare gear available?
                if (tempRareGear.Count > 0)
                {
                    //chance of rare gear -> base chance * actor ability (or 1 if player)
                    int chance = chanceOfRareGear;
                    Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorDataID, globalResistance);
                    if (actor != null)
                    {
                        //if Player doing it then assumed to have an ability of 1, actor (Fixer) may have a higher ability.
                        if (node.nodeID != GameManager.instance.nodeScript.nodePlayer)
                        { chance *= actor.datapoint2; }
                    }
                    else
                    {
                        chance = 0;
                        Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", details.actorDataID));
                    }
                    if (Random.Range(0, 100) < chance)
                    {
                        index = Random.Range(0, tempRareGear.Count);
                        gearID = tempRareGear[index];
                        tempRareGear.RemoveAt(index);
                    }
                    //if failed chance for rare gear then need to get common
                    else if (tempCommonGear.Count > 0)
                    {
                        index = Random.Range(0, tempCommonGear.Count);
                        gearID = tempCommonGear[index];
                        tempCommonGear.RemoveAt(index);
                    }
                }
                //common gear
                else
                {
                    if (tempCommonGear.Count > 0)
                    {
                        index = Random.Range(0, tempCommonGear.Count);
                        gearID = tempCommonGear[index];
                        tempCommonGear.RemoveAt(index);
                    }
                }
                //found some gear?
                if (gearID > -1)
                { arrayOfGear[i] = gearID; countOfGear++; }
            }
            //check there is at least one item of gear available
            if (countOfGear < 1)
            {
                //OUTCOME -> No gear available
                Debug.LogWarning("GearManager: No gear available in InitaliseGenericPickerGear");
                errorFlag = true;
            }
            else
            {
                //
                //loop gearID's that have been selected and package up ready for ModalGenericPicker
                //
                for (int i = 0; i < countOfGear; i++)
                {
                    Gear gear = GameManager.instance.dataScript.GetGear(arrayOfGear[i]);
                    if (gear != null)
                    {                        
                        //tooltip 
                        GenericTooltipDetails tooltipDetails = GetGearTooltipDetails(gear);
                        if (tooltipDetails != null)
                        {
                            //option details
                            GenericOptionDetails optionDetails = new GenericOptionDetails();
                            optionDetails.optionID = gear.gearID;
                            optionDetails.text = gear.name.ToUpper();
                            optionDetails.sprite = gear.sprite;


                            /*StringBuilder builderHeader = new StringBuilder();
                            builderHeader.Append(string.Format("{0}{1}{2}", colourGear, gear.name.ToUpper(), colourEnd));
                            string colourGearEffect = colourEffectNeutral;
                            if(gear.data == 3) { colourGearEffect = colourEffectGood; }
                            else if (gear.data == 1) { colourGearEffect = colourEffectBad; }
                            //add a second line to the gear header tooltip to reflect the specific value of the gear, appropriate to it's type
                            switch(gear.type.name)
                            {
                                case "Movement":
                                    builderHeader.Append(string.Format("{0}{1}{2}{3}", "\n", colourGearEffect, (ConnectionType)gear.data, colourEnd));
                                    break;
                            }
                            tooltipDetails.textHeader = builderHeader.ToString();
                            tooltipDetails.textMain = string.Format("{0}{1}{2}", colourNormal, gear.description, colourEnd);
                            tooltipDetails.textDetails = string.Format("{0}{1}{2}{3}{4}{5} gear{6}", colourEffectGood, gear.rarity.name, colourEnd, 
                                "\n", colourSide, gear.type.name, colourEnd);*/

                            //add to master arrays
                            genericDetails.arrayOfOptions[i] = optionDetails;
                            genericDetails.arrayOfTooltips[i] = tooltipDetails;
                        }
                        else { Debug.LogError(string.Format("Invalid tooltip Details (Null) for gearID {0}", arrayOfGear[i])); }
                    }
                    else { Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", arrayOfGear[i])); }
                }
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid Node (null) for nodeID {0}", details.nodeID));
            errorFlag = true;
        }
        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = globalResistance;
            outcomeDetails.textTop = "There has been an error in communication and no gear can be sourced.";
            outcomeDetails.textBottom = "Heads will roll!";
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails);
        }
    }

    /// <summary>
    /// sets up all needed data for Resistance Player Gear Inventory and triggers ModalInventoryUI to display such
    /// </summary>
    private void InitialiseGearInventoryDisplay()
    {
        int numOfGear;
        string colourRarity;
        bool errorFlag = false;
        //close node tooltip -> safety check
        GameManager.instance.tooltipNodeScript.CloseTooltip();
        //only for Resistance
        if (GameManager.instance.sideScript.PlayerSide == GameManager.instance.globalScript.sideResistance)
        {
            numOfGear = GameManager.instance.playerScript.CheckNumOfGear();
            //Check for presence of gear
            if (numOfGear > 0)
            {
                //At least one item of gear is present
                InventoryInputData data = new InventoryInputData();
                data.textHeader = "Gear Inventory";
                data.textTop = string.Format("{0}You have {1}{2}{3}{4}{5} out of {6}{7}{8}{9}{10} possible item{11} of Gear{12}", colourEffectNeutral, colourEnd, 
                    colourDefault, numOfGear, colourEnd, colourEffectNeutral, colourEnd, colourDefault, maxNumOfGear, colourEnd, colourEffectNeutral, 
                    maxNumOfGear != 1 ? "s" : "", colourEnd);
                data.textBottom = string.Format("{0}LEFT CLICK{1}{2} Item for Info, {3}{4}RIGHT CLICK{5}{6} Item for Gear Options{7}", colourAlert, colourEnd, 
                    colourDefault, colourEnd, colourAlert, colourEnd, colourDefault, colourEnd);
                data.side = GameManager.instance.sideScript.PlayerSide;
                data.handler = RefreshGearInventory;
                data.state = InventoryState.Gear;
                //Loop Gear list and populate arrays
                List<int> listOfGear = GameManager.instance.playerScript.GetListOfGear();
                if (listOfGear != null)
                {
                    for (int i = 0; i < numOfGear; i++)
                    {
                        Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                        if (gear != null)
                        {
                            GenericTooltipDetails tooltipDetails = GameManager.instance.gearScript.GetGearTooltipDetails(gear);
                            if (tooltipDetails != null)
                            {
                                InventoryOptionData optionData = new InventoryOptionData();
                                optionData.sprite = gear.sprite;
                                optionData.textUpper = gear.name.ToUpper();
                                //colour code Rarity
                                switch (gear.rarity.name)
                                {
                                    case "Common": colourRarity = colourEffectBad; break;
                                    case "Rare": colourRarity = colourEffectNeutral; break;
                                    case "Unique": colourRarity = colourEffectGood; break;
                                    default: colourRarity = colourDefault; break;
                                }
                                optionData.textLower = string.Format("{0}{1}{2}{3}{4}{5}{6}", colourRarity, gear.rarity.name, colourEnd, "\n",
                                    colourDefault, gear.type.name, colourEnd);
                                optionData.optionID = gear.gearID;

                                /*//tooltip 
                                GenericTooltipDetails tooltipDetails = new GenericTooltipDetails();
                                StringBuilder builderHeader = new StringBuilder();
                                builderHeader.Append(string.Format("{0}{1}{2}", colourGear, gear.name.ToUpper(), colourEnd));
                                string colourGearEffect = colourEffectNeutral;
                                if (gear.data == 3) { colourGearEffect = colourEffectGood; }
                                else if (gear.data == 1) { colourGearEffect = colourEffectBad; }
                                //add a second line to the gear header tooltip to reflect the specific value of the gear, appropriate to it's type
                                switch (gear.type.name)
                                {
                                    case "Movement":
                                        builderHeader.Append(string.Format("{0}{1}{2}{3}", "\n", colourGearEffect, (ConnectionType)gear.data, colourEnd));
                                        break;
                                }
                                tooltipDetails.textHeader = builderHeader.ToString();
                                tooltipDetails.textMain = string.Format("{0}{1}{2}", colourNormal, gear.description, colourEnd);
                                tooltipDetails.textDetails = string.Format("{0}{1}{2}{3}{4}{5} gear{6}", colourEffectGood, gear.rarity.name, colourEnd,
                                    "\n", colourSide, gear.type.name, colourEnd);*/

                                //add to array
                                data.arrayOfOptions[i] = optionData;
                                data.arrayOfTooltips[i] = tooltipDetails;
                            }
                            else
                            { Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", listOfGear[i])); }
                        }
                        else
                        { Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", listOfGear[i])); }
                    }
                }
                else
                {
                    Debug.LogError("Invalid listOfGear (Null)");
                    errorFlag = true;
                }
                //data package has been populated, proceed if all O.K
                if (errorFlag == true)
                {
                    //error msg
                    ModalOutcomeDetails details = new ModalOutcomeDetails()
                    {
                        side = GameManager.instance.sideScript.PlayerSide,
                        textTop = string.Format("{0}Something has gone wrong with your Inventory{1}", colourAlert, colourEnd),
                        textBottom = "Phone calls are being made. Lots of them.",
                        sprite = GameManager.instance.guiScript.errorSprite,
                        isAction = false
                    };
                    EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details);
                }
                else
                {
                    //open Inventory UI
                    EventManager.instance.PostNotification(EventType.InventoryOpenUI, this, data);
                }
            }
            else
            {
                //No actor in reserve
                ModalOutcomeDetails details = new ModalOutcomeDetails()
                {
                    side = GameManager.instance.sideScript.PlayerSide,
                    textTop = string.Format("{0}There is currently no gear in your inventory{1}", colourAlert, colourEnd),
                    textBottom = string.Format("You can have a maximum of {0} items of Gear in your inventtory", maxNumOfGear),
                    sprite = GameManager.instance.guiScript.infoSprite,
                    isAction = false
                };
                EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details);
            }
        }
        else
        {
            //Authority message
            ModalOutcomeDetails details = new ModalOutcomeDetails()
            {
                side = GameManager.instance.sideScript.PlayerSide,
                textTop = string.Format("{0}Gear is only available when you are playing the Resistance side{1}", colourAlert, colourEnd),
                sprite = GameManager.instance.guiScript.infoSprite,
                isAction = false
            };
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details);
        }
    }

    /// <summary>
    /// Called as a delegate method -> Recalculates option data in InventoryUI after an action has been taken
    /// </summary>
    public InventoryInputData RefreshGearInventory()
    {
        int numOfGear;
        string colourRarity;
        InventoryInputData data = new InventoryInputData();
        //close node tooltip -> safety check
        GameManager.instance.tooltipNodeScript.CloseTooltip();
        //only for Resistance
        if (GameManager.instance.sideScript.PlayerSide == GameManager.instance.globalScript.sideResistance)
        {
            numOfGear = GameManager.instance.playerScript.CheckNumOfGear();
            //At least one item of gear is present
            data.textTop = string.Format("{0}You have {1}{2}{3}{4}{5} out of {6}{7}{8}{9}{10} possible item{11} of Gear{12}", colourEffectNeutral, colourEnd,
                colourDefault, numOfGear, colourEnd, colourEffectNeutral, colourEnd, colourDefault, maxNumOfGear, colourEnd, colourEffectNeutral,
                maxNumOfGear != 1 ? "s" : "", colourEnd);
            if (numOfGear > 0)
            {
                data.textBottom = string.Format("{0}LEFT CLICK{1}{2} Item for Info, {3}{4}RIGHT CLICK{5}{6} Item for Gear Options{7}", colourAlert, colourEnd,
                    colourDefault, colourEnd, colourAlert, colourEnd, colourDefault, colourEnd);
            }
            else { data.textBottom = ""; }
            data.handler = RefreshGearInventory;
            //Loop Gear list and populate arrays
            List<int> listOfGear = GameManager.instance.playerScript.GetListOfGear();
            if (listOfGear != null)
            {
                for (int i = 0; i < numOfGear; i++)
                {
                    Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                    if (gear != null)
                    {
                        GenericTooltipDetails tooltipDetails = GameManager.instance.gearScript.GetGearTooltipDetails(gear);
                        if (tooltipDetails != null)
                        {
                            InventoryOptionData optionData = new InventoryOptionData();
                            optionData.sprite = gear.sprite;
                            optionData.textUpper = gear.name.ToUpper();
                            //colour code Rarity
                            switch (gear.rarity.name)
                            {
                                case "Common": colourRarity = colourEffectBad; break;
                                case "Rare": colourRarity = colourEffectNeutral; break;
                                case "Unique": colourRarity = colourEffectGood; break;
                                default: colourRarity = colourDefault; break;
                            }
                            optionData.textLower = string.Format("{0}{1}{2}{3}{4}{5}{6}", colourRarity, gear.rarity.name, colourEnd, "\n",
                                colourDefault, gear.type.name, colourEnd);
                            optionData.optionID = gear.gearID;

                            /*//tooltip 
                            GenericTooltipDetails tooltipDetails = new GenericTooltipDetails();
                            StringBuilder builderHeader = new StringBuilder();
                            builderHeader.Append(string.Format("{0}{1}{2}", colourGear, gear.name.ToUpper(), colourEnd));
                            string colourGearEffect = colourEffectNeutral;
                            if (gear.data == 3) { colourGearEffect = colourEffectGood; }
                            else if (gear.data == 1) { colourGearEffect = colourEffectBad; }
                            //add a second line to the gear header tooltip to reflect the specific value of the gear, appropriate to it's type
                            switch (gear.type.name)
                            {
                                case "Movement":
                                    builderHeader.Append(string.Format("{0}{1}{2}{3}", "\n", colourGearEffect, (ConnectionType)gear.data, colourEnd));
                                    break;
                            }
                            tooltipDetails.textHeader = builderHeader.ToString();
                            tooltipDetails.textMain = string.Format("{0}{1}{2}", colourNormal, gear.description, colourEnd);
                            tooltipDetails.textDetails = string.Format("{0}{1}{2}{3}{4}{5} gear{6}", colourEffectGood, gear.rarity.name, colourEnd,
                                "\n", colourSide, gear.type.name, colourEnd);*/

                            //add to array
                            data.arrayOfOptions[i] = optionData;
                            data.arrayOfTooltips[i] = tooltipDetails;
                        }
                        else
                        { Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", listOfGear[i])); }
                    }
                    else
                    { Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", listOfGear[i])); }
                }
            }
            else
            { Debug.LogError("Invalid listOfGear (Null)"); }
        }
        return data;
    }

    /// <summary>
    /// Processes choice of Gear
    /// </summary>
    /// <param name="returnDetails"></param>
    public void ProcessGearChoice(GenericReturnData data)
    {
        bool successFlag = true;
        bool isInvisibility = false;
        if (data.optionID > -1)
        {
            //get currently selected node
            if (data.nodeID != -1)
            {
                Gear gear = GameManager.instance.dataScript.GetGear(data.optionID);
                if (gear != null)
                {
                    //check whether it is invisible type gear (if so then invisibility -1 effect is ignored)
                    if (gear.type.name.Equals("Invisibility") == true)
                    { isInvisibility = true; }
                    Sprite sprite = gear.sprite;
                    Node node = GameManager.instance.dataScript.GetNode(data.nodeID);
                    if (node != null)
                    {
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, GameManager.instance.globalScript.sideResistance);
                        if (actor != null)
                        {
                            StringBuilder builderTop = new StringBuilder();
                            StringBuilder builderBottom = new StringBuilder();

                            if (GameManager.instance.playerScript.AddGear(gear.gearID) == true)
                            {
                                //gear successfully acquired
                                builderTop.Append(string.Format("{0}We have the goods!{1}", colourNormal, colourEnd));
                                builderBottom.Append(string.Format("{0}{1}{2}{3} is in our possession{4}", colourGear, gear.name.ToUpper(), colourEnd,
                                    colourDefault, colourEnd));
                                //message
                                string textMsg = string.Format("{0} ({1}) has been acquired at {2}", gear.name, gear.type.name, node.nodeName);
                                Message messageGear = GameManager.instance.messageScript.GearObtained(textMsg, node.nodeID, gear.gearID);
                                if (messageGear != null) { GameManager.instance.dataScript.AddMessage(messageGear); }
                                //Process any other effects, if acquisition was successfull, ignore otherwise
                                Action action = actor.arc.nodeAction;
                                List<Effect> listOfEffects = action.GetEffects();
                                if (listOfEffects.Count > 0)
                                {
                                    EffectDataInput dataInput = new EffectDataInput();
                                    foreach (Effect effect in listOfEffects)
                                    {
                                        if (effect.ignoreEffect == false)
                                        {
                                            //ignore invisiblity effect in case of fixer/player getting invisibility gear 
                                            if (isInvisibility == true && effect.outcome.name.Equals("Invisibility") == true)
                                            { Debug.Log(string.Format("ProcessGearChoice: {0} effect ignored due to Invisibility{1}", effect.name, "\n")); }
                                            else
                                            {
                                                //process effect normally
                                                EffectDataReturn effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput, actor);
                                                if (effectReturn != null)
                                                {
                                                    builderTop.AppendLine();
                                                    builderTop.Append(effectReturn.topText);
                                                    if (builderBottom.Length > 0)
                                                    { builderBottom.AppendLine(); builderBottom.AppendLine(); }
                                                    builderBottom.Append(effectReturn.bottomText);
                                                }
                                                else { Debug.LogError("Invalid effectReturn (Null)"); }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //Problem occurred, gear not acquired
                                builderTop.Append("Problem occured, gear NOT obtained");
                                builderBottom.Append("Who did this? B*stard!");
                                successFlag = false;
                            }
                            //OUTCOME Window
                            ModalOutcomeDetails details = new ModalOutcomeDetails();
                            details.textTop = builderTop.ToString();
                            details.textBottom = builderBottom.ToString();
                            details.sprite = sprite;
                            details.side = GameManager.instance.globalScript.sideResistance;
                            if (successFlag == true)
                            { details.isAction = true; }
                            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details);
                        }
                        else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", data.actorSlotID)); }
                    }
                    else { Debug.LogError(string.Format("Invalid node (Null) for NodeID {0}", data.nodeID)); }
                }
                else { Debug.LogError(string.Format("Invalid Gear (Null) for teamID {0}", data.optionID)); }
            }
            else { Debug.LogError("Highlighted node invalid (default '-1' value)"); }
        }
        else { Debug.LogError("Invalid gearID (default '-1')"); }
    }

    /// <summary>
    /// returns chance of gear being compromised. Depends on gear rarity. Returns '0' is a problem.
    /// </summary>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public int GetChanceOfCompromise(int gearID)
    {
        int chance = 0;
        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
        if (gear != null)
        {
            
            //chance of compromise varies depending on gear rarity
            switch (gear.rarity.name)
            {
                case "Unique":
                    //halved chance
                    chance = chanceOfCompromise / 2;
                    break;
                case "Rare":
                case "Common":
                    chance = chanceOfCompromise;
                    break;
            }
        }
        else
        {
            //problem. Default compromised.
            Debug.LogWarning(string.Format("Invalid gear (Null) for gearID {0}", gearID));
        }
        return chance;
    }



    /// <summary>
    /// submethod to handle gear comprised for ProcessPlayerMove & others (node and Gear not tested for null as already checked in calling method)
    /// </summary>
    /// <param name="gear"></param>
    /// <returns></returns>
    public string GearUsedAndCompromised(Gear gear, Node node)
    {
        //remove gear from inventory
        GameManager.instance.playerScript.RemoveGear(gear.gearID);
        //message -> gear compromised
        string textMsg = string.Format("{0}, ID {1} has been comprised", gear.name, gear.gearID);
        Message messageGear = GameManager.instance.messageScript.GearCompromised(textMsg, node.nodeID, gear.gearID);
        if (messageGear != null) { GameManager.instance.dataScript.AddMessage(messageGear); }
        //return text string for builder
        return string.Format("{0}{1}{2}{3} has been compromised!{4}", "\n", "\n", colourEffectBad, gear.name, colourEnd);
    }

    /// <summary>
    /// submethod to handle gear being used but NOT compromised (node and Gear are checked for null by the calling method)
    /// </summary>
    /// <param name="gear"></param>
    /// <param name="node"></param>
    public string GearUsed(Gear gear, Node node)
    {
        /*//message -> redundant
        string textMsg = string.Format("{0}, ID {1} has been used", gear.name, gear.gearID);
        Message messageGear = GameManager.instance.messageScript.GearUsed(textMsg, node.nodeID, gear.gearID);
        if (messageGear != null) { GameManager.instance.dataScript.AddMessage(messageGear); }*/
        return string.Format("{0}{1}{2}Gear can be reused{3}", "\n", "\n", colourGear, colourEnd);
    }

    /// <summary>
    /// subMethod to handle admin for Player renown expenditure
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public string RenownUsed(Gear gear, Node node, int amount)
    {
        //update player renown
        int renown = GameManager.instance.playerScript.Renown;
        renown -= amount;
        GameManager.instance.playerScript.Renown = renown;
        //message

        string textMsg = string.Format("{0}, ID {1} has been compromised. Saved by using {2} Renown.", gear.name, gear.gearID, amount);
        Message messageRenown = GameManager.instance.messageScript.RenownUsedPlayer(textMsg, node.nodeID, gear.gearID);
        if (messageRenown != null) { GameManager.instance.dataScript.AddMessage(messageRenown); }
        //return text string for builder
        return string.Format("{0}{1}{2}Gear saved, Renown -{3}{4}", "\n", "\n", colourEffectBad, amount, colourEnd);
    }

    /// <summary>
    /// returns a data package of 3 formatted strings ready to slot into a gear tooltip. Null if a problem.
    /// </summary>
    /// <param name="gearID"></param>
    /// <returns></returns>
    public GenericTooltipDetails GetGearTooltipDetails(Gear gear)
    {
        GenericTooltipDetails details = null;
        if (gear != null)
        {
            details = new GenericTooltipDetails();
            StringBuilder builderHeader = new StringBuilder();
            StringBuilder builderDetails = new StringBuilder();
            builderHeader.Append(string.Format("{0}{1}{2}", colourGear, gear.name.ToUpper(), colourEnd));
            string colourGearEffect = colourEffectNeutral;
            if (gear.data == 3) { colourGearEffect = colourEffectGood; }
            else if (gear.data == 1) { colourGearEffect = colourEffectBad; }
            //add a second line to the gear header tooltip to reflect the specific value of the gear, appropriate to it's type
            switch (gear.type.name)
            {
                case "Movement":
                    builderHeader.Append(string.Format("{0}{1}{2}{3}", "\n", colourGearEffect, (ConnectionType)gear.data, colourEnd));
                    break;
            }
            //Node use
            builderHeader.AppendLine();
            switch(gear.type.name)
            {
                case "Hacking":
                case "Kinetic":
                case "Persuasion":
                    builderHeader.Append(string.Format("{0}<size=90%>Node use? Yes{1}", colourAlert, colourEnd));
                    break;
                default:
                    builderHeader.Append(string.Format("{0}<size=80%>Node use? No{1}", colourGrey, colourEnd));
                    break;
            }
            //gear use
            builderHeader.AppendLine();
            builderHeader.Append(string.Format("{0}Gift use? Yes{1}", colourAlert, colourEnd));
            //personal use
            builderHeader.AppendLine();
            if (gear.listOfPersonalEffects != null && gear.listOfPersonalEffects.Count > 0)
            { builderHeader.Append(string.Format("{0}Personal use? Yes{1}", colourAlert, colourEnd)); }
            else
            { builderHeader.Append(string.Format("{0}Personal use? No{1}", colourGrey, colourEnd)); }
            //details
            builderDetails.Append(string.Format("{0}{1}{2}", colourEffectGood, gear.rarity.name, colourEnd));
            builderDetails.AppendLine();
            builderDetails.Append(string.Format("{0}{1} gear{2}", colourSide, gear.type.name, colourEnd));

            //data package
            details.textHeader = builderHeader.ToString();
            details.textMain = string.Format("{0}{1}{2}", colourNormal, gear.description, colourEnd);
            details.textDetails = builderDetails.ToString();
        }
        return details;
    }

    //new methods above here
}
