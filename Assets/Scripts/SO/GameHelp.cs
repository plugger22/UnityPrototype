using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main help SO data for the Game help system 
/// </summary>
[CreateAssetMenu(menuName = "Help / GameHelp")]
public class GameHelp : ScriptableObject
{
    [Tooltip("Used in master help as the list text")]
    public string descriptor;

    [Tooltip("Used in master help as the image header")]
    public string header;

    [Tooltip("Layout used by GameHelpUI.cs")]
    public Layout layout;

    [Header("Sprites")]
    [Tooltip("Info sprites. Add appropriately according to the layout, eg. if the layout requires two then use the first two sprite slots and they will be used in that order")]
    public Sprite sprite0;


    public void OnEnable()
    {
        Debug.Assert(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty)");
        Debug.Assert(string.IsNullOrEmpty(header) == false, "Invalid header (Null or Empty)");

    }

}
