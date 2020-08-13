using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles mouse over help tooltip for storyHelp icons and acts as a front end to TooltipStory.cs
/// </summary>
public class StoryHelpTooltipUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private bool onMouseFlag;                           //flag indicates that onMouseOver is true (used for tooltip coroutine)
    private float mouseOverDelay;                       //tooltip
    private Coroutine myCoroutine;

    private int x_offset = 0;
    private int y_offset = 0;
    private StoryHelp storyHelp;

    public void Start()
    {
        mouseOverDelay = GameManager.i.guiScript.tooltipDelay;
    }

    /// <summary>
    /// Initialise tooltip info
    /// </summary>
    /// <param name="listOfHelpData"></param>
    /// <param name="x_offset"></param>
    /// <param name="y_offset"></param>
    public void SetHelpTooltip(StoryHelp storyHelpInput, int x_offset = 150, int y_offset = 0)
    {
        if (storyHelpInput != null)
        { storyHelp = storyHelpInput; }
        else
        {
            Debug.LogWarning("Invalid storyHelp parameter (Null)");
            //default data
            storyHelp = GameManager.i.dataScript.GetStoryHelp("default");
        }
        //offsets
        this.x_offset = x_offset;
        this.y_offset = y_offset;
    }

    /// <summary>
    /// Mouse over generic tooltip
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        /*//check modal block isn't in place                                    EDIT: Jun '20 removed block check in order to make ModalOutcome help work on ModalLevel 2 (code changed in coroutine)
        if (GameManager.i.guiScript.CheckIsBlocked(2) == false)
        {
            //Tool tip
            onMouseFlag = true;
            //start tooltip routine
            myCoroutine = StartCoroutine("ShowTooltip");
        }*/

        //Tool tip
        onMouseFlag = true;
        //start tooltip routine
        myCoroutine = StartCoroutine("ShowTooltip");
    }

    /// <summary>
    /// mouse over exit, shut down tooltip
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        /*if (GameManager.i.guiScript.CheckIsBlocked(2) == false)                EDIT: Jun '20 removed block check in order to make ModalOutcome help work on ModalLevel 2 (code changed in coroutine)
        {
            onMouseFlag = false;
            if (myCoroutine != null)
            { StopCoroutine(myCoroutine); }
            GameManager.i.tooltipHelpScript.CloseTooltip("GenericHelpTooltipUI.cs -> OnPointerExit");
        }*/

        onMouseFlag = false;
        if (myCoroutine != null)
        { StopCoroutine(myCoroutine); }
        GameManager.i.tooltipHelpScript.CloseTooltip("GenericHelpTooltipUI.cs -> OnPointerExit");
    }

}
