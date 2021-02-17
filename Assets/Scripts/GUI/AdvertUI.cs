using gameAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles start of turns advertisements
/// </summary>
public class AdvertUI : MonoBehaviour
{
    [Header("Main Components")]
    public Canvas advertCanvas;
    public GameObject advertObject;
    public Image innerPanel;
    public Image outerPanel;
    public Image advertBarBack;
    public Image advertBarFront;
    public Image logo;
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textTop;
    public TextMeshProUGUI textBottom;

    //private
    private RectTransform barFrontTransform;
    private RectTransform barBackTransform;
    /*private bool isFading;
    private Color outerColour;*/
    private float flashBorder;
    private float panelOffset;                   //offset distance to get panels off screen during development
    private int maxNameChars;                    //max number of chars in playerName before it's swapped to a default text to prevent overflowing
    private string colourBlue;
    private string colourRed;
    private string endTag;
    private string sizeLarge;
    private Coroutine myCoroutineAdvert;
    private Coroutine myCoroutineGrowBars;
    private Coroutine myCoroutineShrinkBars;

    //bars
    private float barMin;
    private float barSize;
    private float barGrowTime;
    private float barShrinkTime;
    private float barSpeed;
    private float barMax;

    /*//Name text (pulses up and down in size)
    private float fontSizeMax;
    private float fontSizeMin;
    private float fontSizeCurrent;
    private float fontSizeCounter;
    private float fontSizeCounterMax;
    private float fontSizeSpeed;
    private float fontSizeBoost;           //will grow at a faster rate than shrinking due to the boost
    private Pulsing fontSizeState;*/

    private float pos_x;
    private float pos_y;

    #region Static Instance
    private static AdvertUI advertUI;

    /// <summary>
    /// Static instance so the advertUI can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static AdvertUI Instance()
    {
        if (!advertUI)
        {
            advertUI = FindObjectOfType(typeof(AdvertUI)) as AdvertUI;
            if (!advertUI)
            { Debug.LogError("There needs to be one active advertUI script on a GameObject in your scene"); }
        }
        return advertUI;
    }
    #endregion

    #region Initialise
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.StartUp:
            case GameState.NewInitialisation:
                SubInitialiseAsserts();
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                SubInitialiseEvents();
                break;
            case GameState.LoadAtStart:
                SubInitialiseAsserts();
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
    #endregion

    #region Initialise SubMethods

    #region SubInitialiseAsserts
    private void SubInitialiseAsserts()
    {
        Debug.Assert(advertCanvas != null, "Invalid advertCanvas (Null)");
        Debug.Assert(advertObject != null, "Invalid advertObject (Null)");
        Debug.Assert(innerPanel != null, "Invalid innerPanel (Null)");
        Debug.Assert(outerPanel != null, "Invalid outerPanel (Null)");
        Debug.Assert(logo != null, "Invalid logo (Null)");
        Debug.Assert(textName != null, "Invalid textName (Null)");
        Debug.Assert(textTop != null, "Invalid textTop (Null)");
        Debug.Assert(textBottom != null, "Invalid textBottom (Null)");
        //advert Bar Front
        if (advertBarFront != null)
        {
            barFrontTransform = advertBarFront.GetComponent<RectTransform>();
            if (barFrontTransform == null)
            { Debug.LogError("Invalid barTransformFront (Null)"); }
        }
        else { Debug.LogError("Invalid advertBarFront (Null)"); }
        //advert Bar Back
        if (advertBarBack != null)
        {
            barBackTransform = advertBarBack.GetComponent<RectTransform>();
            if (barBackTransform == null)
            { Debug.LogError("Invalid barTransformBack (Null)"); }
        }
        else { Debug.LogError("Invalid advertBarBack (Null)"); }

        
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        flashBorder = GameManager.i.guiScript.billboardFlash;
        panelOffset = GameManager.i.guiScript.billboardOffset;
        /*fontSizeMin = GameManager.i.guiScript.billboardFontMin;
        fontSizeCounterMax = GameManager.i.guiScript.billboardFontPause;
        fontSizeSpeed = GameManager.i.guiScript.billboardFontSpeed;
        fontSizeBoost = GameManager.i.guiScript.billboardFontBoost;*/
        maxNameChars = GameManager.i.guiScript.billboardNameMax;
        //Asserts
        Debug.Assert(flashBorder > 0.0f, "Invalid flashNeon (Zero)");
        Debug.Assert(panelOffset > 0.0f, "Invalid panelOffset (Zero)");
       /* Debug.Assert(fontSizeMin > 0.0f, "Invalid fontSizeMin (Zero)");
        Debug.Assert(fontSizeCounterMax > 0.0f, "Invalid fontSizeCounterMax (Zero)");
        Debug.Assert(fontSizeSpeed > 0.0f, "Invalid fontSizeSpeed (Zero)");
        Debug.Assert(fontSizeBoost > 0.0f, "Invalid fontSizeBoost (Zero)");*/
        Debug.Assert(maxNameChars > 0, "Invalid maxNameChars (Zero)");
    }
    #endregion

