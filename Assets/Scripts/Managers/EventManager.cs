using System.Collections.Generic;
using UnityEngine;


//Event Manager -> Enum defining all possible game events, order independent
//NOTE: Tooltips are closed by direct method calls for speed, not by events
public enum EventType
{
    None,
    //Options
    ChangeColour,
    ChangeSide,
    UseAction,
    //Game State -> ControlManager.cs
    ExitLevel,
    ExitGame,
    ExitCampaign,
    CreateNewGame,
    NewGameOptions,
    CloseNewGame,
    CreateOptions,
    CloseOptions,
    CreateMetaOverall,
    CloseMetaOverall,
    LoadGame,
    CloseLoadGame,
    ResumeGame,
    ResumeMetaGame,
    SaveGame,
    SaveGameAndReturn,
    SaveToMain,
    TutorialOptions,
    CloseTutorialOptions,
    TutorialReturn,
    GameReturn,
    CloseSaveGame,
    SaveAndExit,
    //UI Menus
    CloseActionMenu,
    CloseMainMenu,
    OpenMainMenu,
    CreateMoveMenu,
    CreateGearNodeMenu,
    //ModalOutcome
    OutcomeClose,
    OutcomeOpen,
    OutcomeShowMe,
    OutcomeRestore,
    //ModalConfirm
    ConfirmOpen,
    ConfirmCloseLeft,
    ConfirmCloseRight,
    //highlighted node or connection
    FlashNodesStart,   
    FlashNodesStop,
    FlashSingleConnectionStart,
    FlashSingleConnectionStop,
    FlashMultipleConnectionsStart,
    FlashMultipleConnectionsStop,
    //CloseTeamPicker
    ShowTargetNodes,
    NodeDisplay,
    ActivityDisplay,
    //Top Widget
    ChangeActionPoints,
    ChangeCityBar,
    ChangeHqBar,
    /*ChangeStarLeft,
    ChangeStarMiddle,
    ChangeStarRight,*/
    ChangeTurn,
    StartSecurityFlash,
    StopSecurityFlash,
    //Actions
    NodeAction,
    NodeGearAction,
    TargetAction,
    TargetInfoAction,
    InsertTeamAction,
    RecallTeamAction,
    NeutraliseTeamAction,
    GearAction,
    RecruitAction,
    MoveAction,
    ManageActorAction,
    LeaveActorAction,
    LeavePlayerAction,
    LieLowActorAction,
    LieLowPlayerAction,
    ActivateActorAction,
    ActivatePlayerAction,
    CurePlayerAction,
    GiveGearAction,
    TakeGearAction,
    UseGearAction,
    //decisions
    RecruitDecision,
    //turn manager
    StartTurnEarly,
    StartTurnLate,
    EndTurnAI,
    EndTurnEarly,
    EndTurnLate,
    NewTurn,
    //team Picker
    OpenTeamPicker,
    CloseTeamPicker,
    DeselectOtherTeams,
    ConfirmTeamActivate,
    ConfirmTeamDeactivate,
    ConfirmTeamChoice,
    //generic picker
    OpenGenericPicker,
    CloseGenericPicker,
    DeselectOtherGenerics,
    ConfirmGenericActivate,
    ConfirmGenericDeactivate,
    ConfirmGenericChoice,
    CancelButtonGeneric,
    BackButtonGeneric,
    //generic picker returns
    GenericTeamRecall,
    GenericNeutraliseTeam,
    GenericTargetInfo,
    GenericGearChoice,
    GenericCompromisedGear,
    GenericRecruitActorResistance,
    GenericRecruitActorAuthority,
    GenericHandleActor,
    GenericReserveActor,
    GenericDismissActor,
    GenericDisposeActor,
    //Inventory UI
    InventorySetReserve,
    InventorySetGear,
    InventorySetHQ,
    InventorySetDevice,
    InventoryOpenUI,
    InventoryCloseUI,
    InventoryReassure,
    InventoryThreaten,
    InventoryLetGo,
    InventoryFire,
    InventoryActiveDuty,
    InventoryShowMe,
    InventoryRestore,
    //Review UI
    ReviewOpenUI,
    ReviewCloseUI,
    ReviewStart,
    //topBarUI
    TopBarShow,
    TopBarHide,

