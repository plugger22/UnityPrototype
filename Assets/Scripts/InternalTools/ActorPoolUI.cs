using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using toolsAPI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runs actorPoolUI in internal tools scene
/// </summary>
public class ActorPoolUI : MonoBehaviour
{
    #region public
    [Header("Main UI Components")]
    public Canvas actorCanvas;
    public Image actorPanel;
    public Image dataPanel;
    public Image confirmPanel;

    [Header("Button Interactions")]
    public ToolButtonInteraction newPoolInteraction;
    public ToolButtonInteraction savePoolInteraction;
    public ToolButtonInteraction deletePoolInteraction;
    public ToolButtonInteraction quitPoolInteraction;
    public ToolButtonInteraction createPoolInteraction;
    public ToolButtonInteraction confirmCancelInteraction;
    public ToolButtonInteraction confirmDeleteInteraction;
    public ToolButtonInteraction dataInteraction0;
    public ToolButtonInteraction dataInteraction1;
    public ToolButtonInteraction dataInteraction2;
    public ToolButtonInteraction dataInteraction3;

    [Header("Buttons that toggle")]
    public Button createPoolButton;

    [Header("Pool texts")]
    public TextMeshProUGUI poolName;
    public TextMeshProUGUI poolNameSet;
    public TextMeshProUGUI poolSide;
    public TextMeshProUGUI poolAuthor;
    public TextMeshProUGUI poolDate;
    public TMP_InputField poolNameInput;
    public TMP_InputField poolTagInput;
    public TMP_InputField poolAuthorInput;
    public TMP_InputField poolDateInput;

    [Header("Actor Display")]
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textArc;
    public TextMeshProUGUI textStatus;
    public TextMeshProUGUI textSex;
    public Image actorPortrait;
    public TMP_InputField actorFirstName;
    public TMP_InputField actorLastName;
    public TMP_InputField actorLevel;
    public TMP_InputField actorPower;
    public TMP_InputField backstory0;
    public TMP_InputField backstory1;

    [Header("Confirm Panel")]
    public TextMeshProUGUI confirmText;

    [Header("Data Panel")]
    public TextMeshProUGUI dataHeader;
    public TextMeshProUGUI dataText;

    [Header("Drop down lists")]
    public TMP_Dropdown dropInputPool;
    public TMP_Dropdown dropInputNameSet;
    public TMP_Dropdown dropInputSide;
    public TMP_Dropdown dropInputTrait;

    #endregion

    #region private
    private int dropIndexPool;
    private int dropIndexTrait;
    private int dropIndexNameSet;
    private int dropIndexSide;
    private int actorDraftIndex;
    private int maxActorDraftIndex;
    private string dropStringPool;
    private string dropStringTrait;
    private string dropStringNameSet;
    private string dropStringSide;
    private ActorPool poolObject;
    private ActorDraft actorObject;
    #endregion

    #region Collections
    private List<string> listOfTraitOptions = new List<string>();
    private List<string> listOfPoolOptions = new List<string>();
    private List<string> listOfNameSetOptions = new List<string>();
    private List<string> listOfSideOptions = new List<string>();
    private List<ActorPool> listOfActorPools = new List<ActorPool>();
    private List<ActorDraft> listOfActorDrafts = new List<ActorDraft>();
    private List<NameSet> listOfNameSets = new List<NameSet>();
    private List<Trait> listOfTraits = new List<Trait>();
    List<GlobalSide> listOfSides = new List<GlobalSide>();

    #endregion

    #region static Instance
    //static reference
    private static ActorPoolUI actorPoolUI;

    /// <summary>
    /// provide a static reference to ActorPoolUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ActorPoolUI Instance()
    {
        if (!actorPoolUI)
        {
            actorPoolUI = FindObjectOfType(typeof(ActorPoolUI)) as ActorPoolUI;
            if (!actorPoolUI)
            { Debug.LogError("There needs to be one active actorPoolUI script on a GameObject in your scene"); }
        }
        return actorPoolUI;
    }
    #endregion

    #region Initialise -> Master
    /// <summary>
    /// Master Initialiser
    /// </summary>
    public void Initialise()
    {
        InitialiseAsserts();
        InitialiseOther();
        InitialiseEvents();
        InitialiseButtons();
        InitialiseDropDownPool();
        InitialiseActorDraftList();
        InitialiseActorInputFields();
        InitialiseDropDownNameSet();
        InitialiseDropDownSide();
        InitialiseDropDownTraits();
    }
    #endregion

