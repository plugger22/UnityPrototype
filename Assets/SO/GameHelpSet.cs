using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sets of GameHelp.SO's to enable quick access in ModalHelpUI.cs (Master Help) from home page via buttons (each maps to a GameHelpSet)
/// </summary>
[CreateAssetMenu(menuName = "Help / GameHelpSet")]
public class GameHelpSet : ScriptableObject
{
    [Tooltip("Text for quick access button. Keep short")]
    public string descriptor;

    [Tooltip("All GameHelp.SO's that apply to this set. GameHelp.SO's can be in multiple sets if required. No max limit")]
    public List<GameHelp> listOfGameHelp;

    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name);
        Debug.AssertFormat(listOfGameHelp.Count > 0, "Invalid listOfGameHelp (Empty) for {0}", name);
    }
}
