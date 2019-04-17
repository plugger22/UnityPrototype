using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using gameAPI;
using packageAPI;

/// <summary>
/// Handles mouse over for main info APP RHS item help
/// </summary>
public class MainInfoItemHelpUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Positioning")]
    [Tooltip("x Offset (added to screenPos) from pointer position for display of help. Left is negative, Right is positive")]
    public int x_offset = -400;
    [Tooltip("y Offset (added to screenPos) from pointer postion for display of help. Down is negative, Up is positive")]
    public int y_offset = 0;

    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip
    private Coroutine myCoroutine;

    [HideInInspector] public List<HelpData> listOfHelp;

    public void Start()
    {
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
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
