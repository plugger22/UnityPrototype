using packageAPI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// handles all Help related matters
/// </summary>
public class HelpManager : MonoBehaviour
{

    //colours
    /*private string colourRebel;*/
    private string colourNeutral;
    private string colourNormal;
    /*private string colourGood;
    private string colourBad;*/
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
        /*colourGood = GameManager.instance.colourScript.GetColour(ColourType.dataGood);
        colourBad = GameManager.instance.colourScript.GetColour(ColourType.dataBad);*/
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

        //test0
        HelpData data0 = new HelpData();
        data0.tag = "test0";
        data0.header = "Test help";
        data0.text = string.Format("{0}Text Text. My name is Cameron and I'm not sure who I am but I'm pretty confident I am a knock them down, blow them up, gun, game designere.{1}", colourNormal, colourEnd);
        listOfHelp.Add(data0);
        //test1
        HelpData data1 = new HelpData();
        data1.tag = "test1";
        data1.header = "Test help More";
        data1.text = string.Format("Text Text. My name is Cameron and I'm not sure who I am but I'm pretty confident I am a knock them down, blow them up, gun, game designere");
        listOfHelp.Add(data1);
        //test2
        HelpData data2 = new HelpData();
        data2.tag = "test2";
        data2.header = "Test help Maybe";
        data2.text = string.Format("{0}Text Text. My name is Cameron and I'm not sure who I am but I'm pretty confident I am a knock them down, blow them up, gun, game designere.{1}", colourNormal, colourEnd);
        listOfHelp.Add(data2);
        //test3
        HelpData data3 = new HelpData();
        data3.tag = "test3";
        data3.header = "Test help Wednesday";
        data3.text = string.Format("{0}Text Text. My name is Cameron and I'm not sure who I am but I'm pretty confident I am a knock them down, blow them up, gun, game designere.{1}", colourNormal, colourEnd);
        listOfHelp.Add(data3);

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
