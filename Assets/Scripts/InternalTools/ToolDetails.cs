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
    }

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
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "None",
                refTag = "None",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Into the Unknown",
                refTag = "IntoTheUnknown",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Character is attacked in a Non-Lethal way",
                refTag = "AttackedNonLethal",
                listAction = new List<int>(){},
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
                listTension = new List<int>(){},
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
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Impending Doom",
                refTag = "ImpendingDoom",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listSocial = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Motive Free Crime",
                refTag = "MotiveFreeCrime",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "Collateral Damage",
                refTag = "CollateralDamage",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Character is Attacked in a Lethal Way",
                refTag = "LethalAttack",
                listAction = new List<int>(){},
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
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Remote Location",
                refTag = "RemoteLocation",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Ambush",
                refTag = "Ambush",
                listAction = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Catastrophe",
                refTag = "Catastrophe",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Character has a Clever Idea",
                refTag = "CleverIdea",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "Something is Getting Away",
                refTag = "GettingAway",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Character Disappears",
                refTag = "Disappears",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Hunted",
                refTag = "Hunted",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listSocial = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Bad Decision",
                refTag = "BadDecision",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "This isn't Working",
                refTag = "IsNotWorking",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Distraction",
                refTag = "Distraction",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "Ill Will",
                refTag = "IllWill",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "An Organisation",
                refTag = "Organisation",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Wanted by the Law",
                refTag = "WantedByTheLaw",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Resource Disappears",
                refTag = "ResourceDisappears",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Fortuitous Find",
                refTag = "FortuitousFind",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "All is Revealed",
                refTag = "AllRevealed",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "People Behaving Badly",
                refTag = "BehavingBadly",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Useful Information From a Known Source",
                refTag = "UsefulInfoKnown",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
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
                listMystery = new List<int>(){},
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
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Character is Attacked to Abduct",
                refTag = "Abduct",
                listAction = new List<int>(){},
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
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Immediately",
                refTag = "Immediately",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "Chase",
                refTag = "Chase",
                listAction = new List<int>(){},
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
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Crime is Committed",
                refTag = "CrimeCommitted",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Character is Incapacitated",
                refTag = "Incapacitated",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "It's a Secret",
                refTag = "ItIsASecret",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
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
                listMystery = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Nowhere to Run",
                refTag = "NowhereToRun",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "The Observer",
                refTag = "TheObserver",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "Escape",
                refTag = "Escape",
                listAction = new List<int>(){},
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
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Heavily Guarded",
                refTag = "HeavilyGuarded",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Rescue",
                refTag = "Rescue",
                listAction = new List<int>(){},
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
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Character Acts out of Character",
                refTag = "ActsOutOfCharacter",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Physical Contest of Skills",
                refTag = "PhysicalContest",
                listAction = new List<int>(){},
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
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "A Common Social Gathering",
                refTag = "CommonGathering",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Mystery Solved",
                refTag = "MysterySolved",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
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
                listSocial = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Secret Information Leaked",
                refTag = "SecretInfoLeaked",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Suspicion",
                refTag = "Suspicion",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
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
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "Mass Battle",
                refTag = "MassBattle",
                listAction = new List<int>(){},
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
                listTension = new List<int>(){},
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
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Character is Diminished",
                refTag = "CharDiminished",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "The Plot Thickens",
                refTag = "PlotThickens",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Enemies",
                refTag = "Enemies",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Dubious Rationale",
                refTag = "DubiousRationale",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Menacing Tone",
                refTag = "MenacingTone",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Crucial Life Support System Begins to Fail",
                refTag = "CrucialLifeSupport",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "Doing the Right Thing",
                refTag = "RightThing",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Victory",
                refTag = "Victory",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Taking Chances",
                refTag = "TakingChances",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Sole Survivor",
                refTag = "SoleSurvivor",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Cryptic Information From an Unknown Source",
                refTag = "CrypticInfoUnknown",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
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
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "A Problem Returns",
                refTag = "ProblemReturns",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listTension = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Stop That",
                refTag = "StopThat",
                listAction = new List<int>(){},
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
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
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
                listPersonal = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Public Location",
                refTag = "PublicLocation",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "The Leader",
                refTag = "TheLeader",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Saviour",
                refTag = "Saviour",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Disarmed",
                refTag = "Disarmed",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listMystery = new List<int>(){},
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
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Defend or Not to Defend",
                refTag = "DefendOrNot",
                listAction = new List<int>(){},
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
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "Reinforcements",
                refTag = "Reinforcements",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Physical Barrier to Overcome",
                refTag = "PhysicalBarrier",
                listAction = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Quiet Catastrophe",
                refTag = "QuietCatastrophe",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listMystery = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Celebration",
                refTag = "Celebration",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Standoff",
                refTag = "Standoff",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Double Down",
                refTag = "DoubleDown",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "Hidden Threat",
                refTag = "HiddenThreat",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Religion",
                refTag = "Religion",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Clear the Record",
                refTag = "ClearRecord",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Theft",
                refTag = "Theft",
                listAction = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "A Need to Hide",
                refTag = "NeedToHide",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listTension = new List<int>(){},
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
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Preparation",
                refTag = "Preparation",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "An Improbable Crime",
                refTag = "ImprobableCrime",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
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
                listPersonal = new List<int>(){},
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
                listPersonal = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Dealing with a Calamity",
                refTag = "Calamity",
                listAction = new List<int>(){},
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
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "It's a Trap!",
                refTag = "ItIsATrap",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Time Limit",
                refTag = "TimeLimit",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Needed Resource is Running Short",
                refTag = "NeededResource",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Bad News",
                refTag = "Bad News",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listPersonal = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "Hunker Down",
                refTag = "HunkerDown",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listTension = new List<int>(){},
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
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Used Against Them",
                refTag = "UsedAgainstThem",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Creepy Tone",
                refTag = "CreepyTone",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Travel Setting",
                refTag = "TravelSetting",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "Escort Duty",
                refTag = "EscortDuty",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "An Old Deal",
                refTag = "OldDeal",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A New Enemy",
                refTag = "NewEnemy",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
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
                listSocial = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Mysterious New Person",
                refTag = " MysteriousPerson",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Frenetic Activity",
                refTag = "FreneticActivity",
                listAction = new List<int>(){},
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
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "Likable",
                refTag = "Likable",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Someone is Where they Should Not Be",
                refTag = "ShouldNotBe",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Sneaky Barrier",
                refTag = "SneakyBarrier",
                listAction = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Vulnerability Exploited",
                refTag = "VulnerableExploit",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Fraud",
                refTag = "Fraud",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "It's Business",
                refTag = "ItIsBusiness",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
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
                listSocial = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "A Moment of Peace",
                refTag = "MomentOfPeace",
                listAction = new List<int>(){},
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
                listPersonal = new List<int>(){},
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
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Beat You To It",
                refTag = "BeatYouToIt",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Confrontation",
                refTag = "Confrontation",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "Argument",
                refTag = "Argument",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Protector",
                refTag = "Protector",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Crescendo",
                refTag = "Crescendo",
                listAction = new List<int>(){},
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
                listAction = new List<int>(){},
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
                listMystery = new List<int>(){},
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
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "An Opposing Story",
                refTag = "OpposingStory",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Meta",
                refTag = "Meta",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },

            new Plotpoint(){
                tag = "Character Exits the Adventure",
                refTag = "CharExits",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Character Returns",
                refTag = "CharReturns",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Character Steps Up",
                refTag = "CharStepsUp",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Character Steps Down",
                refTag = "CharStepsDown",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Character Downgrade",
                refTag = "CharDowngrade",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Character Upgrade",
                refTag = "CharUpgrade",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            },
            new Plotpoint(){
                tag = "Plotline Combo",
                refTag = "PlotlineCombo",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotpointType.Normal
            }
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

}