    #region InitialiseOther
    /// <summary>
    /// Initialisation of items not covered by any other Method
    /// </summary>
    private void InitialiseOther()
    {
        listOfTraits = ToolManager.i.actorScript.GetListOfTraits();
        if (listOfTraits != null)
        {
            //
            // - - - traits
            //
            listOfTraitOptions = listOfTraits.Select(txt => txt.tag).ToList();
            //set options
            if (listOfTraitOptions == null)
            { Debug.LogError("Invalid listOfOptions (Null)"); }
            //
            // - - - nameSets
            //
            listOfNameSets = ToolManager.i.jointScript.arrayOfNameSets.ToList();
            if (listOfNameSets != null)
            {
                //names of nameSets
                listOfNameSetOptions = listOfNameSets.Select(txt => txt.name).ToList();
            }
            else { Debug.LogError("Invalid listOfNameSets (Null)"); }
            //
            // - - - side
            //
            listOfSides = ToolManager.i.jointScript.arrayOfGlobalSide.ToList();
            if (listOfSides != null)
            {
                //reverse sort list to make 'Resistance' the default
                var sorted = listOfSides.OrderByDescending(x => x.level);
                listOfSides = sorted.ToList();
                //names of Sides
                listOfSideOptions = listOfSides.Select(txt => txt.name).ToList();
                if (listOfSideOptions == null)
                { Debug.LogError("Invalid listOfOptions (Null)"); }
            }
            else { Debug.LogError("Invalid listOfSides (Null)"); }
        }
        else { Debug.LogError("Invalid listOfTraits (Null)"); }
    }
    #endregion

    #region InitialiseButtons
    /// <summary>
    /// Initialise Buttons
    /// </summary>
    private void InitialiseButtons()
    {
        newPoolInteraction.SetButton(ToolEventType.NewPoolUI);
        savePoolInteraction.SetButton(ToolEventType.SavePoolUI);
        deletePoolInteraction.SetButton(ToolEventType.DeletePoolUI);
        quitPoolInteraction.SetButton(ToolEventType.CloseActorPoolUI);
        createPoolInteraction.SetButton(ToolEventType.CreatePoolUI);
        confirmCancelInteraction.SetButton(ToolEventType.DeletePoolCancel);
        confirmDeleteInteraction.SetButton(ToolEventType.DeletePoolConfirm);
    }
    #endregion

    #region InitialiseAsserts
    /// <summary>
    /// Fast access
    /// </summary>
    public void InitialiseAsserts()
    {
        Debug.Assert(actorCanvas != null, "Invalid actorCanvas (Null)");
        Debug.Assert(actorPanel != null, "Invalid actorPanel (Null)");
        Debug.Assert(dataPanel != null, "Invalid dataPanel (Null)");
        Debug.Assert(confirmPanel != null, "Invalid confirmPanel (Null)");
        //buttons
        Debug.Assert(newPoolInteraction != null, "Invalid newPoolInteraction (Null)");
        Debug.Assert(savePoolInteraction != null, "Invalid savePoolInteraction (Null)");
        Debug.Assert(deletePoolInteraction != null, "Invalid deletePoolInteraction (Null)");
        Debug.Assert(quitPoolInteraction != null, "Invalid quitPoolInteraction (Null)");
        Debug.Assert(createPoolInteraction != null, "Invalid createPoolInteraction (Null)");
        Debug.Assert(createPoolButton != null, "Invalid createPoolButton (Null)");
        Debug.Assert(confirmCancelInteraction != null, "Invalid confirmCancelInteraction (Null)");
        Debug.Assert(confirmDeleteInteraction != null, "Invalid confirmDeleteInteraction (Null)");
        Debug.Assert(dataInteraction0 != null, "Invalid dataInteraction0 (Null)");
        Debug.Assert(dataInteraction1 != null, "Invalid dataInteraction1 (Null)");
        Debug.Assert(dataInteraction2 != null, "Invalid dataInteraction2 (Null)");
        Debug.Assert(dataInteraction3 != null, "Invalid dataInteraction3 (Null)");
        Debug.Assert(dataHeader != null, "Invalid dataHeader (Null)");
        Debug.Assert(dataText != null, "Invalid dataText (Null)");
        //pool texts
        Debug.Assert(poolName != null, "Invalid poolName (Null)");
        Debug.Assert(poolNameSet != null, "Invalid poolNameSet (Null)");
        Debug.Assert(poolSide != null, "Invalid poolSide (Null)");
        Debug.Assert(poolAuthor != null, "Invalid poolAuthor (Null)");
        Debug.Assert(poolDate != null, "Invalid poolDate (Null)");
        Debug.Assert(poolNameInput != null, "Invalid poolNameInput (Null)");
        Debug.Assert(poolTagInput != null, "Invalid poolTagInput (Null)");
        Debug.Assert(poolAuthorInput != null, "Invalid poolAuthorInput (Null)");
        Debug.Assert(poolDateInput != null, "Invalid poolDateInput (Null)");
        //actor texts
        Debug.Assert(textName != null, "Invalid textName (Null)");
        Debug.Assert(textArc != null, "Invalid textArc (Null)");
        Debug.Assert(textStatus != null, "Invalid textStatus (Null)");
        Debug.Assert(textSex != null, "Invalid textSex (Null)");
        Debug.Assert(actorPortrait != null, "Invalid actorPortrait (Null)");
        Debug.Assert(actorFirstName != null, "Invalid actorFirstName (Null)");
        Debug.Assert(actorLastName != null, "Invalid actorLastName (Null)");
        Debug.Assert(actorLevel != null, "Invalid actorLevel (Null)");
        Debug.Assert(actorPower != null, "Invalid actorPower (Null)");
        Debug.Assert(backstory0 != null, "Invalid backstory0 (Null)");
        Debug.Assert(backstory1 != null, "Invalid backstory1 (Null)");
        //confirm texts
        Debug.Assert(confirmText != null, "Invalid confirmText (Null)");
        //drop down lists
        Debug.Assert(dropInputPool != null, "Invalid dropInputPool (Null)");
        Debug.Assert(dropInputNameSet != null, "Invalid dropInputNameSet (Null)");
        Debug.Assert(dropInputSide != null, "Invalid dropInputSide (Null)");
        Debug.Assert(dropInputTrait != null, "Invalid dropInputTrait (Null)");
    }
    #endregion

