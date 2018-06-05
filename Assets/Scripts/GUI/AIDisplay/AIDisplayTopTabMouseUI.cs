using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// handles mouse functionality for AIDisplay top tab (FACTION info)
/// </summary>
public class AIDisplayTopTabMouseUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{


    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip
    private float mouseOverFade;                        //tooltip
    private Coroutine myCoroutine;


    [HideInInspector] public string tooltipHeader;
    [HideInInspector] public string tooltipMain;
    [HideInInspector] public string tooltipDetails;


    public void Start()
    {
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        mouseOverFade = GameManager.instance.tooltipScript.tooltipFade;
    }




    /// <summary>
    /// Mouse over generic tooltip
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        //check modal block is in place
        if (GameManager.instance.guiScript.CheckIsBlocked() == true)
        {
            onMouseFlag = true;
            //start tooltip routine
            myCoroutine = StartCoroutine("ShowTooltip");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click!");
    }


    /// <summary>
    /// mouse over exit, shut down tooltip
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        //check modal block is in place
        if (GameManager.instance.guiScript.CheckIsBlocked() == true)
        {
            onMouseFlag = false;
            if (myCoroutine != null)
            { StopCoroutine(myCoroutine); }
            GameManager.instance.tooltipGenericScript.CloseTooltip();
        }
    }

    /// <summary>
    /// Show generic tooltip
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over button
        if (onMouseFlag == true)
        {
            //do once
            Vector3 screenPos = transform.position;
            screenPos.x -= 20;
            while (GameManager.instance.tooltipGenericScript.CheckTooltipActive() == false)
            {
                tooltipHeader = GameManager.instance.factionScript.GetFactionName();
                tooltipMain = GameManager.instance.factionScript.GetFactionDescription();
                tooltipDetails = GameManager.instance.factionScript.GetFactionDetails();
                GameManager.instance.tooltipGenericScript.SetTooltip(tooltipMain, screenPos, tooltipHeader, tooltipDetails);
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
