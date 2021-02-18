using gameAPI;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// End of turn Billboard UI prior to infoPipeline
/// </summary>
public class BillboardUI : MonoBehaviour
{
    public Canvas billCanvas;
    public GameObject billObject;
    public Image billLeft;
    public Image billRight;
    public Image billPanelOuter;
    public Image billPanelInner;
    public Image billPanelName;
    public Image billPanelFrame;
    public Image billLightLeft;
    public Image billLightRight;
    public Image billBeamLeft;
    public Image billBeamRight;
    public Image billTurn;
    public Image billLogo;
    public TextMeshProUGUI billTextTop;
    public TextMeshProUGUI billTextBottom;
    public TextMeshProUGUI billTextName;
    public TextMeshProUGUI billTextTurn;

    private RectTransform billTransformLeft;
    private RectTransform billTransformRight;

    private float halfScreenWidth;
    private float screenSpeed;
    private float screenCounter;
    private float screenDistance;

    //Name text (pulses up and down in size)
    private float fontSizeMax;
    private float fontSizeMin;
    private float fontSizeCurrent;
    private float fontSizeCounter;
    private float fontSizeCounterMax;
    private float fontSizeSpeed;
    private float fontSizeBoost;           //will grow at a faster rate than shrinking due to the boost
    private Pulsing fontSizeState;

    //billboard borders and text colours for main billboard
    private bool isFading;
    private Color outerColour;
    private float flashBorder;
    private float panelOffset;                   //offset distance to get panels off screen during development
    private int maxNameChars;                    //max number of chars in playerName before it's swapped to a default text to prevent overflowing
    private string colourBlue;
    private string colourRed;
    private string endTag;
    private string sizeLarge;
    

    //light beams
    private bool isBeamLeftOn;
    private bool isBeamRightOn;
    private float beamLeftCounter;
    private float beamRightCounter;
    private float beamCounterMax;
    private int beamChance;

    //assorted
    private Coroutine myCoroutine;

    private static BillboardUI billboardUI;