    #region InitialiseEvents
    /// <summary>
    /// Initialise all event listeners
    /// </summary>
    private void InitialiseEvents()
    {
        //listeners
        ToolEvents.i.AddListener(ToolEventType.OpenActorPoolUI, OnEvent, "ActorPoolUI");
        ToolEvents.i.AddListener(ToolEventType.CloseActorPoolUI, OnEvent, "ActorPoolUI");
        ToolEvents.i.AddListener(ToolEventType.NewPoolUI, OnEvent, "ActorPoolUI");
        ToolEvents.i.AddListener(ToolEventType.CreatePoolUI, OnEvent, "ActorPoolUI");
        ToolEvents.i.AddListener(ToolEventType.SavePoolUI, OnEvent, "ActorPoolUI");
        ToolEvents.i.AddListener(ToolEventType.DeletePoolUI, OnEvent, "ActorPoolUI");
        ToolEvents.i.AddListener(ToolEventType.NextActorDraft, OnEvent, "ActorPoolUI");
        ToolEvents.i.AddListener(ToolEventType.PreviousActorDraft, OnEvent, "ActorPoolUI");
        ToolEvents.i.AddListener(ToolEventType.DeletePoolCancel, OnEvent, "ActorPoolUI");
        ToolEvents.i.AddListener(ToolEventType.DeletePoolConfirm, OnEvent, "ActorPoolUI");
    }
    #endregion

