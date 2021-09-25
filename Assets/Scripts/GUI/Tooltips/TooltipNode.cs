using gameAPI;
using packageAPI;
using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Node based tooltip
/// </summary>
public class TooltipNode : MonoBehaviour
{
    public TextMeshProUGUI nodeName;
    public TextMeshProUGUI nodeType;
    public TextMeshProUGUI spiderTimer;
    public TextMeshProUGUI tracerTimer;
    public TextMeshProUGUI crisis;
    public TextMeshProUGUI nodeActive;
    /*public TextMeshProUGUI nodeStatsFixed;
    public TextMeshProUGUI nodeStatsVar;*/
    public TextMeshProUGUI nodeStats;
    public TextMeshProUGUI ongoingEffects;
    public TextMeshProUGUI nodeTeams;
    public TextMeshProUGUI nodeTarget;

    public Image dividerTop;                   //Side specific sprites for tooltips
    public Image dividerCrisis;
    public Image dividerUpperMiddle;
    public Image dividerLowerMiddle;
    public Image dividerStats;
    public Image dividerBottom;
    public Image spider;
    public Image tracer;

    public GameObject tooltipNodeObject;
    public Canvas tooltipNodeCanvas;

    private string[] arrayOfIcons = new string[3];

    private Image background;
    private static TooltipNode tooltipNode;
    RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    /*private float fadeInTime;*/
    private int offset;
    //fast access fields
    private GlobalSide globalResistance;
    private GlobalSide globalAuthority;
    private int maxStatValue = -1;
    private int minStatValue = -1;

