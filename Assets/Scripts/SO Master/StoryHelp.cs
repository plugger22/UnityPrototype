using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Help data package for story topics via storyTooltips
/// </summary>
[CreateAssetMenu(menuName = "Story / Story Help")]
public class StoryHelp : ScriptableObject
{

    [Tooltip("In game descriptor -> used as Header for tooltip")]
    public string tag;
    [Tooltip("Sprite shown in tooltip, if none then default used")]
    public Sprite sprite;

    [Tooltip("Text at top of tooltip text block, has a limit of 200 chars")]
    [TextArea(3, 4)] public string textTop;
    [Tooltip("Text in middle of tooltip text block, has a limit of 200 chars")]
    [TextArea(3, 4)] public string textMiddle;
    [Tooltip("Text at bottom of tooltip text block, has a limit of 200 chars")]
    [TextArea(3, 4)] public string textBottom;

    [HideInInspector] public bool isKnown;                              //set true once help has been displayed (enables help to be shown for lookup reference -> not yet implemented)


    public void OnEnable()
    {
        Debug.AssertFormat(tag != null, "Invalid tag (Null) for {0}", name);
        /*Debug.AssertFormat(sprite != null, "Invalid sprite (Null) for {0}", name); -> NOTE: O.K to not have one as GUIManager.cs -> InfoSprite used as a default*/
        Debug.AssertFormat(textTop != null, "Invalid textTop (Null) for {0}", name);
        Debug.AssertFormat(textMiddle != null, "Invalid textMiddle (Null) for {0}", name);
        Debug.AssertFormat(textBottom != null, "Invalid textBottom (Null) for {0}", name);
    }
}
