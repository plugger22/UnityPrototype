using gameAPI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// handles the city info display (static class)
/// </summary>
public class CityInfoUI : MonoBehaviour
{
    public GameObject cityInfoObject;
    public Image backgroundPanel;
    public Image cityImage;
    public Image subPanelLeft;
    public Image subPanelMiddle;
    public Image subPanelRight;
    public Image miniPanelTop;
    public Image miniPanelMiddle;
    public Image miniPanelBottom;
    public TextMeshProUGUI cityName;
    public TextMeshProUGUI cityArc;
    public TextMeshProUGUI cityDescription;
    public TextMeshProUGUI mayorName;
    public TextMeshProUGUI mayorTrait;
    public TextMeshProUGUI factionName;
    public TextMeshProUGUI factionTrait;
    public TextMeshProUGUI organisations;

    //left panel -> Districts
    public TextMeshProUGUI districtNames;
    public TextMeshProUGUI districtTotals;

    public Button buttonClose;


    private ButtonInteraction buttonInteraction;
    private GenericTooltipUI districtTooltip;
    private GenericTooltipUI organisationTooltip;
    private GenericTooltipUI factionTooltip;
    private GenericTooltipUI mayorTooltip;


    //cached data
    private List<string> listOfDistrictNames;
    private List<int> listOfDistrictTotals;

    private static CityInfoUI cityInfoUI;


    /// <summary>
    /// provide a static reference to cityInfoUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static CityInfoUI Instance()
    {
        if (!cityInfoUI)
        {
            cityInfoUI = FindObjectOfType(typeof(CityInfoUI)) as CityInfoUI;
            if (!cityInfoUI)
            { Debug.LogError("There needs to be one active CityInfoUI script on a GameObject in your scene"); }
        }
        return cityInfoUI;
    }


    private void Awake()
    {
        //close button event
        buttonInteraction = buttonClose.GetComponent<ButtonInteraction>();
        if (buttonInteraction != null)
        { buttonInteraction.SetEvent(EventType.CityInfoClose); }
        else { Debug.LogError("Invalid buttonInteraction Cancel (Null)"); }
        //tooltips
        districtTooltip = subPanelLeft.GetComponent<GenericTooltipUI>();
        organisationTooltip = miniPanelBottom.GetComponent<GenericTooltipUI>();
        factionTooltip = miniPanelMiddle.GetComponent<GenericTooltipUI>();
        mayorTooltip = miniPanelTop.GetComponent<GenericTooltipUI>();

    }

    public void Initialise()
    {
        int counter = 0;
        //cache list of District names
        listOfDistrictNames = new List<string>();
        Dictionary<string, int> dictOfNodeArcs = GameManager.instance.dataScript.GetDictOfLookUpNodeArcs();
        if (dictOfNodeArcs != null)
        {
            foreach (var record in dictOfNodeArcs)
            { listOfDistrictNames.Add(record.Key); counter++; }
        }
        else { Debug.LogError("Invalid dictOfNodeArcs (Null)"); }
        Debug.LogFormat("CityInfoUI -> Initialise: {0} records addd to listOfDistrictNames", counter);
        Debug.Assert(counter > 0, "Invalid number of records added to listOfDistrictNames");
        SetAllToActive();
        SetOpacities();
        SetFixedTooltips();
    }


    public void Start()
    {
        //event listener
        EventManager.instance.AddListener(EventType.ChangeSide, OnEvent, "CityInfoUI");
        EventManager.instance.AddListener(EventType.CityInfoOpen, OnEvent, "CityInfoUI");
        EventManager.instance.AddListener(EventType.CityInfoClose, OnEvent, "CityInfoUI");
    }

