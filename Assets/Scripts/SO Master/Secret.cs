﻿using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SO for Secrets. Name of SO is the name of the secret, eg. "Paid to Murder"
/// Secrets are generated by the Player and learned by other Actors (Actors don't have secrets themselves)
/// </summary>
[CreateAssetMenu(menuName = "Actor / Secret / Secret")]
public class Secret : ScriptableObject
{
    [Tooltip("General purpose descriptor with no ingame use")]
    public string descriptor;
    [Tooltip("Used in tooltips, etc. Keep short. Three words max")]
    public string tag;
    [Tooltip("Which side does the secret apply to")]
    public GlobalSide side;

    [Tooltip("Effects that happen if secret is revealed")]
    public List<Effect> listOfEffects;
    [Tooltip("What category does the secret belong to")]
    public SecretType type;

    #region Save Data Compatible
    [HideInInspector] public gameAPI.SecretStatus status;           //enum as dynamic data 
    [HideInInspector] public int gainedWhen;                //turn player gains secret
    [HideInInspector] public int revealedWho;               //actorID of person who revealed the secret
    [HideInInspector] public int revealedWhen;              //turn revealed
    [HideInInspector] public int deletedWhen;               //turn deleted (removed from game without being revealed)
    private List<int> listOfActors = new List<int>();       //list of actorID's of actors who know the secret
    #endregion

    public void OnEnable()
    {
        Debug.AssertFormat(string.IsNullOrEmpty(descriptor) == false, "Invalid descriptor (Null or Empty) for {0}", name);
        Debug.AssertFormat(string.IsNullOrEmpty(tag) == false, "Invalid tag (Null or Empty) for {0}", name);
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
        Debug.AssertFormat(listOfEffects != null, "Invalid listOfEffects (Null) for {0}", name);
        Debug.AssertFormat(type != null, "Invalid type (Null) for {0}", name);
    }

    /// <summary>
    /// called by SecretManager.cs -> need to reset data back to default settings otherwise will carry over between sessions
    /// </summary>
    public void Initialise()
    {
        status = gameAPI.SecretStatus.Inactive;
        revealedWho = -1;
        revealedWhen = -1;
        deletedWhen = -1;
        listOfActors.Clear();
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
        revealedWho = -1;
        revealedWhen = -1;
        deletedWhen = -1;
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
