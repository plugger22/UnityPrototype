﻿using gameAPI;
using packageAPI;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Secrets. Name of SO is the name of the secret, eg. "Paid to Murder"
/// Secrets are generated by the Player and learned by other Actors (Actors don't have secrets themselves)
/// </summary>
[CreateAssetMenu(menuName = "Actor / Secret / Secret")]
public class Secret : ScriptableObject
{
    [Tooltip("General purpose descriptor. Used for 'You Gain Secret' message RHS header")]
    public string descriptor;
    [Tooltip("Used in tooltips, etc. Keep short. Three words max")]
    public string tag;
    [Tooltip("Which side does the secret apply to")]
    public GlobalSide side;
    [Tooltip("What category does the secret belong to")]
    public SecretType type;
    [Tooltip("Effects that happen if secret is revealed")]
    public List<Effect> listOfEffects;

    [Header("Investigation")]
    [Tooltip("Tag to be passed to Investigation.cs in event of a revealed secret initiating an investigation. Format '[Investigating your/Players] ...', e.g 'Personal hygiene'")]
    public string investigationTag;
    [Tooltip("The starting value of the evidence for the investigation. '1' indicates incriminating evidence, '2' indicates only ambivalent evidence")]
    [Range(1, 2)] public int investigationEvidence = 2;

    [Header("Organisation")]
    [Tooltip("If an organisational secret then, once revealed, org will break off all contact with Player. Ignore if not an org secret")]
    public Organisation org;


    #region Save Data Compatible
    [HideInInspector] public SecretStatus status;           //enum as dynamic data 
    [HideInInspector] public string revealedWho;            //actor/org who revealed the secret (actor name + arc name / org name)
    [HideInInspector] public int revealedID;                //actorID who revealed (optional, ignore if org (-1))
    [HideInInspector] public TimeStamp gainedWhen;                //turn player gains secret
    [HideInInspector] public TimeStamp revealedWhen;              //turn revealed
    [HideInInspector] public TimeStamp deletedWhen;               //turn deleted (removed from game without being revealed)
    private List<int> listOfActors = new List<int>();       //list of actorID's of actors who know the secret
    #endregion

    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(investigationTag) == false, "Invalid investigationTag (Null or Empty) for {0}", name);
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
        Debug.AssertFormat(listOfEffects != null, "Invalid listOfEffects (Null) for {0}", name);
        Debug.AssertFormat(type != null, "Invalid type (Null) for {0}", name);
    }

    /// <summary>
    /// called by SecretManager.cs -> need to reset data back to default settings otherwise will carry over between sessions
    /// </summary>
    public void Initialise()
    {
        status = SecretStatus.Inactive;
        revealedWho = "";
        revealedID = -1;
        listOfActors.Clear();
        gainedWhen = new TimeStamp();
        revealedWhen = new TimeStamp();
        deletedWhen = new TimeStamp();
    }

    /// <summary>
    /// called when secret removed from Player and removed from game (still in dictionary)
    /// </summary>
    public void ResetSecret()
    {
        listOfActors.Clear();
    }

    /// <summary>
    /// Reset secret data, when required, prior to the start of a FollowOn level
    /// </summary>
    public void ResetFollowOnLevel()
    {
        revealedWho = "";
        revealedID = -1;
        listOfActors.Clear();
    }

    /// <summary>
    /// Add the actorID of an actor who has learned of the secret. Checks if already present (doesn't add, warning)
    /// </summary>
    /// <param name="actorID"></param>
    public void AddActor(int actorID)
    {
        if (listOfActors.Exists(x => x == actorID) == false)
        { listOfActors.Add(actorID); }
        else
        { Debug.LogWarningFormat("ActorID {0} already exists in listOfActors", actorID); }
    }

    /// <summary>
    /// remove an actor from list of actors who know the secret
    /// </summary>
    /// <param name="actorID"></param>
    public void RemoveActor(int actorID)
    {
        //reverse loop through and remove secret
        for (int i = listOfActors.Count - 1; i >= 0; i--)
        {
            if (listOfActors[i] == actorID)
            {
                listOfActors.RemoveAt(i);
                break;
            }
        }
    }

    /// <summary>
    /// clear out all actors, nobody knows secret
    /// </summary>
    public void RemoveAllActors()
    { listOfActors.Clear(); }

    /// <summary>
    /// returns true if actorID is on list Of actors who already know of the secret, false otherwise
    /// </summary>
    /// <param name="actorID"></param>
    /// <returns></returns>
    public bool CheckActorPresent(int actorID)
    { return listOfActors.Exists(x => x == actorID); }

    public List<int> GetListOfActors()
    { return listOfActors; }


    /// <summary>
    /// Copies a full list of Actors across. Used for loading save games.
    /// </summary>
    /// <param name="listOfNewActors"></param>
    public void SetListOfActors(List<int> listOfNewActors)
    {
        listOfActors.Clear();
        listOfActors.AddRange(listOfNewActors);
    }




    /// <summary>
    /// returns number of actors who know of the secret
    /// </summary>
    /// <returns></returns>
    public int CheckNumOfActorsWhoKnow()
    { return listOfActors.Count; }

    /// <summary>
    /// returns list of effects if secret revealed
    /// </summary>
    /// <returns></returns>
    public List<Effect> GetListOfEffects()
    { return listOfEffects; }

}
