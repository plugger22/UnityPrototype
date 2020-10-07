using gameAPI;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public TextMeshProUGUI billTextTop;
    public TextMeshProUGUI billTextBottom;

    private RectTransform billTransformLeft;
    private RectTransform billTransformRight;

    private float halfScreenWidth;
    /*private float width;*/
    private float speed;
    private float counter;
    private float distance;
    private bool isFading;
    private Color outerColour;
    private float flashNeon;
    private float offset;                   //offset distance to get panels off screen during development
    private string colourBlue;
    private string colourRed;
    private string endTag;
    private string sizeLarge;

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
        flashNeon = GameManager.i.guiScript.flashBillboardTime;
        speed = GameManager.i.guiScript.billboardSpeed;
        Debug.Assert(flashNeon > 0.0f, "Invalid flashNeon (Zero)");
        Debug.Assert(speed > 0.0f, "Invalid speed (Zero)");
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
        Debug.Assert(billTextTop != null, "Invalid billTextTop (Null)");
        Debug.Assert(billTextBottom != null, "Invalid billTextBottom (Null)");
        //initialise components
        billTransformLeft = billLeft.GetComponent<RectTransform>();
        billTransformRight = billRight.GetComponent<RectTransform>();
        Debug.Assert(billTransformLeft != null, "Invalid billTransformLeft (Null)");
        Debug.Assert(billTransformRight != null, "Invalid billTransformRight (Null)");
        //colours
        colourRed = "<color=#FF3333>";
        colourBlue = "<color=#66B2FF>";
        endTag = "</color></size>";
        sizeLarge = "<size=120%>";
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
        outerColour = billPanelOuter.color;
        flashNeon = 1.0f;
        speed *= 10;
        //measurements
        halfScreenWidth = Screen.width / 2;
        offset = 135;
        distance = halfScreenWidth + offset;
        //activate
        billCanvas.gameObject.SetActive(true);
        //Reset panels at start
        ResetBillboard();
        //initialise listOfBillboards
        GameManager.i.dataScript.InitialiseBillboardList();
        /*Debug.LogFormat("[Tst] BillboardUI.cs -> halfScreenWidth {0}, panelWidth {1}, distance {2}{3}", halfScreenWidth, width, distance, "\n");*/
    }

    /// <summary>
    /// reset panels just off screen
    /// </summary>
    public void ResetBillboard()
    {
        billPanelOuter.gameObject.SetActive(false);
        billPanelInner.gameObject.SetActive(false);
        billLeft.transform.localPosition = new Vector3(-distance, 0, 0);
        billRight.transform.localPosition = new Vector3(distance, 0, 0);
        Debug.LogFormat("[UI] BillboardUI.cs -> Reset: Reset Billboard{0}", "\n");
    }

    /// <summary>
    /// Main billboard controller
    /// </summary>
    public void RunBillboard()
    {
        /*string displayText = GameManager.i.newsScript.GetAdvert();*/
        Billboard billboard = GameManager.i.dataScript.GetRandomBillboard();
        if (billboard != null)
        {
            Debug.LogFormat("[UI] BillboardUI.cs -> RunBillboard: Start Billboard with \"{0}\"{1}", billboard.name, "\n");
            StartCoroutine("BillOpen", billboard);
        }
        else { Debug.LogWarning("Invalid billboard (Null)"); }
    }

    /// <summary>
    /// coroutine to slide panels together then display billboard (strobes the neon border)
    /// </summary>
    /// <returns></returns>
    private IEnumerator BillOpen(Billboard billboard)
    {
        counter = 0;
        billTextTop.text = ProcessBillboardTextTop(billboard);
        billTextBottom.text = billboard.textBottom;
        GameManager.i.inputScript.SetModalState(new ModalStateData() { mainState = ModalSubState.Billboard });
        while (counter < distance)
        {
            counter += speed * Time.deltaTime;
            billLeft.transform.localPosition = new Vector3(-distance + counter, 0, 0);
            billRight.transform.localPosition = new Vector3(distance - counter, 0, 0);
            yield return null;
        }
        billPanelInner.gameObject.SetActive(true);
        billPanelOuter.gameObject.SetActive(true);
        //indefinitely strobe outer panel (cyan neon borders)
        isFading = true;
        while (true)
        {
            outerColour = billPanelOuter.color;
            if (isFading == false)
            {
                outerColour.a += Time.deltaTime / flashNeon;
                if (outerColour.a >= 1.0f)
                { isFading = true; }
            }
            else
            {
                outerColour.a -= Time.deltaTime / flashNeon;
                if (outerColour.a <= 0.0f)
                { isFading = false; }
            }
            billPanelOuter.color = outerColour;
            yield return null;
        }
    }

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
        if (string.IsNullOrEmpty(billboard.textTop) == false)
        {
            string tempText = billboard.textTop.Replace("[", startTag);
            checkedText = tempText.Replace("]", endTag);
        }
        return checkedText;
    }


    /// <summary>
    /// Close billboard controller
    /// </summary>
    public void CloseBillboard()
    {
        Debug.LogFormat("[UI] BillboardUI.cs -> CloseBillboard: Close Billboard{0}", "\n");
        StopCoroutine("BillOpen");
        StartCoroutine("BillClose");
    }

    /// <summary>
    /// Slides billboard panels back out of view after turning off billboard display
    /// </summary>
    /// <returns></returns>
    private IEnumerator BillClose()
    {
        counter = Convert.ToInt32(halfScreenWidth);
        GameManager.i.inputScript.ResetStates();
        billPanelInner.gameObject.SetActive(false);
        billPanelOuter.gameObject.SetActive(false);
        while (counter > 0)
        {
            counter -= speed * Time.deltaTime;
            billLeft.transform.localPosition = new Vector3(-distance + counter, 0, 0);
            billRight.transform.localPosition = new Vector3(distance - counter, 0, 0);
            yield return null;
        }

    }


    //events above here
}
