using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using gameAPI;

namespace modalAPI
{
    /// <summary>
    /// Standard Event button details -> part of Modal Panel Details below
    /// </summary>
    public class EventButtonDetails
    {
        public string buttonTitle;
        public string buttonTooltipHeader;
        public string buttonTooltipMain;
        public string buttonTooltipDetail;
        public UnityAction action;
    }


    /// <summary>
    /// Specific setup for Modal Action Menu
    /// </summary>
    public class ModalPanelDetails
    {
        public string nodeName;
        public string nodeDetails;
        public int nodeID;
        public List<EventButtonDetails> listOfButtonDetails;            //only the first five are used (Target + 1 action / actor)
        public Vector3 nodePos;                                         //position of node in world units (transform)
    }

    /// <summary>
    /// data fed to ActionManager.cs -> ProcessNodeAction to facilitate processing the node action
    /// </summary>
    public class ModalActionDetails
    {
        public Side side;
        public int NodeID { get; set; }
        public int ActorSlotID { get; set; }
        public EventType EventType { get; set; }                       //event that is triggered when action button clicked
        public int Level { get; set; }                                 //Authority only: level of actor to recruit (1 to 3)
    }


    /// <summary>
    /// data fed from ProcessNodeAction -> ModalOutcome.cs -> SetModalOutcome to populate the window with data
    /// </summary>
    public class ModalOutcomeDetails
    {
        public Side side;
        public string textTop;
        public string textBottom;
        public Sprite sprite;
    }

    /// <summary>
    /// data stored from NodeManager.cs -> CreateMoveMenu -> ProcessPlayerMove
    /// </summary>
    public class ModalMoveDetails
    {
        public int nodeID;                  //destination node
        public int connectionID;           
        public int changeInvisibility;      //adjustment to player invisibility
        public int ai_Delay;                //number of turns before AI is notified that player was spotted, default -1 (Constructor)
        public int gearID;                  //default -1 if none (Constructor)
        public int changeGear;              //adjustment to gear variable

        public ModalMoveDetails()
        {
            ai_Delay = -1;
            gearID = -1;
        }
    }

    /// <summary>
    /// data fed to ModalDiceUI
    /// </summary>
    public class ModalDiceDetails
    {
        public int chance;
        public int renownCost;
        public bool isRenown;                                   //true if player renown > 0
        public string topText;
        public PassThroughDiceData passData;                    //ignore if no gear ivvolved
        
    }

    /// <summary>
    /// used to pass through data when gear is involved in a move
    /// </summary>
    public class PassThroughDiceData
    {
        public int nodeID;             
        public int gearID;
        public int renownCost;
        public string text;
    }
    
    /// <summary>
    /// used to return data to the originating class that called the dice roller
    /// </summary>
    public class DiceReturnData
    {
        public bool isSuccess;                  //was result <= chance of Success
        public bool isRenown;                   //did player spend renown?
        public int result;                      //dice result (1d100)
        public DiceOutcome outcome;             //option chosen by Player at start
        public PassThroughDiceData passData;    //ignore if no gear involved
    }

    //
    // - - - Modal Generic Picker - - -
    //

    /// <summary>
    /// main class passed to ModalGenericPicker as an event param to provide initialisation details
    /// </summary>
    public class GenericPickerDetails
    {
        public EventType returnEvent;                //event that is triggered by ModalGenericPicker to return selection to originating class
        public string textTop;
        public string textMiddle;
        public string textBottom;
        public Side side;
        public int nodeID;
        public int actorSlotID;
        public GenericOptionDetails[] arrayOfOptions = new GenericOptionDetails[3];                 //only the first three are recognised
        public GenericTooltipDetails[] arrayOfTooltips = new GenericTooltipDetails[3];              //same [index] for both arrays. Keep in synch!!
    }

    /// <summary>
    /// sub class for GenericPickerDetails specifying Option sprite, display text and optionId (value returned if selected)
    /// </summary>
    public class GenericOptionDetails
    {
        public Sprite sprite;
        public string text;                 //keep SHORT
        public int optionID;
    }

    /// <summary>
    /// sub class for GenericPickerDetails specifying tooltip details for each option
    /// </summary>
    public class GenericTooltipDetails
    {
        public string textHeader;
        public string textMain;
        public string textDetails;
    }

    /// <summary>
    /// used to return data to the originating class once 'Confirm' has been clicked. Part of ButtonInteraction.cs
    /// </summary>
    public class GenericReturnData
    {
        public int optionID;
        public int nodeID;
        public int actorSlotID;                    
    }
}