    /// <summary>
    /// Static instance so the billboardUI can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static BillboardUI Instance()
    {
        if (!billboardUI)
        {
            billboardUI = FindObjectOfType(typeof(BillboardUI)) as BillboardUI;
            if (!billboardUI)
            { Debug.LogError("There needs to be one active billboardUI script on a GameObject in your scene"); }
        }
        return billboardUI;
    }


    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.StartUp:
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
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
    private void SubInitialiseFastAccess()
    {
        flashBorder = GameManager.i.guiScript.billboardFlash;
        screenSpeed = GameManager.i.guiScript.billboardSpeed;
        panelOffset = GameManager.i.guiScript.billboardOffset;
        fontSizeMin = GameManager.i.guiScript.billboardFontMin;
        fontSizeCounterMax = GameManager.i.guiScript.billboardFontPause;
        fontSizeSpeed = GameManager.i.guiScript.billboardFontSpeed;
        fontSizeBoost = GameManager.i.guiScript.billboardFontBoost;
        maxNameChars = GameManager.i.guiScript.billboardNameMax;
        beamCounterMax = GameManager.i.guiScript.billboardLightOff;
        beamChance = GameManager.i.guiScript.billboardLightChance;
        //Asserts
        Debug.Assert(flashBorder > 0.0f, "Invalid flashNeon (Zero)");
        Debug.Assert(screenSpeed > 0.0f, "Invalid speed (Zero)");
        Debug.Assert(panelOffset > 0.0f, "Invalid panelOffset (Zero)");
        Debug.Assert(fontSizeMin > 0.0f, "Invalid fontSizeMin (Zero)");
        Debug.Assert(fontSizeCounterMax > 0.0f, "Invalid fontSizeCounterMax (Zero)");
        Debug.Assert(fontSizeSpeed > 0.0f, "Invalid fontSizeSpeed (Zero)");
        Debug.Assert(fontSizeBoost > 0.0f, "Invalid fontSizeBoost (Zero)");
        Debug.Assert(maxNameChars > 0, "Invalid maxNameChars (Zero)");
        Debug.Assert(beamCounterMax > 0.0f, "Invalid beamCounterMax (Zero)");
        Debug.Assert(beamChance > 0, "Invalid beamChance (Zero)");
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {
        Debug.Assert(billCanvas != null, "Invalid billCanvas (Null)");
        Debug.Assert(billObject != null, "Invalid billObject (Null)");
        Debug.Assert(billLeft != null, "Invalid billLeft (Null)");
        Debug.Assert(billRight != null, "Invalid billRight (Null)");
        Debug.Assert(billPanelInner != null, "Invalid billPanel (Null)");
        Debug.Assert(billPanelOuter != null, "Invalid billPanel (Null)");
        Debug.Assert(billPanelName != null, "Invalid billPanelName (Null)");
        Debug.Assert(billPanelFrame != null, "Invalid billPanelFrame (Null)");
        Debug.Assert(billLightLeft != null, "Invalid billLightLeft (Null)");
        Debug.Assert(billLightRight != null, "Invalid billLightRight (Null)");
        Debug.Assert(billBeamLeft != null, "Invalid billBeamLeft (Null)");
        Debug.Assert(billBeamRight != null, "Invalid billBeamRight (Null)");
        Debug.Assert(billTurn != null, "Invalid billTurn (Null)");
        Debug.Assert(billLogo != null, "Invalid billLogo (Null)");
        Debug.Assert(billTextTop != null, "Invalid billTextTop (Null)");
        Debug.Assert(billTextBottom != null, "Invalid billTextBottom (Null)");
        Debug.Assert(billTextName != null, "Invalid billTextName (Null)");
        Debug.Assert(billTextTurn != null, "Invalid billTextTurn (Null)");
        //initialise components
        billTransformLeft = billLeft.GetComponent<RectTransform>();
        billTransformRight = billRight.GetComponent<RectTransform>();
        Debug.Assert(billTransformLeft != null, "Invalid billTransformLeft (Null)");
        Debug.Assert(billTransformRight != null, "Invalid billTransformRight (Null)");
        //colours
        colourRed = "<color=#f71735>";
        colourBlue = "<color=#55C1FF>";
        endTag = "</color></size>";
        sizeLarge = "<size=130%>";
        //initialise billboard
        InitialiseBillboard();
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listener
        EventManager.i.AddListener(EventType.BillboardClose, OnEvent, "BillboardUI");
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
            case EventType.BillboardClose:
                CloseBillboard();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// Set billboard parameters and start position
    /// </summary>
    private void InitialiseBillboard()
    {
        /*fontSizeCounterMax = 1.25f;
        fontSizeSpeed = 1.25f;
        fontSizeBoost = 1.75f;
        fontSizeMin = 12.0f;
        flashBorder = 1.0f;
        panelOffset = 135;
        beamCounterMax = 0.10f;
        beamChance = 1;*/

        screenSpeed *= 10;
        outerColour = billPanelOuter.color;
        //measurements
        halfScreenWidth = Screen.width / 2;
        screenDistance = halfScreenWidth + panelOffset;
        //Name text won't pulse with this on
        billTextName.enableAutoSizing = false;
        //activate
        billCanvas.gameObject.SetActive(true);
        //Reset panels at start
        ResetBillboard();
        //initialise listOfBillboard texts
        GameManager.i.dataScript.InitialiseBillboardList();
        /*Debug.LogFormat("[Tst] BillboardUI.cs -> halfScreenWidth {0}, panelWidth {1}, distance {2}{3}", halfScreenWidth, width, distance, "\n");*/
    }

    #region ResetBillboard
    /// <summary>
    /// reset panels just off screen
    /// </summary>
    public void ResetBillboard()
    {
        billPanelOuter.gameObject.SetActive(false);
        billPanelInner.gameObject.SetActive(false);
        billPanelFrame.gameObject.SetActive(false);
        billLightLeft.gameObject.SetActive(false);
        billLightRight.gameObject.SetActive(false);
        billBeamLeft.gameObject.SetActive(false);
        billBeamRight.gameObject.SetActive(false);
        billTurn.gameObject.SetActive(false);
        billLeft.transform.localPosition = new Vector3(-screenDistance, 0, 0);
        billRight.transform.localPosition = new Vector3(screenDistance, 0, 0);
        Debug.LogFormat("[UI] BillboardUI.cs -> Reset: Reset Billboard{0}", "\n");
    }
    #endregion

    /// <summary>
    /// Main billboard controller (true if billboard displayed for an advertisement, etc., false for a simple turn display)
    /// </summary>
    public void RunBillboard(bool isBillboard)
    {
        /*string displayText = GameManager.i.newsScript.GetAdvert();*/
        if (isBillboard == true)
        {
            //full billboard display
            Billboard billboard = GameManager.i.dataScript.GetRandomBillboard();
            if (billboard != null)
            {
                Debug.LogFormat("[UI] BillboardUI.cs -> RunBillboard: Start BillboardMain with \"{0}\" display{1}", billboard.name, "\n");
                myCoroutine = StartCoroutine("BillOpenMain", billboard);
            }
            else { Debug.LogWarning("Invalid billboard (Null)"); }
        }
        else
        {
            //normal turn display
            int dayNum = GameManager.i.turnScript.Turn;
            Debug.LogFormat("[UI] BillboardUI.cs -> RunBillboard: Start BillboardTurn for Day {0} display{1}", dayNum, "\n");
            myCoroutine = StartCoroutine("BillOpenTurn", dayNum);
        }
    }

    #region BillOpenMain
    /// <summary>
    /// coroutine to slide panels together then display full billboard (strobes the neon border)
    /// </summary>
    /// <returns></returns>
    private IEnumerator BillOpenMain(Billboard billboard)
    {
        /*lightIndex = 0;
        lightCounter = 0;*/

        screenCounter = 0;
        billTextTop.text = ProcessBillboardTextTop(billboard);
        billTextBottom.text = billboard.textBottom.ToUpper();
        billTextName.text = GameManager.i.playerScript.FirstName.ToUpper();
        if (billboard.sprite != null)
        {
            billLogo.sprite = billboard.sprite;
            billLogo.gameObject.SetActive(true);
        }
        else { billLogo.gameObject.SetActive(false); }
        //any longer than set num of char's will cause issues with pulsing, use a default text instead
        if (billTextName.text.Length > maxNameChars)
        { billTextName.text = "Yes YOU!"; }
        //determine parameters for name text font size pulsing (max size is current max size feasible in space)
        fontSizeMax = billTextName.fontSize;
        fontSizeCurrent = fontSizeMax;
        fontSizeState = Pulsing.Fading;

        fontSizeCounter = 0.0f;
        isBeamLeftOn = true;
        isBeamRightOn = true;
        //modal state
        GameManager.i.inputScript.SetModalState(new ModalStateData() { mainState = ModalSubState.Billboard });
        //slide blinds
        while (screenCounter < screenDistance)
        {
            screenCounter += screenSpeed * Time.deltaTime;
            billLeft.transform.localPosition = new Vector3(-screenDistance + screenCounter, 0, 0);
            billRight.transform.localPosition = new Vector3(screenDistance - screenCounter, 0, 0);
            yield return null;
        }
        SetBillboardCentre(true);
        //indefinitely strobe outer panel (cyan neon borders)
        isFading = true;
        while (true)
        {
            outerColour = billPanelOuter.color;
            if (isFading == false)
            {
                outerColour.a += Time.deltaTime / flashBorder;
                if (outerColour.a >= 1.0f)
                { isFading = true; }
            }
            else
            {
                outerColour.a -= Time.deltaTime / flashBorder;
                if (outerColour.a <= 0.0f)
                { isFading = false; }
            }
            billPanelOuter.color = outerColour;
            //Name text font size Pulsing
            switch (fontSizeState)
            {
                case Pulsing.Constant:
                    //pause pulsing at max font size for a short moment
                    fontSizeCounter += Time.deltaTime;
                    if (fontSizeCounter > fontSizeCounterMax)
                    {
                        fontSizeState = Pulsing.Fading;
                        fontSizeCounter = 0.0f;
                    }
                    break;
                case Pulsing.Growing:
                    //grow at a faster rate than shrinking
                    fontSizeCurrent += fontSizeCurrent * Time.deltaTime * fontSizeSpeed * fontSizeBoost;
                    if (fontSizeCurrent >= fontSizeMax)
                    {
                        fontSizeState = Pulsing.Constant;
                        //make sure fontsize doesn't momentarily go over the max and be outside the displayable area
                        fontSizeCurrent = Mathf.Min(fontSizeCurrent, fontSizeMax);
                    }
                    break;
                case Pulsing.Fading:
                    //shrinking
                    fontSizeCurrent -= fontSizeCurrent * Time.deltaTime * fontSizeSpeed;
                    if (fontSizeCurrent <= fontSizeMin)
                    {
                        fontSizeCurrent = Mathf.Max(0.0f, fontSizeCurrent);
                        fontSizeState = Pulsing.Growing;
                    }
                    break;
                default: Debug.LogWarningFormat("Unrecognised fontSizeState \"{0}\"", fontSizeState); break;
            }
            //adjust font size
            billTextName.fontSize = fontSizeCurrent;
            //light beams at top randomly flash on/off
            if (isBeamLeftOn == true)
            {
                if (Random.Range(0, 100) < beamChance)
                {

                    //switch off
                    billBeamLeft.gameObject.SetActive(false);
                    isBeamLeftOn = false;
                    beamLeftCounter = 0.0f;
                }
            }
            else
            {
                //momentary off (flicker)
                beamLeftCounter += Time.deltaTime;
                if (beamLeftCounter > beamCounterMax)
                {
                    //switch on
                    billBeamLeft.gameObject.SetActive(true);
                    isBeamLeftOn = true;
                }
            }
            if (isBeamRightOn == true)
            {
                if (Random.Range(0, 100) < beamChance)
                {
                    //switch off
                    billBeamRight.gameObject.SetActive(false);
                    isBeamRightOn = false;
                    beamRightCounter = 0.0f;
                }
            }
            else
            {
                beamRightCounter += Time.deltaTime;
                if (beamRightCounter > beamCounterMax)
                {
                    //switch on
                    billBeamRight.gameObject.SetActive(true);
                    isBeamRightOn = true;
                }
            }
            yield return null;
        }
    }
    #endregion

    #region ProcessBillboardTextTop
    /// <summary>
    /// Takes textTop from billboard and converts any '[' and ']' tags (topText only) into textmeshPro tags
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private string ProcessBillboardTextTop(Billboard billboard)
    {
        string checkedText = "Unknown";
        string startTag;
        //normally a blue highlight but can be red. Always upsized
        if (billboard.isRedHighlight == true)
        { startTag = string.Format("{0}{1}", sizeLarge, colourRed); }
        else { startTag = string.Format("{0}{1}", sizeLarge, colourBlue); }
        //switch to Caps
        string text = billboard.textTop.ToUpper();
        //replace tags
        if (string.IsNullOrEmpty(text) == false)
        {
            string tempText = text.Replace("[", startTag);
            checkedText = tempText.Replace("]", endTag);
        }
        return checkedText;
    }
    #endregion

    #region BillOpenTurn
    /// <summary>
    /// coroutine to slide panels together then display simple turn display
    /// </summary>
    /// <returns></returns>
    private IEnumerator BillOpenTurn(int dayNum)
    {
        screenCounter = 0;
        billTextTurn.text = string.Format("Day {0}{1}{2}", dayNum, "\n", GameManager.i.cityScript.GetCityName());
        //modal state
        GameManager.i.inputScript.SetModalState(new ModalStateData() { mainState = ModalSubState.Billboard });
        //slide blinds
        while (screenCounter < screenDistance)
        {
            screenCounter += screenSpeed * Time.deltaTime;
            billLeft.transform.localPosition = new Vector3(-screenDistance + screenCounter, 0, 0);
            billRight.transform.localPosition = new Vector3(screenDistance - screenCounter, 0, 0);
            yield return null;
        }
        SetBillboardCentre(false);
        SetBillboardTurn(true);
        yield return null;
    }
    #endregion


    /// <summary>
    /// Close billboard controller
    /// </summary>
    public void CloseBillboard()
    {
        Debug.LogFormat("[UI] BillboardUI.cs -> CloseBillboard: Close Billboard{0}", "\n");
        StopCoroutine(myCoroutine);
        StartCoroutine("BillClose");
    }

    /// <summary>
    /// Slides billboard panels back out of view after turning off billboard display
    /// </summary>
    /// <returns></returns>
    private IEnumerator BillClose()
    {
        screenCounter = Convert.ToInt32(halfScreenWidth);
        //revert modal state back to normal
        GameManager.i.inputScript.ResetStates();
        //disable centre elements
        SetBillboardCentre(false);
        SetBillboardTurn(false);
        //open up side panels
        while (screenCounter > 0)
        {
            screenCounter -= screenSpeed * Time.deltaTime;
            billLeft.transform.localPosition = new Vector3(-screenDistance + screenCounter, 0, 0);
            billRight.transform.localPosition = new Vector3(screenDistance - screenCounter, 0, 0);
            yield return null;
        }
        billTextName.fontSize = fontSizeMax;

    }

    /// <summary>
    /// Toggles billboard centre elements on/off
    /// </summary>
    /// <param name="isSwitchOn"></param>
    private void SetBillboardCentre(bool isSwitchOn)
    {
        if (isSwitchOn)
        {
            //enable centre elements
            billPanelInner.gameObject.SetActive(true);
            billPanelOuter.gameObject.SetActive(true);
            billPanelFrame.gameObject.SetActive(true);
            billLightLeft.gameObject.SetActive(true);
            billLightRight.gameObject.SetActive(true);
            billBeamLeft.gameObject.SetActive(true);
            billBeamRight.gameObject.SetActive(true);
        }
        else
        {
        //disable centre elements
        billPanelInner.gameObject.SetActive(false);
        billPanelOuter.gameObject.SetActive(false);
        billPanelFrame.gameObject.SetActive(false);
        billLightLeft.gameObject.SetActive(false);
        billLightRight.gameObject.SetActive(false);
        billBeamLeft.gameObject.SetActive(false);
        billBeamRight.gameObject.SetActive(false);
        }
    }


    /// <summary>
    /// Toggles billboard turn display elements on/off
    /// </summary>
    /// <param name="isSwitchOn"></param>
    private void SetBillboardTurn(bool isSwitchOn)
    {
        if (isSwitchOn)
        {
            //enable turn display
            billTurn.gameObject.SetActive(true);
            billTextTurn.gameObject.SetActive(true);
        }
        else
        {
            //disable turn display
            billTurn.gameObject.SetActive(false);
            billTextTurn.gameObject.SetActive(false);
        }
    }


    //events above here
}
