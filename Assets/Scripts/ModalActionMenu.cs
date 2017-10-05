﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using modalAPI;



/// <summary>
/// handles modal Action Menu for nodes
/// </summary>
public class ModalActionMenu : MonoBehaviour
{
    public GameObject modalActionObject;
    public GameObject modalMenu;
    public Image modalPanel;
    public Image background;
    public Image divider;
    public TextMeshProUGUI nodeDetails;
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;
    public Button button5;
    public Button button6;
    public TextMeshProUGUI button1Text;
    public TextMeshProUGUI button2Text;
    public TextMeshProUGUI button3Text;
    public TextMeshProUGUI button4Text;
    public TextMeshProUGUI button5Text;
    public TextMeshProUGUI button6Text;
    [HideInInspector] public string tooltip1;
    [HideInInspector] public string tooltip2;
    [HideInInspector] public string tooltip3;
    [HideInInspector] public string tooltip4;
    [HideInInspector] public string tooltip5;


    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private float fadeInTime;
    private int offset;

    //colour palette
    /*private string colourEffects;
    private string colourEnd;*/

    private static ModalActionMenu modalActionMenu;

    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        canvasGroup = modalMenu.GetComponent<CanvasGroup>();
        rectTransform = modalMenu.GetComponent<RectTransform>();
        fadeInTime = GameManager.instance.tooltipScript.tooltipFade;
        offset = GameManager.instance.tooltipScript.tooltipOffset * 2;
        //SetColours();
    }


   /* public void SetColours()
    {
        colourEffects = GameManager.instance.colourScript.GetColour(ColourType.actionEffect);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }*/

    /// <summary>
    /// Static instance so the Modal Menu can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalActionMenu Instance()
    {
        if (!modalActionMenu)
        {
            modalActionMenu = FindObjectOfType(typeof(ModalActionMenu)) as ModalActionMenu;
            if (!modalActionMenu)
            { Debug.LogError("There needs to be one active ModalActionMenu script on a GameObject in your scene"); }
        }
        return modalActionMenu;
    }

    /// <summary>
    /// Initialise and activate modal Action Menu
    /// </summary>
    /// <param name="details"></param>
    public void SetActionMenu(ModalPanelDetails details)
    {
        modalActionObject.SetActive(true);
        //set all states to off
        button1.gameObject.SetActive(false);
        button2.gameObject.SetActive(false);
        button3.gameObject.SetActive(false);
        button4.gameObject.SetActive(false);
        button5.gameObject.SetActive(false);
        button6.gameObject.SetActive(false);

        //set up ModalActionObject
        nodeDetails.text = string.Format("{0}{1}{2}", details.nodeName, "\n", details.nodeDetails);
        //pass nodeID onto text script to facilitate node tooltip on mouseover
        ModalMenuUI modal = nodeDetails.GetComponent<ModalMenuUI>();
        modal.NodeID = details.nodeID;
        //There can be a max of 6 buttons (1 x target, 4 x actor actions, 1 x Cancel)
        int counter = 0;
        Button tempButton;
        TextMeshProUGUI title;
        foreach(EventButtonDetails buttonDetails in details.listOfButtonDetails)
        {
            tempButton = null;
            title = null;
            counter++;
            //get the relevent UI elements
            switch (counter)
            {
                case 1:
                    tempButton = button1;
                    title = button1Text;
                    break;
                case 2:
                    tempButton = button2;
                    title = button2Text;
                    break;
                case 3:
                    tempButton = button3;
                    title = button3Text;
                    break;
                case 4:
                    tempButton = button4;
                    title = button4Text;
                    break;
                case 5:
                    tempButton = button5;
                    title = button5Text;
                    break;
                case 6:
                    tempButton = button6;
                    title = button6Text;
                    break;
                default:
                    Debug.LogWarning("To many EventButtonDetails in list!\n");
                    break;
            }
            //set up the UI elements
            if (tempButton != null && title != null)
            {
                tempButton.onClick.RemoveAllListeners();
                tempButton.onClick.AddListener(buttonDetails.action);
                tempButton.onClick.AddListener(CloseActionMenu);
                title.text = buttonDetails.buttonTitle;
                tempButton.gameObject.SetActive(true);
                GenericTooltipUI generic = tempButton.GetComponent<GenericTooltipUI>();
                generic.ToolTipHeader = buttonDetails.buttonTooltipHeader;
                generic.ToolTipMain = buttonDetails.buttonTooltipMain;
                generic.ToolTipEffect = buttonDetails.buttonTooltipDetail;
            }
        }

        //block raycasts to gameobjects
        GameManager.instance.isBlocked = true;

        //convert coordinates
        Vector3 screenPos = Camera.main.WorldToScreenPoint(details.nodePos);
        //get dimensions of dynamic tooltip
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        //height showing zero due to layout group for first call
        if (height == 0)
        {
            Canvas.ForceUpdateCanvases();
            height = rectTransform.rect.height;
        }
        //calculate offset - height (default above)
        if (screenPos.y + height + offset < Screen.height)
        { screenPos.y += height + offset; }
        else { screenPos.y -= offset; }
        //width - default right
        if (screenPos.x + offset >= Screen.width)
        { screenPos.x -= + offset + screenPos.x - Screen.width; }
        //go left if needed
        else if (screenPos.x - offset - width <= 0)
        { screenPos.x += - offset - width - screenPos.x; }
        else
        { screenPos.x += offset; }
        //set new position
        modalMenu.transform.position = screenPos;
    }


    /// <summary>
    /// close Action Menu
    /// </summary>
    public void CloseActionMenu()
    {
        modalActionObject.SetActive(false);
        GameManager.instance.isBlocked = false;
        //remove highlight from node
        GameManager.instance.nodeScript.ToggleNodeHighlight();
    }
}


