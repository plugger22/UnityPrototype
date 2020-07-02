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
            new Plotpoint(){ tag = "Conclusion"},
            new Plotpoint(){ tag = "None"},
            new Plotpoint(){ tag = "Into the Unknown"},
            new Plotpoint(){ tag = "A Character is attacked in a Non-Lethal way"},
            new Plotpoint(){ tag = "A needed Resource runs out"},
            new Plotpoint(){ tag = "Useful Information from an Uknown Source"},
            new Plotpoint(){ tag = "Impending Doom"},
            new Plotpoint(){ tag = "Outcast"},
            new Plotpoint(){ tag = "Persuasion"},
            new Plotpoint(){ tag = "A Motive Free Crime"},

            new Plotpoint(){ tag = "Collateral Damage"},
            new Plotpoint(){ tag = "Shady Places"},
            new Plotpoint(){ tag = "A Character is Attacked in a Lethal Way"},
            new Plotpoint(){ tag = "Do it, or Else"},
            new Plotpoint(){ tag = "Remote Location"},
            new Plotpoint(){ tag = "Ambush"},
            new Plotpoint(){ tag = "Sold"},
            new Plotpoint(){ tag = "Catastrophe"},
            new Plotpoint(){ tag = "Grisly Tone"},
            new Plotpoint(){ tag = "Character has a Clever Idea"},

            new Plotpoint(){ tag = "Something is Getting Away"},
            new Plotpoint(){ tag = "Retaliation"},
            new Plotpoint(){ tag = "A Character Disappears"},
            new Plotpoint(){ tag = "Hunted"},
            new Plotpoint(){ tag = "A High Energy Gathering"},
            new Plotpoint(){ tag = "A Rare or Unique Social Gathering"},
            new Plotpoint(){ tag = "Bad Decision"},
            new Plotpoint(){ tag = "This isn't Working"},
            new Plotpoint(){ tag = "Distraction"},

            new Plotpoint(){ tag = "Ill Will"},
            new Plotpoint(){ tag = "An Organisation"},
            new Plotpoint(){ tag = "Wanted by the Law"},
            new Plotpoint(){ tag = "A Resource Disappears"},
            new Plotpoint(){ tag = "It's Your Duty"},
            new Plotpoint(){ tag = "Fortuitous Find"},
            new Plotpoint(){ tag = "Character Connection Severed"},
            new Plotpoint(){ tag = "All is Revealed"},
            new Plotpoint(){ tag = "Humiliation"},

            new Plotpoint(){ tag = "People Behaving Badly"},
            new Plotpoint(){ tag = "Useful Information From a Known Source"},
            new Plotpoint(){ tag = "Cryptic Information From a Known Source"},
            new Plotpoint(){ tag = "Lie Discovered"},
            new Plotpoint(){ tag = "A Character is Attacked to Abduct"},
            new Plotpoint(){ tag = "Something Exotic"},
            new Plotpoint(){ tag = "Immediately"},
            new Plotpoint(){ tag = "Fame"},

            new Plotpoint(){ tag = "Chase"},
            new Plotpoint(){ tag = "Betrayal"},
            new Plotpoint(){ tag = "A Crime is Committed"},
            new Plotpoint(){ tag = "A Character is Incapacitated"},
            new Plotpoint(){ tag = "It's a Secret"},
            new Plotpoint(){ tag = "Something Lost has been Found"},
            new Plotpoint(){ tag = "Scapegoat"},
            new Plotpoint(){ tag = "Nowhere to Run"},
            new Plotpoint(){ tag = "At Night"},
            new Plotpoint(){ tag = "The Observer"},

            new Plotpoint(){ tag = "Escape"},
            new Plotpoint(){ tag = "A Secret Weapon"},
            new Plotpoint(){ tag = "Heavily Guarded"},
            new Plotpoint(){ tag = "Rescue"},
            new Plotpoint(){ tag = "Liar!"},
            new Plotpoint(){ tag = "Home Sweet Home"},
            new Plotpoint(){ tag = "A Character Acts out of Character"},
            new Plotpoint(){ tag = "Headquarters"},
            new Plotpoint(){ tag = "Physical Contest of Skills"},
            new Plotpoint(){ tag = "Dead"},

            new Plotpoint(){ tag = "A Common Social Gathering"},
            new Plotpoint(){ tag = "Light Urban Setting"},
            new Plotpoint(){ tag = "Mystery Solved"},
            new Plotpoint(){ tag = "A Work Related Gathering"},
            new Plotpoint(){ tag = "Family Matters"},
            new Plotpoint(){ tag = "Secret Information Leaked"},
            new Plotpoint(){ tag = "Suspicion"},
            new Plotpoint(){ tag = "Lose Lose"},
            new Plotpoint(){ tag = "A Figure From the Past"},

            new Plotpoint(){ tag = "Mass Battle"},
            new Plotpoint(){ tag = "Out in the Open"},
            new Plotpoint(){ tag = "Evidence"},
            new Plotpoint(){ tag = "A Character is Diminished"},
            new Plotpoint(){ tag = "The Plot Thickens"},
            new Plotpoint(){ tag = "Enemies"},
            new Plotpoint(){ tag = "Dubious Rationale"},
            new Plotpoint(){ tag = "Menacing Tone"},
            new Plotpoint(){ tag = "A Crucial Life Support System Begins to Fail"},
            new Plotpoint(){ tag = "Dense Urban Setting"},

            new Plotpoint(){ tag = "Doing the Right Thing"},
            new Plotpoint(){ tag = "Victory"},
            new Plotpoint(){ tag = "Taking Chances"},
            new Plotpoint(){ tag = "A Group is in Trouble"},
            new Plotpoint(){ tag = "Sole Survivor"},
            new Plotpoint(){ tag = "Token Response"},
            new Plotpoint(){ tag = "Cryptic Information From an Unknown Source"},
            new Plotpoint(){ tag = "A Common Thread"},

            new Plotpoint(){ tag = "A Problem Returns"},
            new Plotpoint(){ tag = "Stuck"},
            new Plotpoint(){ tag = "At your Mercy"},
            new Plotpoint(){ tag = "Stop That"},
            new Plotpoint(){ tag = "Not their Master"},
            new Plotpoint(){ tag = "Fall From Power"},
            new Plotpoint(){ tag = "Help is Offered, For a Price"},
            new Plotpoint(){ tag = "Public Location"},

            new Plotpoint(){ tag = "The Leader"},
            new Plotpoint(){ tag = "Prized Possession"},
            new Plotpoint(){ tag = "Saviour"},
            new Plotpoint(){ tag = "Disarmed"},
            new Plotpoint(){ tag = "The Secret To The Power"},
            new Plotpoint(){ tag = "Hidden Agenda"},
            new Plotpoint(){ tag = "Defend or Not to Defend"},
            new Plotpoint(){ tag = "Crash"},

            new Plotpoint(){ tag = "Reinforcements"},
            new Plotpoint(){ tag = "Government"},
            new Plotpoint(){ tag = "Physical Barrier to Overcome"},
            new Plotpoint(){ tag = "Injustice"},
            new Plotpoint(){ tag = "Quiet Catastrophe"},
            new Plotpoint(){ tag = "An Object of Unknown Use is Found"},
            new Plotpoint(){ tag = "It's all about You"},
            new Plotpoint(){ tag = "A Celebration"},
            new Plotpoint(){ tag = "Standoff"},
            new Plotpoint(){ tag = "Double Down"},

            new Plotpoint(){ tag = "Hidden Threat"},
            new Plotpoint(){ tag = "Character Connection"},
            new Plotpoint(){ tag = "Religion"},
            new Plotpoint(){ tag = "Innocence"},
            new Plotpoint(){ tag = "Clear the Record"},
            new Plotpoint(){ tag = "Willing to Talk"},
            new Plotpoint(){ tag = "Theft"},
            new Plotpoint(){ tag = "Character Harm"},

            new Plotpoint(){ tag = "A Need to Hide"},
            new Plotpoint(){ tag = "Followed"},
            new Plotpoint(){ tag = "Framed"},
            new Plotpoint(){ tag = "Preparation"},
            new Plotpoint(){ tag = "An Improbable Crime"},
            new Plotpoint(){ tag = "Friend Focus"},
            new Plotpoint(){ tag = "Untouchable"},
            new Plotpoint(){ tag = "Bribe"},
            new Plotpoint(){ tag = "Dealing with a Calamity"},
            new Plotpoint(){ tag = "Sudden Cessation"},

            new Plotpoint(){ tag = "It's a Trap!"},
            new Plotpoint(){ tag = "A Meeting of Minds"},
            new Plotpoint(){ tag = "Time Limit"},
            new Plotpoint(){ tag = "The Hidden Hand"},
            new Plotpoint(){ tag = "A Needed Resource is Running Short"},
            new Plotpoint(){ tag = "Organisations in Conflict"},
            new Plotpoint(){ tag = "Bad News"},
            new Plotpoint(){ tag = "Character Assistance"},
            new Plotpoint(){ tag = "Asking for Help"},

            new Plotpoint(){ tag = "Hunker Down"},
            new Plotpoint(){ tag = "Abandoned"},
            new Plotpoint(){ tag = "Find it Or Else"},
            new Plotpoint(){ tag = "Used Against Them"},
            new Plotpoint(){ tag = "Powerful Person"},
            new Plotpoint(){ tag = "Creepy Tone"},
            new Plotpoint(){ tag = "Welcome to the Plot"},
            new Plotpoint(){ tag = "Travel Setting"},

            new Plotpoint(){ tag = "Escort Duty"},
            new Plotpoint(){ tag = "An Old Deal"},
            new Plotpoint(){ tag = "A New Enemy"},
            new Plotpoint(){ tag = "Alliance"},
            new Plotpoint(){ tag = "Power over Others"},
            new Plotpoint(){ tag = "A Mysterious New Person"},
            new Plotpoint(){ tag = "Frenetic Activity"},
            new Plotpoint(){ tag = "Rural Setting"},

            new Plotpoint(){ tag = "Likable"},
            new Plotpoint(){ tag = "Someone is Where they Should Not Be"},
            new Plotpoint(){ tag = "Sneaky Barrier"},
            new Plotpoint(){ tag = "Corruption"},
            new Plotpoint(){ tag = "Vulnerability Exploited"},
            new Plotpoint(){ tag = "The Promise of Reward"},
            new Plotpoint(){ tag = "Fraud"},

            new Plotpoint(){ tag = "It's Business"},
            new Plotpoint(){ tag = "Just Cause Gone Awry"},
            new Plotpoint(){ tag = "Expert Knowledge"},
            new Plotpoint(){ tag = "A Moment of Peace"},
            new Plotpoint(){ tag = "A Focus on the Mundane"},
            new Plotpoint(){ tag = "Run Away!"},
            new Plotpoint(){ tag = "Beat You To It"},
            new Plotpoint(){ tag = "Confrontation"},

            new Plotpoint(){ tag = "Argument"},
            new Plotpoint(){ tag = "Social Tension Set to Boiling"},
            new Plotpoint(){ tag = "Protector"},
            new Plotpoint(){ tag = "Crescendo"},
            new Plotpoint(){ tag = "Destroy the Thing"},
            new Plotpoint(){ tag = "Conspiracy Theory"},
            new Plotpoint(){ tag = "Servant"},
            new Plotpoint(){ tag = "An Opposing Story"},
            new Plotpoint(){ tag = "Meta"},

            new Plotpoint(){ tag = "Character Exits the Adventure"},
            new Plotpoint(){ tag = "Character Returns"},
            new Plotpoint(){ tag = "Character Steps Up"},
            new Plotpoint(){ tag = "Character Steps Down"},
            new Plotpoint(){ tag = "Character Downgrade"},
            new Plotpoint(){ tag = "Character Upgrade"},
            new Plotpoint(){ tag = "Plotline Combo"}
        };

        Dictionary<string, Plotpoint> dictOfPlotpoints = ToolManager.i.toolDataScript.GetDictOfPlotpoints();
        if (dictOfPlotpoints != null)
        {
            Debug.LogFormat("[Tst] ToolDetails -> InitialisePlotpoints: There are {0} records in the listOfPlotponts{1}", listOfPlotpoints.Count, "\n");
            //convert list to dictionary
            dictOfPlotpoints = listOfPlotpoints.ToDictionary(k => k.tag);
            Debug.LogFormat("[Tst] ToolDetails -> InitialisePlotpoints: There are {0} records in the dictOfPlotponts{1}", dictOfPlotpoints.Count, "\n");
        }
        else { Debug.LogError("Invalid dictOfPlotpoints (Null)"); }


    }

}