    #region diceArchive
    /*//Dice UI
    OpenDiceUI,
    CloseDiceUI,
    DiceIgnore,
    DiceAuto,
    DiceRoll,
    DiceConfirm,
    DiceRenownYes,
    DiceRenownNo,
    DiceBypass,
    DiceReturnMove,
    DiceReturnGear,*/
    #endregion

    //Tabbed UI
    TabbedOpen,
    TabbedClose,
    TabbedSideTabOpen,
    TabbedSubordinates,
    TabbedPlayer,
    TabbedHq,
    TabbedReserves,
    TabbedUpArrow,
    TabbedDownArrow,
    TabbedRightArrow,
    TabbedLeftArrow,
    TabbedPageUp,
    TabbedPageDown,
    TabbedOpenPage,
    TabbedScrollUp,
    TabbedScrollDown,
    TabbedGearNormal,
    TabbedGearCapture,
    TabbedOrganisation,
    TabbedMegaCorp,
    TabbedHistoryEvents,
    TabbedHistoryEmotions,          //mood or option history depending on actor or player
    TabbedHistoryMegaCorps,
    TabbedHistoryMoves,
    //Main Info
    MainInfoOpen,
    MainInfoOpenInterim,
    MainInfoClose,
    MainInfoTabOpen,
    MainInfoShowDetails,
    MainInfoHome,
    MainInfoEnd,
    MainInfoBack,
    MainInfoForward,
    MainInfoUpArrow,
    MainInfoDownArrow,
    MainInfoLeftArrow,
    MainInfoRightArrow,
    MainInfoShowMe,
    MainInfoRestore,
    MainInfoTickerFaster,
    MainInfoTickerSlower,
    //MetaGame UI
    MetaGameClose,
    MetaGameSideTabOpen,
    MetaGameTopTabOpen,
    MetaGameOpen,
    MetaGameShowDetails,
    MetaGameSelect,
    MetaGameDeselect,
    MetaGameButton,
    MetaGameReset,
    MetaGameRecommended,
    MetaGameConfirm,
    MetaGameUpArrow,
    MetaGameDownArrow,
    MetaGameLeftArrow,
    MetaGameRightArrow,
    MetaGamePageUp,
    MetaGamePageDown,
    //TransitionUI
    TransitionOpen,
    TransitionClose,
    TransitionContinue,
    TransitionBack,
    TransitionHqEvents,
    TransitionObjectives,
    //Topic UI
    TopicDisplayOpen,
    TopicDisplayClose,
    TopicDisplayRestore,
    TopicDisplayShowMe,
    TopicDisplayOption,
    TopicDisplayIgnore,
    //Billboard
    BillboardClose,
    //Advert
    AdvertClose,
    //ShowMe
    ShowMeRestore,
    //ActorPanelUI
    ActorInfo,
    //City Info
    CityInfoOpen,
    CityInfoClose,
    //AI Info Display
    AIDisplayOpen,
    AIDisplayPanelOpen,
    AIDisplayClose,
    AISideTabOpen,
    AISideTabClose,
    AISendDisplayData,
    AISendHackingData,
    AISendSideData,
    //AI
    Capture,
    /*ReleasePlayer,*/
    ReleaseActor
};

//EventManager to send events to listeners
//Works with IListener implementations
public class EventManager : MonoBehaviour
{
    public static EventManager i = null;

    // Declare a delegate type for events
    public delegate void OnEvent(EventType Event_Type, Component Sender, object Param = null);

    //Array of listener objects (all objects registered to listen for events)
    private Dictionary<EventType, List<OnEvent>> dictOfListeners = new Dictionary<EventType, List<OnEvent>>();


