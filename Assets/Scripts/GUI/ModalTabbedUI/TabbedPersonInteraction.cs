using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles interaction for TabbedUI Personality matrix prefab
/// </summary>
public class TabbedPersonInteraction : MonoBehaviour
{
    [Header("Middle")]
    public Image middleCentre;
    public Image middleLeft;
    public Image middleFarLeft;
    public Image middleRight;
    public Image middleFarRight;
    public TextMeshProUGUI textMiddleCentre;
    public TextMeshProUGUI textMiddleLeft;
    public TextMeshProUGUI textMiddleFarLeft;
    public TextMeshProUGUI textMiddleRight;
    public TextMeshProUGUI textMiddleFarRight;

    [Header("Bottom")]
    public Image bottom;
    public TextMeshProUGUI textBottom;

    [Header("Top")]
    public Image topCentre;
    public Image topLeft;
    public Image topFarLeft;
    public Image topRight;
    public Image topFarRight;

    [Header("Help")]
    public GenericHelpTooltipUI help;


    public void Awake()
    {
        //middle
        Debug.Assert(middleCentre != null, "Invalid middleCentre (Null)");
        Debug.Assert(middleLeft != null, "Invalid middleLeft (Null)");
        Debug.Assert(middleFarLeft != null, "Invalid middleFarLeft (Null)");
        Debug.Assert(middleRight != null, "Invalid middleRight (Null)");
        Debug.Assert(middleFarRight != null, "Invalid middleFarRight (Null)");
        Debug.Assert(textMiddleCentre != null, "Invalid textMiddleCentre (Null)");
        Debug.Assert(textMiddleLeft != null, "Invalid textMiddleLeft (Null)");
        Debug.Assert(textMiddleFarLeft != null, "Invalid textMiddleFarLeft (Null)");
        Debug.Assert(textMiddleRight != null, "Invalid textMiddleRight (Null)");
        Debug.Assert(textMiddleFarRight != null, "Invalid textMiddleFarRight (Null)");
        //bottom
        Debug.Assert(bottom != null, "Invalid bottom (Null)");
        Debug.Assert(textBottom != null, "Invalid textBottom (Null)");
        //top
        Debug.Assert(topCentre != null, "Invalid topCentre (Null)");
        Debug.Assert(topLeft != null, "Invalid topLeft (Null)");
        Debug.Assert(topFarLeft != null, "Invalid topFarLeft (Null)");
        Debug.Assert(topRight != null, "Invalid topRight (Null)");
        Debug.Assert(topFarRight != null, "Invalid topFarRight (Null)");
        //help
        Debug.Assert(help != null, "Invalid help (Null)");
    }

}