    /// <summary>
    /// Event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.ChangeSide:
                ChangeSides((GlobalSide)Param);
                break;
            case EventType.CityInfoOpen:
                SetCityInfo((City)Param);
                break;
            case EventType.CityInfoClose:
                CloseCityInfo();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }




    /// <summary>
    /// Set background image and cancel button for the appropriate side
    /// </summary>
    /// <param name="side"></param>
    private void ChangeSides(GlobalSide side)
    {
        Debug.Assert(side != null, "Invalid side (Null)");
        switch (side.level)
        {
            case 1:
                backgroundPanel.sprite = GameManager.instance.sideScript.info_background_Authority;
                buttonClose.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Authority;
                //set sprite transitions
                SpriteState spriteStateAuthority = new SpriteState();
                spriteStateAuthority.highlightedSprite = GameManager.instance.sideScript.button_highlight_Authority;
                spriteStateAuthority.pressedSprite = GameManager.instance.sideScript.button_Click;
                buttonClose.spriteState = spriteStateAuthority;
                break;
            case 2:
                backgroundPanel.sprite = GameManager.instance.sideScript.info_background_Resistance;
                buttonClose.GetComponent<Image>().sprite = GameManager.instance.sideScript.button_Resistance;
                //set sprite transitions
                SpriteState spriteStateRebel = new SpriteState();
                spriteStateRebel.highlightedSprite = GameManager.instance.sideScript.button_highlight_Resistance;
                spriteStateRebel.pressedSprite = GameManager.instance.sideScript.button_Click;
                buttonClose.spriteState = spriteStateRebel;
                break;
        }
    }

    /// <summary>
    /// set all UI components (apart from main) to active. Run at level start to ensure no problems (something hasn't been switched off in the editor)
    /// </summary>
    private void SetAllToActive()
    {
        cityInfoObject.gameObject.SetActive(true);
        backgroundPanel.gameObject.SetActive(true);
        cityImage.gameObject.SetActive(true);
        cityName.gameObject.SetActive(true);
        cityArc.gameObject.SetActive(true);
        cityDescription.gameObject.SetActive(true);
        districtNames.gameObject.SetActive(true);
        districtTotals.gameObject.SetActive(true);
        subPanelLeft.gameObject.SetActive(true);
        subPanelMiddle.gameObject.SetActive(true);
        subPanelRight.gameObject.SetActive(true);
    }

    /// <summary>
    /// sets all panel opacities to values specified in CityManager.cs
    /// </summary>
    private void SetOpacities()
    {
        //subPanel opacities (same for all three)
        float opacitySub = GameManager.instance.cityScript.subPanelOpacity;
        float opacityMini = GameManager.instance.cityScript.miniPanelOpacity;
        Debug.Assert(opacitySub >= 0 && opacitySub <= 100.0f, string.Format("Invalid subPanel opacity \"{0}\"", opacitySub));
        //left and right panels faintly visible
        Color panelColor = subPanelLeft.color;
        panelColor.a = opacitySub;
        subPanelLeft.color = panelColor;
        subPanelRight.color = panelColor;
        //middle panel invisibile
        panelColor = subPanelMiddle.color;
        panelColor.a = 0.0f;
        subPanelMiddle.color = panelColor;
        //mini panels more visible
        panelColor = miniPanelTop.color;
        panelColor.a = opacityMini;
        miniPanelTop.color = panelColor;
        miniPanelMiddle.color = panelColor;
        miniPanelBottom.color = panelColor;
    }

    /// <summary>
    /// sets all generic tooltip data for tooltips which are constant
    /// </summary>
    private void SetFixedTooltips()
    {
        districtTooltip.tooltipHeader =  string.Format("<b>City Districts</b>");
        districtTooltip.tooltipMain = "Only districts that are accessible are shown. Districts in permanent Lockdown are not visible";
        organisationTooltip.tooltipMain = "Organisations that are active in the city";

    }

    /// <summary>
    /// Open City Info display
    /// </summary>
    private void SetCityInfo(City city)
    {
        if (city != null)
        {
            //exit any generic or node tooltips
            /*StopCoroutine("ShowTooltip");*/
            GameManager.instance.tooltipGenericScript.CloseTooltip();
            GameManager.instance.tooltipNodeScript.CloseTooltip();
            //populate data
            cityName.text = city.name;
            cityArc.text = city.Arc.name;
            cityDescription.text = city.descriptor;
            //district details -> keep in this order (GetDistrictNames initialises data for GetDistrictTotals)
            districtNames.text = GetDistrictNames(city);
            districtTotals.text = GetDistrictTotals();
            //centre panel -> mayor / faction / organisation
            if (city.mayor != null)
            {
                mayorName.text = city.mayor.name;
                if (city.mayor.GetTrait() != null)
                { mayorTrait.text = city.mayor.GetTrait().tagFormatted; }
                else { mayorTrait.text = "Unknown"; }
            }
            else { mayorName.text = "Unknown"; }
            if (city.faction != null)
            {
                factionName.text = city.faction.name;
                if (city.faction.GetTrait() != null)
                { factionTrait.text = city.faction.GetTrait().tagFormatted; }
                else { factionTrait.text = "Unknown"; }
            }
            else { factionName.text = "Unknown"; }
            organisations.text = GameManager.instance.cityScript.GetNumOfOrganisations();
            //city image (uses arc sprite, GUIManager.cs default sprite if arc sprite is null)
            if (city.Arc.sprite != null)
            { cityImage.sprite = city.Arc.sprite; }
            else { cityImage.sprite = GameManager.instance.guiScript.cityArcDefaultSprite; }
            Debug.Assert(cityImage.sprite != null, "Invalid city Arc default sprite");
            //Organisation tooltip
            mayorTooltip.tooltipHeader = GameManager.instance.cityScript.GetMayorName();
            mayorTooltip.tooltipMain = GameManager.instance.cityScript.GetMayorTrait();
            mayorTooltip.tooltipDetails = GameManager.instance.cityScript.GetMayorFaction();
            mayorTooltip.x_offset = 25;
            factionTooltip.tooltipHeader = GameManager.instance.cityScript.GetFactionName();
            factionTooltip.tooltipMain = GameManager.instance.cityScript.GetFactionTrait();
            factionTooltip.tooltipDetails = GameManager.instance.cityScript.GetFactionDetails();
            factionTooltip.x_offset = 25;
            organisationTooltip.tooltipHeader = GameManager.instance.cityScript.GetCityName();
            organisationTooltip.tooltipDetails = GameManager.instance.cityScript.GetOrganisationsTooltip();
            organisationTooltip.x_offset = 25;
            //activate main panel
            cityInfoObject.SetActive(true);
            //set modal status
            GameManager.instance.guiScript.SetIsBlocked(true);
            //set game state
            GameManager.instance.inputScript.SetModalState(ModalState.InfoDisplay, ModalInfo.CityInfo);
            Debug.Log("UI: Open -> Open City Info Display" + "\n");
        }
        else { Debug.LogWarning("Invalid city (Null) -> tooltip cancelled"); }
    }

    /// <summary>
    /// close city Info display
    /// </summary>
    private void CloseCityInfo()
    {
        GameManager.instance.tooltipGenericScript.CloseTooltip();
        cityInfoObject.SetActive(false);
        GameManager.instance.guiScript.SetIsBlocked(false);
        //set game state
        GameManager.instance.inputScript.ResetStates();
        Debug.Log("UI: Close -> CityInfoDisplay" + "\n");
    }

    /// <summary>
    /// sub method to get formatted string for district names (cached)
    /// </summary>
    /// <returns></returns>
    private string GetDistrictNames(City city)
    {
        StringBuilder builder = new StringBuilder();
        //import list of totals (needed for formatting options and error checking)
        listOfDistrictTotals = city.GetListOfDistrictTotals();
        if (listOfDistrictTotals != null)
        {
            Debug.Assert(listOfDistrictTotals.Count == listOfDistrictNames.Count, "Counts don't match for listOfDistrict Names and Totals");
            for (int i = 0; i < listOfDistrictNames.Count; i++)
            {
                if (builder.Length > 0) { builder.AppendLine(); }
                if (listOfDistrictTotals[i] > 0)
                { builder.AppendFormat("<b>{0}</b>", listOfDistrictNames[i]); }
                else { builder.AppendFormat("{0}", listOfDistrictNames[i]); }
            }
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendFormat("<b>TOTAL</b>");
        }
        else { Debug.LogError("Invalid listOfDistrictTotals (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// sub method to get formatted string for district totals (direct from city SO). Null if none
    /// NOTE: listOfDistrictTotals is initialised and error checked by GetDistrictNames which runs immediately before this metho
    /// </summary>
    /// <returns></returns>
    private string GetDistrictTotals()
    {
        StringBuilder builder = new StringBuilder();
        int total = 0;
        for (int i = 0; i < listOfDistrictTotals.Count; i++)
        {
            if (builder.Length > 0) { builder.AppendLine(); }
            if (listOfDistrictTotals[i] > 0)
            { builder.AppendFormat("<b>{0}</b>", listOfDistrictTotals[i]); }
            else { builder.AppendFormat("{0}", listOfDistrictTotals[i]); }
            total += listOfDistrictTotals[i];
        }
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendFormat("<b>{0}</b>", total);
        return builder.ToString();
    }

    //new methods above here
}
