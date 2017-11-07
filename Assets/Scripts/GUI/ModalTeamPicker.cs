using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using modalAPI;
using gameAPI;
using System.Text;

/// <summary>
/// Handles Modal Team Picker UI
/// </summary>
public class ModalTeamPicker : MonoBehaviour
{
    public GameObject modalTeamObject;
    public Image modalPanel;

    public TextMeshProUGUI topText;
    public TextMeshProUGUI middleText;
    public TextMeshProUGUI bottomText;

    public Button buttonCancel;
    public Button buttonConfirm;
    
    public Image[] arrayOfTeamImages;                //place team image UI elements here (should be seven)
    public TextMeshProUGUI[] arrayOfTeamTexts;       //place team texts UI elements here (should be seven)


    private CanvasGroup canvasGroup;
    private static ModalTeamPicker modalTeamPicker;

    private string colourEffect;
    private string colourDefault;
    private string colourNormal;
    private string colourEnd;

    private void Start()
    {
        canvasGroup = modalPanel.GetComponent<CanvasGroup>();
    }

    public void Initialise()
    {
        //assign sprites to team Images
        Dictionary<int, TeamArc> dictOfTeamArcs = GameManager.instance.dataScript.GetTeamArcs();
        if (dictOfTeamArcs != null)
        {
            if (dictOfTeamArcs.Count != arrayOfTeamImages.Length)
            { Debug.LogWarning(string.Format("dictOfTeamArcs.Count {0} != arrayOfTeamImages.Length {1}", dictOfTeamArcs.Count, arrayOfTeamImages.Length)); }
            else
            {
                if (dictOfTeamArcs.Count != arrayOfTeamTexts.Length)
                { Debug.LogWarning(string.Format("dictOfTeamArcs.Count {0} != arrayOfTeamTexts.Length {1}", dictOfTeamArcs.Count, arrayOfTeamTexts.Length)); }
                else
                {
                    int limit = Mathf.Min(dictOfTeamArcs.Count, arrayOfTeamImages.Length);
                    limit = Mathf.Min(dictOfTeamArcs.Count, arrayOfTeamTexts.Length);
                    for (int index = 0; index < limit; index++)
                    {
                        //get TeamArc from dict based on index
                        TeamArc arc = null;
                        if (dictOfTeamArcs.ContainsKey(index) == true)
                        {
                            arc = dictOfTeamArcs[index];
                            //assign to sprite
                            arrayOfTeamImages[index].sprite = arc.sprite;
                            //assign to text (name of teamArc)
                            arrayOfTeamTexts[index].text = arc.name;
                        }
                        else { Debug.LogWarning(string.Format("Invalid arc index \"{0}\" for \"{1}\" -> No Sprite assigned", index, arc.name)); }
                    }
                }
            }
        }
        else { Debug.LogError("Invalid dictOfTeamArcs (null) -> Sprites not assigned to ModalTeamPicker"); }
        //register listener
        EventManager.instance.AddListener(EventType.OpenTeamPicker, OnEvent);
        EventManager.instance.AddListener(EventType.CloseTeamPicker, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
    }



    /// <summary>
    /// Static instance so the Modal Team Picker can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalTeamPicker Instance()
    {
        if (!modalTeamPicker)
        {
            modalTeamPicker = FindObjectOfType(typeof(ModalTeamPicker)) as ModalTeamPicker;
            if (!modalTeamPicker)
            { Debug.LogError("There needs to be one active ModalTeamPicker script on a GameObject in your scene"); }
        }
        return modalTeamPicker;
    }

    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch (eventType)
        {
            case EventType.OpenTeamPicker:
                ModalActionDetails details = Param as ModalActionDetails;
                SetTeamPicker(details);
                break;
            case EventType.CloseTeamPicker:
                ModalActionDetails detailsTeam = Param as ModalActionDetails;
                CloseTeamPicker();
                break;
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Initialise and Activate Team Picker (insert ANY TEAM)
    /// </summary>
    public void SetTeamPicker(ModalActionDetails details)
    {
        GameManager.instance.Blocked(true);
        modalTeamObject.SetActive(true);
        canvasGroup.alpha = 100;
        //Set up texts
        topText.text = string.Format("{0}Select {1}{2}ANY{3}{4} Team{5}", colourDefault, colourEnd, colourEffect, colourEnd, colourDefault, colourEnd);
        //node details
        StringBuilder builder = new StringBuilder();
        Node node = GameManager.instance.dataScript.GetNode(details.NodeID);
        if (node != null)
        {
            int numTeams = node.CheckNumOfTeams();
            builder.Append(string.Format("{0}{1} \"{2}\"{3}", colourNormal, node.arc.name.ToUpper(), node.NodeName, colourEnd));
            builder.AppendLine();
            builder.Append(string.Format("{0}Currently {1} Team{2} present{3}", colourNormal, numTeams, numTeams != 1 ? "s" : "", colourEnd));
        }
        else { Debug.LogError(string.Format("Invalid node (Null) for details.NodeID {0}", details.NodeID)); }
        middleText.text = builder.ToString();
        //set Cancel Button
        buttonCancel.onClick.RemoveAllListeners();
        buttonCancel.onClick.AddListener(CloseTeamPicker);
        //set Confirm Button
        buttonConfirm.onClick.RemoveAllListeners();
        buttonConfirm.onClick.AddListener(CloseTeamPicker);
        //buttonConfirm.onClick.AddListener(buttonDetails.action);

        //set game state
        GameManager.instance.inputScript.GameState = GameState.ModalTeamPicker;
        Debug.Log("UI: Open -> ModalTeamPicker" + "\n");
    }

    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        colourEffect = GameManager.instance.colourScript.GetColour(ColourType.actionEffect);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// close Action Menu
    /// </summary>
    public void CloseTeamPicker()
    {
        modalTeamObject.SetActive(false);
        GameManager.instance.Blocked(false);

        //set game state
        GameManager.instance.inputScript.GameState = GameState.Normal;
        Debug.Log("UI: Close -> ModalTeamPicker" + "\n");
    }
}
