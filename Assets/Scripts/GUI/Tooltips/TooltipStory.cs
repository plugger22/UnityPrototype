using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles Story tooltips (explanatory background info)
/// </summary>
public class TooltipStory : MonoBehaviour
{
    public Canvas storyCanvas;

    public GameObject storyObject;
    public Image storyImage;

    public TextMeshProUGUI textHeader;
    public TextMeshProUGUI textTop;
    public TextMeshProUGUI textMiddle;
    public TextMeshProUGUI textBottom;

    private static TooltipStory tooltipStory;

    /// <summary>
    /// provide a static reference to the Story Tooltip that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TooltipStory Instance()
    {
        if (!tooltipStory)
        {
            tooltipStory = FindObjectOfType(typeof(TooltipStory)) as TooltipStory;
            if (!tooltipStory)
            { Debug.LogError("There needs to be one active TooltipStory script on a GameObject in your scene"); }
        }
        return tooltipStory;
    }

    /// <summary>
    /// Initialisation
    /// </summary>
    private void Start()
    {
        Debug.Assert(storyCanvas != null, "Invalid storyCanvas (Null)");
        Debug.Assert(storyObject != null, "Invalid storyObject (Null)");
        Debug.Assert(storyImage != null, "Invalid storyImage (Null)");
        Debug.Assert(textHeader != null, "Invalid textHeader (Null)");
        Debug.Assert(textTop != null, "Invalid textTop (Null)");
        Debug.Assert(textMiddle != null, "Invalid textMiddle (Null)");
        Debug.Assert(textBottom != null, "Invalid textBottom (Null)");
    }


    //new methods above here
}
