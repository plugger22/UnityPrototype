using modalAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles Modal Outcome window
/// </summary>
public class ModalOutcome : MonoBehaviour
{
    public TextMeshProUGUI outcomeText;
    public TextMeshProUGUI effectText;
    public Image dividerMiddle;
    public Image outcomeImage;

    public GameObject modalOutcomeObject;
    public GameObject modalPanelObject;

    //private Image background;
    private static ModalOutcome modalOutcome;
    RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private float fadeInTime;

    //colour Palette
    /*private string colourGood;
    private string colourNeutral;
    private string colourBad;
    private string colourActive;*/
    private string colourDefault;
    private string colourEnd;

    /// <summary>
    /// initialisation
    /// </summary>
    private void Start()
    {
        canvasGroup = modalOutcomeObject.GetComponent<CanvasGroup>();
        rectTransform = modalOutcomeObject.GetComponent<RectTransform>();
        fadeInTime = GameManager.instance.tooltipScript.tooltipFade;
        //register a listener
        GameManager.instance.eventScript.AddListener(EventType.OpenOutcomeWindow, OnEvent);
    }

    /// <summary>
    /// provide a static reference to tooltipNode that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalOutcome Instance()
    {
        if (!modalOutcome)
        {
            modalOutcome = FindObjectOfType(typeof(ModalOutcome)) as ModalOutcome;
            if (!modalOutcome)
            { Debug.LogError("There needs to be one active modalOutcome script on a GameObject in your scene"); }
        }
        return modalOutcome;
    }



    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //detect event type
        switch(eventType)
        {
            case EventType.OpenOutcomeWindow:
                ModalOutcomeDetails details = Param as ModalOutcomeDetails;
                SetModalOutcome(details.textTop, details.textBottom);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /*public void DebugSet()
    {
        //GameManager.instance.actionMenuScript.CloseActionMenu();
        string textTop = "Node has been dealt with";
        string textBottom = "Node Security +1";
        SetModalOutcome(textTop, textBottom);
    }*/

    public void SetModalOutcome(string textTop, string textBottom, Sprite sprite = null)
    {
        //set modal true
        GameManager.instance.Blocked(true);
        //open panel at start
        modalOutcomeObject.SetActive(true);
        modalPanelObject.SetActive(true);
        //set opacity to zero (invisible)
        //SetOpacity(0f);

        //set up modalOutcome elements
        outcomeText.text = string.Format("{0}{1}{2}", colourDefault, textTop, colourEnd);
        effectText.text = string.Format("{0}{1}{2}", colourDefault, textBottom.ToUpper(), colourEnd);

        //get dimensions of dynamic tooltip
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        //Fixed position at screen centre
        Vector3 screenPos = new Vector3();
        screenPos.x = Screen.width / 2;
        screenPos.y = Screen.height / 2;
        //set position
        modalOutcomeObject.transform.position = screenPos;
        Debug.Log("UI: Open -> ModalOutcome window" + "\n");
    }


    /// <summary>
    /// fade in tooltip over time
    /// </summary>
    /// <returns></returns>
    IEnumerator FadeInTooltip()
    {
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime / fadeInTime;
            yield return null;
        }
    }


    /// <summary>
    /// returns true if tooltip is active and visible
    /// </summary>
    /// <returns></returns>
    public bool CheckModalOutcomeActive()
    { return modalOutcomeObject.activeSelf; }

    /// <summary>
    /// close tool tip
    /// </summary>
    public void CloseModalOutcome()
    {
        Debug.Log("UI: Close -> ModalOutcome window" + "\n");
        modalOutcomeObject.SetActive(false);
        //set modal false
        GameManager.instance.Blocked(false);
    }


    public void SetOpacity(float opacity)
    { canvasGroup.alpha = opacity; }

    public float GetOpacity()
    { return canvasGroup.alpha; }

}
