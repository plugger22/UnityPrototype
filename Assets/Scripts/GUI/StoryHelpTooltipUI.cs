using gameAPI;
using System.Collections;
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
    public void SetHelpTooltip(StoryHelp storyHelpInput, int x_offset = 200, int y_offset = 0)
    {
        if (storyHelpInput != null)
        { storyHelp = storyHelpInput; }
        else
        {
            Debug.LogWarning("Invalid storyHelp parameter (Null)");
            //default data
            storyHelp = GameManager.i.dataScript.GetStoryHelp("default");
            if (storyHelp == null)
            { Debug.LogWarning("Invalid storyHelp (Null) as default help not found in dict"); }
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
        //check modal block isn't in place                                   
        if (GameManager.i.guiScript.CheckIsBlocked(2) == false)
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
        if (GameManager.i.guiScript.CheckIsBlocked(2) == false)                
        {
            onMouseFlag = false;
            if (myCoroutine != null)
            { StopCoroutine(myCoroutine); }
            GameManager.i.tooltipStoryScript.CloseTooltip("StoryHelpTooltipUI.cs -> OnPointerExit");
        }
    }


    /// <summary>
    /// Show storyHelp tooltip
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over button
        if (onMouseFlag == true && GameManager.i.inputScript.ModalState == ModalState.ModalUI)
        {
            if (storyHelp != null)
            {
                //do once
                Vector3 screenPos = transform.position;
                screenPos.x += x_offset;
                screenPos.y += y_offset;
                while (GameManager.i.tooltipStoryScript.CheckTooltipActive() == false && GameManager.i.guiScript.CheckIsBlocked(2) == false)
                {
                    GameManager.i.tooltipStoryScript.SetTooltip(storyHelp, screenPos);
                    yield return null;
                }
            }
            else { Debug.LogWarning("Invalid storyHelp (Null)"); }
        }
    }


}
