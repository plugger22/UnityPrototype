using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Handles change of Sides, eg. Rebel / Authority, admin
/// </summary>
public class SideManager : MonoBehaviour
{
    //node tooltip
    public Sprite toolTip_backgroundAuthority;
    public Sprite toolTip_backgroundRebel;
    public Sprite toolTip_dividerAuthority;
    public Sprite toolTip_dividerRebel;


    /// <summary>
    /// Swaps gfx around for UI elements
    /// </summary>
    /// <param name="side"></param>
    public void SwapSides(Side side)
    {
        GameManager.instance.tooltipNodeScript.InitialiseTooltip(side);
        GameManager.instance.tooltipActorScript.InitialiseTooltip(side);
    }
}
