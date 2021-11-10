using gameAPI;
using packageAPI;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// top screen centre widget
/// </summary>
public class WidgetTopUI : MonoBehaviour
{

    // NOTE: Globals parameters for Icon coroutines are to be found in GUIManager.cs
    [Header("Main")]
    public Canvas canvas;

    [Header("UI Elements")]
    public Image widgetTopSprite;
    public Image barCityFront;
    public Image barHqFront;
    public Image barCityBack;
    public Image barHqBack;
    public Image starLeft;
    public Image starMiddle;
    public Image starRight;
    public Image flashRedInner;                             //inner circle that covers action point area (flashes when Security Measures in force)
    public Image flashRedOuter;                             //outer red rim (flashes when Security Measures in force)
    public TextMeshProUGUI actionPoints;
    public TextMeshProUGUI turnNumber;
    public TextMeshProUGUI city;
    public TextMeshProUGUI hq;

    private RectTransform transformCity;
    private RectTransform transformFaction;
    private bool isFading;                                  //flashing red security measure indicator, if true then opacity fading, otherwise increasing
    private bool isSecurityFlash;                           //true if security flash is on
    private Coroutine myFlashRedCoroutine;
    private Coroutine myCityCoroutine;
    private Coroutine myHqCoroutine;
    private float flashRedTime;
    private Color innerColour;
    private Color outerColour;

    //coroutines
    private bool isCityCoroutine;                           //true if active, false if not
    private bool isHqCoroutine;
    private bool isGrowCity;                                //true if city icon getting larger (in coroutine), false if getting smaller
    private bool isGrowHq;

    //icon variables for coroutines
    private int minFontSize;
    private int maxFontSize;
    private int defaultFontSize;
    private float timeInterval;
    private float fontIncrement;


    private static WidgetTopUI widgetTopUI;

    public void Awake()
    {
        Debug.Assert(canvas != null, "Invalid canvas (Null)");
        Debug.Assert(starLeft != null, "Invalid starLeft (Null)");
        Debug.Assert(starMiddle != null, "Invalid starMiddle (Null)");
        Debug.Assert(starRight != null, "Invalid starRight (Null)");
        Debug.Assert(flashRedInner != null, "Invalid flashRedInner (Null)");
        Debug.Assert(flashRedOuter != null, "Invalid flashRedOuter (Null)");
        Debug.Assert(widgetTopSprite != null, "Invalid widgetTopSprite (Null)");
        Debug.Assert(barCityBack != null, "Invalid barCityBack (Null)");
        Debug.Assert(barCityFront != null, "Invalid barCityFront (Null)");
        Debug.Assert(barHqFront != null, "Invalid barHqFront (Null)");
        //cache components -> needs to be here instead of initialise sub method as a loadAtStart references it prior to the subInitialisation method assigning it
        transformCity = barCityFront.GetComponent<RectTransform>();
        transformFaction = barHqFront.GetComponent<RectTransform>();
        Debug.Assert(transformCity != null, "Invalid transformCity (Null)");
        Debug.Assert(transformFaction != null, "Invalid transformFaction (Null)");
    }


