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
    public Canvas actorCanvas;

    [Header("Button Interactions")]
    public ToolButtonInteraction newPoolInteraction;
    public ToolButtonInteraction savePoolInteraction;
    public ToolButtonInteraction deletePoolInteraction;
    public ToolButtonInteraction quitPoolInteraction;
    public ToolButtonInteraction createPoolInteraction;

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

    [Header("Actor texts")]
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textArc;
    public TextMeshProUGUI textStatus;
    public TextMeshProUGUI textSex;
    public TextMeshProUGUI textTrait;

    [Header("Drop down lists")]
    public TMP_Dropdown dropInputPool;
    public TMP_Dropdown dropInputNameSet;
    public TMP_Dropdown dropInputSide;
    public TMP_Dropdown dropInputTrait;

    #endregion

    #region private
    private int dropIntPool;
    private int dropIntTrait;
    private int dropIntNameSet;
    private int dropIntSide;
    private string dropStringPool;
    private string dropStringTrait;
    private string dropStringNameSet;
    private string dropStringSide;
    private ActorPool poolObject;
    #endregion

    #region Collections
    private List<string> listOfTraitOptions = new List<string>();
    private List<string> listOfPoolOptions = new List<string>();
    private List<string> listOfNameSetOptions = new List<string>();
    private List<string> listOfSideOptions = new List<string>();
    private List<ActorPool> listOfActorPools = new List<ActorPool>();

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
        InitialiseEvents();
        InitialiseButtons();
        InitialiseDropDownPool();
        InitialiseDropDownNameSet();
        InitialiseDropDownSide();
        InitialiseDropDownTraits();
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
    }
    #endregion

    #region InitialiseAsserts
    /// <summary>
    /// Fast access
    /// </summary>
    public void InitialiseAsserts()
    {
        Debug.Assert(actorCanvas != null, "Invalid actorCanvas (Null)");
        //buttons
        Debug.Assert(newPoolInteraction != null, "Invalid newPoolInteraction (Null)");
        Debug.Assert(savePoolInteraction != null, "Invalid savePoolInteraction (Null)");
        Debug.Assert(deletePoolInteraction != null, "Invalid deletePoolInteraction (Null)");
        Debug.Assert(quitPoolInteraction != null, "Invalid quitPoolInteraction (Null)");
        Debug.Assert(createPoolInteraction != null, "Invalid createPoolInteraction (Null)");
        Debug.Assert(createPoolButton != null, "Invalid createPoolButton (Null)");
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
        Debug.Assert(textTrait != null, "Invalid textTrait (Null)");
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
        ToolEvents.i.AddListener(ToolEventType.SavePoolUI, OnEvent, "ActorPoolUI");
        ToolEvents.i.AddListener(ToolEventType.DeletePoolUI, OnEvent, "ActorPoolUI");
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
            case ToolEventType.CloseActorPoolUI:
                CloseActorPoolUI();
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

    }
    #endregion

    #region Create PoolUI
    /// <summary>
    /// Once data is entered for a new pool, press button to create
    /// </summary>
    private void CreatePoolUI()
    {
        ActorPoolData data = new ActorPoolData();

        data.poolName = poolNameInput.text;
        data.tag = poolTagInput.text;

        //temporary
        data.nameSet = ToolManager.i.jointScript.arrayOfNameSets[Random.Range(0, ToolManager.i.jointScript.arrayOfNameSets.Length)];
        data.side = ToolManager.i.jointScript.sideResistance;

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
            //create pool and actorDrafts in SO/Temp folder
            ToolManager.i.actorScript.CreateActorPool(data);
            //disable button
            createPoolButton.gameObject.SetActive(false);
            //swap fields
            UpdateSidePanel(false);
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
    /// Deletes actor pool and all related actorDrafts
    /// </summary>
    private void DeletePoolUI()
    {

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


    #region InitialiseActorDraft
    /// <summary>
    /// sets up actor
    /// </summary>
    private void InitialiseActorDraft()
    {

    }
    #endregion

    //
    // - - - Drop Downs
    //

    #region InitialiseDropDownPool
    /// <summary>
    /// Initialise drop down control for Actor Pools
    /// </summary>
    private void InitialiseDropDownPool()
    {
        //set up base list
        UpdateListOfActorPools();
        //delegate for dropDown
        dropInputPool.onValueChanged.AddListener(delegate { DropDownPoolSelected(); });
        //reset input fields to defaults
        dropIntPool = -1;
        dropStringPool = "";
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
        //set index
        dropInputPool.value = -1;
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
        dropIntNameSet = -1;
        dropStringNameSet = "";
        List<NameSet> listOfNameSets = new List<NameSet>();
        listOfNameSets = ToolManager.i.jointScript.arrayOfNameSets.ToList();
        if (listOfNameSets != null)
        {
            //names of nameSets
            listOfNameSetOptions = listOfNameSets.Select(txt => txt.name).ToList();
            //set options
            if (listOfNameSetOptions != null)
            {
                dropInputNameSet.options.Clear();
                for (int i = 0; i < listOfNameSetOptions.Count; i++)
                { dropInputNameSet.options.Add(new TMP_Dropdown.OptionData() { text = listOfNameSetOptions[i] }); }
            }
            else { Debug.LogError("Invalid listOfOptions (Null)"); }
        }
        else { Debug.LogError("Invalid listOfNameSets (Null)"); }
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
        dropIntSide = -1;
        dropStringSide = "";
        List<GlobalSide> listOfSides = new List<GlobalSide>();
        listOfSides = ToolManager.i.jointScript.arrayOfGlobalSide.ToList();
        if (listOfSides != null)
        {
            //reverse sort list to make 'Resistance' the default
            var sorted = listOfSides.OrderByDescending(x => x.level);
            listOfSides = sorted.ToList();
            //names of Sides
            listOfSideOptions = listOfSides.Select(txt => txt.name).ToList();
            //set options
            if (listOfSideOptions != null)
            {
                dropInputSide.options.Clear();
                for (int i = 0; i < listOfSideOptions.Count; i++)
                { dropInputSide.options.Add(new TMP_Dropdown.OptionData() { text = listOfSideOptions[i] }); }
            }
            else { Debug.LogError("Invalid listOfOptions (Null)"); }
        }
        else { Debug.LogError("Invalid listOfSides (Null)"); }
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
        //reset input fields to defaults
        dropIntTrait = -1;
        dropStringTrait = "";
        List<Trait> listOfTraits = new List<Trait>();
        listOfTraits = ToolManager.i.actorScript.GetListOfTraits();
        if (listOfTraits != null)
        {
            //names of traits
            listOfTraitOptions = listOfTraits.Select(txt => txt.tag).ToList();
            //set options
            if (listOfTraitOptions != null)
            {
                dropInputTrait.options.Clear();
                for (int i = 0; i < listOfTraitOptions.Count; i++)
                { dropInputTrait.options.Add(new TMP_Dropdown.OptionData() { text = listOfTraitOptions[i] }); }
            }
            else { Debug.LogError("Invalid listOfOptions (Null)"); }
        }
        else { Debug.LogError("Invalid listOfTraits (Null)"); }
        //set index
        dropInputTrait.value = -1;
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
        dropIntPool = index;
        dropStringPool = dropInputPool.options[index].text;
        //set label
        poolName.text = dropStringPool;
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
        dropIntNameSet = index;
        dropStringNameSet = dropInputNameSet.options[index].text;
        //set label
        poolNameSet.text = dropStringNameSet;
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
        dropIntSide = index;
        dropStringSide = dropInputSide.options[index].text;
        //set label
        poolSide.text = dropStringSide;
    }
    #endregion

    #region DropDownTraitSelected
    /// <summary>
    /// Get option selected from dropDown pick list
    /// </summary>
    private void DropDownTraitSelected()
    {
        int index = dropInputTrait.value;
        //set input values
        dropIntTrait = index;
        dropStringTrait = dropInputTrait.options[index].text;
        //set label
        textTrait.text = dropStringTrait;
    }
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
                else { Debug.LogErrorFormat("Invalid path (Empty) for guid \"{0}\", guids[{1}] at /ActorPoolss", arrayOfGuids[i], i); }
            }
        }
        //Temp folder
        arrayOfGuids = AssetDatabase.FindAssets("t:ActorPool", new[] { "Assets/SO/TempPool" });
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
                else { Debug.LogErrorFormat("Invalid path (Empty) for guid \"{0}\", guids[{1}] at /TempPool", arrayOfGuids[i], i); }
            }
        }
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
