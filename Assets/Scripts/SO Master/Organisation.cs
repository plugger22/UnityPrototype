using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// SO for 3rd party Organisations. Name of SO is the name of the organisation, eg. "Mafia"
/// </summary>
[CreateAssetMenu(menuName = "Game / Organisation / Organisation")]
public class Organisation : ScriptableObject
{
    [Header("Texts")]
    [Tooltip("Short text description (3 words max)")]
    public string descriptor;
    [Tooltip("In game name")]
    public string tag;

    [Header("Vitals")]
    [Tooltip("Type of org")]
    public OrgType orgType;
    [Tooltip("Sprite")]
    public Sprite sprite;

    [Header("Demands")]
    [Tooltip("What the org wants when Rep 0 topic hits, in format '[wants you to]....' Keep short as possible")]
    [TextArea] public string textWant;
    [Tooltip("Secret that is given to Player if they do what the org wants at Rep 0")]
    public Secret secret;
   
    [Header("Archived")]
    [Tooltip("Preferred node type for this Organisation. The more of this type of node the greater the chance of an organisation being present in a city")]
    public NodeArc nodeArc;


    #region Save Data compatible
    [HideInInspector] public bool isContact;                //have you made contact with the org?
    [HideInInspector] public bool isSecretKnown;            //if true, player has secret, false otherwise
    [HideInInspector] public int maxStat;                   //max stat value (can't initialise in OnEnable, done in OrganisationManager.cs instead
    [HideInInspector] public int timer;                     //multipurpose timer depending on Org (optional)
    private int reputation;                                 //reputation with you (0 to 3)
    private int freedom;                                    //your freedom from obligation to the organisation (0 to 3)   
    #endregion



    public void OnEnable()
    {
        //field asserts
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(orgType != null, "Invalid orgType (Null)");
        Debug.AssertFormat(nodeArc != null, "Invalid nodeArc (Null) for {0}", name);
        Debug.AssertFormat(sprite != null, "Invalid sprite (Null) for {0}", name);
        Debug.AssertFormat(secret != null, "Invalid secret (Null) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(textWant) == false, "Invalid textWant (Null or Empty) for {0}", name);
    }


    public int GetReputation()
    { return reputation; }

    public int GetFreedom()
    {return freedom; }

    /// <summary>
    /// Set reputation to a specific value, range checks made and Log msg generated
    /// </summary>
    /// <param name="value"></param>
    public void SetReputation(int value)
    {
        int orginalValue = reputation;
        reputation = value;
        reputation = Mathf.Clamp(reputation, 0, GameManager.instance.actorScript.maxStatValue);
        Debug.LogFormat("[Org] Organisation.cs -> SetReputation: Reputation now {0}, was {1}{2}", reputation, orginalValue, "\n");
    }

    /// <summary>
    /// Set Freedom to a specific value, range checks made and Log msg generated
    /// </summary>
    /// <param name="value"></param>
    public void SetFreedom(int value)
    {
        int orginalValue = freedom;
        freedom = value;
        freedom = Mathf.Clamp(freedom, 0, GameManager.instance.actorScript.maxStatValue);
        Debug.LogFormat("[Org] Organisation.cs -> SetFreedom: Freedom now {0}, was {1}{2}", freedom, orginalValue, "\n");
    }

    /// <summary>
    /// Change Reputation value by an amount, eg. +2, -2, range checks and Log msg generated
    /// </summary>
    /// <param name="change"></param>
    public void ChangeReputation(int change)
    {
        int orginalValue = reputation;
        reputation += change;
        reputation = Mathf.Clamp(reputation, 0, GameManager.instance.actorScript.maxStatValue);
        Debug.LogFormat("[Org] Organisation.cs -> ChangeReputation: Reputation now {0}, was {1} (change {2}{3}){4}", reputation, orginalValue, change > 0 ? "+" : "", change, "\n");
    }

    /// <summary>
    /// Change Freedom value by an amount, eg. +2, -2, range checks made and Log msg generated
    /// </summary>
    /// <param name="value"></param>
    public void ChangeFreedom(int change)
    {
        int orginalValue = freedom;
        freedom += change;
        freedom = Mathf.Clamp(freedom, 0, GameManager.instance.actorScript.maxStatValue);
        Debug.LogFormat("[Org] Organisation.cs -> ChangeFreedom: Freedom now {0}, was {1} (change {2}{3}){4}", freedom, orginalValue, change > 0 ? "+" : "", change, "\n");
    }

    //new methods above here
}
