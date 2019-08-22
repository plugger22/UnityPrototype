using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using gameAPI;
using packageAPI;

/// <summary>
/// Handles mouse over help tooltip for help icon for all non-MainInfoApp UI help
/// </summary>
public class GenericHelpTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip
    private Coroutine myCoroutine;

    private int x_offset = 0;
    private int y_offset = 0;
    private List<HelpData> listOfHelp = new List<HelpData>();

    public void Start()
    {
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
    }

    /// <summary>
    /// Initialise tooltip info
    /// </summary>
    /// <param name="listOfHelpData"></param>
    /// <param name="x_offset"></param>
    /// <param name="y_offset"></param>
    public void SetHelpTooltip(List<HelpData> listOfHelpData, int x_offset = 150, int y_offset = 200)
    {
        if (listOfHelpData != null && listOfHelpData.Count > 0)
        {
            listOfHelp.AddRange(listOfHelpData);
        }
        else { Debug.LogWarning("Invalid listOfHelpData (Null or Empty)"); }
        //offsets
        this.x_offset = x_offset;
        this.y_offset = y_offset;
    }

    /// <summary>
    /// Mouse over generic tooltip
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        //check modal block isn't in place
        if (GameManager.instance.guiScript.CheckIsBlocked(2) == false)
        {
            //Tool tip
            onMouseFlag = true;

            //start tooltip routine
            myCoroutine = StartCoroutine("ShowTooltip");
        }
    }

    /// <summary>
    /// mouse over exit, shut down tooltip
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameManager.instance.guiScript.CheckIsBlocked(2) == false)
        {
            onMouseFlag = false;
            if (myCoroutine != null)
            { StopCoroutine(myCoroutine); }
            GameManager.instance.tooltipHelpScript.CloseTooltip("MainInfoItemHelp.cs -> OnPointerExit");
        }
    }

    /// <summary>
    /// Show Help tooltip
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over button
        if (onMouseFlag == true && GameManager.instance.inputScript.ModalState == ModalState.ModalUI)
        {
            if (listOfHelp != null)
            {
                if (listOfHelp.Count > 0)
                {
                    //do once
                    Vector3 screenPos = transform.position;
                    screenPos.x += x_offset;
                    screenPos.y += y_offset;
                    while (GameManager.instance.tooltipHelpScript.CheckTooltipActive() == false && GameManager.instance.guiScript.CheckIsBlocked(2) == false)
                    {
                        GameManager.instance.tooltipHelpScript.SetTooltip(listOfHelp, screenPos);
                        yield return null;
                    }
                }
                else { Debug.LogWarning("Invalid listOfHelp (Empty)"); }
            }
            else { Debug.LogWarning("Invalid listOfHelp (Null)"); }
        }
    }
}
