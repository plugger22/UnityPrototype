using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// SO archetype for Node type. Name of SO is the type of Node Arc, eg. "Corporate"
/// </summary>
[CreateAssetMenu(menuName = "Node / Archetype")]
public class NodeArc : ScriptableObject
{
    [HideInInspector] public int nodeArcID;               //unique #, zero based, assigned automatically by DataManager.Initialise

    //node stats -> base level stats (0 to 3)
    [Tooltip("Values between 0 and 3 only")] [Range(0,3)] public int Stability;
    [Tooltip("Values between 0 and 3 only")] [Range(0, 3)] public int Support;
    [Tooltip("Values between 0 and 3 only")] [Range(0, 3)] public int Security;

    public Sprite sprite;

    [Tooltip("Contact Types that are possible in this NodeArc")]
    public ContactType[] contactTypes;


    public void OnEnable()
    {
        Debug.Assert(contactTypes != null, string.Format("Invalid array of contactTypes (Null) for NodeArc \"{0}\"", this.name));
    }

    /// <summary>
    /// returns a randomly selected ContactType, null if none found
    /// </summary>
    /// <returns></returns>
    public ContactType GetRandomContactType()
    {
        ContactType typeReturn = null;
        int numOfTypes = contactTypes.Length;
        if (numOfTypes > 0)
        {
            if (numOfTypes > 1)
            { typeReturn = contactTypes[Random.Range(0, numOfTypes)]; }
            else { typeReturn = contactTypes[0]; }
        }
        return typeReturn;
    }
}
