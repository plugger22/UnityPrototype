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
    [HideInInspector] public string tooltipEffect;

    [HideInInspector] public int x_offset;                  //ignore unless you want to shift the default position of the tooltip
    [HideInInspector] public int y_offset;

    private float mouseOverDelay;
    private float mouseOverFade;
    private bool onMouseFlag;
    private Coroutine myCoroutine;


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
    }

    /// <summary>
    /// Mouse click on button, don't forget to close tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        onMouseFlag = false;
        if (myCoroutine != null)
        { StopCoroutine(myCoroutine); }
        GameManager.instance.tooltipGenericScript.CloseTooltip();        
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
                GameManager.instance.tooltipGenericScript.SetTooltip (tooltipMain, position, tooltipHeader, tooltipEffect );
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
