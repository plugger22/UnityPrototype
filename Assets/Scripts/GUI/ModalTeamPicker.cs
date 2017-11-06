using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using modalAPI;
using gameAPI;

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
    
    public Image[] arrayOfTeamImages;                //place team image UI elements here (should be seven)
    public TextMeshProUGUI[] arrayOfTeamTexts;       //place team texts UI elements here (should be seven)


    private CanvasGroup canvasGroup;
    private static ModalTeamPicker modalTeamPicker;

    private void Start()
    {
        canvasGroup = modalTeamObject.GetComponent<CanvasGroup>();
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
    /// Initialise and Activate Team Picker (insert ANY TEAM)
    /// </summary>
    public void SetTeamPicker()
    {

    }
}
