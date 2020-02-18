﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReviewInteraction : MonoBehaviour, IPointerClickHandler
{

    public Image optionImage;
    public Image resultImage;
    public TextMeshProUGUI textUpper;
    public TextMeshProUGUI textLower;
    public TextMeshProUGUI textResult;
    public GenericTooltipUI tooltipSprite;                        //EDIT -> tooltip component attached to gameobject rather than sprite as can't get mouseover and click detection from sprite without code change
    public GenericTooltipUI tooltipStars;
    public GenericTooltipUI tooltipResult;
    [HideInInspector] public int optionData;                                            //multipurpose field to hold ID of actor, etc.



    public void OnEnable()
    {
        Debug.Assert(optionImage != null, "Invalid optionImage (Null)");
        Debug.Assert(resultImage != null, "Invalid resultImage (Null)");
        Debug.Assert(textUpper != null, "Invalid textUpper (Null)");
        Debug.Assert(textLower != null, "Invalid textLower (Null)");
        Debug.Assert(textResult != null, "Invalid textResult (Null)");
        Debug.Assert(tooltipSprite != null, "Invalid tooltipSprite (Null)");
        Debug.Assert(tooltipStars != null, "Invalid tooltipStars (Null)");
        Debug.Assert(tooltipResult != null, "Invalid tooltipResult (Null)");        
    }

    /// <summary>
    /// Mouse click -> Right: Actor Action Menu
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                Debug.LogFormat("[Tst] InventoryInteraction.cs -> OnPointerClick: LEFT BUTTON CLICKED{0}", "\n");
                break;
            case PointerEventData.InputButton.Right:
                Debug.LogFormat("[Tst] InventoryInteraction.cs -> OnPointerClick: RIGHT BUTTON CLICKED{0}", "\n");
                break;
            default:
                Debug.LogError("Unknown InputButton");
                break;
        }
    }
}
