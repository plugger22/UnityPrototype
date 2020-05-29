using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using gameAPI;
using delegateAPI;

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
    /// Specific setup for Generic Modal  Menu
    /// </summary>
    public class ModalGenericMenuDetails
    {
        public string itemKey;                                          //dictionary key for stuff that has a string key
        public string itemName;                                         //multipurpose node, Actor or gear name (inGame tag)
        public string itemDetails;
        public int itemID;                                              //multipurpose datapoint, default -1
        public int modalLevel;                                          //what level modal masking do you want? default 1
        public ModalSubState modalState;                                   //modal level to return to once outcome window closes (only for modallevel's 2+, ignore for rest)
        public List<EventButtonDetails> listOfButtonDetails;            //only the first five are used (Target + 1 action / actor)
        public Vector3 menuPos;                                         //position of item in world units (transform)
        public ActionMenuType menuType;                                 //what type of action menu is it? (Node / Actor / Gear, etc.) Ignore if not an Action Menu
        
        public ModalGenericMenuDetails()
        {
            modalLevel = 1;
            modalState = ModalSubState.None;
            itemID = -1;
            itemKey = null;
            listOfButtonDetails = new List<EventButtonDetails>();
        }
    }

    /// <summary>
    /// data fed to ActionManager.cs -> Process 'x' Action to facilitate processing the action
    /// </summary>
    public class ModalActionDetails
    {
        public GlobalSide side;
        public int nodeID;
        public int actorDataID;                                         //for standard actor based node actions, ignore otherwise, Could be actorSlotID or actorID
        public EventType eventType;                                     //event that is triggered (Only used for Recruit Actors, ignore otherwise)
        public int modalLevel;                                          //modal level that the outcome window will be on (same as current), default 1
        public ModalSubState modalState;                                   //modal level to return to once outcome window closes (only for modallevel's 2+, ignore for rest)
        public ActionButtonDelegate handler;                            //method to call once button pressed, ignore if default null
        //special case fields
        public int level;                                               //Authority only: level of actor to recruit (1 to 3)
        public string gearName;                                         //Resistance only: special node gear actions
        public Action gearAction;                                       //Resistance only: Special node Gear actions, eg. gearKinetic
        public int renownCost;                                          //renown cost of an action, eg. dismiss, dispose off

        public ModalActionDetails()
        {
            nodeID = -1;
            actorDataID = -1;
            level = -1;
            gearAction = null;
            gearName = null;
            modalLevel = 1;
            modalState = ModalSubState.None;
        }
    }

    /// <summary>
    /// Main main configuration
    /// </summary>
    public class ModalMainMenuDetails
    {
        public AlignHorizontal alignHorizontal;
        public Background background;                                   //what type of background to display behind menu, default "None"
        public bool isResume;                                           //individual button toggles (default true on all)
        public bool isNewGame;
        public bool isLoadGame;
        public bool isOptions;
        public bool isCustomise;
        public bool isFeedback;
        public bool isCredits;
        public bool isInformation;
        public bool isExit;

        public ModalMainMenuDetails()
        {
            alignHorizontal = AlignHorizontal.Centre;
            background = Background.None;
            isResume = true;
            isNewGame = true;
            isLoadGame = true;
            isOptions = true;
            isFeedback = true;
            isCustomise = true;
            isCredits = true;
            isInformation = true;
            isExit = true;
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
        public int modalLevel;              //modal level of outcome window, default 1
        public ModalSubState modalState;       //modal level to return to once outcome window closes (only for modallevel's 2+, ignore otherwise)
        public bool isAction;               //true if an action has been used
        public string reason;               //short text giving reason for outcome window, eg. "Select Gear" (used for debugging)
        public MsgPipelineType type;        //used for start of turn messages in message Queue (ignore for messages displayed during turn)
        public string help0;                //if help0 specified a help icon will auto appear, otherwise invisible
        public string help1;                //optional
        public string help2;                //optional
        public string help3;                //optional
        public List<Node> listOfNodes;      //optional -> if valid then a 'Show Me' button will appear (replaces Confirm until used)
        public EventType hideEvent;         //only if underlying UI element and a possible ShowMe use. Event to call when ShowMe pressed in Outcome window to hide underlying UI 
        public EventType restoreEvent;      //only if underlying UI element and a possible ShowMe use. Event to call when ShowMe pressed in Outcome window to restore underlying UI 

        public ModalOutcomeDetails()
        {
            modalLevel = 1;
            modalState = ModalSubState.None;
            isAction = false;
            reason = "Unknown";
            side = GameManager.i.sideScript.PlayerSide;
            sprite = GameManager.i.guiScript.infoSprite;
            type = MsgPipelineType.None;
            listOfNodes = new List<Node>();
        }
    }

    /// <summary>
    /// ModalConfirm window data package
    /// </summary>
    public class ModalConfirmDetails
    {
        public string topText;                  //Statement text, eg. 'You have not chosen any options'
        public string bottomText;               //Question text, eg. 'Continue?' (displayed in colourNeutral)
        public string buttonFalse;              //text of left button, default 'No', returns false if selected
        public string buttonTrue;               //text of right button, default 'Yes', returns true if selected
        public int modalLevel;                  //modal level of outcome window, default 1
        public ModalSubState modalState;        //modal level to return to once outcome window closes (only for modallevel's 2+, ignore otherwise)

        public ModalConfirmDetails()
        {
            bottomText = "Are you sure?";
            buttonFalse = "No";
            buttonTrue = "Yes";
            modalLevel = 1;
            modalState = ModalSubState.None;
        }
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
        public string gearName;              //default null if none (Constructor)

        public ModalMoveDetails()
        {
            changeInvisibility = 0;
            ai_Delay = -1;
            gearName = null;
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
        public string textHeader;
        public string textTop;
        public string textMiddle;
        public string textBottom;
        public GlobalSide side;
        public int nodeID;
        public int actorSlotID;
        public string gearName;
        public string help0;                                                                        //optional help, icon displayed only if help0 present
        public string help1;
        public string help2;
        public string help3;
        public int data;                                                                           //general purpose datapoint, can be ignored
        public bool isHaltExecution;                                                               //if true execution is halted until outcome obtained, ignore otherwise
        public ModalGenericPickerSubState subState;                                                         //can be ignored unless needed
        public GenericOptionDetails[] arrayOfOptions = new GenericOptionDetails[3];                 //only the first three are recognised
        public GenericTooltipDetails[] arrayOfTooltips = new GenericTooltipDetails[3];              //same [index] for both arrays. Keep in synch!!

        public GenericPickerDetails()
            { subState = ModalGenericPickerSubState.Normal; }
    }

    /// <summary>
    /// sub class for GenericPickerDetails specifying Option sprite, display text and optionId (value returned if selected)
    /// </summary>
    public class GenericOptionDetails
    {
        public Sprite sprite;
        public string text;                 //keep SHORT
        public int optionID;                //Used when the Generic Picker returns a result which is then processed, eg. teamID, actorID, gearID, etc.
        public string optionText;           //Used instead of optionID where you have multiple nested Generic Pickers, ignore otherwise
        public string optionName;           //Used instead of optionID where you name keys, e.g gear
        public bool isOptionActive;         //if false then option shown greyed out an is unselectable

        public GenericOptionDetails()
        { isOptionActive = true; }
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
        public int optionID;                //Used when the Generic Picker returns a result which is then processed, eg. teamID, actorID, gearID, etc.
        public string optionText;           //used instead of an ID when you multiple nested Generic pickers, ignore otherwise
        public string optionNested;         //Used instead of optionID where you have multiple nested Generic Pickers, ignore otherwise
        public string optionName;           //Used instead of optionID for name orientated objects, eg. gear
        public int nodeID;                  //Used for MANAGE
        public int actorSlotID;             //Used for MANAGE
    }

    /// <summary>
    /// Main class passed to InventoryUI for initilisation
    /// </summary>
    public class InventoryInputData
    {
        public EventType leftClickEvent;                                                        //type of event triggered when any option is left clicked
        public EventType rightClickEvent;                                                       //type of event triggered when any option is right clicked
        public InventoryDelegate handler;                                                       //method to call for refreshing inventory options
        public ModalInventorySubState state;                                                              //enum -> type of Inventory, eg. Gear / Reserve Pool
        public string textHeader;
        public string textTop;
        public string textBottom;
        public string help0;                                                                    //optional help, icon displayed only if help0 present
        public string help1;
        public string help2;
        public string help3;
        public GlobalSide side;
        public GenericOptionData[] arrayOfOptions = new GenericOptionData[4];                               //only the first four are recognised
        public GenericTooltipDetails[] arrayOfTooltipsSprite = new GenericTooltipDetails[4];                //same [index] for both arrays. Keep in synch!! -> Sprite tooltip
        public GenericTooltipDetails[] arrayOfTooltipsStars = new GenericTooltipDetails[4];                 //same [index] for both arrays. Keep in synch!! -> bottom text (Stars) tooltip, optional
        public GenericTooltipDetails[] arrayOfTooltipsCompatibility = new GenericTooltipDetails[4];         //same [index] for both arrays. Keep in synch!! -> top text (Stars) tooltip, optional
    }

    /// <summary>
    /// subclass for InventoryInputData and ReviewInputData detailing specific option information
    /// </summary>
    public class GenericOptionData
    {
        public Sprite sprite;
        public string textTop;                  //used for Compatibility stars, etc
        public string textUpper;                //keep SHORT, name of inventory item, eg. 'FIXER' or 'CHAOS CRITTER'
        public string textLower;                //details that go below name, eg. "Unhappy in 2 turns", motivational stars, etc
        public int optionID;                    //Used when the Generic Picker returns a result which is then processed, eg. teamID, actorID, etc.
        public string optionName;                  //used when Generic Picker returns a result which is then processed, for name key's, eg. gear. Optional
    }

    /// <summary>
    /// Main class passed (internally) to ReviewUI for initialisation
    /// </summary>
    public class ReviewInputData
    {
        public int votesFor;
        public int votesAgainst;
        public int votesAbstained;
        public GenericOptionData[] arrayOfOptions = new GenericOptionData[9];                           //5 x HQ (Boss is twice first up), 4 x Subordinates
        public GenericTooltipDetails[] arrayOfTooltipsSprite = new GenericTooltipDetails[9];                //same [index] for both arrays. Keep in synch!! -> Sprite tooltip
        public GenericTooltipDetails[] arrayOfTooltipsResult = new GenericTooltipDetails[9];                 //same [index] for both arrays. Keep in synch!! -> bottom text (Result) tooltip
    }




}

