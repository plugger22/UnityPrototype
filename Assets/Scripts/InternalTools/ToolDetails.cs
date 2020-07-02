using System.Collections.Generic;
using System.Linq;
using toolsAPI;
using UnityEngine;

/// <summary>
/// Plotpoint and character details for reference and lookup dictionaries
/// </summary>
public class ToolDetails : MonoBehaviour
{

    /// <summary>
    /// master initialisation
    /// </summary>
    public void Initialise()
    {
        InitialisePlotpoints();
        InitialiseMetaPlotpoints();
    }


    #region InitialisePlotpoints
    /// <summary>
    /// Plotpoints initialisation
    /// </summary>
    private void InitialisePlotpoints()
    {
        List<Plotpoint> listOfPlotpoints = new List<Plotpoint>()
        {
            new Plotpoint()
            {
                tag = "Conclusion",
                refTag = "Conclusion",
                listAction = new List<int>(){1,2,3,4,5,6,7,8},
                listTension = new List<int>(){1,2,3,4,5,6,7,8},
                listMystery = new List<int>(){1,2,3,4,5,6,7,8},
                listSocial = new List<int>(){1,2,3,4,5,6,7,8},
                listPersonal = new List<int>(){1,2,3,4,5,6,7,8},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "None",
                refTag = "None",
                listAction = new List<int>(){9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24},
                listTension = new List<int>(){9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24},
                listMystery = new List<int>(){9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24},
                listSocial = new List<int>(){9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24},
                listPersonal = new List<int>(){9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Into the Unknown",
                refTag = "IntoTheUnknown",
                listAction = new List<int>(){},
                listTension = new List<int>(){25,26},
                listMystery = new List<int>(){25,26},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Character is attacked in a Non-Lethal way",
                refTag = "AttackedNonLethal",
                listAction = new List<int>(){25,26},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A needed Resource runs out",
                refTag = "ResourceRunsOut",
                listAction = new List<int>(){},
                listTension = new List<int>(){27},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Useful Information from an Uknown Source",
                refTag = "UsefulInfoUnknown",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){27,28},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Impending Doom",
                refTag = "ImpendingDoom",
                listAction = new List<int>(){},
                listTension = new List<int>(){28},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Outcast",
                refTag = "Outcast",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){25,26},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Persuasion",
                refTag = "Persuasion",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){25,26},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Motive Free Crime",
                refTag = "MotiveFreeCrime",
                listAction = new List<int>(){},
                listTension = new List<int>(){29},
                listMystery = new List<int>(){29,30},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
             
            // - - -

            new Plotpoint(){
                tag = "Collateral Damage",
                refTag = "CollateralDamage",
                listAction = new List<int>(){27},
                listTension = new List<int>(){30},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Shady Places",
                refTag = "ShadyPlaces",
                listAction = new List<int>(){},
                listTension = new List<int>(){31,32},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Character is Attacked in a Lethal Way",
                refTag = "LethalAttack",
                listAction = new List<int>(){28,29},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Do it, or Else",
                refTag = "DoItOrElse",
                listAction = new List<int>(){},
                listTension = new List<int>(){33},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){27},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Remote Location",
                refTag = "RemoteLocation",
                listAction = new List<int>(){},
                listTension = new List<int>(){34},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Ambush",
                refTag = "Ambush",
                listAction = new List<int>(){30,31},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Sold",
                refTag = "Sold",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){27,28},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Catastrophe",
                refTag = "Catastrophe",
                listAction = new List<int>(){32},
                listTension = new List<int>(){35},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Grisly Tone",
                refTag = "GrislyTone",
                listAction = new List<int>(){},
                listTension = new List<int>(){36},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Character has a Clever Idea",
                refTag = "CleverIdea",
                listAction = new List<int>(){33},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "Something is Getting Away",
                refTag = "GettingAway",
                listAction = new List<int>(){34},
                listTension = new List<int>(){37},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Retaliation",
                refTag = "Retaliation",
                listAction = new List<int>(){},
                listTension = new List<int>(){38,39},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){29,30},
                listPersonal = new List<int>(){28},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Character Disappears",
                refTag = "Disappears",
                listAction = new List<int>(){},
                listTension = new List<int>(){40},
                listMystery = new List<int>(){31,32},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Hunted",
                refTag = "Hunted",
                listAction = new List<int>(){35,36},
                listTension = new List<int>(){41},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A High Energy Gathering",
                refTag = "HighEnergyGathering",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){31},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Rare or Unique Social Gathering",
                refTag = "UniqueGathering",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){32},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Bad Decision",
                refTag = "BadDecision",
                listAction = new List<int>(){},
                listTension = new List<int>(){42},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){29},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "This isn't Working",
                refTag = "IsNotWorking",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){33},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Distraction",
                refTag = "Distraction",
                listAction = new List<int>(){37},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - - 

            new Plotpoint(){
                tag = "Ill Will",
                refTag = "IllWill",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){30,31},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "An Organisation",
                refTag = "Organisation",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){33,34},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Wanted by the Law",
                refTag = "WantedByTheLaw",
                listAction = new List<int>(){},
                listTension = new List<int>(){43},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){32,33},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Resource Disappears",
                refTag = "ResourceDisappears",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){34,35},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "It's Your Duty",
                refTag = "YourDuty",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){34,35},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Fortuitous Find",
                refTag = "FortuitousFind",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){36},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Character Connection Severed",
                refTag = "ConnectionSevered",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){36,37},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "All is Revealed",
                refTag = "AllRevealed",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){37},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Humiliation",
                refTag = "Humiliation",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){38},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "People Behaving Badly",
                refTag = "BehavingBadly",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){35},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Useful Information From a Known Source",
                refTag = "UsefulInfoKnown",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){38,39},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Cryptic Information From a Known Source",
                refTag = "CrypticInfoKnown",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){40},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Lie Discovered",
                refTag = "LieDiscovered",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){41,42},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Character is Attacked to Abduct",
                refTag = "Abduct",
                listAction = new List<int>(){38,39},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Something Exotic",
                refTag = "SomethingExotic",
                listAction = new List<int>(){40},
                listTension = new List<int>(){44},
                listMystery = new List<int>(){43},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Immediately",
                refTag = "Immediately",
                listAction = new List<int>(){41,42},
                listTension = new List<int>(){45},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Fame",
                refTag = "Fame",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){36},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "Chase",
                refTag = "Chase",
                listAction = new List<int>(){43,44},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Betrayal",
                refTag = "Betrayal",
                listAction = new List<int>(){},
                listTension = new List<int>(){46},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){39,40},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Crime is Committed",
                refTag = "CrimeCommitted",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){44,45},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Character is Incapacitated",
                refTag = "Incapacitated",
                listAction = new List<int>(){},
                listTension = new List<int>(){47},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){41,42},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "It's a Secret",
                refTag = "ItIsASecret",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){46,47},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Something Lost has been Found",
                refTag = "LostFound",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){48},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Scapegoat",
                refTag = "Scapegoat",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){37},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Nowhere to Run",
                refTag = "NowhereToRun",
                listAction = new List<int>(){},
                listTension = new List<int>(){48},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "At Night",
                refTag = "AtNight",
                listAction = new List<int>(){},
                listTension = new List<int>(){49,50},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "The Observer",
                refTag = "Observer",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){49},
                listSocial = new List<int>(){38},
                listPersonal = new List<int>(){43},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "Escape",
                refTag = "Escape",
                listAction = new List<int>(){45,46},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Secret Weapon",
                refTag = "SecretWeapon",
                listAction = new List<int>(){},
                listTension = new List<int>(){51},
                listMystery = new List<int>(){50},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Heavily Guarded",
                refTag = "HeavilyGuarded",
                listAction = new List<int>(){47,48},
                listTension = new List<int>(){52},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Rescue",
                refTag = "Rescue",
                listAction = new List<int>(){49,50},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Liar!",
                refTag = "Liar",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){51,52},
                listSocial = new List<int>(){39},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Home Sweet Home",
                refTag = "HomeSweetHome",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){44,45},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Character Acts out of Character",
                refTag = "ActsOutOfCharacter",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){53},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Headquarters",
                refTag = "Headquarters",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){40,41},
                listPersonal = new List<int>(){46},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Physical Contest of Skills",
                refTag = "PhysicalContest",
                listAction = new List<int>(){51,52},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Dead",
                refTag = "Dead",
                listAction = new List<int>(){},
                listTension = new List<int>(){53},
                listMystery = new List<int>(){54},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "A Common Social Gathering",
                refTag = "CommonGathering",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){42,43},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Light Urban Setting",
                refTag = "LightUrban",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){44,45},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Mystery Solved",
                refTag = "MysterySolved",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){55,56},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Work Related Gathering",
                refTag = "WorkGathering",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){46,47},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Family Matters",
                refTag = "FamilyMatters",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){47,48},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Secret Information Leaked",
                refTag = "SecretInfoLeaked",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){57},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Suspicion",
                refTag = "Suspicion",
                listAction = new List<int>(){},
                listTension = new List<int>(){54},
                listMystery = new List<int>(){58,59},
                listSocial = new List<int>(){48},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Lose Lose",
                refTag = "LoseLose",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Figure From the Past",
                refTag = "FigureFromPast",
                listAction = new List<int>(){},
                listTension = new List<int>(){55},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){49},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "Mass Battle",
                refTag = "MassBattle",
                listAction = new List<int>(){53,54},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Out in the Open",
                refTag = "OutInTheOpen",
                listAction = new List<int>(){},
                listTension = new List<int>(){56},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Evidence",
                refTag = "Evidence",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){60,61},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Character is Diminished",
                refTag = "CharDiminished",
                listAction = new List<int>(){},
                listTension = new List<int>(){57,58},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){50,51},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "The Plot Thickens",
                refTag = "PlotThickens",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){62,63},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Enemies",
                refTag = "Enemies",
                listAction = new List<int>(){},
                listTension = new List<int>(){59},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){49},
                listPersonal = new List<int>(){52,53},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Dubious Rationale",
                refTag = "DubiousRationale",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){64},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Menacing Tone",
                refTag = "MenacingTone",
                listAction = new List<int>(){},
                listTension = new List<int>(){60},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Crucial Life Support System Begins to Fail",
                refTag = "CrucialLifeSupport",
                listAction = new List<int>(){55},
                listTension = new List<int>(){61},
                listMystery = new List<int>(){65},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Dense Urban Setting",
                refTag = "DenseUrban",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){50,51},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - - 

            new Plotpoint(){
                tag = "Doing the Right Thing",
                refTag = "RightThing",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){54},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Victory",
                refTag = "Victory",
                listAction = new List<int>(){56,57},
                listTension = new List<int>(){62},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Taking Chances",
                refTag = "TakingChances",
                listAction = new List<int>(){58,59},
                listTension = new List<int>(){63},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Group is in Trouble",
                refTag = "GroupInTrouble",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){52,53},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Sole Survivor",
                refTag = "SoleSurvivor",
                listAction = new List<int>(){60,61},
                listTension = new List<int>(){64},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Token Response",
                refTag = "TokenResponse",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){54},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Cryptic Information From an Unknown Source",
                refTag = "CrypticInfoUnknown",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){66,67},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Common Thread",
                refTag = "CommonThread",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){68,69},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "A Problem Returns",
                refTag = "ProblemReturns",
                listAction = new List<int>(){},
                listTension = new List<int>(){65,66},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Stuck",
                refTag = "Stuck",
                listAction = new List<int>(){},
                listTension = new List<int>(){67,68},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "At your Mercy",
                refTag = "AtYourMercy",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){55,56},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Stop That",
                refTag = "StopThat",
                listAction = new List<int>(){62,63},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Not their Master",
                refTag = "NotTheirMaster",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){70},
                listSocial = new List<int>(){55},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Fall From Power",
                refTag = "FallFromPower",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){57,58},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Help is Offered, For a Price",
                refTag = "HelpOffered",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){59,60},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Public Location",
                refTag = "PublicLocation",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){56,57},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "The Leader",
                refTag = "TheLeader",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){58,59},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Prized Possession",
                refTag = "PrizedPossession",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){61,62},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Saviour",
                refTag = "Saviour",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){60,61},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Disarmed",
                refTag = "Disarmed",
                listAction = new List<int>(){},
                listTension = new List<int>(){69,70},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "The Secret To The Power",
                refTag = "SecretPower",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){71,72},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Hidden Agenda",
                refTag = "HiddenAgenda",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){73,74},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Defend or Not to Defend",
                refTag = "DefendOrNot",
                listAction = new List<int>(){64,65},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Crash",
                refTag = "Crash",
                listAction = new List<int>(){66,67},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - - 

            new Plotpoint(){
                tag = "Reinforcements",
                refTag = "Reinforcements",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){62,63},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Government",
                refTag = "Government",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){64,65},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Physical Barrier to Overcome",
                refTag = "PhysicalBarrier",
                listAction = new List<int>(){68,69},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Injustice",
                refTag = "Injustice",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){66,67},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Quiet Catastrophe",
                refTag = "QuietCatastrophe",
                listAction = new List<int>(){},
                listTension = new List<int>(){71},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "An Object of Unknown Use is Found",
                refTag = "ObjectUnknown",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){75},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "It's all about You",
                refTag = "AllAboutYou",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){64,65},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Celebration",
                refTag = "Celebration",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){68,69},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Standoff",
                refTag = "Standoff",
                listAction = new List<int>(){},
                listTension = new List<int>(){72},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){70},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Double Down",
                refTag = "DoubleDown",
                listAction = new List<int>(){70,71},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "Hidden Threat",
                refTag = "HiddenThreat",
                listAction = new List<int>(){},
                listTension = new List<int>(){73},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Character Connection",
                refTag = "Connection",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){66,67},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Religion",
                refTag = "Religion",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){71},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Innocence",
                refTag = "Innocence",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){72},
                listPersonal = new List<int>(){68},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Clear the Record",
                refTag = "ClearRecord",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){76},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Willing to Talk",
                refTag = "WillingToTalk",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){69,70},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Theft",
                refTag = "Theft",
                listAction = new List<int>(){72,73},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Character Harm",
                refTag = "CharacterHarm",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){71,72},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "A Need to Hide",
                refTag = "NeedToHide",
                listAction = new List<int>(){},
                listTension = new List<int>(){74,75},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Followed",
                refTag = "Followed",
                listAction = new List<int>(){},
                listTension = new List<int>(){76,77},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Framed",
                refTag = "Framed",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){77},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){73},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Preparation",
                refTag = "Preparation",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){73,74},
                listPersonal = new List<int>(){74,75},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "An Improbable Crime",
                refTag = "ImprobableCrime",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){78},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Friend Focus",
                refTag = "FriendFocus",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){76},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Untouchable",
                refTag = "Untouchable",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){77},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Bribe",
                refTag = "Bribe",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){78},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Dealing with a Calamity",
                refTag = "Calamity",
                listAction = new List<int>(){74,75},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Sudden Cessation",
                refTag = "SuddenCessation",
                listAction = new List<int>(){76,77},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "It's a Trap!",
                refTag = "ItIsATrap",
                listAction = new List<int>(){},
                listTension = new List<int>(){78,79},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Meeting of Minds",
                refTag = "MeetingOfMinds",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){75},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Time Limit",
                refTag = "TimeLimit",
                listAction = new List<int>(){},
                listTension = new List<int>(){80,81},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "The Hidden Hand",
                refTag = "HiddenHand",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){79,80},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Needed Resource is Running Short",
                refTag = "NeededResource",
                listAction = new List<int>(){},
                listTension = new List<int>(){82,83},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Organisations in Conflict",
                refTag = "OrgsInConflict",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){76},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Bad News",
                refTag = "Bad News",
                listAction = new List<int>(){},
                listTension = new List<int>(){84,85},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Character Assistance",
                refTag = "CharAssistance",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){79,80},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Asking for Help",
                refTag = "AskingForHelp",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){81,82},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "Hunker Down",
                refTag = "HunkerDown",
                listAction = new List<int>(){},
                listTension = new List<int>(){86},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Abandoned",
                refTag = "Abandoned",
                listAction = new List<int>(){},
                listTension = new List<int>(){87,88},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Find it Or Else",
                refTag = "FindItOrElse",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){81,82},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Used Against Them",
                refTag = "UsedAgainstThem",
                listAction = new List<int>(){78},
                listTension = new List<int>(){89},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Powerful Person",
                refTag = "PowerfulPerson",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){77},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Creepy Tone",
                refTag = "CreepyTone",
                listAction = new List<int>(){},
                listTension = new List<int>(){90,91},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Welcome to the Plot",
                refTag = "WelcomePlot",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){83},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Travel Setting",
                refTag = "TravelSetting",
                listAction = new List<int>(){79},
                listTension = new List<int>(){92},
                listMystery = new List<int>(){83},
                listSocial = new List<int>(){78},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "Escort Duty",
                refTag = "EscortDuty",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){79},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "An Old Deal",
                refTag = "OldDeal",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){84},
                listSocial = new List<int>(){80},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A New Enemy",
                refTag = "NewEnemy",
                listAction = new List<int>(){},
                listTension = new List<int>(){93},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Alliance",
                refTag = "Alliance",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){81,82},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Power over Others",
                refTag = "PowerOthers",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){83,84},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Mysterious New Person",
                refTag = " MysteriousPerson",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){85},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Frenetic Activity",
                refTag = "FreneticActivity",
                listAction = new List<int>(){80,81},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Rural Setting",
                refTag = "RuralSetting",
                listAction = new List<int>(){},
                listTension = new List<int>(){94},
                listMystery = new List<int>(){86},
                listSocial = new List<int>(){85},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "Likable",
                refTag = "Likable",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){84},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Someone is Where they Should Not Be",
                refTag = "ShouldNotBe",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){87,88},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Sneaky Barrier",
                refTag = "SneakyBarrier",
                listAction = new List<int>(){82,83},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Corruption",
                refTag = "Corruption",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){86,87},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Vulnerability Exploited",
                refTag = "VulnerableExploit",
                listAction = new List<int>(){},
                listTension = new List<int>(){95},
                listMystery = new List<int>(){89},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "The Promise of Reward",
                refTag = "PromiseOfReward",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){85,86},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Fraud",
                refTag = "Fraud",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){90,91},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "It's Business",
                refTag = "ItIsBusiness",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){88,89},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Just Cause Gone Awry",
                refTag = "JustCause",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){90},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Expert Knowledge",
                refTag = "ExpertKnowledge",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){87},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Moment of Peace",
                refTag = "MomentOfPeace",
                listAction = new List<int>(){84,85},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Focus on the Mundane",
                refTag = "FocusMundane",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){88,89},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Run Away!",
                refTag = "RunAway",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){90,91},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Beat You To It",
                refTag = "BeatYouToIt",
                listAction = new List<int>(){86,87},
                listTension = new List<int>(){},
                listMystery = new List<int>(){92,93},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Confrontation",
                refTag = "Confrontation",
                listAction = new List<int>(){88,89},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){91},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            // - - -

            new Plotpoint(){
                tag = "Argument",
                refTag = "Argument",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){92,93},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Social Tension Set to Boiling",
                refTag = "SocialTension",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){94},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Protector",
                refTag = "Protector",
                listAction = new List<int>(){90,91},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){92,93},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Crescendo",
                refTag = "Crescendo",
                listAction = new List<int>(){92,93},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Destroy the Thing",
                refTag = "DestroyThing",
                listAction = new List<int>(){94,95},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Conspiracy Theory",
                refTag = "ConspiracyTheory",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){94},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Servant",
                refTag = "Servant",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){95},
                listPersonal = new List<int>(){94,95},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "An Opposing Story",
                refTag = "OpposingStory",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){95},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Meta",
                refTag = "Meta",
                listAction = new List<int>(){96, 97, 98, 99, 100},
                listTension = new List<int>(){96, 97, 98, 99, 100},
                listMystery = new List<int>(){96, 97, 98, 99, 100},
                listSocial = new List<int>(){96, 97, 98, 99, 100},
                listPersonal = new List<int>(){96, 97, 98, 99, 100},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
        };

        Dictionary<string, Plotpoint> dictOfPlotpoints = ToolManager.i.toolDataScript.GetDictOfPlotpoints();
        if (dictOfPlotpoints != null)
        {
            Debug.LogFormat("[Tst] ToolDetails -> InitialisePlotpoints: There are {0} records in the listOfPlotponts{1}", listOfPlotpoints.Count, "\n");
            //convert list to dictionary
            dictOfPlotpoints = listOfPlotpoints.ToDictionary(k => k.refTag);
            Debug.LogFormat("[Tst] ToolDetails -> InitialisePlotpoints: There are {0} records in the dictOfPlotponts{1}", dictOfPlotpoints.Count, "\n");
        }
        else { Debug.LogError("Invalid dictOfPlotpoints (Null)"); }
    }
    #endregion


    #region InitialiseMetaPlotpoints
    /// <summary>
    /// MetaPlotpoints initialisation
    /// </summary>
    private void InitialiseMetaPlotpoints()
    {
        List<MetaPlotpoint> listOfMetaPlotpoints = new List<MetaPlotpoint>()
        {
                new MetaPlotpoint(){
                tag = "Character Exits the Adventure",
                refTag = "CharExits",
                listToRoll = new List<int>(){1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18},
            },
            new MetaPlotpoint(){
                tag = "Character Returns",
                refTag = "CharReturns",
                listToRoll = new List<int>(){19,20,21,22,23,24,25,26,27},
            },
            new MetaPlotpoint(){
                tag = "Character Steps Up",
                refTag = "CharStepsUp",
                listToRoll = new List<int>(){28,29,30,31,32,33,34,35,36},
            },
            new MetaPlotpoint(){
                tag = "Character Steps Down",
                refTag = "CharStepsDown",
                listToRoll = new List<int>(){37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55},
            },
            new MetaPlotpoint(){
                tag = "Character Downgrade",
                refTag = "CharDowngrade",
                listToRoll = new List<int>(){56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73},
            },
            new MetaPlotpoint(){
                tag = "Character Upgrade",
                refTag = "CharUpgrade",
                listToRoll = new List<int>(){74,75,76,77,78,79,80,81,82},
            },
            new MetaPlotpoint(){
                tag = "Plotline Combo",
                refTag = "PlotlineCombo",
                listToRoll = new List<int>(){83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100},
            }
        };
        Dictionary<string, MetaPlotpoint> dictOfMetaPlotpoints = ToolManager.i.toolDataScript.GetDictOfMetaPlotpoints();
        if (dictOfMetaPlotpoints != null)
        {
            Debug.LogFormat("[Tst] ToolDetails -> InitialiseMetaPlotpoints: There are {0} records in the listOfMetaPlotponts{1}", listOfMetaPlotpoints.Count, "\n");
            //convert list to dictionary
            dictOfMetaPlotpoints = listOfMetaPlotpoints.ToDictionary(k => k.refTag);
            Debug.LogFormat("[Tst] ToolDetails -> InitialiseMetaPlotpoints: There are {0} records in the dictOfMetaPlotponts{1}", dictOfMetaPlotpoints.Count, "\n");
        }
        else { Debug.LogError("Invalid dictOfMetaPlotpoints (Null)"); }
    }
    #endregion


    //new methods above here
}
