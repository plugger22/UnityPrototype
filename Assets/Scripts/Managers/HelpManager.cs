using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using gameAPI;

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
        builder.AppendFormat(" Activity Known Time -> F9{0}", "\n");
        builder.AppendFormat(" Activity Possible Time -> F10{0}", "\n");
        builder.AppendFormat(" Activity Known Count -> F11{0}", "\n");
        builder.AppendFormat(" Activity Possible Count -> F12{0}", "\n");
        return builder.ToString();
    }

}