    #region SubInitialiseSessionStart
    private void SubInitialiseSessionStart()
    {

        //colours
        colourRed = "<color=#EB6424>";
        colourBlue = "<color=#1AFFD5>";
        endTag = "</color></size>";
        sizeLarge = "<size=130%>";

        /*outerColour = outerPanel.color;*/

        //positions
        pos_x = Screen.width / 2;
        pos_y = Screen.height / 2;
        //Fixed position at screen centre
        advertObject.transform.position = new Vector3(pos_x, pos_y);
        //name text won't pulse without this
        textName.gameObject.SetActive(false);   //remove if you want to reactivate name

        /*textName.enableAutoSizing = false;*/

        //initialise listOfBillboard text
        GameManager.i.dataScript.InitialiseBillboardList();
        //turn off canvas (just in case)
        advertCanvas.gameObject.SetActive(false);
        //reset panels On at start
        innerPanel.gameObject.SetActive(true);
        outerPanel.gameObject.SetActive(false);
        /*SetAdvertCentre(true);*/

        //set bars at just under panel width
        barMin = innerPanel.rectTransform.sizeDelta.x;
        barGrowTime = GameManager.i.guiScript.advertGrowTime;
        barShrinkTime = GameManager.i.guiScript.advertShrinkTime;
        barSpeed = Screen.width;
        barMax = Screen.width;
        barFrontTransform.sizeDelta = new Vector3 (barSize, barFrontTransform.sizeDelta.y);
        barBackTransform.sizeDelta = new Vector3 (barSize, barBackTransform.sizeDelta.y);
        //activate bars
        advertBarBack.gameObject.SetActive(true);
        advertBarFront.gameObject.SetActive(true);
        
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listener
        EventManager.i.AddListener(EventType.AdvertClose, OnEvent, "AdvertUI");
    }
    #endregion

    #endregion