    //colour Palette
    private string colourGoodSide;      //side dependant
    private string colourBadSide;
    private string colourNeutral;
    //private string colourGood;          //absolute
    private string colourBad;
    private string colourActive;
    private string colourAlert;
    private string colourDefault;
    private string colourNormal;
    private string colourEnd;
    private string colourTeam;

    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        Debug.Assert(nodeName != null, "Invalid nodeName (Null)");
        Debug.Assert(nodeType != null, "Invalid nodeType (Null)");
        Debug.Assert(nodeActive != null, "Invalid nodeActive (Null)");
        Debug.Assert(nodeStats != null, "Invalid nodeStats (Null)");
        Debug.Assert(nodeTeams != null, "Invalid nodeTeams (Null)");
        Debug.Assert(nodeTarget != null, "Invalid nodeTarget (Null)");
        Debug.Assert(spiderTimer != null, "Invalid spiderTimer (Null)");
        Debug.Assert(tracerTimer != null, "Invalid tracerTimer (Null)");
        Debug.Assert(crisis != null, "Invalid crisis (Null)");
        Debug.Assert(ongoingEffects != null, "Invalid ongoingEffects (Null)");
        Debug.Assert(dividerTop != null, "Invalid dividerTop (Null)");
        Debug.Assert(dividerCrisis != null, "Invalid dividerCrisis (Null)");
        Debug.Assert(dividerUpperMiddle != null, "Invalid dividerUpperMiddle (Null)");
        Debug.Assert(dividerLowerMiddle != null, "Invalid dividerLowerMiddle (Null)");
        Debug.Assert(dividerStats != null, "Invalid dividerStats (Null)");
        Debug.Assert(dividerBottom != null, "Invalid dividerBottom (Null)");
        Debug.Assert(spider != null, "Invalid spider (Null)");
        Debug.Assert(tracer != null, "Invalid tracer (Null)");
        //components
        canvasGroup = tooltipNodeObject.GetComponent<CanvasGroup>();
        rectTransform = tooltipNodeObject.GetComponent<RectTransform>();
        /*fadeInTime = GameManager.i.guiScript.tooltipFade;*/
        offset = GameManager.i.guiScript.tooltipOffset;
        //event listener
        EventManager.i.AddListener(EventType.ChangeColour, OnEvent, "TooltipNode");
        EventManager.i.AddListener(EventType.ChangeSide, OnEvent, "TooltipNode");
    }

    /// <summary>
    /// needed for sequencing issues. Not for GameState.LoadGame
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                break;
            case GameState.FollowOnInitialisation:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseFastAccess
    /// <summary>
    /// Fast access submethod
    /// </summary>
    private void SubInitialiseFastAccess()
    {
        //fast access fields
        globalResistance = GameManager.i.globalScript.sideResistance;
        globalAuthority = GameManager.i.globalScript.sideAuthority;
        maxStatValue = GameManager.i.nodeScript.maxNodeValue;
        minStatValue = GameManager.i.nodeScript.minNodeValue;
        Debug.Assert(globalResistance != null, "Invalid globalResistance (Null)");
        Debug.Assert(globalAuthority != null, "Invalid globalAuthority (Null)");
        Debug.Assert(maxStatValue > -1, "Invalid maxStatValue (-1)");
        Debug.Assert(minStatValue > -1, "Invalid minStatValue (-1)");
        //Node datapoint Icons
        arrayOfIcons[0] = GameManager.i.guiScript.stabilityIcon;
        arrayOfIcons[1] = GameManager.i.guiScript.supportIcon;
        arrayOfIcons[2] = GameManager.i.guiScript.securityIcon;
        Debug.Assert(arrayOfIcons[0] != null, "Invalid arrayOfIcons[0], Stability (Null)");
        Debug.Assert(arrayOfIcons[1] != null, "Invalid arrayOfIcons[1], Support (Null)");
        Debug.Assert(arrayOfIcons[2] != null, "Invalid arrayOfIcons[2], Security (Null)");
    }
    #endregion



    #endregion

    /// <summary>
    /// provide a static reference to tooltipNode that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TooltipNode Instance()
    {
        if (!tooltipNode)
        {
            tooltipNode = FindObjectOfType(typeof(TooltipNode)) as TooltipNode;
            if (!tooltipNode)
            { Debug.LogError("There needs to be one active TooltipNode script on a GameObject in your scene"); }
        }
        return tooltipNode;
    }

    //Called when events happen
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.ChangeSide:
                InitialiseTooltip((GlobalSide)Param);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Set colour palette for tooltip
    /// </summary>
    public void SetColours()
    {
        //output colours for good and bad depend on player side
        switch (GameManager.i.sideScript.PlayerSide.level)
        {
            case 1:
                //Authority
                colourGoodSide = GameManager.i.colourScript.GetColour(ColourType.dataBad);
                colourBadSide = GameManager.i.colourScript.GetColour(ColourType.dataGood);
                break;
            case 2:
                //Resistance
                colourGoodSide = GameManager.i.colourScript.GetColour(ColourType.dataGood);
                colourBadSide = GameManager.i.colourScript.GetColour(ColourType.dataBad);
                break;
            default:
                Debug.LogError(string.Format("Invalid playerSide \"{0}\"", GameManager.i.sideScript.PlayerSide.name));
                break;
        }
        //colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourNeutral = GameManager.i.colourScript.GetColour(ColourType.dataNeutral);
        colourAlert = GameManager.i.colourScript.GetColour(ColourType.salmonText);
        colourBad = GameManager.i.colourScript.GetColour(ColourType.dataBad);
        //colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodText);
        colourActive = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourDefault = GameManager.i.colourScript.GetColour(ColourType.whiteText);
        colourNormal = GameManager.i.colourScript.GetColour(ColourType.normalText);
        colourTeam = GameManager.i.colourScript.GetColour(ColourType.neutralText);
        colourEnd = GameManager.i.colourScript.GetEndTag();
    }

    #region SetTooltip
    /// <summary>
    /// Initialise node tooltip
    /// </summary>
    /// <param name="data"></param>
    public void SetTooltip(NodeTooltipData data)
    {
        /*//final modal check to prevent inappropriate display of tooltip, eg. of MainInfoApp
        if (GameManager.instance.guiScript.CheckIsBlocked() == false)                           EDIT: Removed as doesn't work for menu tooltip as modal 1 in this situation
        {*/
        bool proceedFlag;
        int numRecordsCurrent, numRecordsOther;
        GlobalSide playerSide = GameManager.i.sideScript.PlayerSide;
        GlobalSide currentSide = GameManager.i.turnScript.currentSide;
        //open panel at start
        tooltipNodeCanvas.gameObject.SetActive(true);
        tooltipNodeObject.SetActive(true);

        /*//set opacity to zero (invisible)
        //SetOpacity(0f);*/

        //set state of all items in tooltip window
        nodeName.gameObject.SetActive(true);
        nodeType.gameObject.SetActive(true);
        /*nodeStatsVar.gameObject.SetActive(true);
        nodeStatsFixed.gameObject.SetActive(true);*/
        nodeStats.gameObject.SetActive(true);
        ongoingEffects.gameObject.SetActive(false);
        dividerTop.gameObject.SetActive(true);
        //show only if node active for at least one actor
        nodeActive.gameObject.SetActive(false);
        dividerCrisis.gameObject.SetActive(false);
        dividerUpperMiddle.gameObject.SetActive(false);
        dividerLowerMiddle.gameObject.SetActive(true);
        dividerStats.gameObject.SetActive(false);
        //target
        nodeTarget.gameObject.SetActive(true);
        dividerBottom.gameObject.SetActive(true);
        //show spider only if present and known
        if (data.isSpiderKnown == true)
        {
            spider.gameObject.SetActive(true);
            spiderTimer.gameObject.SetActive(true);
            spiderTimer.text = Convert.ToString(data.spiderTimer);
        }
        else
        {
            spider.gameObject.SetActive(false);
            spiderTimer.gameObject.SetActive(false);
            spiderTimer.text = "";
        }
        //
        // - - - Tracer (if present and known)
        //
        proceedFlag = false;
        if (data.isTracer == true)
        {
            switch (playerSide.level)
            {
                case 1:
                    //authority
                    if (data.isTracerKnown == true) { proceedFlag = true; }
                    break;
                case 2:
                    //resistance
                    proceedFlag = true;
                    break;
            }
        }
        if (proceedFlag == true)
        {
            tracer.gameObject.SetActive(true);
            tracerTimer.gameObject.SetActive(true);
            if (data.isTracer)
            {
                //show Yellow Timer if tracer in node, otherise ignore timer
                tracerTimer.text = string.Format("{0}{1}{2}", colourNeutral, data.tracerTimer, colourEnd);
            }
            else { tracerTimer.text = ""; }
        }
        else
        {
            tracer.gameObject.SetActive(false);
            tracer.gameObject.SetActive(false);
            tracerTimer.text = "";
        }
        //set up tooltipNode object
        if (string.IsNullOrEmpty(data.specialName) == false)
        { nodeName.text = string.Format("{0}{1}{2}{3}{4}", data.nodeName, "\n", colourAlert, data.specialName, colourEnd); }
        else { nodeName.text = string.Format("{0}{1}{2}", colourDefault, data.nodeName, colourEnd); }
        nodeType.text = string.Format("{0}{1}{2}", colourAlert, data.type, colourEnd);
        //
        // - - - Stats (BEFORE Crisis)- - - 
        //
        int checkCounter = 0;
        int statData;
        bool isPossibleCrisis = false;
        if (data.arrayOfStats.Length == 3)
        {
            StringBuilder builderStats = new StringBuilder();
            for (int i = 0; i < data.arrayOfStats.Length; i++)
            {
                statData = data.arrayOfStats[i];
                if (i > 0) { builderStats.AppendLine(); }
                builderStats.AppendFormat("{0} {1}<pos=57%>{2}", arrayOfIcons[i], (NodeData)i, GameManager.i.guiScript.GetNormalStars(statData));
                //idiot check to handle case of being too many stats
                checkCounter++;
                if (checkCounter >= 3) { Debug.LogWarningFormat("Stats Counter Exceeded for node {0}, {1}", data.nodeName, data.type); break; }
            }
            nodeStats.text = builderStats.ToString();

            /*
            //fixed node stat tags -> show red (authority) / green (resistance) if in the danger zone (possible or actual crisis), default white otherwise
            string colourCrisis = colourGood;
            if (playerSide.level == 1) { colourCrisis = colourBad; }
            StringBuilder builderFixed = new StringBuilder();
            if (data.arrayOfStats[0] == minStatValue) { builderFixed.AppendFormat("{0}Stability{1}", colourCrisis, colourEnd); isPossibleCrisis = true; }
            else { builderFixed.Append("Stability"); }
            builderFixed.AppendLine();
            if (data.arrayOfStats[1] == maxStatValue) { builderFixed.AppendFormat("{0}Support{1}", colourCrisis, colourEnd); isPossibleCrisis = true; }
            else { builderFixed.Append("Support"); }
            builderFixed.AppendLine();
            if (data.arrayOfStats[2] == minStatValue) { builderFixed.AppendFormat("{0}Security{1}", colourCrisis, colourEnd); isPossibleCrisis = true; }
            else { builderFixed.Append("Security"); }
            */
        }
        else { Debug.LogWarning("Invalid length for arrayOfStats (not 3)"); }
        //
        // - - - Crisis - - -
        //
        int numOfCrisis = 0;
        if (data.listOfCrisis != null)
        { numOfCrisis = data.listOfCrisis.Count; }
        if (numOfCrisis > 0)
        {
            StringBuilder builderCrisis = new StringBuilder();
            //listOfCrisis has 4 fixed topic entries
            for (int index = 0; index < numOfCrisis; index++)
            {
                switch (index)
                {
                    case 0:
                        //'x' CRISIS
                        builderCrisis.AppendFormat("{0}<b>{1}</b>{2}", colourBad, data.listOfCrisis[index], colourEnd);
                        break;
                    case 1:
                        //'x' turns left
                        builderCrisis.AppendFormat("{0}{1}{2}{3}", "\n", colourNeutral, data.listOfCrisis[index], colourEnd);
                        break;
                    case 2:
                    case 3:
                        //info dump
                        builderCrisis.AppendFormat("{0}<size=90%>{1}<b>{2}</b>{3}</size>", "\n", colourAlert, data.listOfCrisis[index], colourEnd);
                        break;
                    default:
                        Debug.LogWarningFormat("Invalid listOfCrisis[{0}] \"{1}\"", index, data.listOfCrisis[index]);
                        break;
                }
            }
            //divider
            dividerCrisis.gameObject.SetActive(true);
            crisis.text = builderCrisis.ToString();
            crisis.gameObject.SetActive(true);
        }
        else if (isPossibleCrisis == true)
        {
            crisis.text = string.Format("{0}Potential Crisis{1}", colourAlert, colourEnd);
            crisis.gameObject.SetActive(true);
            dividerCrisis.gameObject.SetActive(true);
        }
        else
        {
            crisis.text = "";
            crisis.gameObject.SetActive(false);
        }
        //
        // - - - Contacts - - -
        //
        //set tooltip elements to show
        nodeActive.gameObject.SetActive(true);
        dividerUpperMiddle.gameObject.SetActive(true);
        //build string
        StringBuilder builderActor = new StringBuilder();
        builderActor.Append(colourActive);
        //ascertain whether actors shown or not
        proceedFlag = false;
        numRecordsCurrent = data.listOfContactsCurrent.Count;
        if (data.listOfContactsOther != null)
        { numRecordsOther = data.listOfContactsOther.Count; }
        else { numRecordsOther = 0; }
        //contact info depends on side status
        if (currentSide.level == globalResistance.level)
        {
            //Resistance
            if (numRecordsCurrent > 0)
            {
                //contacts present
                for (int i = 0; i < numRecordsCurrent; i++)
                {
                    if (i > 0) { builderActor.AppendLine(); }
                    builderActor.AppendFormat("{0}<b>{1}</b>{2}", colourNeutral, data.listOfContactsCurrent[i], colourEnd);
                }
            }
            else
            {
                //no contacts present
                builderActor.AppendFormat("{0}<size=90%>No Contacts</size>{1}", colourDefault, colourEnd);
            }
        }
        else if (currentSide.level == globalAuthority.level)
        {
            //Authority
            if (numRecordsCurrent > 0)
            {
                //contacts present
                for (int i = 0; i < numRecordsCurrent; i++)
                {
                    if (i > 0) { builderActor.AppendLine(); }
                    builderActor.AppendFormat("{0}<b>{1}</b>{2}", colourNeutral, data.listOfContactsCurrent[i], colourEnd);
                }
            }
            else
            {
                //no contacts present
                builderActor.AppendFormat("{0}<size=90%>No Contacts</size>{1}", colourDefault, colourEnd);
            }
            builderActor.AppendLine();
            //Authority comment on resistance contacts
            if (GameManager.i.optionScript.isFogOfWar == true)
            {
                //FOW ON
                if (data.isContactKnown == true)
                {
                    if (numRecordsOther > 0)
                    {
                        //other contacts present
                        builderActor.AppendFormat("{0}<b>Resistance Contact</b>{1}{2}", colourAlert, numRecordsOther != 1 ? "s" : "", colourEnd);
                    }
                    else
                    {
                        //no other contacts present
                        builderActor.AppendFormat("{0}<size=90%>No Resistance Contacts</size>{1}", colourNormal, colourEnd);
                    }
                }
                else { builderActor.AppendFormat("{0}<size=90%>Resistance Contacts unknown</size>{1}", colourNormal, colourEnd); }
            }
            else
            {
                //FOW OFF
                if (numRecordsOther > 0)
                {
                    //other contacts present
                    builderActor.AppendFormat("{0}<b>Resistance Contact</b>{1}{2}", colourAlert, numRecordsOther != 1 ? "s" : "", colourEnd);
                }
                else
                {
                    //no other contacts present
                    builderActor.AppendFormat("{0}<size=90%>No Resistance Contacts</size>{1}", colourDefault, colourEnd);
                }
            }
        }
        nodeActive.text = builderActor.ToString();
        //
        // - - - Ongoing effects - - - 
        //
        if (data.listOfEffects.Count > 0)
        {
            string effectText = "Unknown";
            ongoingEffects.gameObject.SetActive(true);
            dividerStats.gameObject.SetActive(true);
            StringBuilder effectBuilder = new StringBuilder();
            effectBuilder.AppendFormat("{0}<size=90%>Ongoing Effects</size>{1}", colourDefault, colourEnd);
            for (int i = 0; i < data.listOfEffects.Count; i++)
            {
                switch (data.listOfEffects[i].typeLevel)
                {
                    case 2:
                        //Good
                        effectText = string.Format("{0}{1}{2}", colourGoodSide, data.listOfEffects[i].text, colourEnd);
                        break;
                    case 1:
                        //Neutral
                        effectText = string.Format("{0}{1}{2}", colourNeutral, data.listOfEffects[i].text, colourEnd);
                        break;
                    case 0:
                        //Bad
                        effectText = string.Format("{0}{1}{2}", colourBadSide, data.listOfEffects[i].text, colourEnd);
                        break;
                    default:
                        effectText = string.Format("{0}{1}{2}", colourDefault, data.listOfEffects[i].text, colourEnd);
                        Debug.LogError(string.Format("Invalid ongoingEffect.type \"{0}\"", data.listOfEffects[i].typeLevel));
                        break;
                }
                effectBuilder.AppendLine();
                effectBuilder.Append(effectText);
            }
            ongoingEffects.text = effectBuilder.ToString();
        }
        else { ongoingEffects.text = ""; }
        //
        // - - - Teams  - - -
        //
        proceedFlag = false;
        if (playerSide.level == globalAuthority.level) { proceedFlag = true; }
        else if (playerSide.level == globalResistance.level)
        {
            if (GameManager.i.optionScript.isFogOfWar == true)
            {
                if (data.isTracer == true || data.isTeamKnown == true || data.isActiveContact == true)
                { proceedFlag = true; }
            }
            else { proceedFlag = true; }
        }
        //show teams show only (Resistance) if node has a tracer or actor has a contact there or isTeamKnown true (if FOW option 'true')
        if (proceedFlag == true)
        {
            if (data.listOfTeams.Count > 0)
            {
                StringBuilder teamBuilder = new StringBuilder();
                foreach (string teamText in data.listOfTeams)
                {
                    if (teamBuilder.Length > 0) { teamBuilder.AppendLine(); }
                    teamBuilder.AppendFormat("{0}<b>{1}</b>{2}", colourTeam, teamText, colourEnd);
                }
                nodeTeams.text = teamBuilder.ToString();
            }
            else { nodeTeams.text = string.Format("{0}{1}{2}", colourDefault, "<size=90%>No Teams present</size>", colourEnd); }
        }
        else { nodeTeams.text = string.Format("{0}Team Info unavailable{1}{2}{3}<size=90%>requires Tracer or Contact</size>{4}", colourBadSide, colourEnd, "\n", colourDefault, colourEnd); }
        //
        // - - Target (multipurpose, shows activity data if NodeManager.cs -> activityState > 'None') - - -
        //
        StringBuilder builder = new StringBuilder();
        //NO Activity
        if (GameManager.i.nodeScript.activityState == ActivityUI.None)
        {
            if (GameManager.i.optionScript.isTargets == true)
            {
                if (data.listOfTargets.Count > 0)
                {
                    //normal target info
                    builder.Append(colourDefault);
                    //foreach (string target in listOfTarget)
                    for (int i = 0; i < data.listOfTargets.Count; i++)
                    {
                        if (i > 0)
                        {
                            builder.AppendLine();
                            builder.AppendFormat("<size=90%>{0}</size>", data.listOfTargets[i]);
                        }
                        else { builder.Append(data.listOfTargets[i]); }
                    }
                    builder.Append(colourEnd);
                }
                else { builder.AppendFormat("{0}{1}{2}", colourDefault, "<size=90%>No Target present</size>", colourEnd); }
                nodeTarget.text = builder.ToString();
            }
            else { nodeTarget.text = string.Format("{0}{1}{2}", colourDefault, "<size=90%>Targets Disabled</size>", colourEnd); }
        }
        //Activity
        else
        {
            //show one line at top for target/no target and activity data for the rest
            if (GameManager.i.optionScript.isTargets == true)
            {
                if (data.listOfTargets.Count > 0)
                { builder.Append(data.listOfTargets[0]); }
                else { builder.AppendFormat("{0}{1}{2}", colourDefault, "<size=90%>No Target present</size>", colourEnd); }
            }
            else { builder.AppendFormat("{0}{1}{2}", colourDefault, "<size=90%>Targets Disabled</size>", colourEnd); }
            builder.AppendLine(); builder.AppendLine();

            //Activity information
            if (data.listOfActivity != null && data.listOfActivity.Count > 0)
            {
                //activity info
                builder.Append(colourNormal);
                for (int i = 0; i < data.listOfActivity.Count; i++)
                {
                    if (i > 0) { builder.AppendLine(); }
                    builder.Append(data.listOfActivity[i]);
                }
                builder.Append(colourEnd);
            }
            else { builder.AppendFormat("{0}<b>Unknown Activity Info</b>{1}", colourBadSide, colourEnd); }
            nodeTarget.text = builder.ToString();
        }

        //convert coordinates
        Vector3 screenPos = Camera.main.WorldToScreenPoint(data.tooltipPos);
        Canvas.ForceUpdateCanvases();
        rectTransform = tooltipNodeObject.GetComponent<RectTransform>();
        //get dimensions of dynamic tooltip
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        //calculate offset - height (default above)
        if (screenPos.y + height + offset < Screen.height)
        { screenPos.y += height / 2 + offset; }
        else { screenPos.y -= height / 2 + offset; }
        //width - default centred
        if (screenPos.x + width / 2 >= Screen.width)
        { screenPos.x -= width / 2 + screenPos.x - Screen.width; }
        else if (screenPos.x - width / 2 <= 0)
        { screenPos.x += width / 2 - screenPos.x; }
        //set new position
        tooltipNodeObject.transform.position = screenPos;
        Debug.LogFormat("[UI] TooltipNode.cs -> SetTooltip, node \"{0}\"{1}", data.nodeName, "\n");
        /*}*/
    }
    #endregion

    /*/// <summary>
    /// fade in tooltip over time
    /// </summary>
    /// <returns></returns>
    IEnumerator FadeInTooltip()
    {
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime / fadeInTime;
            yield return null;
        }
    }*/

    private string GetStatColour(int data, NodeData type)
    {
        string colour = colourDefault;
        //authority
        switch (type)
        {
            case NodeData.Stability:
            case NodeData.Security:
                switch (data)
                {
                    case 3: colour = colourBadSide; break;
                    case 2: colour = colourNeutral; break;
                    case 1: colour = colourGoodSide; break;
                    case 0: colour = colourGoodSide; break;
                }
                break;
            case NodeData.Support:
                switch (data)
                {
                    case 3: colour = colourGoodSide; break;
                    case 2: colour = colourNeutral; break;
                    case 1: colour = colourBadSide; break;
                    case 0: colour = colourBadSide; break;
                }
                break;
        }
        return colour;
    }

    /// <summary>
    /// returns true if tooltip is active and visible
    /// </summary>
    /// <returns></returns>
    public bool CheckTooltipActive()
    { return tooltipNodeObject.activeSelf; }

    /// <summary>
    /// close tool tip
    /// </summary>
    public void CloseTooltip(string callingMethod)
    {
        Debug.LogFormat("[UI] TooltipNode.cs -> CloseTooltip: called by {0}{1}", callingMethod, "\n");
        tooltipNodeObject.SetActive(false);
        tooltipNodeCanvas.gameObject.SetActive(false);
    }


    public void SetOpacity(float opacity)
    { canvasGroup.alpha = opacity; }

    public float GetOpacity()
    { return canvasGroup.alpha; }


    /// <summary>
    /// set up sprites on node tooltip for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    public void InitialiseTooltip(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        //get component reference (done where because method called from GameManager which happens prior to this.Awake()
        background = tooltipNodeObject.GetComponent<Image>();
        //assign side specific sprites
        switch (side.level)
        {
            case 0:
                //AI both
                background.sprite = GameManager.i.sideScript.toolTip_backgroundRebel;
                dividerTop.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                dividerCrisis.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                dividerStats.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                dividerUpperMiddle.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                dividerLowerMiddle.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                dividerBottom.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                break;
            case 1:
                //Authority
                background.sprite = GameManager.i.sideScript.toolTip_backgroundAuthority;
                dividerTop.sprite = GameManager.i.sideScript.toolTip_dividerAuthority;
                dividerCrisis.sprite = GameManager.i.sideScript.toolTip_dividerAuthority;
                dividerStats.sprite = GameManager.i.sideScript.toolTip_dividerAuthority;
                dividerUpperMiddle.sprite = GameManager.i.sideScript.toolTip_dividerAuthority;
                dividerLowerMiddle.sprite = GameManager.i.sideScript.toolTip_dividerAuthority;
                dividerBottom.sprite = GameManager.i.sideScript.toolTip_dividerAuthority;
                break;
            case 2:
                //Resistance
                background.sprite = GameManager.i.sideScript.toolTip_backgroundRebel;
                dividerTop.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                dividerCrisis.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                dividerStats.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                dividerUpperMiddle.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                dividerLowerMiddle.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                dividerBottom.sprite = GameManager.i.sideScript.toolTip_dividerRebel;
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", side.name));
                break;
        }
    }

}
