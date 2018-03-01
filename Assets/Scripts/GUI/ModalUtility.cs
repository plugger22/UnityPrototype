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
        public string itemName;                                         //multipurpose node, Actor or gear name
        public string itemDetails;
        public int itemID;
        public List<EventButtonDetails> listOfButtonDetails;            //only the first five are used (Target + 1 action / actor)
        public Vector3 itemPos;                                         //position of item in world units (transform)
        public ActionMenuType menuType;                                 //what type of action menu is it? (Node / Actor / Gear, etc.)
    }

    /// <summary>
    /// data fed to ActionManager.cs -> Process 'x' Action to facilitate processing the node action
    /// </summary>
    public class ModalActionDetails
    {
        public GlobalSide side;
        public int nodeID;
        public int actorSlotID;                                         //for standard actor based node actions, ignore otherwise
        public EventType eventType;                                     //event that is triggered (Only used for Recruit Actors, ignore otherwise)

        //special case fields
        public int level;                                               //Authority only: level of actor to recruit (1 to 3)
        public int gearID;                                              //Resistance only: special node gear actions
        public Action gearAction;                                       //Resistance only: Special node Gear actions, eg. gearKinetic

        public ModalActionDetails()
        {
            nodeID = -1;
            actorSlotID = -1;
            level = -1;
            gearAction = null;
            gearID = -1;
        }
    }


    /// <summary>
    /// data fed from ProcessNodeAction -> ModalOutcome.cs -> SetModalOutcome to populate the window with data
    /// </summary>
    public class ModalOutcomeDetails
    {
        public GlobalSide side;
        public string textTop;
        public string textBottom;
        public Sprite sprite;
        public bool isAction;               //true if an action has been used

        public ModalOutcomeDetails()
        { isAction = false; }
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
        public bool isEnoughRenown;                             //true if player renown > 0
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
        public DiceType type;                                   //what is the dice being rolled for?
        public ModalOutcomeDetails outcome;                     //used for gear effect outcomes, ignore for rest

        public PassThroughDiceData()
        {
            outcome = null;
        }
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
        public GlobalSide side;
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
        [Tooltip("Used when the Generic Picker returns a result which is then processed, eg. teamID, actorID, gearID, etc.")]
        public int optionID;
        [Tooltip("Used instead of optionID where you have multiple nested Generic Pickers, ignore otherwise")]
        public string optionText;
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
        [Tooltip("Used when the Generic Picker returns a result which is then processed, eg. teamID, actorID, gearID, etc.")]
        public int optionID;
        [Tooltip("Used instead of optionID where you have multiple nested Generic Pickers, ignore otherwise")]
        public string optionText;
        public int nodeID;
        public int actorSlotID;                    
    }

    //
    // - - - AI return
    //

    /// <summary>
    /// General Purpose data container to send to AI functions, eg. CaptureActor
    /// </summary>
    public class AIDetails
    {
        public Node node;
        public Team team;
        public Actor actor;
        public string effects;                  //carry over effects for a combined outcome window, eg insert tracer and then get captured
    }
}

