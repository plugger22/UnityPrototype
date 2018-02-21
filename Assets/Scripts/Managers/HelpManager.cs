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
        builder.Append(string.Format(" Keyboard Commands{0},{1}", "\n", "\n"));
        builder.Append(string.Format(" End Turn -> Enter{0}", "\n"));
        builder.Append(string.Format(" Exit     -> X{0}{1}", "\n", "\n"));
        builder.Append(string.Format(" Targets -> T{0}", "\n"));
        builder.Append(string.Format(" Spiders -> S{0}", "\n"));
        builder.Append(string.Format(" Tracers -> C{0}{1}", "\n", "\n"));
        builder.Append(string.Format(" Teams -> M{0}{1}", "\n", "\n"));
        builder.Append(string.Format(" Actions -> Left Click{0}", "\n"));
        builder.Append(string.Format(" Move -> Right Click{0}{1}", "\n", "\n"));
        builder.Append(string.Format(" Corporate Nodes -> F1{0}", "\n"));
        builder.Append(string.Format(" Gated Nodes -> F2{0}", "\n"));
        builder.Append(string.Format(" Government Nodes -> F3{0}", "\n"));
        builder.Append(string.Format(" Industrial Nodes -> F4{0}", "\n"));
        builder.Append(string.Format(" Research Nodes -> F5{0}", "\n"));
        builder.Append(string.Format(" Sprawl Nodes -> F6{0}", "\n"));
        builder.Append(string.Format(" Utility Nodes -> F7{0}{1}", "\n", "\n"));
        builder.Append(string.Format(" Debug Show -> D{0}", "\n"));
        return builder.ToString();
    }

}
