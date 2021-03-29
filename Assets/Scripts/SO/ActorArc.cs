using UnityEngine;

/// <summary>
/// Actor archetype, eg. 'FIXER', always UPPERCASE
/// </summary>
[CreateAssetMenu(menuName = "Actor / ActorArc")]
public class ActorArc : ScriptableObject
{
    [Header("Main")]
    /*public int ActorArcID { get; set; }               //unique #, zero based -> assigned automatically by DataManager.Initialise*/
    public GlobalSide side;
    public string actorName;

    [Header("Actor Tooltip")]
    [Tooltip("Face of actor -> NOTE: should be a list with a few variations, perhaps?")]
    public Sprite sprite;
    [Tooltip("One action for interacting with nodes")]
    public Action nodeAction;
    [Tooltip("Preferred team (applies to authority actors only")]
    public TeamArc preferredTeam;

    [Header("Preferences")]
    [Tooltip("The type of Gear the actor prefers (you gain a Power transfer from them for giving them this type of gear)")]
    public GearType preferredGear;

    [Header("Arc name Tooltip")]
    [Tooltip("A short summary")]
    [TextArea] public string summary;
    [Tooltip("A detailed explanation of the Arc abilities")]
    [TextArea] public string details;


    /// <summary>
    /// initialisation
    /// </summary>
    private void OnEnable()
    {
        Debug.AssertFormat(sprite != null, "Invalid sprite (Null) for {0}", name);
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(actorName) == false, "Invalid actorName (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(summary) == false, "Invalid summary (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(details) == false, "Invalid explanation (Null or Empty) for {0}", name);
        //conditional
        switch (side.name)
        {
            case "Authority":
                Debug.AssertFormat(preferredTeam != null, "Invalid preferred Team (Null) for {0}", name);
                break;
            case "Resistance":
                Debug.AssertFormat(preferredGear != null, "Invalid preferred gear (Null) for {0}", name);
                Debug.AssertFormat(nodeAction != null, "Invalid nodeAction (Null) for {0}", name);
                break;
            default: Debug.LogWarningFormat("Unrecognised side \"{0}\"", side); break;
        }
    }

}
