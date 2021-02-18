using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Master tutorial SO for a specific side
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / Tutorial")]
public class Tutorial : ScriptableObject
{

    public GlobalSide side;
    public Scenario scenario;


    public void OnEnable()
    {
        Debug.AssertFormat(side != null, "Invalid side (Null) for {0}", name);
        Debug.AssertFormat(scenario != null, "Invalid scenario (Null) for {0}", name);
    }

}