    #region Events
    /// <summary>
    /// handles events
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(ToolEventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case ToolEventType.OpenActorPoolUI:
                OpenActorPoolUI();
                break;
            case ToolEventType.NewPoolUI:
                NewPoolUI();
                break;
            case ToolEventType.CreatePoolUI:
                CreatePoolUI();
                break;
            case ToolEventType.SavePoolUI:
                SavePoolUI();
                break;
            case ToolEventType.DeletePoolUI:
                DeletePoolUI();
                break;
            case ToolEventType.NextActorDraft:
                NextActor();
                break;
            case ToolEventType.PreviousActorDraft:
                PreviousActor();
                break;
            case ToolEventType.CloseActorPoolUI:
                CloseActorPoolUI();
                break;
            case ToolEventType.DeletePoolCancel:
                CancelDeletePool();
                break;
            case ToolEventType.DeletePoolConfirm:
                ConfirmDeletePool();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion

    #region Actions...
    //
    // - - - Actions
    //

    #region OpenActorPoolUI
    /// <summary>
    /// opens ActorPoolUI
    /// </summary>
    private void OpenActorPoolUI()
    {
        //turn on
        ToolManager.i.toolUIScript.CloseTools();
        actorCanvas.gameObject.SetActive(true);
        actorPanel.gameObject.SetActive(true);
        dataPanel.gameObject.SetActive(true);
        confirmPanel.gameObject.SetActive(false);
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.ActorPool);
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Edit);
        UpdateSidePanel(false);
    }
    #endregion

    #region New PoolUI
    /// <summary>
    /// Create a new pool and auto populate with ActorDrafts
    /// </summary>
    private void NewPoolUI()
    {
        UpdateSidePanel(true);
        //Set Modal state
        ToolManager.i.toolInputScript.SetModalType(ToolModalType.Input);
        //toggle on button
        createPoolButton.gameObject.SetActive(true);
        //toggle off actor Panel
        actorPanel.gameObject.SetActive(false);

    }
    #endregion

    #region Create PoolUI
    /// <summary>
    /// Once data is entered for a new pool, press button to create
    /// </summary>
    private void CreatePoolUI()
    {
        ActorPoolData data = new ActorPoolData();

        //populate data package
        data.poolName = poolNameInput.text;
        data.tag = poolTagInput.text;
        data.side = listOfSides.Find(x => x.name.Equals(dropStringSide, StringComparison.Ordinal));
        data.nameSet = listOfNameSets.Find(x => x.name.Equals(dropStringNameSet, StringComparison.Ordinal));
        data.author = poolAuthorInput.text;
        data.dateCreated = poolDateInput.text;

        //error check prior to activating button
        bool isProceed = true;
        if (string.IsNullOrEmpty(data.poolName) == true) { isProceed = false; Debug.LogWarning("Invalid data.poolName (Null or Empty)"); }
        if (string.IsNullOrEmpty(data.tag) == true) { isProceed = false; Debug.LogWarning("Invalid data.tag (Null or Empty)"); }
        if (string.IsNullOrEmpty(data.author) == true) { isProceed = false; Debug.LogWarning("Invalid data.author (Null or Empty)"); }
        if (string.IsNullOrEmpty(data.dateCreated) == true) { isProceed = false; Debug.LogWarning("Invalid data.dateCreated (Null or Empty)"); }
        if (data.nameSet == null) { isProceed = false; Debug.LogWarning("Invalid data.nameSet (Null)"); }
        if (data.side == null) { isProceed = false; Debug.LogWarning("Invalid data.side (Null)"); }

        //new ActorPool only if all data present
        if (isProceed == true)
        {
            dropStringPool = data.poolName;
            //toggle actor panel back on
            actorPanel.gameObject.SetActive(true);
            //create pool and actorDrafts in SO/Temp folder
            ToolManager.i.actorScript.CreateActorPool(data);
            //disable button
            createPoolButton.gameObject.SetActive(false);
            //swap fields
            UpdateSidePanel(false);
            //swap modes
            ToolManager.i.toolInputScript.SetModalType(ToolModalType.Edit);
            //Make new actor pool current
            UpdateListOfActorPools();
            UpdatePoolObject();
            //Update actor drafts
            InitialiseActorDraftList();
            UpdateActorDraft();
            //drop down Pool list
            InitialiseDropDownPool(false);
        }
        else { Debug.LogWarning("Actor Pool NOT created due to invalid data"); }

    }
    #endregion

    #region Save PoolUI
    /// <summary>
    /// Save actorPool data -> Writes to Campaign.SO
    /// </summary>
    private void SavePoolUI()
    {

    }
    #endregion

    #region Delete Pool
    /// <summary>
    /// Deletes actor pool -> Opens up confirmation dialogue, eg. 'Are you sure?'
    /// </summary>
    private void DeletePoolUI()
    {
        if (poolObject != null)
        {
            //check if proceed
            actorPanel.gameObject.SetActive(false);
            dataPanel.gameObject.SetActive(false);
            confirmPanel.gameObject.SetActive(true);
            //confirm text
            confirmText.text = string.Format("Delete ActorPool{0}<size=120%>{1}</size>{2}Are you sure?", "\n", poolObject.tag, "\n");
        }
        else { Debug.LogWarningFormat("Invalid poolObject (Null)"); }
    }
    #endregion

    #region CancelDeletePool
    /// <summary>
    /// Abort delete pool operation, return to main menu
    /// </summary>
    private void CancelDeletePool()
    {
        confirmPanel.gameObject.SetActive(false);
        actorPanel.gameObject.SetActive(true);
        dataPanel.gameObject.SetActive(true);
    }
    #endregion

    #region ConfirmDeletePool
    /// <summary>
    /// Confirm delete pool operation, return to main menu, set first pool in list as default
    /// </summary>
    private void ConfirmDeletePool()
    {
        string path;
        //delete actorDrafts -> reverse loop
        for (int i = listOfActorDrafts.Count - 1; i >= 0; i--)
        {
            path = string.Format("Assets/SO/ActorDrafts/{0}.asset", listOfActorDrafts[i].name);
            if (AssetDatabase.DeleteAsset(path) == false)
            { Debug.LogWarningFormat("{0} failed to delete", path); }
        }
        //delete actorPool
        path = string.Format("Assets/SO/ActorPools/{0}.asset", poolObject.name);
        if (AssetDatabase.DeleteAsset(path) == false)
        { Debug.LogWarningFormat("{0} failed to delete", path); }
        //Update 
        confirmPanel.gameObject.SetActive(false);
        actorPanel.gameObject.SetActive(true);
        dataPanel.gameObject.SetActive(true);
        UpdateListOfActorPools();
        InitialiseDropDownPool();
        InitialiseActorDraftList();
        UpdateActorDraft();
        UpdatePoolObject();
    }
    #endregion

    #region NextActor
    /// <summary>
    /// Move down an actor in the listOfActorDrafts -> RIGHT arrow
    /// </summary>
    private void NextActor()
    {
        actorDraftIndex += 1;
        //check for rollover
        if (actorDraftIndex >= maxActorDraftIndex)
        { actorDraftIndex = 0; }
        //update actor
        actorObject = listOfActorDrafts[actorDraftIndex];
        //Update details
        UpdateActorDraft();
    }
    #endregion

    #region PreviousActor
    /// <summary>
    /// Move up an actor in the listOfActorDrafts -> Left arrow
    /// </summary>
    private void PreviousActor()
    {
        actorDraftIndex -= 1;
        //check for rollover
        if (actorDraftIndex < 0)
        { actorDraftIndex = maxActorDraftIndex - 1; }
        //update actor
        actorObject = listOfActorDrafts[actorDraftIndex];
        //Update details
        UpdateActorDraft();
    }
    #endregion

    #region CloseActorPoolUI
    /// <summary>
    /// close ActorPoolUI
    /// </summary>
    private void CloseActorPoolUI()
    {
        ToolManager.i.toolUIScript.OpenTools();
        actorCanvas.gameObject.SetActive(false);
        //set Modal State
        ToolManager.i.toolInputScript.SetModalState(ToolModal.Menu);
    }
    #endregion

    #endregion

    #region ActorDrafts...
    //
    // - - - ActorDrafts
    //

    #region InitialiseActorDraftList
    /// <summary>
    /// sets up actordrafts of currently selected actorPool in an ordered list ready for editing
    /// </summary>
    private void InitialiseActorDraftList()
    {
        actorDraftIndex = 0;
        //Create an ordered list of ActorDrafts within an ActorPool starting with HqBoss0
        listOfActorDrafts.Clear();
        listOfActorDrafts.Add(poolObject.hqBoss0);
        listOfActorDrafts.Add(poolObject.hqBoss1);
        listOfActorDrafts.Add(poolObject.hqBoss2);
        listOfActorDrafts.Add(poolObject.hqBoss3);
        listOfActorDrafts.AddRange(poolObject.listHqWorkers);
        listOfActorDrafts.AddRange(poolObject.listOnMap);
        listOfActorDrafts.AddRange(poolObject.listLevelOne);
        listOfActorDrafts.AddRange(poolObject.listLevelTwo);
        listOfActorDrafts.AddRange(poolObject.listLevelThree);
        //check there is the correct number of actor Drafts
        maxActorDraftIndex = ToolManager.i.actorScript.numOfActors;
        if (listOfActorDrafts.Count != maxActorDraftIndex)
        { Debug.LogWarningFormat("Mismatch on actorPool \"{0}\" count (should be {1}, is {2} ActorDraft SO's)", poolObject.name, maxActorDraftIndex, listOfActorDrafts.Count); }
        else
        {
            actorObject = listOfActorDrafts[actorDraftIndex];
            UpdateActorDraft();
        };
    }
    #endregion

    #region UpdateActorDraft
    /// <summary>
    /// Update and populate page for currently selected ActorDraft
    /// </summary>
    private void UpdateActorDraft()
    {
        if (actorObject != null)
        {
            //labels
            textName.text = actorObject.actorName;
            textArc.text = actorObject.arc.name;
            textStatus.text = actorObject.status.name;
            textSex.text = actorObject.sex.name;
            //sprite
            actorPortrait.sprite = actorObject.sprite;
            //inputs
            actorFirstName.text = actorObject.firstName;
            actorLastName.text = actorObject.lastName;
            actorLevel.text = Convert.ToString(actorObject.level);
            actorPower.text = Convert.ToString(actorObject.power);
            backstory0.text = actorObject.backstory0;
            backstory1.text = actorObject.backstory1;
            //set trait dropDown
            dropIndexTrait = listOfTraitOptions.FindIndex(x => x.Equals(actorObject.trait.tag, StringComparison.Ordinal) == true);
            if (dropIndexTrait > -1)
            { dropInputTrait.value = dropIndexTrait; }
            else { Debug.LogError("Invalid dropIntTrait (-1)"); }
        }
        else { Debug.LogError("Invalid actorObject (Null)"); }
    }
    #endregion

    #endregion

    #region DropDowns...
    //
    // - - - Drop Downs
    //

    #region InitialiseDropDownPool
    /// <summary>
    /// Initialise drop down control for Actor Pools. 'isDefault' true if first item in list is to be default. If false then dropStringPool record is used, eg. in case of newly created pool
    /// </summary>
    private void InitialiseDropDownPool(bool isFirstItemDefault = true)
    {
        //set up base list
        UpdateListOfActorPools();
        //delegate for dropDown
        dropInputPool.onValueChanged.AddListener(delegate { DropDownPoolSelected(); });
        if (isFirstItemDefault == true)
        {
            //reset input fields to defaults
            dropIndexPool = -1;
            dropStringPool = "";
        }
        if (listOfActorPools != null)
        {
            //names of Pools
            listOfPoolOptions = listOfActorPools.Select(txt => txt.name).ToList();
            //set options
            if (listOfPoolOptions != null)
            {
                dropInputPool.options.Clear();
                for (int i = 0; i < listOfPoolOptions.Count; i++)
                { dropInputPool.options.Add(new TMP_Dropdown.OptionData() { text = listOfPoolOptions[i] }); }
            }
            else { Debug.LogError("Invalid listOfOptions (Null)"); }
        }
        else { Debug.LogError("Invalid listOfActorPools (Null)"); }
        if (isFirstItemDefault == true)
        {
            //set index
            dropInputPool.value = -1;
            //assign default as first entry
            dropStringPool = dropInputPool.options[0].text;
        }
        else
        {
            //set list to whatever record corresponds to dropStringPool
            dropIndexPool = listOfPoolOptions.FindIndex(x => x.Equals(dropStringPool, StringComparison.Ordinal));
            if (dropIndexPool > -1)
            {
                dropInputPool.value = dropIndexPool;
            }
            else
            {
                //Not found -> error condition, display default first record instead
                Debug.LogWarningFormat("Invalid dropIndexPool (-1) for dropStringPool \"{0}\"", dropStringPool);
                dropInputPool.value = -1;
                dropStringPool = dropInputPool.options[0].text;
            }
            dropInputPool.RefreshShownValue();
        }
        //Update LHS labels showing data for currently selected pool (which will be the one at the top of the drop down list at initialisation)
        UpdatePoolObject();


    }
    #endregion

    #region InitialiseDropDownNameSet
    /// <summary>
    /// Initialise drop down control for NameSet
    /// </summary>
    private void InitialiseDropDownNameSet()
    {
        //delegate for dropDown
        dropInputNameSet.onValueChanged.AddListener(delegate { DropDownNameSetSelected(); });
        //reset input fields to defaults
        dropIndexNameSet = -1;
        dropStringNameSet = "";
        //set options
        if (listOfNameSetOptions != null)
        {
            dropInputNameSet.options.Clear();
            for (int i = 0; i < listOfNameSetOptions.Count; i++)
            { dropInputNameSet.options.Add(new TMP_Dropdown.OptionData() { text = listOfNameSetOptions[i] }); }
        }
        else { Debug.LogError("Invalid listOfOptions (Null)"); }
        //set index
        dropInputNameSet.value = -1;
    }
    #endregion

    #region InitialiseDropDownSide
    /// <summary>
    /// Initialise drop down control for GlobalSide
    /// </summary>
    private void InitialiseDropDownSide()
    {
        //delegate for dropDown
        dropInputSide.onValueChanged.AddListener(delegate { DropDownSideSelected(); });
        //reset input fields to defaults
        dropIndexSide = -1;
        dropStringSide = "";
        if (listOfSideOptions != null)
        {
            dropInputSide.options.Clear();
            for (int i = 0; i < listOfSideOptions.Count; i++)
            { dropInputSide.options.Add(new TMP_Dropdown.OptionData() { text = listOfSideOptions[i] }); }
        }
        else { Debug.LogError("Invalid listOfOptions (Null)"); }
        //set index
        dropInputSide.value = -1;
    }
    #endregion

    #region InitialiseDropDownTraits
    /// <summary>
    /// Initialise drop down control for Traits
    /// </summary>
    private void InitialiseDropDownTraits()
    {
        //delegate for dropDown
        dropInputTrait.onValueChanged.AddListener(delegate { DropDownTraitSelected(); });

        /*dropStringTrait = "";*/

        //set options
        if (listOfTraitOptions != null)
        {
            dropInputTrait.options.Clear();
            for (int i = 0; i < listOfTraitOptions.Count; i++)
            { dropInputTrait.options.Add(new TMP_Dropdown.OptionData() { text = listOfTraitOptions[i] }); }
        }
        else { Debug.LogError("Invalid listOfOptions (Null)"); }
        //set index
        dropInputTrait.value = dropIndexTrait;
        dropStringTrait = dropInputTrait.options[dropIndexTrait].text;
        dropInputTrait.RefreshShownValue();
    }
    #endregion

    #region DropDownPoolSelected
    /// <summary>
    /// Get option selected from dropDown pick list
    /// </summary>
    private void DropDownPoolSelected()
    {
        int index = dropInputPool.value;
        //set input values
        dropIndexPool = index;
        dropStringPool = dropInputPool.options[index].text;
        UpdatePoolObject();
        //update ActorDrafts
        InitialiseActorDraftList();
        //update actor details
        UpdateActorDraft();
    }
    #endregion

    #region DropDownNameSetSelected
    /// <summary>
    /// Get option selected from dropDown pick list
    /// </summary>
    private void DropDownNameSetSelected()
    {
        int index = dropInputNameSet.value;
        //set input values
        dropIndexNameSet = index;
        dropStringNameSet = dropInputNameSet.options[index].text;
    }
    #endregion

    #region DropDownSideSelected
    /// <summary>
    /// Get option selected from dropDown pick list
    /// </summary>
    private void DropDownSideSelected()
    {
        int index = dropInputSide.value;
        //set input values
        dropIndexSide = index;
        dropStringSide = dropInputSide.options[index].text;
    }
    #endregion

    #region DropDownTraitSelected
    /// <summary>
    /// Get option selected from dropDown pick list
    /// </summary>
    private void DropDownTraitSelected()
    {
        dropIndexTrait = dropInputTrait.value;
        //set input values
        dropStringTrait = dropInputTrait.options[dropIndexTrait].text;
        //set label
        Trait trait = listOfTraits.Find(x => x.tag.Equals(dropStringTrait, StringComparison.Ordinal) == true);
        if (trait != null)
        { actorObject.trait = trait; }
        else { Debug.LogErrorFormat("Invalid trait (Null) for dropStringTrait \"{0}\"", dropStringTrait); }
    }
    #endregion

    #endregion

    #region InputFields...
    //
    // - - - Input fields
    //

    private void InitialiseActorInputFields()
    {
        actorFirstName.onValueChanged.AddListener(delegate { UpdateActorFirstName(); });
        actorLastName.onValueChanged.AddListener(delegate { UpdateActorLastName(); });
        actorLevel.onValueChanged.AddListener(delegate { UpdateActorLevel(); });
        actorPower.onValueChanged.AddListener(delegate { UpdateActorPower(); });
        backstory0.onValueChanged.AddListener(delegate { UpdateBackstory0(); });
        backstory1.onValueChanged.AddListener(delegate { UpdateBackstory1(); });
    }

    /// <summary>
    /// Actor first name changed
    /// </summary>
    private void UpdateActorFirstName()
    {
        actorObject.firstName = actorFirstName.text;
        actorObject.actorName = string.Format("{0} {1}", actorFirstName.text, actorLastName.text);
        textName.text = actorObject.actorName;
    }

    /// <summary>
    /// Actor last name changed
    /// </summary>
    private void UpdateActorLastName()
    {
        actorObject.lastName = actorLastName.text;
        actorObject.actorName = string.Format("{0} {1}", actorFirstName.text, actorLastName.text);
        textName.text = actorObject.actorName;
    }


    private void UpdateActorLevel()
    { actorObject.level = Convert.ToInt32(actorLevel.text); }

    private void UpdateActorPower()
    { actorObject.power = Convert.ToInt32(actorPower.text); }

    private void UpdateBackstory0()
    { actorObject.backstory0 = backstory0.text; }

    private void UpdateBackstory1()
    { actorObject.backstory1 = backstory1.text; }

    #endregion

    #region Utilities...
    //
    // - - - Utilities
    //

    #region UpdateListOfActorPools
    /// <summary>
    /// Updates and Initialises listOfPoolOptions by searching for actorPools in SO/Temp and SO/ActorPool folders
    /// </summary>
    private void UpdateListOfActorPools()
    {
        string pathName;
        ActorPool pool;
        string[] arrayOfGuids;
        //clear list prior to fresh update 
        listOfActorPools.Clear();
        //ActorPool folder
        arrayOfGuids = AssetDatabase.FindAssets("t:ActorPool", new[] { "Assets/SO/ActorPools" });
        if (arrayOfGuids.Length > 0)
        {
            for (int i = 0; i < arrayOfGuids.Length; i++)
            {
                pathName = AssetDatabase.GUIDToAssetPath(arrayOfGuids[i]);
                if (pathName.Length > 0)
                {
                    pool = (ActorPool)AssetDatabase.LoadAssetAtPath(pathName, typeof(ActorPool));
                    if (pool != null)
                    { listOfActorPools.Add(pool); }
                    else { Debug.LogErrorFormat("Invalid pool (Null) for path \"{0}\"", pathName); }
                }
                else { Debug.LogErrorFormat("Invalid path (Empty) for guid \"{0}\", guids[{1}] at /ActorPools", arrayOfGuids[i], i); }
            }
        }
    }
    #endregion

    #region UpdatePoolObject
    /// <summary>
    /// Updates pool object based on current selection in drop down pool name list 
    /// </summary>
    private void UpdatePoolObject()
    {
        //assign to poolObject and update data in LHS panel
        poolObject = listOfActorPools.Find(x => x.name.Equals(dropStringPool));
        if (poolObject != null)
        {
            poolName.text = poolObject.tag;
            poolNameSet.text = poolObject.nameSet.name;
            poolSide.text = poolObject.side.name;
            poolAuthor.text = poolObject.author;
            poolDate.text = poolObject.dateCreated;
        }
        else { Debug.LogError("Invalid poolObject (Null), not found in listOfActorPools"); }
    }
    #endregion

    #region UpdateSidePanel
    /// <summary>
    /// Toggles side labels/inputs on/off depending on whether 'isInput' true/false
    /// </summary>
    private void UpdateSidePanel(bool isInput)
    {
        if (isInput == true)
        {
            //INPUT mode -> switch off side labels
            dropInputPool.gameObject.SetActive(false);
            poolName.gameObject.SetActive(false);
            poolNameSet.gameObject.SetActive(false);
            poolSide.gameObject.SetActive(false);
            poolAuthor.gameObject.SetActive(false);
            poolDate.gameObject.SetActive(false);
            //switch on side panel inputs
            poolNameInput.gameObject.SetActive(true);
            poolTagInput.gameObject.SetActive(true);
            poolAuthorInput.gameObject.SetActive(true);
            poolDateInput.gameObject.SetActive(true);
            dropInputNameSet.gameObject.SetActive(true);
            dropInputSide.gameObject.SetActive(true);
        }
        else
        {
            //EDIT mode -> switch on side labels
            dropInputPool.gameObject.SetActive(true);
            poolName.gameObject.SetActive(true);
            poolNameSet.gameObject.SetActive(true);
            poolSide.gameObject.SetActive(true);
            poolAuthor.gameObject.SetActive(true);
            poolDate.gameObject.SetActive(true);
            //switch off side panel inputs
            poolNameInput.gameObject.SetActive(false);
            poolTagInput.gameObject.SetActive(false);
            poolAuthorInput.gameObject.SetActive(false);
            poolDateInput.gameObject.SetActive(false);
            dropInputNameSet.gameObject.SetActive(false);
            dropInputSide.gameObject.SetActive(false);
            //button
            createPoolButton.gameObject.SetActive(false);
        }
    }
    #endregion


    #endregion



    //new methods above here
}
