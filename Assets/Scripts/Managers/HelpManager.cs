using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using packageAPI;

/// <summary>
/// handles all Help related matters
/// </summary>
public class HelpManager : MonoBehaviour
{

    //colours
    /*private string colourRebel;*/
    private string colourNeutral;
    private string colourNormal;
    private string colourGood;
    private string colourBad;
    private string colourAlert;
    private string colourEnd;

    public void Initialise()
    {
        SetColours();
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent, "ItemDataManager");
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
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        /*colourRebel = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);*/
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }



    /// <summary>
    /// creates item Help data. Returns an empty list if none
    /// </summary>
    /// <returns></returns>
    public List<HelpData> GetItemDataHelp()
    {
        List<HelpData> listOfHelp = new List<HelpData>();

        //test
        HelpData data = new HelpData();
        data.tag = "Test";
        data.header = "Test help";
        data.text = string.Format("{0}Text Text{1}", colourNormal, colourEnd);
        listOfHelp.Add(data);

        return listOfHelp;
    }


    /// <summary>
    /// debug method to display all keyboard commands
    /// </summary>
    public string DisplayHelp()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat(" Keyboard Commands{0},{1}", "\n", "\n");
        builder.AppendFormat(" End Turn -> Enter{0}", "\n");
        builder.AppendFormat(" Exit     -> X{0}{1}", "\n", "\n");
        builder.AppendFormat(" Reserves -> R{0}", "\n");
        builder.AppendFormat(" Gear -> G{0}{1}", "\n", "\n");
        builder.AppendFormat(" Targets -> T{0}", "\n");
        builder.AppendFormat(" Spiders -> S{0}", "\n");
        builder.AppendFormat(" Tracers -> C{0}", "\n");
        builder.AppendFormat(" Teams -> M{0}{1}", "\n", "\n");
        builder.AppendFormat(" Actions -> Left Click{0}", "\n");
        builder.AppendFormat(" Move -> Right Click{0}{1}", "\n", "\n");
        builder.AppendFormat(" Corporate Nodes -> F1{0}", "\n");
        builder.AppendFormat(" Gated Nodes -> F2{0}", "\n");
        builder.AppendFormat(" Government Nodes -> F3{0}", "\n");
        builder.AppendFormat(" Industrial Nodes -> F4{0}", "\n");
        builder.AppendFormat(" Research Nodes -> F5{0}", "\n");
        builder.AppendFormat(" Sprawl Nodes -> F6{0}", "\n");
        builder.AppendFormat(" Utility Nodes -> F7{0}{1}", "\n", "\n");
        builder.AppendFormat(" Debug Show -> D{0}{1}", "\n", "\n");
        builder.AppendFormat(" Activity Time -> F9{0}", "\n");
        builder.AppendFormat(" Activity Count -> F10{0}{1}", "\n", "\n");
        builder.AppendFormat(" MainInfoApp display -> I{0}", "\n");
        builder.AppendFormat(" MainInfoApp ShowMe -> Space{0}", "\n");
        builder.AppendFormat(" MainInfoApp Home -> Home{0}", "\n");
        builder.AppendFormat(" MainInfoApp End -> End{0}", "\n");
        builder.AppendFormat(" MainInfoApp Back -> PageDn{0}", "\n");
        builder.AppendFormat(" MainInfoApp Forward -> PageUp{0}", "\n");
        return builder.ToString();
    }

}