    #region OnEvent
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
            case EventType.AdvertClose:
                StartCoroutine(CloseAdvert());
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }
    #endregion



    /// <summary>
    /// Update and run AdvertUI
    /// </summary>
    public void RunAdvert()
    {
        //full billboard display
        Billboard billboard = GameManager.i.dataScript.GetRandomBillboard();
        if (billboard != null)
        {
            Debug.LogFormat("[UI] AdvertUI.cs -> RunAdvert: Start SetAdvertUI with \"{0}\" display{1}", billboard.name, "\n");
            myCoroutineAdvert = StartCoroutine("OpenAdvert", billboard);
            myCoroutineGrowBars = StartCoroutine("GrowBars");
        }
        else { Debug.LogWarning("Invalid billboard (Null)"); }
    }


    /// <summary>
    /// Open Advert
    /// </summary>
    private IEnumerator OpenAdvert(Billboard billboard)
    {
        /*int counter;*/
        textTop.text = ProcessAdvertTextTop(billboard);
        textBottom.text = billboard.textBottom.ToUpper();
        /*textName.text = GameManager.i.playerScript.FirstName.ToUpper();*/
        if (billboard.sprite != null)
        {
            logo.sprite = billboard.sprite;
            logo.gameObject.SetActive(true);
        }
        else { logo.gameObject.SetActive(false); }

        #region name Archive
        /*
        //any longer than set num of char's will cause issues with pulsing, use a default text instead
        if (textName.text.Length > maxNameChars)
        { textName.text = "Yes YOU!"; }
        //determine parameters for name text font size pulsing (max size is current max size feasible in space)
        fontSizeMax = textName.fontSize;
        fontSizeCurrent = fontSizeMax;
        fontSizeState = Pulsing.Fading;
        fontSizeCounter = 0.0f;
        */
        #endregion

        //set states
        ModalStateData package = new ModalStateData() { mainState = ModalSubState.Advert };
        GameManager.i.inputScript.SetModalState(package);
        GameManager.i.guiScript.SetIsBlocked(true);
        Debug.LogFormat("[UI] AdvertUI.cs -> OpenAdvert{0}", "\n");
        //open canvas
        advertCanvas.gameObject.SetActive(true);

        /*SetAdvertCentre(true);
        counter = 0;*/

        //indefinitely strobe outer panel (cyan neon borders)
        /*isFading = true;*/

        #region while archive
        /*while (true)
        {
            // - - - Strobe outer panel
            outerColour = outerPanel.color;
            if (isFading == false)
            {
                outerColour.a += Time.deltaTime / flashBorder;
                if (outerColour.a >= 1.0f)
                {
                    isFading = true;
                    counter = 0;
                }
            }
            else
            {
                if (counter == 0)
                {
                    outerColour.a -= Time.deltaTime / flashBorder;
                    if (outerColour.a <= 0.0f)
                    { counter++; }
                }
                else
                {
                    //when panel at 0 alpha, pause before repeating
                    if (counter > 120)
                    { isFading = false; }
                    else { counter++; }
                }
            }
            outerPanel.color = outerColour;
            /*
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
            textName.fontSize = fontSizeCurrent;
            
            yield return null;
        }*/
        #endregion

        yield return null;
    }

    /// <summary>
    /// Grow bars behind advert upon opening
    /// </summary>
    /// <returns></returns>
    private IEnumerator GrowBars()
    {
        barSize = barMin;
        while (barBackTransform.sizeDelta.x < barMax)
        {
            barSize += Time.deltaTime / barGrowTime * barSpeed;
            //both grow together
            barBackTransform.sizeDelta = new Vector2(barSize, barBackTransform.sizeDelta.y);
            barFrontTransform.sizeDelta = new Vector2(barSize, barFrontTransform.sizeDelta.y);
            yield return null;
        }
    }

    /// <summary>
    /// Shrink bars (behind advert) to width of Advert panel. Used when closing
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShrinkBars()
    {
        barSize = barBackTransform.sizeDelta.x;
        while (barBackTransform.sizeDelta.x > barMin)
        {
            barSize -= Time.deltaTime / barShrinkTime * barSpeed;
            //both shrink together
            barBackTransform.sizeDelta = new Vector2(barSize, barBackTransform.sizeDelta.y);
            barFrontTransform.sizeDelta = new Vector2(barSize, barFrontTransform.sizeDelta.y);
            yield return null;
        }
    }



    /// <summary>
    /// Close advert UI
    /// </summary>
    private IEnumerator CloseAdvert()
    {
        if (myCoroutineGrowBars != null)
        { StopCoroutine(myCoroutineGrowBars); }
        myCoroutineShrinkBars = StartCoroutine("ShrinkBars");
        yield return myCoroutineShrinkBars;
        StopCoroutine(myCoroutineAdvert);
        //disable canvas
        advertCanvas.gameObject.SetActive(false);

        /*//reset name size
        textName.fontSize = fontSizeMax;*/

        //close advertUI and go straight to MainInfoApp
        GameManager.i.guiScript.waitUntilDone = false;
    }

    

    #region ProcessAdvertTextTop
    /// <summary>
    /// Takes textTop from advert and converts any '[' and ']' tags (topText only) into textmeshPro tags
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private string ProcessAdvertTextTop(Billboard billboard)
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

    /*
    /// <summary>
    /// Toggles advert centre elements on/off
    /// </summary>
    /// <param name="isSwitchOn"></param>
    private void SetAdvertCentre(bool isSwitchOn)
    {
        if (isSwitchOn)
        {
            //enable centre elements
            innerPanel.gameObject.SetActive(true);
            outerPanel.gameObject.SetActive(true);
        }
        else
        {
            //disable centre elements
            innerPanel.gameObject.SetActive(false);
            outerPanel.gameObject.SetActive(false);
        }
    }
    */



    //new methods above here
}
