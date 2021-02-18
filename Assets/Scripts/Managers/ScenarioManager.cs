using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds current scenario
/// </summary>
public class ScenarioManager : MonoBehaviour
{
    //Auto-assigned from CampaignManager.cs OR TutorialManager.cs
    [HideInInspector] public Scenario scenario;                                 //current scenario in use

}
