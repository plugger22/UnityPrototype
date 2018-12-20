using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles mouse over / mouse click for main info APP RHS item help
/// </summary>
public class MainInfoItemHelpUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip
    private Coroutine myCoroutine;

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
        if (GameManager.instance.guiScript.CheckIsBlocked() == false)
        {
            //Tool tip
            onMouseFlag = true;

            //start tooltip routine
            myCoroutine = StartCoroutine("ShowTooltip");
        }
    }

}