    /// <summary>
    /// provide a static reference to widgetTopUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static WidgetTopUI Instance()
    {
        if (!widgetTopUI)
        {
            widgetTopUI = FindObjectOfType(typeof(WidgetTopUI)) as WidgetTopUI;
            if (!widgetTopUI)
            { Debug.LogError("There needs to be one active WidgetTopUI script on a GameObject in your scene"); }
        }
        return widgetTopUI;
    }


    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
            case GameState.StartUp:
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                SubInitialiseSessionStart();
                SubInitialiseResetWidget();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseEvents();
                SubInitialiseSessionStart();
                SubInitialiseResetWidget();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseResetWidget();
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }


    #region Initialise SubMethods

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        flashRedTime = GameManager.i.guiScript.flashRedTime;
        Debug.Assert(flashRedTime > 0.0f, "Invalid flashRedTime (Zero)");
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listener
        EventManager.i.AddListener(EventType.ChangeActionPoints, OnEvent, "WidgetTopUI");
        EventManager.i.AddListener(EventType.ChangeCityBar, OnEvent, "WidgetTopUI");
        EventManager.i.AddListener(EventType.ChangeHqBar, OnEvent, "WidgetTopUI");
        EventManager.i.AddListener(EventType.ChangeTurn, OnEvent, "WidgetTopUI");
        EventManager.i.AddListener(EventType.ChangeSide, OnEvent, "WidgetTopUI");
        EventManager.i.AddListener(EventType.StartSecurityFlash, OnEvent, "WidgetTopUI");
        EventManager.i.AddListener(EventType.StopSecurityFlash, OnEvent, "WidgetTopUI");
        /*EventManager.instance.AddListener(EventType.ChangeStarLeft, OnEvent, "WidgetTopUI");
        EventManager.instance.AddListener(EventType.ChangeStarMiddle, OnEvent, "WidgetTopUI");
        EventManager.instance.AddListener(EventType.ChangeStarRight, OnEvent, "WidgetTopUI");*/
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        Debug.Assert(city != null, "Invalid city tmp (Null)");
        Debug.Assert(hq != null, "Invalid hq tmp (Null)");
        Debug.Assert(actionPoints != null, "Invalid actionPoints (Null)");
        Debug.Assert(turnNumber != null, "Invalid turnNumber (Null)");
        //coroutines
        isCityCoroutine = false;
        isHqCoroutine = false;
        minFontSize = GameManager.i.guiScript.iconMinFontSize;
        maxFontSize = GameManager.i.guiScript.iconMaxFontSize;
        defaultFontSize = GameManager.i.guiScript.iconDefaultFontSize;
        timeInterval = GameManager.i.guiScript.iconFlashInterval;
        fontIncrement = GameManager.i.guiScript.iconFontIncrement;
        //flash red inner opacity set to 0
        Color tempColor = flashRedInner.color;
        tempColor.a = 0.0f;
        flashRedInner.color = tempColor;
        //flash red outer rim opacity set to 0
        tempColor = flashRedOuter.color;
        tempColor.a = 0.0f;
        flashRedOuter.color = tempColor;
        isFading = false;
        //main widget sprite colour
        tempColor = GameManager.i.uiScript.TopWidget;
        tempColor.a = 0.75f;
        widgetTopSprite.color = tempColor;
        //back bars
        tempColor = GameManager.i.uiScript.TopWidgetBarBacks;
        tempColor.a = 0.4f;
        barCityBack.color = tempColor;
        barHqBack.color = tempColor;
        // dim down objective stars to default values -> Otherwise done in ObjectiveManager.cs -> SetObjectives / UpdateObjectiveProgress
        SetStar(10f, AlignHorizontal.Left);
        SetStar(10f, AlignHorizontal.Centre);
        SetStar(10f, AlignHorizontal.Right);
        //set bars to starting values
        SetSides(GameManager.i.sideScript.PlayerSide);
        //set city and HQ icons
        city.fontSize = defaultFontSize;
        hq.fontSize = defaultFontSize;
        city.text = GameManager.i.guiScript.cityIcon;
        hq.text = GameManager.i.guiScript.hqIcon;
    }
    #endregion

    #region SubInitialiseResetWidget
    /// <summary>
    /// reset data to start of level
    /// </summary>
    private void SubInitialiseResetWidget()
    {
        SetActionPoints(GameManager.i.turnScript.GetActionsTotal());
        SetTurn(GameManager.i.turnScript.Turn);
        SetSecurityFlasher(false);
    }
    #endregion

    #endregion

    /// <summary>
    /// Event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.ChangeActionPoints:
                SetActionPoints((int)Param);
                break;
            case EventType.ChangeTurn:
                SetTurn((int)Param);
                break;
            case EventType.ChangeSide:
                SetSides((GlobalSide)Param);
                break;
            case EventType.ChangeCityBar:
                SetCityBar((int)Param);
                UpdateCityIcon((int)Param);
                break;
            case EventType.ChangeHqBar:
                SetFactionBar((int)Param);
                UpdateHqIcon((int)Param);
                break;
            /*case EventType.ChangeStarLeft: -> Done directly by ObjectiveManager.cs methods, issues with events at level start (sequencing)
                SetStar((float)Param, AlignHorizontal.Left);
                break;
            case EventType.ChangeStarMiddle:
                SetStar((float)Param, AlignHorizontal.Centre);
                break;
            case EventType.ChangeStarRight:
                SetStar((float)Param, AlignHorizontal.Right);
                break;*/
            case EventType.StartSecurityFlash:
                SetSecurityFlasher(true);
                break;
            case EventType.StopSecurityFlash:
                SetSecurityFlasher(false);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Toggle widget on/off (used by tutorial only, default is to be on at all times)
    /// </summary>
    /// <param name="isActive"></param>
    public void SetWidget(bool isActive)
    { canvas.gameObject.SetActive(isActive); }


    /// <summary>
    /// Changes Turn number display
    /// </summary>
    /// <param name="points"></param>
    private void SetTurn(int turn)
    {
        Debug.Assert(turn > -1, "Invalid Turn number");
        turnNumber.text = Convert.ToString(turn);
    }


    /// <summary>
    /// Set city loyalty, faction bar colour & action points for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    private void SetSides(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        SetCityBar(GameManager.i.cityScript.CityLoyalty);
        SetActionPoints(GameManager.i.turnScript.GetActionsTotal());
        switch (side.level)
        {
            case 1:
                //authority
                SetFactionBar(GameManager.i.hqScript.ApprovalAuthority);
                break;
            case 2:
                //resistance
                SetFactionBar(GameManager.i.hqScript.ApprovalResistance);
                break;
        }
    }


    /// <summary>
    /// Changes action point display
    /// </summary>
    /// <param name="points"></param>
    private void SetActionPoints(int points)
    {
        Debug.AssertFormat(points > -1 && points < 6, "WidgetTopUI.cs -> SetActionPoints: Invalid action points \"{0}\"", points);
        actionPoints.text = Convert.ToString(points);
    }


    /// <summary>
    /// Sets length and colour of city bar based on size (0 to 10)
    /// </summary>
    /// <param name="size"></param>
    private void SetCityBar(int size)
    {
        Debug.Assert(size > -1 && size < 11, string.Format("Invalid size {0}", size));
        float floatSize = size;
        transformCity.sizeDelta = new Vector2(floatSize * 10f, transformCity.sizeDelta.y);
        //set colour
        float factor = floatSize * 0.1f;
        //City Loyalty color depends on side (100% is green for Authority and red for Resistance)
        switch (GameManager.i.sideScript.PlayerSide.level)
        {
            case 1:
                //authority
                barCityFront.color = new Color(2.0f * (1 - factor), 1.0f * factor, 0);
                break;
            case 2:
                //resistance
                barCityFront.color = new Color(2.0f * factor, 1.0f * (1 - factor), 0);
                break;
        }
    }

    /// <summary>
    /// Sets length and colour of faction bar based on size (0 to 10)
    /// </summary>
    /// <param name="size"></param>
    private void SetFactionBar(int size)
    {
        Debug.Assert(size > -1 && size < 11, string.Format("Invalid size {0}", size));
        float floatSize = size;
        transformFaction.sizeDelta = new Vector2(floatSize * 10f, transformFaction.sizeDelta.y);
        //set colour
        float factor = floatSize * 0.1f;
        barHqFront.color = new Color(2.0f * (1 - factor), 1.0f * factor, 0);
    }

    /// <summary>
    /// Change opacity (float, NOT int) of 3 Objective stars at top centre
    /// </summary>
    /// <param name="opacity"></param>
    /// <param name="uiPosition"></param>
    public void SetStar(float opacity, AlignHorizontal uiPosition)
    {
        Debug.Assert(opacity >= 0 && opacity <= 100f, string.Format("Invalid opacity \"{0}\"", opacity));
        //convert opacity to 0 to 1.0
        opacity *= 0.01f;
        //change opacity of relevant star
        switch (uiPosition)
        {
            case AlignHorizontal.Left:
                Color tempLeftColor = starLeft.color;
                tempLeftColor.a = opacity;
                starLeft.color = tempLeftColor;
                break;
            case AlignHorizontal.Centre:
                Color tempMiddleColor = starMiddle.color;
                tempMiddleColor.a = opacity;
                starMiddle.color = tempMiddleColor;
                break;
            case AlignHorizontal.Right:
                Color tempRightColor = starRight.color;
                tempRightColor.a = opacity;
                starRight.color = tempRightColor;
                break;
            default:
                Debug.LogWarningFormat("Invalid UIPosition \"{0}\"", uiPosition);
                break;
        }

    }


    /// <summary>
    /// Start (true) or Stop (false) the Action button red flash indicating that Security Measures are in place
    /// </summary>
    /// <param name="isStart"></param>
    private void SetSecurityFlasher(bool isStart)
    {
        switch (isStart)
        {
            case true:
                if (myFlashRedCoroutine == null)
                {
                    myFlashRedCoroutine = StartCoroutine("ShowFlashRed");
                    isSecurityFlash = true;
                }
                break;
            case false:
                //normally false and not null but could be LoadAtStart and coroutine null
                if (myFlashRedCoroutine != null)
                {
                    StopCoroutine(myFlashRedCoroutine);
                    myFlashRedCoroutine = null;
                }
                isFading = false;
                isSecurityFlash = false;
                //reset opacity back to zero -> inner
                Color tempColor = flashRedInner.color;
                tempColor.a = 0.0f;
                flashRedInner.color = tempColor;
                //reset opacity back to zero -> outer
                tempColor = flashRedOuter.color;
                tempColor.a = 0.0f;
                flashRedOuter.color = tempColor;
                break;
        }
    }

    /// <summary>
    /// Coroutine to display a flashing red alarm at action point display indicating Security Measures in place
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowFlashRed()
    {
        //infinite while loop
        while (true)
        {
            innerColour = flashRedInner.color;
            outerColour = flashRedOuter.color;
            if (isFading == false)
            {
                innerColour.a += Time.deltaTime / flashRedTime;
                outerColour.a += Time.deltaTime / flashRedTime;
                if (innerColour.a >= 1.0f)
                { isFading = true; }
            }
            else
            {
                innerColour.a -= Time.deltaTime / flashRedTime;
                outerColour.a -= Time.deltaTime / flashRedTime;
                if (innerColour.a <= 0.0f)
                { isFading = false; }
            }
            flashRedInner.color = innerColour;
            flashRedOuter.color = outerColour;
            yield return null;
        }
    }

    /// <summary>
    /// returns true if security flash ON, false otherwise
    /// </summary>
    /// <returns></returns>
    public bool CheckSecurityFlash()
    { return isSecurityFlash; }

    /// <summary>
    /// directly updates top widget UI with loaded saved game data (bypasses multiple event calls)
    /// </summary>
    /// <param name="data"></param>
    public void LoadSavedData(TopWidgetData data)
    {
        if (data != null)
        {
            SetSides(data.side);
            SetTurn(data.turn);
            SetActionPoints(data.actionPoints);
            SetCityBar(data.cityLoyalty);
            SetFactionBar(data.factionSupport);
            SetSecurityFlasher(data.isSecurityFlash);
            //Objectives -> handled by ObjectiveManager.cs
        }
        else { Debug.LogError("Invalid data (Null)"); }
    }

    /// <summary>
    /// updates city icon color, normal or alert version  (green or red depending on Player side)
    /// </summary>
    /// <param name="isAlert"></param>
    public void UpdateCityIcon(int loyalty)
    {
        if (loyalty > 0 && loyalty < 10)
        {
            city.text = GameManager.i.guiScript.cityIcon;
            if (isCityCoroutine == true)
            {
                if (myCityCoroutine != null)
                {
                    StopCoroutine(myCityCoroutine);
                    myCityCoroutine = null;
                }
                isCityCoroutine = false;
                city.fontSize = minFontSize;
            }
        }
        else
        {
            switch (GameManager.i.sideScript.PlayerSide.level)
            {
                case 1:
                    //authority
                    switch (loyalty)
                    {
                        case 0:
                            city.text = GameManager.i.guiScript.cityIconBad;
                            if (isCityCoroutine == false) { myCityCoroutine = StartCoroutine("FlashCityIcon"); }
                            break;
                        case 10:
                            city.text = GameManager.i.guiScript.cityIconGood;
                            if (isCityCoroutine == false) { myCityCoroutine = StartCoroutine("FlashCityIcon"); }
                            break;
                        default: Debug.LogWarningFormat("Unrecognised loyalty \"{0}\"", loyalty); break;
                    }
                    break;
                case 2:
                    //resistance
                    switch (loyalty)
                    {
                        case 0:
                            city.text = GameManager.i.guiScript.cityIconGood;
                            if (isCityCoroutine == false) { myCityCoroutine = StartCoroutine("FlashCityIcon"); }
                            break;
                        case 10:
                            city.text = GameManager.i.guiScript.cityIconBad;
                            if (isCityCoroutine == false) { myCityCoroutine = StartCoroutine("FlashCityIcon"); }
                            break;
                        default: Debug.LogWarningFormat("Unrecognised loyalty \"{0}\"", loyalty); break;
                    }
                    break;
                default: Debug.LogWarningFormat("Unrecognised Player side \"{0}\"", GameManager.i.sideScript.PlayerSide.name); break;
            }
        }
    }

    /// <summary>
    /// updates hq icon, normal or alert version (red)
    /// </summary>
    /// <param name="isAlert"></param>
    public void UpdateHqIcon(int approval)
    {
        if (approval == 0)
        {
            hq.text = GameManager.i.guiScript.hqIconBad;
            if (isHqCoroutine == false)
            { myHqCoroutine  = StartCoroutine("FlashHqIcon"); }
        }
        else
        {
            hq.text = GameManager.i.guiScript.hqIcon;
            if (isHqCoroutine == true)
            {
                if (myHqCoroutine != null)
                {
                    StopCoroutine(myHqCoroutine);
                    myHqCoroutine = null;
                }
                isHqCoroutine = false;
                hq.fontSize = minFontSize;
            }
        }
    }


    /// <summary>
    /// coroutine to flash city icon while ever at critical level.
    /// </summary>
    /// <returns></returns>
    IEnumerator FlashCityIcon()
    {
        isCityCoroutine = true;
        isGrowCity = true;
        do
        {
            switch (isGrowCity)
            {
                case true:
                    //Grow
                    if (city.fontSize < maxFontSize)
                    {
                        //grow by half a font size
                        city.fontSize += fontIncrement;
                    }
                    else { isGrowCity = false; }
                    break;
                case false:
                    //Shrink
                    if (city.fontSize > minFontSize)
                    {
                        //shrink by half a font size
                        city.fontSize -= fontIncrement;
                    }
                    else { isGrowCity = true; }
                    break;
            }
            //pause
            yield return new WaitForSeconds(timeInterval);
        }
        while (isCityCoroutine == true);
        yield return null;
    }

    /// <summary>
    /// coroutine to flash hq icon while ever at crisis level
    /// </summary>
    /// <returns></returns>
    IEnumerator FlashHqIcon()
    {
        isHqCoroutine = true;
        isGrowHq = true;


        /*do
        {
            //adjust font size up and down
            if (hq.fontSize == 14)
            { hq.fontSize = 16; }
            else { hq.fontSize = 14; }
            //pause
            yield return new WaitForSeconds(0.5f);
        }*/

        do
        {
            switch (isGrowHq)
            {
                case true:
                    //Grow
                    if (hq.fontSize < maxFontSize)
                    {
                        //grow by half a font size
                        hq.fontSize += fontIncrement;
                    }
                    else { isGrowHq = false; }
                    break;
                case false:
                    //Shrink
                    if (hq.fontSize > minFontSize)
                    {
                        //shrink by half a font size
                        hq.fontSize -= fontIncrement;
                    }
                    else { isGrowHq = true; }
                    break;
            }
            //pause
            yield return new WaitForSeconds(timeInterval);
        }
        while (isHqCoroutine == true);
        yield return null;
    }

    //new methods above here
}
