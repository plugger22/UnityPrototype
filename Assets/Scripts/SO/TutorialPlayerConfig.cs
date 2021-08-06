using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// specifies player starting state for a tutorial state
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialPlayerConfig")]
public class TutorialPlayerConfig : ScriptableObject
{
    [Header("Side")]
    [Tooltip("Which side")]
    public GlobalSide side;

    [Header("Stats")]
    [Tooltip("Amount of invisibility player starts set with")]
    public int invisibility = 3;
    [Tooltip("Amount of mood player starts set with")]
    public int mood = 3;
    [Tooltip("Amount of innocence player starts set with")]
    public int innocence = 3;
    [Tooltip("Amount of Power player starts set with")]
    public int power = 0;

    [Header("State")]
    [Tooltip("Any Conditions that the player starts with")]
    public List<Condition> listOfConditions;
    [Tooltip("Any Secrets that the player starts with")]
    public List<Secret> listOfSecrets;
    [Tooltip("Any Gear that the player starts with")]
    public List<Gear> listOfGear;


    public void OnEnable()
    {
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
    }

}
