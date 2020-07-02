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
            new Plotpoint(){ tag = "Conclusion", refTag = "Conclusion"},
            new Plotpoint(){ tag = "None", refTag = "None"},
            new Plotpoint(){ tag = "Into the Unknown", refTag = "IntoTheUnknown"},
            new Plotpoint(){ tag = "A Character is attacked in a Non-Lethal way", refTag = "AttackedNonLethal"},
            new Plotpoint(){ tag = "A needed Resource runs out", refTag = "ResourceRunsOut"},
            new Plotpoint(){ tag = "Useful Information from an Uknown Source", refTag = "UsefulInfoUnknown"},
            new Plotpoint(){ tag = "Impending Doom", refTag = "ImpendingDoom"},
            new Plotpoint(){ tag = "Outcast", refTag = "Outcast"},
            new Plotpoint(){ tag = "Persuasion", refTag = "Persuasion"},
            new Plotpoint(){ tag = "A Motive Free Crime", refTag = "MotiveFreeCrime"},

            new Plotpoint(){ tag = "Collateral Damage", refTag = "CollateralDamage"},
            new Plotpoint(){ tag = "Shady Places", refTag = "ShadyPlaces"},
            new Plotpoint(){ tag = "A Character is Attacked in a Lethal Way", refTag = "LethalAttack"},
            new Plotpoint(){ tag = "Do it, or Else", refTag = "DoItOrElse"},
            new Plotpoint(){ tag = "Remote Location", refTag = "RemoteLocation"},
            new Plotpoint(){ tag = "Ambush", refTag = "Ambush"},
            new Plotpoint(){ tag = "Sold", refTag = "Sold"},
            new Plotpoint(){ tag = "Catastrophe", refTag = "Catastrophe"},
            new Plotpoint(){ tag = "Grisly Tone", refTag = "GrislyTone"},
            new Plotpoint(){ tag = "Character has a Clever Idea", refTag = "CleverIdea"},

            new Plotpoint(){ tag = "Something is Getting Away", refTag = "GettingAway"},
            new Plotpoint(){ tag = "Retaliation", refTag = "Retaliation"},
            new Plotpoint(){ tag = "A Character Disappears", refTag = "Disappears"},
            new Plotpoint(){ tag = "Hunted", refTag = "Hunted"},
            new Plotpoint(){ tag = "A High Energy Gathering", refTag = "HighEnergyGathering"},
            new Plotpoint(){ tag = "A Rare or Unique Social Gathering", refTag = "UniqueGathering"},
            new Plotpoint(){ tag = "Bad Decision", refTag = "BadDecision"},
            new Plotpoint(){ tag = "This isn't Working", refTag = "IsNotWorking"},
            new Plotpoint(){ tag = "Distraction", refTag = "Distraction"},

            new Plotpoint(){ tag = "Ill Will", refTag = "IllWill"},
            new Plotpoint(){ tag = "An Organisation", refTag = "Organisation"},
            new Plotpoint(){ tag = "Wanted by the Law", refTag = "WantedByTheLaw"},
            new Plotpoint(){ tag = "A Resource Disappears", refTag = "ResourceDisappears"},
            new Plotpoint(){ tag = "It's Your Duty", refTag = "YourDuty"},
            new Plotpoint(){ tag = "Fortuitous Find", refTag = "FortuitousFind"},
            new Plotpoint(){ tag = "Character Connection Severed", refTag = "ConnectionSevered"},
            new Plotpoint(){ tag = "All is Revealed", refTag = "AllRevealed"},
            new Plotpoint(){ tag = "Humiliation", refTag = "Humiliation"},

            new Plotpoint(){ tag = "People Behaving Badly", refTag = "BehavingBadly"},
            new Plotpoint(){ tag = "Useful Information From a Known Source", refTag = "UsefulInfoKnown"},
            new Plotpoint(){ tag = "Cryptic Information From a Known Source", refTag = "CrypticInfoKnown"},
            new Plotpoint(){ tag = "Lie Discovered", refTag = "LieDiscovered"},
            new Plotpoint(){ tag = "A Character is Attacked to Abduct", refTag = "Abduct"},
            new Plotpoint(){ tag = "Something Exotic", refTag = "SomethingExotic"},
            new Plotpoint(){ tag = "Immediately", refTag = "Immediately"},
            new Plotpoint(){ tag = "Fame", refTag = "Fame"},

            new Plotpoint(){ tag = "Chase", refTag = "Chase"},
            new Plotpoint(){ tag = "Betrayal", refTag = "Betrayal"},
            new Plotpoint(){ tag = "A Crime is Committed", refTag = "CrimeCommitted"},
            new Plotpoint(){ tag = "A Character is Incapacitated", refTag = "Incapacitated"},
            new Plotpoint(){ tag = "It's a Secret", refTag = "ItIsASecret"},
            new Plotpoint(){ tag = "Something Lost has been Found", refTag = "LostFound"},
            new Plotpoint(){ tag = "Scapegoat", refTag = "Scapegoat"},
            new Plotpoint(){ tag = "Nowhere to Run", refTag = "NowhereToRun"},
            new Plotpoint(){ tag = "At Night", refTag = "AtNight"},
            new Plotpoint(){ tag = "The Observer", refTag = "TheObserver"},

            new Plotpoint(){ tag = "Escape", refTag = "Escape"},
            new Plotpoint(){ tag = "A Secret Weapon", refTag = "SecretWeapon"},
            new Plotpoint(){ tag = "Heavily Guarded", refTag = "HeavilyGuarded"},
            new Plotpoint(){ tag = "Rescue", refTag = "Rescue"},
            new Plotpoint(){ tag = "Liar!", refTag = "Liar"},
            new Plotpoint(){ tag = "Home Sweet Home", refTag = "HomeSweetHome"},
            new Plotpoint(){ tag = "A Character Acts out of Character", refTag = "ActsOutOfCharacter"},
            new Plotpoint(){ tag = "Headquarters", refTag = "Headquarters"},
            new Plotpoint(){ tag = "Physical Contest of Skills", refTag = "PhysicalContest"},
            new Plotpoint(){ tag = "Dead", refTag = "Dead"},

            new Plotpoint(){ tag = "A Common Social Gathering", refTag = "CommonGathering"},
            new Plotpoint(){ tag = "Light Urban Setting", refTag = "LightUrban"},
            new Plotpoint(){ tag = "Mystery Solved", refTag = "MysterySolved"},
            new Plotpoint(){ tag = "A Work Related Gathering", refTag = "WorkGathering"},
            new Plotpoint(){ tag = "Family Matters", refTag = "FamilyMatters"},
            new Plotpoint(){ tag = "Secret Information Leaked", refTag = "SecretInfoLeaked"},
            new Plotpoint(){ tag = "Suspicion", refTag = "Suspicion"},
            new Plotpoint(){ tag = "Lose Lose", refTag = "LoseLose"},
            new Plotpoint(){ tag = "A Figure From the Past", refTag = "FigureFromPast"},

            new Plotpoint(){ tag = "Mass Battle", refTag = ""},
            new Plotpoint(){ tag = "Out in the Open", refTag = ""},
            new Plotpoint(){ tag = "Evidence", refTag = ""},
            new Plotpoint(){ tag = "A Character is Diminished", refTag = ""},
            new Plotpoint(){ tag = "The Plot Thickens", refTag = ""},
            new Plotpoint(){ tag = "Enemies", refTag = ""},
            new Plotpoint(){ tag = "Dubious Rationale", refTag = ""},
            new Plotpoint(){ tag = "Menacing Tone", refTag = ""},
            new Plotpoint(){ tag = "A Crucial Life Support System Begins to Fail", refTag = ""},
            new Plotpoint(){ tag = "Dense Urban Setting", refTag = ""},

            new Plotpoint(){ tag = "Doing the Right Thing", refTag = "RightThing"},
            new Plotpoint(){ tag = "Victory", refTag = "Victory"},
            new Plotpoint(){ tag = "Taking Chances", refTag = "TakingChances"},
            new Plotpoint(){ tag = "A Group is in Trouble", refTag = "GroupInTrouble"},
            new Plotpoint(){ tag = "Sole Survivor", refTag = "SoleSurvivor"},
            new Plotpoint(){ tag = "Token Response", refTag = "TokenResponse"},
            new Plotpoint(){ tag = "Cryptic Information From an Unknown Source", refTag = "CrypticInfoUnknown"},
            new Plotpoint(){ tag = "A Common Thread", refTag = "CommonThread"},

            new Plotpoint(){ tag = "A Problem Returns", refTag = "ProblemReturns"},
            new Plotpoint(){ tag = "Stuck", refTag = "Stuck"},
            new Plotpoint(){ tag = "At your Mercy", refTag = "AtYourMercy"},
            new Plotpoint(){ tag = "Stop That", refTag = "StopThat"},
            new Plotpoint(){ tag = "Not their Master", refTag = "NotTheirMaster"},
            new Plotpoint(){ tag = "Fall From Power", refTag = "FallFromPower"},
            new Plotpoint(){ tag = "Help is Offered, For a Price", refTag = "HelpOffered"},
            new Plotpoint(){ tag = "Public Location", refTag = "PublicLocation"},

            new Plotpoint(){ tag = "The Leader", refTag = "TheLeader"},
            new Plotpoint(){ tag = "Prized Possession", refTag = "PrizedPossession"},
            new Plotpoint(){ tag = "Saviour", refTag = "Saviour"},
            new Plotpoint(){ tag = "Disarmed", refTag = "Disarmed"},
            new Plotpoint(){ tag = "The Secret To The Power", refTag = "SecretPower"},
            new Plotpoint(){ tag = "Hidden Agenda", refTag = "HiddenAgenda"},
            new Plotpoint(){ tag = "Defend or Not to Defend", refTag = "DefendOrNot"},
            new Plotpoint(){ tag = "Crash", refTag = "Crash"},

            new Plotpoint(){ tag = "Reinforcements", refTag = "Reinforcements"},
            new Plotpoint(){ tag = "Government", refTag = "Government"},
            new Plotpoint(){ tag = "Physical Barrier to Overcome", refTag = "PhysicalBarrier"},
            new Plotpoint(){ tag = "Injustice", refTag = "Injustice"},
            new Plotpoint(){ tag = "Quiet Catastrophe", refTag = "QuietCatastrophe"},
            new Plotpoint(){ tag = "An Object of Unknown Use is Found", refTag = "ObjectUnknown"},
            new Plotpoint(){ tag = "It's all about You", refTag = "AllAboutYou"},
            new Plotpoint(){ tag = "A Celebration", refTag = "Celebration"},
            new Plotpoint(){ tag = "Standoff", refTag = "Standoff"},
            new Plotpoint(){ tag = "Double Down", refTag = "DoubleDown"},

            new Plotpoint(){ tag = "Hidden Threat", refTag = "HiddenThreat"},
            new Plotpoint(){ tag = "Character Connection", refTag = "Connection"},
            new Plotpoint(){ tag = "Religion", refTag = "Religion"},
            new Plotpoint(){ tag = "Innocence", refTag = "Innocence"},
            new Plotpoint(){ tag = "Clear the Record", refTag = "ClearRecord"},
            new Plotpoint(){ tag = "Willing to Talk", refTag = "WillingToTalk"},
            new Plotpoint(){ tag = "Theft", refTag = "Theft"},
            new Plotpoint(){ tag = "Character Harm", refTag = "CharacterHarm"},

            new Plotpoint(){ tag = "A Need to Hide", refTag = "NeedToHide"},
            new Plotpoint(){ tag = "Followed", refTag = "Followed"},
            new Plotpoint(){ tag = "Framed", refTag = "Framed"},
            new Plotpoint(){ tag = "Preparation", refTag = "Preparation"},
            new Plotpoint(){ tag = "An Improbable Crime", refTag = "ImprobableCrime"},
            new Plotpoint(){ tag = "Friend Focus", refTag = "FriendFocus"},
            new Plotpoint(){ tag = "Untouchable", refTag = "Untouchable"},
            new Plotpoint(){ tag = "Bribe", refTag = "Bribe"},
            new Plotpoint(){ tag = "Dealing with a Calamity", refTag = "Calamity"},
            new Plotpoint(){ tag = "Sudden Cessation", refTag = "SuddenCessation"},

            new Plotpoint(){ tag = "It's a Trap!", refTag = "ItIsATrap"},
            new Plotpoint(){ tag = "A Meeting of Minds", refTag = "MeetingOfMinds"},
            new Plotpoint(){ tag = "Time Limit", refTag = "TimeLimit"},
            new Plotpoint(){ tag = "The Hidden Hand", refTag = "HiddenHand"},
            new Plotpoint(){ tag = "A Needed Resource is Running Short", refTag = "NeededResource"},
            new Plotpoint(){ tag = "Organisations in Conflict", refTag = "OrgsInConflict"},
            new Plotpoint(){ tag = "Bad News", refTag = "Bad News"},
            new Plotpoint(){ tag = "Character Assistance", refTag = "CharAssistance"},
            new Plotpoint(){ tag = "Asking for Help", refTag = "AskingForHelp"},

            new Plotpoint(){ tag = "Hunker Down", refTag = "HunkerDown"},
            new Plotpoint(){ tag = "Abandoned", refTag = "Abandoned"},
            new Plotpoint(){ tag = "Find it Or Else", refTag = "FindItOrElse"},
            new Plotpoint(){ tag = "Used Against Them", refTag = "UsedAgainstThem"},
            new Plotpoint(){ tag = "Powerful Person", refTag = "PowerfulPerson"},
            new Plotpoint(){ tag = "Creepy Tone", refTag = "CreepyTone"},
            new Plotpoint(){ tag = "Welcome to the Plot", refTag = "WelcomePlot"},
            new Plotpoint(){ tag = "Travel Setting", refTag = "TravelSetting"},

            new Plotpoint(){ tag = "Escort Duty", refTag = "EscortDuty"},
            new Plotpoint(){ tag = "An Old Deal", refTag = "OldDeal"},
            new Plotpoint(){ tag = "A New Enemy", refTag = "NewEnemy"},
            new Plotpoint(){ tag = "Alliance", refTag = "Alliance"},
            new Plotpoint(){ tag = "Power over Others", refTag = "PowerOthers"},
            new Plotpoint(){ tag = "A Mysterious New Person", refTag = " MysteriousPerson"},
            new Plotpoint(){ tag = "Frenetic Activity", refTag = "FreneticActivity"},
            new Plotpoint(){ tag = "Rural Setting", refTag = "RuralSetting"},

            new Plotpoint(){ tag = "Likable", refTag = "Likable"},
            new Plotpoint(){ tag = "Someone is Where they Should Not Be", refTag = "ShouldNotBe"},
            new Plotpoint(){ tag = "Sneaky Barrier", refTag = "SneakyBarrier"},
            new Plotpoint(){ tag = "Corruption", refTag = "Corruption"},
            new Plotpoint(){ tag = "Vulnerability Exploited", refTag = "VulnerableExploit"},
            new Plotpoint(){ tag = "The Promise of Reward", refTag = "PromiseOfReward"},
            new Plotpoint(){ tag = "Fraud", refTag = "Fraud"},

            new Plotpoint(){ tag = "It's Business", refTag = "ItIsBusiness"},
            new Plotpoint(){ tag = "Just Cause Gone Awry", refTag = "JustCause"},
            new Plotpoint(){ tag = "Expert Knowledge", refTag = "ExpertKnowledge"},
            new Plotpoint(){ tag = "A Moment of Peace", refTag = "MomentOfPeace"},
            new Plotpoint(){ tag = "A Focus on the Mundane", refTag = "FocusMundane"},
            new Plotpoint(){ tag = "Run Away!", refTag = "RunAway"},
            new Plotpoint(){ tag = "Beat You To It", refTag = "BeatYouToIt"},
            new Plotpoint(){ tag = "Confrontation", refTag = "Confrontation"},

            new Plotpoint(){ tag = "Argument", refTag = "Argument"},
            new Plotpoint(){ tag = "Social Tension Set to Boiling", refTag = "SocialTension"},
            new Plotpoint(){ tag = "Protector", refTag = "Protector"},
            new Plotpoint(){ tag = "Crescendo", refTag = "Crescendo"},
            new Plotpoint(){ tag = "Destroy the Thing", refTag = "DestroyThing"},
            new Plotpoint(){ tag = "Conspiracy Theory", refTag = "ConspiracyTheory"},
            new Plotpoint(){ tag = "Servant", refTag = "Servant"},
            new Plotpoint(){ tag = "An Opposing Story", refTag = "OpposingStory"},
            new Plotpoint(){ tag = "Meta", refTag = "Meta"},

            new Plotpoint(){ tag = "Character Exits the Adventure", refTag = "CharExits"},
            new Plotpoint(){ tag = "Character Returns", refTag = "CharReturns"},
            new Plotpoint(){ tag = "Character Steps Up", refTag = "CharStepsUp"},
            new Plotpoint(){ tag = "Character Steps Down", refTag = "CharStepsDown"},
            new Plotpoint(){ tag = "Character Downgrade", refTag = "CharDowngrade"},
            new Plotpoint(){ tag = "Character Upgrade", refTag = "CharUpgrade"},
            new Plotpoint(){ tag = "Plotline Combo", refTag = "PlotlineCombo"}
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
