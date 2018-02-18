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


    private string colourEffectGood;
    private string colourEffectNeutral;
    private string colourEffectBad;
    private string colourSide;
    private string colourGear;
    private string colourDefault;
    private string colourNormal;
    private string colourGood;
    private string colourActor;
    private string colourBad;
    private string colourEnd;


    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        Dictionary<int, Gear> dictOfGear = GameManager.instance.dataScript.GetAllGear();
        if (dictOfGear != null)
        {
            int gameLevel = (int)GameManager.instance.turnScript.metaLevel;
            //set up an array of Lists, with index corresponding to GearLevel enum, eg. Common / Rare / Unique
            List<int>[] arrayOfGearLists = new List<int>[(int)GearLevel.Count];
            for (int i = 0; i < arrayOfGearLists.Length; i++)
            { arrayOfGearLists[i] = new List<int>(); }
            //loop dict and allocate gear to various lists
            foreach (var gearEntry in dictOfGear)
            {
                //check appropraite for metaLevel (same level or 'none', both are acceptable)
                if (gearEntry.Value.metaLevel == MetaLevel.None || (int)gearEntry.Value.metaLevel == gameLevel)
                {
                    //assign to a list based on rarity
                    arrayOfGearLists[(int)gearEntry.Value.rarity].Add(gearEntry.Key);
                }
            }
            //initialise dataManager lists with local lists
            for (int i = 0; i < arrayOfGearLists.Length; i++)
            { GameManager.instance.dataScript.SetGearList(arrayOfGearLists[i], (GearLevel)i); }
        }
        else { Debug.LogError("Invalid dictOfGear (Null) -> Gear not initialised"); }
        //event Listeners
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        EventManager.instance.AddListener(EventType.GearAction, OnEvent);
        EventManager.instance.AddListener(EventType.GenericGearChoice, OnEvent);
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
        colourGear = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
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
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Node node = GameManager.instance.dataScript.GetNode(details.NodeID);
        if (node != null)
        {
            //check for player/actor being captured
            int actorID = 999;
            if (node.nodeID != GameManager.instance.nodeScript.nodePlayer)
            {
                Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.ActorSlotID, Side.Resistance);
                if (actor != null)
                { actorID = actor.actorID; }
                else { Debug.LogError(string.Format("Invalid actor (Null) fro details.ActorSlotID {0}", details.ActorSlotID)); errorFlag = true; }
            }
            //check capture provided no errors
            if (errorFlag == false)
            {
                AIDetails aiDetails = GameManager.instance.captureScript.CheckCaptured(node.nodeID, actorID);
                if (aiDetails != null)
                {
                    //capture happened, abort recruitment
                    aiDetails.effects = string.Format("{0}The contact wasn't there. Nor was the gear.{1}", colourEffectNeutral, colourEnd);
                    EventManager.instance.PostNotification(EventType.Capture, this, aiDetails);
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
            genericDetails.side = Side.Resistance;
            genericDetails.nodeID = details.NodeID;
            genericDetails.actorSlotID = details.ActorSlotID;
            //picker text
            genericDetails.textTop = string.Format("{0}Gear{1} {2}available{3}", colourEffectNeutral, colourEnd, colourNormal, colourEnd);
            genericDetails.textMiddle = string.Format("{0}Gear will be placed in your inventory{1}",
                colourNormal, colourEnd);
            genericDetails.textBottom = "Click on an item to Select. Press CONFIRM to obtain gear. Mouseover gear for more information.";
            //
            //generate temp list of gear to choose from
            //
            List<int> tempCommonGear = new List<int>(GameManager.instance.dataScript.GetListOfGear(GearLevel.Common));
            List<int> tempRareGear = new List<int>(GameManager.instance.dataScript.GetListOfGear(GearLevel.Rare));
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
                    Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.ActorSlotID, Side.Resistance);
                    if (actor != null)
                    {
                        //if Player doing it then assumed to have an ability of 1, actor (Fixer) may have a higher ability.
                        if (node.nodeID != GameManager.instance.nodeScript.nodePlayer)
                        { chance *= actor.datapoint2; }
                    }
                    else
                    {
                        chance = 0;
                        Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", details.ActorSlotID));
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
                        //option details
                        GenericOptionDetails optionDetails = new GenericOptionDetails();
                        optionDetails.optionID = gear.gearID;
                        optionDetails.text = gear.name.ToUpper();
                        optionDetails.sprite = gear.sprite;
                        //tooltip 
                        GenericTooltipDetails tooltipDetails = new GenericTooltipDetails();
                        StringBuilder builderHeader = new StringBuilder();
                        builderHeader.Append(string.Format("{0}{1}{2}", colourGear, gear.name.ToUpper(), colourEnd));
                        string colourGearEffect = colourEffectNeutral;
                        if(gear.data == 3) { colourGearEffect = colourEffectGood; }
                        else if (gear.data == 1) { colourGearEffect = colourEffectBad; }
                        //add a second line to the gear header tooltip to reflect the specific value of the gear, appropriate to it's type
                        switch(gear.type)
                        {
                            case GearTypeEnum.Movement:
                                builderHeader.Append(string.Format("{0}{1}{2}{3}", "\n", colourGearEffect, (ConnectionType)gear.data, colourEnd));
                                break;
                        }
                        tooltipDetails.textHeader = builderHeader.ToString();
                        tooltipDetails.textMain = string.Format("{0}{1}{2}", colourNormal, gear.description, colourEnd);
                        tooltipDetails.textDetails = string.Format("{0}{1}{2}{3}{4}{5} gear{6}", colourEffectGood, gear.rarity, colourEnd, 
                            "\n", colourSide, gear.type, colourEnd);
                        //add to master arrays
                        genericDetails.arrayOfOptions[i] = optionDetails;
                        genericDetails.arrayOfTooltips[i] = tooltipDetails;
                    }
                    else { Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", arrayOfGear[i])); }
                }
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid Node (null) for nodeID {0}", details.NodeID));
            errorFlag = true;
        }
        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = Side.Resistance;
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
    /// Processes choice of Gear
    /// </summary>
    /// <param name="returnDetails"></param>
    public void ProcessGearChoice(GenericReturnData data)
    {
        bool successFlag = true;
        if (data.optionID > -1)
        {
            //get currently selected node
            if (data.nodeID != -1)
            {
                Gear gear = GameManager.instance.dataScript.GetGear(data.optionID);
                
                if (gear != null)
                {
                    Sprite sprite = gear.sprite;
                    Node node = GameManager.instance.dataScript.GetNode(data.nodeID);
                    if (node != null)
                    {
                        Actor actor = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, Side.Resistance);
                        if (actor != null)
                        {
                            StringBuilder builderTop = new StringBuilder();
                            StringBuilder builderBottom = new StringBuilder();

                            if (GameManager.instance.playerScript.AddGear(data.optionID) == true)
                            {
                                //gear successfully acquired
                                builderTop.Append(string.Format("{0}We have the goods!{1}", colourNormal, colourEnd));
                                builderBottom.Append(string.Format("{0}{1}{2}{3} is in our possession{4}", colourGear, gear.name.ToUpper(), colourEnd,
                                    colourDefault, colourEnd));
                                //message
                                string textMsg = string.Format("{0}, ID {1}, ({2}) has been acquired", gear.name, gear.gearID, gear.type, node.nodeID);
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
                                            EffectDataReturn effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput, actor);
                                            if (effectReturn != null)
                                            {
                                                builderTop.AppendLine();
                                                builderTop.Append(effectReturn.topText);
                                                builderBottom.AppendLine();
                                                builderBottom.AppendLine();
                                                builderBottom.Append(effectReturn.bottomText);
                                            }
                                            else { Debug.LogError("Invalid effectReturn (Null)"); }
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
                            details.side = Side.Resistance;
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
            switch (gear.rarity)
            {
                case GearLevel.Unique:
                    //halved chance
                    chance = chanceOfCompromise / 2;
                    break;
                case GearLevel.Rare:
                case GearLevel.Common:
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


    //new methods above here
}
