using System.Collections;
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
    public TextMeshProUGUI nodeName;
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

    //private static TooltipNode tooltipNode;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private float fadeInTime;
    private int offset;

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
    }

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
        this.nodeName.text = details.nodeName;
        nodeDetails.text = details.nodeDetails;

        //first button
        if (details.button1Details != null)
        {
            button1.onClick.RemoveAllListeners();
            button1.onClick.AddListener(details.button2Details.action);
            button1.onClick.AddListener(CloseActionMenu);
            button1Text.text = details.button1Details.buttonTitle;
            button1.gameObject.SetActive(true);
        }
        //second button
        if (details.button2Details != null)
        {
            button2.onClick.RemoveAllListeners();
            button2.onClick.AddListener(details.button2Details.action);
            button2.onClick.AddListener(CloseActionMenu);
            button2Text.text = details.button2Details.buttonTitle;
            button2.gameObject.SetActive(true);
        }
        //third button
        if (details.button3Details != null)
        {
            button3.onClick.RemoveAllListeners();
            button3.onClick.AddListener(details.button3Details.action);
            button3.onClick.AddListener(CloseActionMenu);
            button3Text.text = details.button3Details.buttonTitle;
            button3.gameObject.SetActive(true);
        }
        //fourth button
        if (details.button4Details != null)
        {
            button4.onClick.RemoveAllListeners();
            button4.onClick.AddListener(details.button4Details.action);
            button4.onClick.AddListener(CloseActionMenu);
            button4Text.text = details.button4Details.buttonTitle;
            button4.gameObject.SetActive(true);
        }
        //fifth button
        if (details.button5Details != null)
        {
            button5.onClick.RemoveAllListeners();
            button5.onClick.AddListener(details.button5Details.action);
            button5.onClick.AddListener(CloseActionMenu);
            button5Text.text = details.button5Details.buttonTitle;
            button5.gameObject.SetActive(true);
        }
        //sixth button is always 'Cancel'
        button6.onClick.RemoveAllListeners();
        button6.onClick.AddListener(CloseActionMenu);
        button6Text.text = "CANCEL";
        button6.gameObject.SetActive(true);

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
        else { screenPos.y -= height + offset; }
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