    /// <summary>
    /// Internal initialisation
    /// </summary>
    private void Awake()
    {
        //check if instance already exists
        if (i == null)
        { i = this; }
        //if instance already exists and it's not this
        else if (i != this)
        {
            //Then destroy this in order to reinforce the singleton pattern (can only ever be one instance of GameManager)
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Function to add specified listener-object to array of listeners
    /// </summary>
    /// <param name="eventType">Event to Listen for</param>
    /// <param name="Listener">Object to listen for event</param>
    /// <param name="methodName">Optional -> name of method posting notification (for debugging purposes)</param>
    public void AddListener(EventType eventType, OnEvent Listener, string methodName = "Unknown")
    {
        //List of listeners for this event
        List<OnEvent> ListenList = null;
        //New item to be added. Check for existing event type key. If one exists, add to list
        if (dictOfListeners.TryGetValue(eventType, out ListenList))
        {
            //List exists, so add new item (if not already present -> prevents duplicates in the case of a followOn level)
            if (ListenList.Exists(x => x == Listener) == false)
            { ListenList.Add(Listener); }
            /*else { Debug.LogFormat("[Tst] EventManager.cs -> AddListeners: {0} NOT added (duplicate){1}", eventType, "\n"); }*/
        }
        else
        {
            //Otherwise create new list as dictionary key
            ListenList = new List<OnEvent>();
            ListenList.Add(Listener);
            //Add to internal listeners list
            dictOfListeners.Add(eventType, ListenList);
        }
        Debug.Log(string.Format("[Evm] -> Listener Added -> type: {0},  sender: {1}{2}", eventType, methodName, "\n"));
    }

    /// <summary>
    /// Function to post event to listeners
    /// </summary>
    /// <param name="eventType">Event to invoke</param>
    /// <param name="Sender">Object invoking event</param>
    /// <param name="Param">Optional argument</param>
    /// <param name="methodName">Optional -> name of method posting notification (for debugging purposes)</param>
    public void PostNotification(EventType eventType, Component Sender, object Param = null, string methodName = "Unknown")
    {
        //Notify all listeners of an event

        //List of listeners for this event only
        List<OnEvent> ListenList = null;

        //If no event entry exists, then exit because there are no listeners to notify
        if (!dictOfListeners.TryGetValue(eventType, out ListenList))
        {
            Debug.LogWarningFormat("EventManager: Invalid event (No listeners present in dictOfListeners) for \"{0}\"", eventType);
            return;
        }

        //Entry exists. Now notify appropriate listeners
        for (int i = 0; i < ListenList.Count; i++)
        {
            if (ListenList[i] != null)
            {
                if (Param != null)
                { Debug.Log(string.Format("[Evm]: PostNotification -> type: {0}, param: {1}, {2}, sender: {3}{4}", eventType, Param.ToString(), Param.GetType(), methodName, "\n")); }
                else { Debug.Log(string.Format("[Evm]: PostNotification -> type: {0}, NO param, sender: {1}{2}", eventType, methodName, "\n")); }
                //If object is not null, then send message via delegate
                ListenList[i](eventType, Sender, Param);
            }
        }
    }


    //Remove event type entry from dictionary, including all listeners
    public void RemoveEvent(EventType eventType)
    {
        //Remove entry from dictionary
        dictOfListeners.Remove(eventType);
    }


    //Remove all redundant entries from the Dictionary
    public void RemoveRedundancies()
    {
        //Create new dictionary
        Dictionary<EventType, List<OnEvent>> TmpListeners = new Dictionary<EventType, List<OnEvent>>();

        //Cycle through all dictionary entries
        foreach (KeyValuePair<EventType, List<OnEvent>> Item in dictOfListeners)
        {
            //Cycle through all listener objects in list, remove null objects
            for (int i = Item.Value.Count - 1; i >= 0; i--)
            {
                //If null, then remove item
                if (Item.Value[i] == null)
                {
                    Debug.LogWarningFormat("EventManager.cs -> RemoveRedundancies: eventType {0} removed from dict as Null -> Info only", Item.Key);
                    Item.Value.RemoveAt(i);
                }
            }

            //If items remain in list for this notification, then add this to tmp dictionary
            if (Item.Value.Count > 0)
                TmpListeners.Add(Item.Key, Item.Value);
        }

        //Replace listeners object with new, optimized dictionary
        dictOfListeners = TmpListeners;
    }


}
