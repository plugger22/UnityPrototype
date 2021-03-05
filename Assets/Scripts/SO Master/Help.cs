using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main help SO data for the help system (this is independent of the tutorial help system)
/// </summary>
[CreateAssetMenu(menuName = "Help / Help")]
public class Help : ScriptableObject
{

    public string header;

    [Tooltip("Layout used by helpUI.cs")]
    public Layout layout;

    [Tooltip("Info sprites. Add appropriately according to the layout, eg. if the layout requires two then use the first two sprite slots and they will be used in that order")]
    public Sprite sprite0;

}
