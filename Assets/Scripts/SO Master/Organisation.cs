using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// SO for 3rd party Organisations. Name of SO is the name of the organisation, eg. "Mafia"
/// </summary>
[CreateAssetMenu(menuName = "Game / Organisation")]
public class Organisation : ScriptableObject
{
    [Tooltip("Short text description (3 words max)")]
    public string descriptor;
    [Tooltip("In game name")]
    public string tag;
    [Tooltip("Sprite")]
    public Sprite sprite;

    [Tooltip("Preferred node type for this Organisation. The more of this type of node the greater the chance of an organisation being present in a city")]
    public NodeArc nodeArc;


    #region Save Data compatible
    [HideInInspector] public bool isContact;                //have you made contact with the org?
    [HideInInspector] public int maxStat;                   //max stat value (can't initialise in OnEnable, done in OrganisationManager.cs instead
    private int relationship;                               //relationship with you (0 to 3)
    private int freedom;                                    //your freedom from obligation to the organisation (0 to 3)   
    #endregion



    public void OnEnable()
    {
        //field asserts
        Debug.Assert(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty)");
        Debug.Assert(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty)");
        Debug.Assert(nodeArc != null, "Invalid nodeArc (Null)");
        Debug.Assert(sprite != null, "Invalid sprite (Null)");
    }


    public int GetRelationship()
    { return relationship; }

    public int GetFreedom()
    {return freedom; }

    /// <summary>
    /// Set relationship to a specific value, range checks made and Log msg generated
    /// </summary>
    /// <param name="value"></param>
    public void SetRelationship(int value)
    {
        int orginalValue = relationship;
        relationship = value;
        relationship = Mathf.Clamp(relationship, 0, GameManager.instance.actorScript.maxStatValue);
        Debug.LogFormat("[Org] Organisation.cs -> SetRelationship: Relationship now {0}, was {1}{2}", relationship, orginalValue, "\n");
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
    /// Change Relationship value by an amount, eg. +2, -2, range checks and Log msg generated
    /// </summary>
    /// <param name="change"></param>
    public void ChangeRelationship(int change)
    {
        int orginalValue = relationship;
        relationship += change;
        relationship = Mathf.Clamp(relationship, 0, GameManager.instance.actorScript.maxStatValue);
        Debug.LogFormat("[Org] Organisation.cs -> ChangeRelationship: Relationship now {0}, was {1} (change {2}{3}){4}", relationship, orginalValue, change > 0 ? "+" : "", change, "\n");
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
