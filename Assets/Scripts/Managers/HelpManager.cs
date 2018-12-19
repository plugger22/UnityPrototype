﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using packageAPI;

/// <summary>
/// handles all Help related matters
/// </summary>
public class HelpManager : MonoBehaviour
{

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
        data.text = "Text Text";
        listOfHelp.Add(data);

        return listOfHelp;
    }

}
