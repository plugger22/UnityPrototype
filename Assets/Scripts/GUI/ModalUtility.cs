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

}

