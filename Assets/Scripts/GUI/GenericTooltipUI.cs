using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// handles Generic tooltips. Text only. Attach to required GameObject
/// </summary>
public class GenericTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [HideInInspector] public string tooltipHeader;
    [HideInInspector] public string tooltipMain;
    [HideInInspector] public string tooltipDetails;

    [HideInInspector] public int x_offset;                  //ignore unless you want to shift the default position of the tooltip
    [HideInInspector] public int y_offset;

    [HideInInspector] public int nodeID;                    //node ID, default -1, if viable ( >= 0) will serve to highlight that node while tooltip is showing
    [HideInInspector] public int connID;                    //connID, default -1, if viable ( >= 0) will serve to highlight that connection while tooltip is showing

    [HideInInspector] public string testTag;                //use for debugging purposes to see where the tooltip originated from
    [HideInInspector] public bool isIgnoreClick;            //if true then will ignore all mouse clicks

    private bool isHighlightOn;                             //true if highlight currently on (node or connection)
    private float mouseOverDelay;
    private float mouseOverFade;
    private bool onMouseFlag;
    private Coroutine myCoroutine;

    private void Awake()
    {
        //NOTE: need to initialise here in Awake, doing so in Start causes issues
        nodeID = -1;
        connID = -1;
        isHighlightOn = false;
        isIgnoreClick = false;
    }

    /// <summary>
    /// constructor -> needs to be Start as GameManager hasn't initialised prior to this (UI elements initialise before normal gameObjects?)
    /// </summary>
    private void Start()
    {
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        mouseOverFade = GameManager.instance.tooltipScript.tooltipFade;
    }

    /// <summary>
    /// Mouse Over event
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        onMouseFlag = true;
        if (string.IsNullOrEmpty(tooltipMain) == false)
        { myCoroutine = StartCoroutine("ShowGenericTooltip"); }
        //node Highlight?
        if (nodeID > -1 && isHighlightOn == false)
        {
            Debug.LogFormat("[Tst] GenericTooltipUI.cs -> OnPointerEnter: {0} x_offset {1}{2}", testTag, x_offset, "\n");
            EventManager.instance.PostNotification(EventType.HighlightNodeShow, this, nodeID, "GenericTooltipUI.cs -> OnPointerEnter");
            isHighlightOn = true;
        }
    }

    /// <summary>
    /// Mouse exit, close tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        onMouseFlag = false;
        if (myCoroutine != null)
        { StopCoroutine(myCoroutine); }
        GameManager.instance.tooltipGenericScript.CloseTooltip();
        //cancel node highlight
        if (nodeID > -1 && isHighlightOn == true)
        {
            Debug.LogFormat("[Tst] GenericTooltipUI.cs -> OnPointerEXIT: {0} x_offset {1}{2}", testTag, x_offset, "\n");
            EventManager.instance.PostNotification(EventType.HighlightNodeReset, this, nodeID, "GenericTooltipUI.cs -> OnPointerExit");
            isHighlightOn = false;
        }
    }

    /// <summary>
    /// Mouse click on button, don't forget to close tooltip. If isIgnoreClick then mouse clicks have no effect for this class
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isIgnoreClick == false)
        {
            onMouseFlag = false;
            if (myCoroutine != null)
            { StopCoroutine(myCoroutine); }
            GameManager.instance.tooltipGenericScript.CloseTooltip();
        }
    }


    IEnumerator ShowGenericTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over button
        if (onMouseFlag == true)
        {
            //take into account any user supplied position adjustments (by default will be '0')
            Vector3 position = transform.position;
            position.x += x_offset;
            position.y += y_offset;
            while (GameManager.instance.tooltipGenericScript.CheckTooltipActive() == false)
            {
                GameManager.instance.tooltipGenericScript.SetTooltip (tooltipMain, position, tooltipHeader, tooltipDetails );
                yield return null;
            }
            //fade in
            float alphaCurrent;
            while (GameManager.instance.tooltipGenericScript.GetOpacity() < 1.0)
            {
                alphaCurrent = GameManager.instance.tooltipGenericScript.GetOpacity();
                alphaCurrent += Time.deltaTime / mouseOverFade;
                GameManager.instance.tooltipGenericScript.SetOpacity(alphaCurrent);
                yield return null;
            }
        }
    }


}
