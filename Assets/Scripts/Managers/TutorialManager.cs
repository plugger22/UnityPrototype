using gameAPI;
using packageAPI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Handles Tutorial
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("Tutorials")]
    public Tutorial resistanceTutorial;
    public Tutorial authorityTutorial;

    #region save data compatibile
    [HideInInspector] public Tutorial tutorial;
    [HideInInspector] public TutorialSet set;
    [HideInInspector] public int index;                         //index that tracks player's progress (set #) through current tutorial
    #endregion


    #region Initialisation...
    /// <summary>
    /// Master Initialisation
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.TutorialOptions:
                SubInitialiseFastAccess();
                SubInitialiseTutorial();
                break;
            case GameState.LoadGame:
            case GameState.NewInitialisation:
            case GameState.LoadAtStart:
            case GameState.StartUp:
            case GameState.FollowOnInitialisation:
                //do nothing
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        Debug.Assert(resistanceTutorial != null, "Invalid resistanceTutorial (Null)");
        /*Debug.Assert(authorityTutorial != null, "Invalid authorityTutorial (Null)");*/  //EDIT -> TO DO -> switch back on for Authority side
    }
    #endregion

    #region SubInitialiseTutorial
    private void SubInitialiseTutorial()
    {
        InitialiseTutorial();
    }
    #endregion

    #endregion



    #region InitialiseTutorial
    /// <summary>
    /// Set up tutorial prior to running
    /// </summary>
    public void InitialiseTutorial()
    {
        //Debug
        tutorial = resistanceTutorial;
        Debug.LogFormat("[Tut] TutorialManager.cs -> InitialiseTutorial: tutorial \"{0}\" loaded{1}", tutorial.name, "\n");
        //set scenario
        if (tutorial.scenario != null)
        {
            GameManager.i.scenarioScript.scenario = tutorial.scenario;
            if (tutorial.scenario.city != null)
            {
                //set city
                GameManager.i.cityScript.SetCity(tutorial.scenario.city);
                //get index
                index = GameManager.i.dataScript.GetTutorialIndex(tutorial.name);
                if (index > -1)
                {
                    Debug.LogFormat("[Tut] TutorialManager.cs -> InitialiseTutorial: index \"{0}\" loaded{1}", index, "\n");
                    //get set
                    if (tutorial.listOfSets.Count > index)
                    {
                        set = tutorial.listOfSets[index];
                        if (set != null)
                        {
                            Debug.LogFormat("[Tut] TutorialManager.cs -> InitialiseTutorial: set \"{0}\" loaded{1}", set.name, "\n");
                            // Features toggle on/off -> To Do
                            UpdateFeatures(set.listOfFeaturesOff);
                        }
                        else { Debug.LogErrorFormat("Invalid tutorialSet (Null) for index {0}", index); }
                    }
                    else { Debug.LogErrorFormat("Invalid tutorialIndex (index {0}, there are {1} sets in tutorial.listOfSets)", index, tutorial.listOfSets.Count); }
                }
                else { Debug.LogError("Invalid tutorial index (-1)"); }
            }
            else { Debug.LogError("Invalid tutorial city (Null)"); }
        }
        else { Debug.LogError("Invalid tutorial Scenario (Null)"); }
    }
    #endregion

    #region UpdateFeatures
    /// <summary>
    /// Update features prior for current tutorial set. Note: this will override anything set in GameManager prefab -> FeatureManager
    /// </summary>
    /// <param name="listOfFeatures"></param>
    public void UpdateFeatures(List<TutorialFeature> listOfFeaturesToToggleOff)
    {
        if (listOfFeaturesToToggleOff != null)
        {
            //set all features true
            GameManager.i.optionScript.isAI = true;
            GameManager.i.optionScript.isNemesis = true;
            GameManager.i.optionScript.isFogOfWar = true;
            GameManager.i.optionScript.isDecisions = true;
            GameManager.i.optionScript.isMainInfoApp = true;
            GameManager.i.optionScript.isNPC = true;
            GameManager.i.optionScript.isSubordinates = true;
            GameManager.i.optionScript.isReviews = true;
            GameManager.i.optionScript.isObjectives = true;
            GameManager.i.optionScript.isOrganisations = true;
            GameManager.i.optionScript.isTargets = true;
            //turn OFF any features in list
            for (int i = 0; i < listOfFeaturesToToggleOff.Count; i++)
            {
                TutorialFeature feature = listOfFeaturesToToggleOff[i];
                if (feature != null)
                {
                    switch (feature.name)
                    {
                        case "AI":
                            GameManager.i.optionScript.isAI = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: AI toggled Off{0}", "\n");
                            break;
                        case "Decisions":
                            GameManager.i.optionScript.isDecisions = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Decisions toggled Off{0}", "\n");
                            break;
                        case "FOW":
                            GameManager.i.optionScript.isFogOfWar = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Fog of War toggled Off{0}", "\n");
                            break;
                        case "MainInfoApp":
                            GameManager.i.optionScript.isMainInfoApp = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: MainInfoApp toggled Off{0}", "\n");
                            break;
                        case "Nemesis":
                            GameManager.i.optionScript.isNemesis = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Nemesis toggled Off{0}", "\n");
                            break;
                        case "NPC":
                            GameManager.i.optionScript.isNPC = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: NPC toggled Off{0}", "\n");
                            break;
                        case "Objectives":
                            GameManager.i.optionScript.isObjectives = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Objectives toggled Off{0}", "\n");
                            break;
                        case "Reviews":
                            GameManager.i.optionScript.isReviews = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Reviews toggled Off{0}", "\n");
                            break;
                        case "Subordinates":
                            GameManager.i.optionScript.isSubordinates = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Subordinates toggled Off{0}", "\n");
                            break;
                        case "Organisations":
                            GameManager.i.optionScript.isOrganisations = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Organisations toggled Off{0}", "\n");
                            break;
                        case "Targets":
                            GameManager.i.optionScript.isTargets = false;
                            //auto off if targets are off
                            GameManager.i.optionScript.isOrganisations = false;
                            GameManager.i.optionScript.isObjectives = false;
                            Debug.LogFormat("[Tut] TutorialManager.cs -> UpdateFeatures: Targets, Organisations and Objectives all toggled Off{0}", "\n");
                            break;
                        default: Debug.LogWarningFormat("Unrecognised feature.name \"{0}\"", feature.name); break;
                    }
                }
                else { Debug.LogWarningFormat("Invalid feature (Null) for listOfFeaturesToToggleOff[{0}]", i); }
            }
            //discretionary GUI elements toggled off if required (those not directly effected by option settings)
            if (GameManager.i.optionScript.isAI == true)
            { GameManager.i.featureScript.ToggleAISideWidget(true); }
            else { GameManager.i.featureScript.ToggleAISideWidget(false); }
        }
        else { Debug.LogError("Invalid listOfFeaturesToToggleOff (Null)"); }
    }
    #endregion

    #region DebugDisplayTutorialData
    /// <summary>
    /// Display all relevant tutorial data
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayTutorialData()
    {
        if (GameManager.i.inputScript.GameState == GameState.Tutorial)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("-Tutorial Data{0}{1}", "\n", "\n");
            builder.AppendFormat("tutorial: {0}{1}", tutorial.name, "\n");
            builder.AppendFormat("tutorialSet: {0}{1}", set.name, "\n");
            builder.AppendFormat("index: {0}{1}", index, "\n");

            //current tutorialSet -> Features Off
            builder.AppendFormat("{0}-features OFF for \"{1}\"{2}", "\n", set.name, "\n");
            for (int i = 0; i < set.listOfFeaturesOff.Count; i++)
            { builder.AppendFormat(" {0}{1}", set.listOfFeaturesOff[i].name, "\n"); }

            //current tutorial -> Sets
            builder.AppendFormat("{0}-tutorialSets for \"{1}\"{2}", "\n", tutorial.name, "\n");
            for (int i = 0; i < tutorial.listOfSets.Count; i++)
            { builder.AppendFormat(" {0} -> index {1}{2}", tutorial.listOfSets[i].name, i, "\n"); }

            //dictOfTutorialData
            Dictionary<string, TutorialData> dictOfData = GameManager.i.dataScript.GetDictOfTutorialData();
            if (dictOfData != null)
            {
                builder.AppendFormat("{0}-dictOfTutorialData{1}", "\n", "\n");
                if (dictOfData.Count > 0)
                {
                    foreach (var data in dictOfData)
                    { builder.AppendFormat(" {0} -> index {1}{2}", data.Value.tutorialName, data.Value.index, "\n"); }
                }
                else { builder.AppendLine("No records present"); }
            }
            else { Debug.LogError("Invalid dictOfTutorialData (Null)"); }
            return builder.ToString();
        }
        else { return "You must be in Tutorial mode to access this information"; }
    }
    #endregion
    
    
    //new methods above here
}
