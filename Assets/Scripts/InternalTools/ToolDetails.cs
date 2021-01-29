using System.Collections.Generic;
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
        InitialisePlotpointLookup();
        InitialiseMetaPlotpointLookup();
        InitialiseCharacterSpecial();
        InitialiseCharacterIdentity();
        InitialiseCharacterDescriptors();
        InitialiseCharacterGoal();
        InitialiseCharacterMotivation();
        InitialiseCharacterFocus();
        //organisations
        InitialiseOrganisationType();
        InitialiseOrganisationOrigin();
        InitialiseOrganisationHistory();
        InitialiseOrganisationLeadership();
        InitialiseOrganisationMotivation();
        InitialiseOrganisationMethod();
        InitialiseOrganisationStrength();
        InitialiseOrganisationWeakness();
        InitialiseOrganisationObstacle();
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
                type = PlotPointType.Conclusion,
                special = SpecialType.None,
                details = " If this Turning Point is currently a Plotline Development, then it becomes a Plotline Conclusion. Incorporate anything necessary into this Turning Point to end this Plotline " +
                "and remove it from the Plotlines List. If this Turning Point is a New Plotline or already a Conclusion, then consider this Plot Point a None"
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
                type = PlotPointType.None,
                special = SpecialType.None,
                details = "Leave this Plot Point blank and go on to the next Plot Point, unless it would leave you with fewer than 2 Plot Points in this Turning Point, in which case re-roll."
            },
            new Plotpoint(){
                tag = "Into the Unknown",
                refTag = "IntoTheUnknown",
                listAction = new List<int>(){},
                listTension = new List<int>(){25,26},
                listMystery = new List<int>(){25,26},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves Characters entering a situation with unknown factors. To know the unknown, you have to commit to it. For instance, a magic portal where " +
                "there is no way of knowing what’s on the other side except by walking through it. Or, you discover a machine that is very powerful but you have no idea what it does, except if " +
                "you turn it on.The only way to discover the unknown is to engage it, when it will be too late if you regret it."
            },
            new Plotpoint(){
                tag = "A Character is attacked in a Non-Lethal way",
                refTag = "AttackedNonLethal",
                listAction = new List<int>(){25,26},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is attacked, but the assailant will not attack to kill"
            },
            new Plotpoint(){
                tag = "A needed Resource runs out",
                refTag = "ResourceRunsOut",
                listAction = new List<int>(){},
                listTension = new List<int>(){27},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A resource a Character needs has run out. The lack will cause problems. For instance, traveling a dinosaur filled jungle and running out of ammunition"
            },
            new Plotpoint(){
                tag = "Useful Information from an Unknown Source",
                refTag = "UsefulInfoUnknown",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){27,28},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character receives useful information from an anonymous source. Perhaps a note is found laying on your doorstep, or an email appears in your inbox with a " +
                "photo that reveals something to the Character. Whatever the information is, it should impact the Plotline"
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Something terrible is going to happen, and it is approaching. For instance, an enemy army is advancing to invade and will be at the borders in a week."
            },
            new Plotpoint(){
                tag = "Outcast",
                refTag = "Outcast",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){25,26},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is considered an outcast by other Characters for some reason. Maybe the Character is part of an ethnic group that is disliked in the area, " +
                "or perhaps the Character is popularly believed to be the perpetrator of a heinous crime"
            },
            new Plotpoint(){
                tag = "Persuasion",
                refTag = "Persuasion",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){25,26},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character tries to persuade another Character to do something.This persuasion can take many forms, from pleading with them to threatening them, for instance."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A crime is committed either in this Turning Point or is learned about in this Turning Point, with no clear reason why the crime was committed. Maybe someone " +
                "was murdered for no obvious reason, or a building was broken into with nothing stolen."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever is going on in this Turning Point, the activity will spill over from the focus of that activity to things around it. This is particularly true for " +
                "damaging events. For instance, a superhero defeats a villain in a downtown brawl, but doing significant damage to the buildings around them in the process.The collateral " +
                "damage does not have to be physical.For instance, it could be the legal fallout from a major court decision."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a location that is less than legitimate, such as a back alley where drug deals are commonly transacted or a secret gambling hall in a bar."
            },
            new Plotpoint(){
                tag = "A Character is Attacked in a Lethal Way",
                refTag = "LethalAttack",
                listAction = new List<int>(){28,29},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "An assailant is trying to kill a Character."
            },
            new Plotpoint(){
                tag = "Do it, or Else",
                refTag = "DoItOrElse",
                listAction = new List<int>(){},
                listTension = new List<int>(){33},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){27},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is being given a task, and is being pressured into completing the task with a threat. For instance, a spy is forcing a diplomat to " +
                "hand over technology secrets or he will expose the diplomat’s illegal activities and send him to jail. Of course, probably the most common form of this Plot Point " +
                "is “Do this or I will kill you”."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a remote location, such as a cave or a cabin in the woods."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever is happening in this Turning Point involves sudden action at an unexpected time."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a sale of some kind. Maybe goods are being sold, or information is being bought. Whatever is happening, goods and money are exchanging hands."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Just about the worst thing that can happen does happen, and it happens spectacularly and with much action. This could be the impregnable fortress that gets sacked, " +
                "the unstoppable superhero who gets defeated, the unsinkable ship that starts to sink."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever is going on in this Turning Point, the tone of it is grisly, something that causes horror or disgust. For instance, if a note is discovered with a " +
                "grisly tone it may be smeared in blood or be accompanied by a severed hand."
            },
            new Plotpoint(){
                tag = "Character has a Clever Idea",
                refTag = "CleverIdea",
                listAction = new List<int>(){33},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character has an idea that has an impact on this Turning Point. For instance, the con man speaks up and just happens to know a secret way " +
                "through the sewers into the walled city."
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
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a time limit where, at the end of it, something will get away. For instance, a ship carrying a magic artifact is " +
                "about to leave the dock and a Character has to fight their way through a pack of armed goons to board the ship before it sets sail."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever is happening in this Turning Point, it involves an element of retaliation or revenge"
            },
            new Plotpoint(){
                tag = "A Character Disappears",
                refTag = "Disappears",
                listAction = new List<int>(){},
                listTension = new List<int>(){40},
                listMystery = new List<int>(){31,32},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = " A Character is nowhere to be found. Whether there is evidence or not as to what happened to the Character is up to you depending on the other Plot Points involved in this Turning Point."
            },
            new Plotpoint(){
                tag = "Hunted",
                refTag = "Hunted",
                listAction = new List<int>(){35,36},
                listTension = new List<int>(){41},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is being hunted by someone or something that is not strictly legitimate. In other words, as opposed to Wanted By The Law, Hunted may mean a hit man is pursuing a " +
                "Character to fulfill a mafia contract on them, or a ghost may be after a Character. The hunter doesn’t have to be seeking to kill."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a social gathering with a great deal of energy or activity. This could be a busy nightclub, a loud party, or a sporting event, for instance."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This is a social gathering for a specific and rare purpose.Examples would include funerals or a wedding."
            },
            new Plotpoint(){
                tag = "Bad Decision",
                refTag = "BadDecision",
                listAction = new List<int>(){},
                listTension = new List<int>(){42},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){29},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A decision a Character made has turned out to be a very bad one. This can be a decision made earlier in the Adventure, or it can be something from before the Adventure. " +
                "This earlier decision may not have seemed like a bad one at the time, but it has turned out to be bad, either for the Character, for others, or both.For instance, maybe a " +
                "ship’s captain decided to investigate a distress beacon in deep space, only to find it’s a trap laid by pirates."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Something that is supposed to be working is not for some reason, causing a problem.For instance, a binding spell is failing to hold a demon, " +
                "or a crime boss is delivering stolen goods through a shipping port that is supposed to be secure but turns out to be swarming with police. Whatever isn’t  " +
                "working is something that was assumed would work."
            },
            new Plotpoint(){
                tag = "Distraction",
                refTag = "Distraction",
                listAction = new List<int>(){37},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is distracted in this Turning Point in such a way that it impacts events. For instance, before a villain delivers his killing blow " +
                "he’s distracted by an image of his lost love, giving the hero time to escape."
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
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character harbors ill will toward another Character for some reason. The animosity should be deep seated and color the Character’s reactions " +
                "when it comes to the unliked Character. The dislike may be reciprocated or not."
            },
            new Plotpoint(){
                tag = "An Organisation",
                refTag = "Organisation",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){33,34},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.Organisation,
                details = "This Turning Point involves an organization of some kind. This can be an organization already in the Characters List or not. Whatever is happening " +
                "in this Turning Point, the organization is formally involved in some way. For instance, a crime has been committed and a local guild had knowledge of it and " +
                "covered it up to protect its own interests."
            },
            new Plotpoint(){
                tag = "Wanted by the Law",
                refTag = "WantedByTheLaw",
                listAction = new List<int>(){},
                listTension = new List<int>(){43},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){32,33},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is wanted for a crime. It doesn’t matter if they actually did the crime, but the law is after them as the main suspect either way."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "An important object or resource is stolen by an unknown thief. The resource should be something either useful to a Character, or it should pertain to the Plotline in question."
            },
            new Plotpoint(){
                tag = "It's Your Duty",
                refTag = "YourDuty",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){34,35},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is charged with carrying out a duty. This should be something that the Character has little choice in the matter, whether they want to do it or not. Whoever " +
                "the duty is coming from, that source has authority over the Character. For instance, a soldier wants to join in the pivotal battle but his commander gives him the duty of guarding " +
                "the fortress gate instead"
            },
            new Plotpoint(){
                tag = "Fortuitous Find",
                refTag = "FortuitousFind",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){36},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character runs across something very useful for resolving the Plotline.This may be a piece of information, a useful tool, a resource that is needed, a person who can " +
                "help, etc.Whatever it is, it’s the right thing at the right time, and it falls into the Character’s lapA Character runs across something very useful for resolving the Plotline. " +
                "This may be a piece of information, a useful tool, a resource that is needed, a person who can help, etc.Whatever it is, it’s the right thing at the right time, and it falls into the Character’s lap"
            },
            new Plotpoint(){
                tag = "Character Connection Severed",
                refTag = "ConnectionSevered",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){36,37},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character who has a connection with another Character severs that connection. This can happen for any of a number of reasons, from the Character dropping out of " +
                "the story to the Character getting angry at the other Character for something.The severed connection does not have to be permanent."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A source in this Turning Point gives a lot of detail about something. For instance, a guard is captured and tells where the king has hidden the Sacred Scrolls."
            },
            new Plotpoint(){
                tag = "Humiliation",
                refTag = "Humiliation",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){38},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a Character being humiliated or facing humiliation. Whatever is happening, it should be something deeply embarrassing to the Character. " +
                "For instance, a member of an unpopular community is being bullied and mocked, or a public figure has something personal publicly exposed"
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves someone behaving in a socially unacceptable way. For example, a group of drunks throwing bottles, or a heckler in a crowd yelling at a speaker."
            },
            new Plotpoint(){
                tag = "Useful Information From a Known Source",
                refTag = "UsefulInfoKnown",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){38,39},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character acquires useful information from a known source. For instance, a detective investigating a homicide gets a tip from an informant she sometimes uses, giving her a clue."
            },
            new Plotpoint(){
                tag = "Cryptic Information From a Known Source",
                refTag = "CrypticInfoKnown",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){40},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character acquires information that is not immediately useful from a known source. The information is cryptic, the Character doesn’t know what it means. " +
                "For instance, a crewmember leaves behind a note to be found that simply says, “Kraton,” where the Character receiving the note has no idea what “Kraton” is."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves the discovery of a lie. The lie could have happened within this Turning Point, or it could have happened earlier in the Adventure " +
                "or even before the Adventure. For instance, Characters may learn that the detective did not destroy the cult artifact like he said he did, but instead took it home to " +
                "try and summon the Beast From Beyond."
            },
            new Plotpoint(){
                tag = "A Character is Attacked to Abduct",
                refTag = "Abduct",
                listAction = new List<int>(){38,39},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "An assailant is attempting to abduct a Character."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever is happening in this Turning Point it involves an unusual or exotic element. For instance, if the Turning Point is about someone being attacked by " +
                "an assassin, the assassin may have a very unusual identity or mode of attack (maybe he’s disguised as a clown and attacks with exploding balloons, or " +
                "he is a martial artist with fantastic moves)."
            },
            new Plotpoint(){
                tag = "Immediately",
                refTag = "Immediately",
                listAction = new List<int>(){41,42},
                listTension = new List<int>(){45},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Immediate action is required in this Turning Point, whatever is going on. For instance, if this Turning Point involves engine failure on a starship, " +
                "the Character doesn’t have days to resolve the issue, he may only have an hour. Whatever is going on, it requires immediate action."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever is happening in this Turning Point involves someone famous to some extent. This doesn’t necessarily mean that a famous Character is Invoked, " +
                "just that the Turning Point has some connection to fame. For instance, if this Turning Point involves learning a secret about another Character, you may learn that " +
                "they were once a member of a famous superhero group decades ago"
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
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a chase, where one Character is pursuing another"
            },
            new Plotpoint(){
                tag = "Betrayal",
                refTag = "Betrayal",
                listAction = new List<int>(){},
                listTension = new List<int>(){46},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){39,40},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = " A Character, who was thought to be an ally or to be benign, turns on another Character. This can be a fundamental betrayal, such as they are " +
                "actually on opposing sides, or it can be a momentary betrayal, such as attacking someone out of a fit of anger."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A crime is committed either in this Turning Point or is learned about in this Turning Point"
            },
            new Plotpoint(){
                tag = "A Character is Incapacitated",
                refTag = "Incapacitated",
                listAction = new List<int>(){},
                listTension = new List<int>(){47},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){41,42},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is rendered out of commission for some reason. Perhaps they are wounded badly, they lose their powers, are trapped somewhere, etc."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves an activity that is done in secret, such as smuggling or embezzlement. The activity doesn’t have to be illegal, " +
                "but whatever it is, it is something hidden or being done behind an otherwise legitimate front. For instance, a fast food chain is using it’s delivery " +
                "trucks to smuggle drugs across the border."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Something that has been lost turns up in this Turning Point. The thing could have been lost in this Adventure or before. " +
                "It can be an object, a person, or anything. For instance, a ring of power suddenly turns up in a creek bed, or a Character who disappeared early in the Adventure suddenly makes a reappearance."
            },
            new Plotpoint(){
                tag = "Scapegoat",
                refTag = "Scapegoat",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){37},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves an innocent Character accused of wrongdoing to throw suspicion off of the real culprit. For instance, the woman who took all the ammo blames the " +
                "newcomer to the zombie survivalist group, or the mayor of the little New England town blames the practitioners of a religion for the bizarre events going on when he is actually at fault."
            },
            new Plotpoint(){
                tag = "Nowhere to Run",
                refTag = "NowhereToRun",
                listAction = new List<int>(){},
                listTension = new List<int>(){48},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character faces a peril with no means to escape."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point takes place at night"
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever is happening in this Turning Point that is presumed to be private from someone, is actually being witnessed or observed. " +
                "The observed are not aware of this observer.For instance, two enemy generals are meeting in secret to form an alliance and betray their respective kings, " +
                "but the meeting is observed by a princess who knows exactly what it means."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves an escape of some sort. For instance, a Character who was captured by brigands in an earlier " +
                "Turning Point manages to slip away from his captors and escape into the forest"
            },
            new Plotpoint(){
                tag = "A Secret Weapon",
                refTag = "SecretWeapon",
                listAction = new List<int>(){},
                listTension = new List<int>(){51},
                listMystery = new List<int>(){50},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves the reveal of a secret weapon in possession by a Character. This weapon should be significant " +
                "enough to sway the balance of power or to otherwise require a solution to resolve. For instance, the motley band of orcs is unexpectedly " +
                "backed by a large ogre whose aid they enlisted. Or, the galactic empire unveils a new, planet-busting warship that changes everything"
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves entering a heavily guarded and dangerous location. For instance, this could be needing to infiltrate " +
                "a high tech security facility to steal information, or breaking into the necromancers lair full of guardian zombies to destroy his magic crystal."
            },
            new Plotpoint(){
                tag = "Rescue",
                refTag = "Rescue",
                listAction = new List<int>(){49,50},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character needs to be rescued in this Turning Point."
            },
            new Plotpoint(){
                tag = "Liar!",
                refTag = "Liar",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){51,52},
                listSocial = new List<int>(){39},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves an active lie. The lie is being committed in this Turning Point. Something someone said or claimed is false. " +
                "For instance, a vampire lord claims he knows nothing of a magic book, when actually he is seeking it himself. The lie may or may not be detected in this Turning Point."
            },
            new Plotpoint(){
                tag = "Home Sweet Home",
                refTag = "HomeSweetHome",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){44,45},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point takes place in the private home of a Character"
            },
            new Plotpoint(){
                tag = "A Character Acts out of Character",
                refTag = "ActsOutOfCharacter",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){53},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character does something that runs counter to that Character’s perceived goals or personality. The action may seem at odds to how they’ve been acting " +
                "(such as a trusted member of a team sabotaging a crucial resource) or the action is vague with no discernible purpose (such as a Character meeting with an unknown person in secret)."
            },
            new Plotpoint(){
                tag = "Headquarters",
                refTag = "Headquarters",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){40,41},
                listPersonal = new List<int>(){46},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A setting in this Turning Point is a Character’s main headquarters. For instance, it may be the ritzy bar where the mob boss runs his empire, or the wizard’s wilderness tower."
            },
            new Plotpoint(){
                tag = "Physical Contest of Skills",
                refTag = "PhysicalContest",
                listAction = new List<int>(){51,52},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves Characters squaring off against each other in a physical contest of skills. This can be anything such as combat, a sporting event, duel, arm wrestling contest, etc."
            },
            new Plotpoint(){
                tag = "Dead",
                refTag = "Dead",
                listAction = new List<int>(){},
                listTension = new List<int>(){53},
                listMystery = new List<int>(){54},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,        //needs to be zero, character removal handled by GetPlotPoint
                type = PlotPointType.RemoveCharacter,
                special = SpecialType.None,
                details = "A Character is dead. This can either be expected or unexpected, but whatever the circumstances, this Turning Point involves a dead Character."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a social gathering. This can be any gathering of people, generally for a common purpose, such as gathering for " +
                "dinner at a home or restaurant, or an afternoon at a mall.The social gathering itself should be considered of a mundane nature, although what else " +
                "transpires at the gathering doesn’t necessarily have to be."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point takes place in a light urban setting, such as a small town or village."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A mystery is solved. This can be a large, unanswered question in the Adventure or something minor, but it is not a Plotline resolved unless " +
                "this Turning Point is also a Plotline Conclusion. A Mystery Solved could be any number of things, from finally figuring out what a device does to locating the missing Chancellor."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This is a social gathering that involves professionals or workers. The gathering itself may or may not involve their actual work. For instance, police officers gathering " +
                "at a “cop bar” or a team of super heroes gathering at their headquarters would both count."
            },
            new Plotpoint(){
                tag = "Family Matters",
                refTag = "FamilyMatters",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){47,48},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a family member or members of a Character. For instance, an occult investigator is about to head off on a mission when his sister " +
                "unexpectedly appears on his doorstep, or one of the Characters has an uncle who is a feudal lord and is summoning them for their help in defending his land because no one else will stand by him."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = " Information that should not have gotten into the wrong hands has. For instance, outlaws always seem to know when the stagecoach " +
                "is coming through Gateway Gulch with the railroad payroll. How are they finding out? Or, an enemy spy has learned of the realm’s secret military plans."
            },
            new Plotpoint(){
                tag = "Suspicion",
                refTag = "Suspicion",
                listAction = new List<int>(){},
                listTension = new List<int>(){54},
                listMystery = new List<int>(){58,59},
                listSocial = new List<int>(){48},
                listPersonal = new List<int>(){},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a Character being suspicious of another Character for some reason. For instance, a beloved leader on a " +
                "space station is murdered and suddenly every newcomer on board is viewed with suspicion."
            },
            new Plotpoint(){
                tag = "Lose Lose",
                refTag = "LoseLose",
                listAction = new List<int>(){},
                listTension = new List<int>(){55},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 0,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a choice where both or all options are bad in some way"
            },
            new Plotpoint(){
                tag = "A Figure From the Past",
                refTag = "FigureFromPast",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){49},
                numberOfCharacters = 1,
                type = PlotPointType.NewCharacter,
                special = SpecialType.None,
                details = "A new Character joins the Adventure, someone from a Character’s past. This Plot Point requires a new Character to be added to the Characters List and Invoked."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a combat between many combatants. This can be a throw down between two teams or a battle in a war, for instance."
            },
            new Plotpoint(){
                tag = "Out in the Open",
                refTag = "OutInTheOpen",
                listAction = new List<int>(){},
                listTension = new List<int>(){56},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever is happening in this Turning Point, it is happening out in the open for all to see.For instance, a Character is attacked at a " +
                "public festival in the middle of the day, or, something a Character is doing that they thought is private is actually being filmed and viewed by others."
            },
            new Plotpoint(){
                tag = "Evidence",
                refTag = "Evidence",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){60,61},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = " A Character finds something that helps settle an existing question. For instance, the gun that killed a victim is found stashed under a suspect’s bed."
            },
            new Plotpoint(){
                tag = "A Character is Diminished",
                refTag = "CharDiminished",
                listAction = new List<int>(){},
                listTension = new List<int>(){57,58},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){50,51},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is reduced in some way that makes them less effective. Perhaps they are wounded, or their energy is low, or they lose some authority, etc. " +
                "The Character is not entirely powerless, but loses a significant portion of their power or utility."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A promising lead or clue to solving an open question turns out to be a dead end. For instance, Characters follow through on a " +
                "tip to go to a warehouse to find an abducted heiress, but instead of finding a nest of bad guys they just find an empty building."
            },
            new Plotpoint(){
                tag = "Enemies",
                refTag = "Enemies",
                listAction = new List<int>(){},
                listTension = new List<int>(){59},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){49},
                listPersonal = new List<int>(){52,53},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves enemies of a Character. Whatever activity is going on in this Turning Point, those enemies play an important role."
            },
            new Plotpoint(){
                tag = "Dubious Rationale",
                refTag = "DubiousRationale",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){64},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character does something that is in keeping with their Character, but the action could also have been for another reason and it is not " +
                "clear which reason the Character acted on.For instance, the CEO goes into his office late at night, as he sometimes does, on the same night another " +
                "executive is murdered.The action should seem innocent, except for other events or information that cast doubt on it."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a menacing tone of some kind. For instance, one Character may be threatening another Character, or a " +
                "villain may be gloating over a captured opponent."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This can be an actual life support system, like the oxygen ventilation of a starship, or a safety system, like the brakes on a car. " +
                "The failure will constitute an emergency for the Characters involved."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point takes place in a heavily urban area, such as a large city."
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
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character who is acting in bad faith in some way has a change of heart and decides to do the right thing. For instance, " +
                "a con man stealing medicine from a diseased community decides he can’t leave all those people to die."
            },
            new Plotpoint(){
                tag = "Victory",
                refTag = "Victory",
                listAction = new List<int>(){56,57},
                listTension = new List<int>(){62},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character achieves a victory over another Character in this Turning Point. For instance, a band of marauders successfully " +
                "waylay the king’s couriers, or a hacker worms his way into a corporate computer system."
            },
            new Plotpoint(){
                tag = "Taking Chances",
                refTag = "TakingChances",
                listAction = new List<int>(){58,59},
                listTension = new List<int>(){63},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character acts in a very risky way. For instance, a Character may suddenly show no regard for their life as they walk out " +
                "across a narrow beam above a valley to save a friend. Or, the villain you are fighting takes a drug that makes him go into a battle " +
                "frenzy where he loses all caution."
            },
            new Plotpoint(){
                tag = "A Group is in Trouble",
                refTag = "GroupInTrouble",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){52,53},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A group, such as a community, is in trouble in this Plot Point. The group or community is facing a difficulty. For instance, " +
                "maybe a village is being harassed by monsters, or a corporation is facing a lawsuit that could destroy it. Whatever the trouble is, " +
                "it should be something that can be solved and will likely constitute a problem for a Character"
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves some kind of process of elimination where there is only one left. This can be a battle, " +
                "but doesn’t have to be. For instance, maybe a sinking ship has a single survivor who washes up on shore, or a group of crewmen " +
                "from a starship playing chess with an alien intelligence is down to their last crewmember who is now chosen for the alien’s ultimate challenge."
            },
            new Plotpoint(){
                tag = "Token Response",
                refTag = "TokenResponse",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){54},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.OrgOrChar,
                details = "A Character or organization acting in this Turning Point does the bare minimum to address a problem, or makes just a token effort, " +
                "as opposed to doing something truly effective. For instance, a notorious space pirate has been captured, but instead of receiving serious " +
                "prison time, the federation government goes very lenient on him and releases him from prison in a week"
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Information that is unclear what it means is received from an anonymous source. Maybe an odd word is found scrawled on a mirror, or a " +
                "stranger’s diary is found talking about events similar to the Plotline."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "It is learned that events that appeared to be unrelated have a commonality after all. For instance, a rash of crimes has beset the city, " +
                "from car jackings to break ins.It turns out the culprits all work as security guards in the same building."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A problem that had been thought resolved returns in some fashion. This can be a problem from this Adventure, from a previous Adventure, " +
                "or something inferred from the past. For instance, a kingdom may be enjoying a decade of peace following the vanquishing of the Dark Lord, " +
                "but it is discovered that he is not dead and is now returning. The magnitude of the problem is open to interpretation and can range from large to minor, " +
                "such as a previously sealed leak in a boat has sprung open again."
            },
            new Plotpoint(){
                tag = "Stuck",
                refTag = "Stuck",
                listAction = new List<int>(){},
                listTension = new List<int>(){67,68},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is stuck in this Turning Point, unable to act, while the events of the Turning Point transpire. Whatever has them stuck is not " +
                "necessarily permanent, but at the moment it renders them powerless or mostly powerless.For instance, maybe the character is bound or trapped in a jail cell."
            },
            new Plotpoint(){
                tag = "At your Mercy",
                refTag = "AtYourMercy",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){55,56},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is helpless and desperate for some reason, and must rely on the mercy of another Character who has the power to address their problem. " +
                "For instance, a Character is afflicted with a magical curse that only one sorcerer can cure"
            },
            new Plotpoint(){
                tag = "Stop That",
                refTag = "StopThat",
                listAction = new List<int>(){62,63},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character takes action to stop something from happening in this Turning Point. The action could be expected, such as a hero putting an " +
                "arrow through the executioner before he drops his axe. Or, it could be unexpected, like a Character suddenly shooting a captured villain right " +
                "before he was about to reveal crucial details."
            },
            new Plotpoint(){
                tag = "Not their Master",
                refTag = "NotTheirMaster",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){70},
                listSocial = new List<int>(){55},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character in this Turning Point who is assumed to be working for one source turns out to be working for another.For instance, " +
                "the hitman who’s been trying to kill a Character doesn’t work for the mafia like you thought, but for a corporation who has an interest in that Character."
            },
            new Plotpoint(){
                tag = "Fall From Power",
                refTag = "FallFromPower",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){57,58},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character loses their power in this Turning Point. For instance, a king is found to be a fraud by his brother, " +
                "who asserts his own claim to the throne and takes it."
            },
            new Plotpoint(){
                tag = "Help is Offered, For a Price",
                refTag = "HelpOffered",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){59,60},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character offers to help another Character in exchange for something. What’s being asked for could be anything, from mutual aid to a fee. " +
                "Whatever the price, it should be steep enough to be of real significance to the paying Character."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a public location, such as a town square or a park in the middle of the day."
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
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.OrgOrChar,
                details = " This Turning Point involves the leader of someone or some organization."
            },
            new Plotpoint(){
                tag = "Prized Possession",
                refTag = "PrizedPossession",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){61,62},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever is happening in this Turning Point, it involves an important possession of a Character. " +
                "For instance, if the Turning Point is about something being stolen, maybe a sorcerer’s magic staff is taken."
            },
            new Plotpoint(){
                tag = "Saviour",
                refTag = "Saviour",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){60,61},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is involved in this Turning Point who offers to save the day."
            },
            new Plotpoint(){
                tag = "Disarmed",
                refTag = "Disarmed",
                listAction = new List<int>(){},
                listTension = new List<int>(){69,70},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){63},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character loses their primary method of defending themselves. This could mean the loss of a weapon, " +
                "or maybe a powerful bureaucrat is powerless in another’s kingdom, etc. The disarmament should be temporary for the " +
                "Turning Point and deprive the Character of crucial defenses."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "There is a power, and it has a secret source. For instance, an evil wizard may derive his abilities from " +
                "his ancient staff, or the warship hurtling through space may be dependent on a simple power core inside that will " +
                "cripple the ship if it is damaged. This secret gives Characters an option to stop an otherwise overwhelming or powerful problem"
            },
            new Plotpoint(){
                tag = "Hidden Agenda",
                refTag = "HiddenAgenda",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){73,74},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character either reveals, or is found out to have, a motive that they had not previously exposed. For instance, " +
                "maybe the detective isn’t investigating the murder out of dedication to his job, but the victim used to be a love interest of his. " +
                "Classically, this can also be the ally who turns out to be an enemy. The hidden agenda doesn’t have to be something nefarious, " +
                "although it can be. Whichever the case, the agenda now becomes known to others."
            },
            new Plotpoint(){
                tag = "Defend or Not to Defend",
                refTag = "DefendOrNot",
                listAction = new List<int>(){64,65},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a confrontation between two Characters, where another Character views it and has the option to " +
                "intervene or not. The observing Character is not directly part of the confrontation, but will become so if they step in. " +
                "This Plot Point calls for three Characters to be Invoked."
            },
            new Plotpoint(){
                tag = "Crash",
                refTag = "Crash",
                listAction = new List<int>(){66,67},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a vehicle carrying a Character to crash or threaten to crash. The Character(s) " +
                "involved must either mitigate the damage of the crash, prevent the crash in the first place, and/or survive the crash. " +
                "The vehicle can be anything from a plane to a car to a snow sled ... anything that can transport a Character and its crashing would be dangerous."
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
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character who is running low on a human resource gets a boost. For instance, the battle is going poorly for King Leonard, but " +
                "just before they lose King Ferdinand appears on the hill with his forces ready to save the day."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves government in some way. Maybe a Character has to deal with a border crossing checkpoint, or a starship needs to get proper authorization to leave port."
            },
            new Plotpoint(){
                tag = "Physical Barrier to Overcome",
                refTag = "PhysicalBarrier",
                listAction = new List<int>(){68,69},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character faces a physical barrier of some sort that must be overcome. It could be a cliff that needs to be climbed, a rickety bridge to cross, " +
                "a door that needs to be knocked down, etc. Whatever the barrier is, it will require physical action to get past"
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a social injustice of some kind. For instance, a corrupt politician uses a civic ordinance to " +
                "foreclose on an apartment building where friends of a certain hero, who has upset the politician, live."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Just about the worst thing that can happen does happen. This is similar to the Action Plot Point Catastrophe, " +
                "except that it is accompanied by less action. For instance, a colonizing spaceship stops midway through a 40 year journey, " +
                "waking everyone up from their cryo sleep. Or, the investigator discovers the ancient vampire he had destroyed is, somehow, back"
            },
            new Plotpoint(){
                tag = "An Object of Unknown Use is Found",
                refTag = "ObjectUnknown",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){75},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character finds something that they think is useful, but they do not know in what way.This may be a magic wand " +
                "that they don’t know how to use, a key that they don’t know the lock it goes to, a device with an unknown purpose but currently has no power, etc."
            },
            new Plotpoint(){
                tag = "It's all about You",
                refTag = "AllAboutYou",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){64,65},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever the main action of this Turning Point, it is focused primarily on one Character."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Plot Point involves a celebration of some sort, such as a birthday party or a high school graduation party."
            },
            new Plotpoint(){
                tag = "Standoff",
                refTag = "Standoff",
                listAction = new List<int>(){},
                listTension = new List<int>(){72},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){70},
                listPersonal = new List<int>(){},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves two or more Characters in a tense standoff. For instance, a group of mercenaries " +
                "have the Characters pinned down behind rubble with gunfire, while the Characters fire back. Neither side can take out the other, " +
                "but neither can they leave without resolving the conflict."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever is happening in this Turning Point, those events will intensify. For instance, if a ship is leaking on the " +
                "high seas during a storm, maybe torrential winds tear down the sails."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "There is a threat in this Turning Point that has been in the Adventure previous to this Turning Point but went undetected. " +
                "This could be anything from an evil spirit lurking in an ancient vase to a virus in a person’s body to a good guy who turns out to be a bad guy, etc."
            },
            new Plotpoint(){
                tag = "Character Connection",
                refTag = "Connection",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){66,67},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character forms a connection with another Character. This connection can be anything from showing a personal interest " +
                "in the Character to asking them to become a business partner, etc. Whatever the connection is, it will have a lasting impact beyond this Turning Point."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves some aspect of religion or religious belief. For instance, maybe an event is taking place at a church, " +
                "or Characters stumble upon a cult preparing a magic ritual for their otherworldly god."
            },
            new Plotpoint(){
                tag = "Innocence",
                refTag = "Innocence",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){72},
                listPersonal = new List<int>(){68},
                numberOfCharacters = 1,
                type = PlotPointType.NewCharacter,
                special = SpecialType.None,
                details = "This Turning Point involves an element of innocence, usually an innocent person in an otherwise less than innocent situation. " +
                "For instance, an average citizen finds herself in the middle of two vampires battling. This can also be considered a " +
                "“fish out of water” Plot Point, where someone who does not belong in a situation finds themselves in that situation."
            },
            new Plotpoint(){
                tag = "Clear the Record",
                refTag = "ClearRecord",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){76},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is given the task of clearing someone or something of a false claim. For instance, a friend says " +
                "they are wrongly convicted of a crime and that the evidence is out there to prove it. The task may come to the Character officially, " +
                "given by another Character, or it may be something that falls into their lap, such as discovering the truth themselves and only they know it. " +
                "For instance, a foreign power has staged a catastrophe to start a war, but a handful of Characters know the truth ... if only they can reach " +
                "headquarters in time to tell them before warships are launched."
            },
            new Plotpoint(){
                tag = "Willing to Talk",
                refTag = "WillingToTalk",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){69,70},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is in a mood to talk. Whatever it is they have to say, it’s important to furthering the Plotline."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a theft, whether attempted or successful. What is being stolen is an object of some kind, " +
                "or information, or anything that can be taken. This Turning Point involves the actual activity and action of the theft or attempted theft. " +
                "For instance, the Character is strolling through a museum when a group of men burst in to steal a ritual mask."
            },
            new Plotpoint(){
                tag = "Character Harm",
                refTag = "CharacterHarm",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){71,72},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character hurts another Character in some personal way. For instance, a villain harms a wizard’s familiar or a Character hurls a personal insult at another Character."
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
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character must hide from something or someone in this Turning Point. For instance, the Character may have escaped " +
                "from a bounty hunter but must hide long enough to recover their wounds. Or, a terrible storm has struck and the Character " +
                "must take shelter, hiding from the storm"
            },
            new Plotpoint(){
                tag = "Followed",
                refTag = "Followed",
                listAction = new List<int>(){},
                listTension = new List<int>(){76,77},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is being followed by another Character."
            },
            new Plotpoint(){
                tag = "Framed",
                refTag = "Framed",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){77},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){73},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is unfairly framed by another Character. For instance, a mob boss plants evidence to make it look like a police detective has committed a crime"
            },
            new Plotpoint(){
                tag = "Preparation",
                refTag = "Preparation",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){73,74},
                listPersonal = new List<int>(){74,75},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a Character needing to prepare for something. For instance, a wizard must study up on how to banish demons " +
                "before a villain arrives, or a town of prospectors and merchants must learn how to fight before the band of outlaws arrives to exact their revenge for hanging a comrade."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a crime that seems either improbable or impossible to have occurred, " +
                "such as someone found murdered in a secure room or a piece of artwork stolen from a museum with no visible break in."
            },
            new Plotpoint(){
                tag = "Friend Focus",
                refTag = "FriendFocus",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){76},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever the main action of this Turning Point, it is focused on a friend or someone close to a Character. " +
                "This friend can be an already existing Character in the Adventure or someone not on the Characters List. Whoever the " +
                "friend is attached to, that is the Character Invoked, not the friend."
            },
            new Plotpoint(){
                tag = "Untouchable",
                refTag = "Untouchable",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){77},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is, in some manner, untouchable by others in this Turning Point. For instance, a villain " +
                "who is a world leader and thus can’t be directly attacked without triggering an international incident, or a " +
                "superhero who is nearly impervious to harm. The untouchableness should serve a plot purpose, so that " +
                "Characters are forced to take other actions to advance the Plotline."
            },
            new Plotpoint(){
                tag = "Bribe",
                refTag = "Bribe",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){78},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is offered a bribe by another Character to do something that is not legitimate. For instance, " +
                "a villain may offer money to a Character if they walk away from a murder scene."
            },
            new Plotpoint(){
                tag = "Dealing with a Calamity",
                refTag = "Calamity",
                listAction = new List<int>(){74,75},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a Character having to “fight” a calamity of some kind. For instance, maybe the Character is " +
                "battling a fire to put it out, or he must fight his way through an ancient stone temple as it collapses around him."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever is happening in this Turning Point, it will suddenly cease. This could occur at any time and the causes may be unknown. " +
                "For instance, if Characters are attacked by a group, the group may suddenly break off and run away."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a trap of some kind. This can be a physical trap, such as adventurers falling prey to a pit in a hallway, " +
                "to other kinds of traps, such as the summons to the peace negotiation was really just a ruse to get the leader in sights for an assassination."
            },
            new Plotpoint(){
                tag = "A Meeting of Minds",
                refTag = "MeetingOfMinds",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){75},
                listPersonal = new List<int>(){},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves two Characters coming together for a discussion of importance"
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A task must be accomplished within a certain amount of time or a Character will suffer consequences. " +
                "The time limit does not need to expire within this Turning Point and could extend beyond it further into the Adventure, " +
                "but it should terminate within this Adventure to give the Characters a reason to accomplish the task. Failure to accomplish " +
                "the task should be significant. For instance, if a cure to a toxin isn’t found within a day, the prince will die."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever is happening in this Turning Point it is clear that it was caused on purpose by someone of unknown identity. " +
                "For instance, if a Character is ambushed by bandits, the bandit leader may make a mysterious reference to their “benefactor” " +
                "having paid for the attack. Or, an engine failure on a ship may be found to have been caused by obvious tampering."
            },
            new Plotpoint(){
                tag = "A Needed Resource is Running Short",
                refTag = "NeededResource",
                listAction = new List<int>(){},
                listTension = new List<int>(){82,83},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A resource a Character needs is running low and will need to be replenished. This causes problems for the Character. " +
                "For instance, a starship’s warp engine functions on crystals that are running out."
            },
            new Plotpoint(){
                tag = "Organisations in Conflict",
                refTag = "OrgsInConflict",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){76},
                listPersonal = new List<int>(){},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.Organisation,
                details = "This Turning Point involves two or more organizations that are at odds with each other. For instance, " +
                "two rival mafia organizations may be trying to capture a master counterfeiter to use for their own purposes."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Something negative that happens in this Turning Point doesn’t happen directly in the Turning Point but is delivered " +
                "in the form of information. The event happened remotely, and a Character is learning of it. For instance, " +
                "Characters may learn their allies lost a crucial battle elsewhere."
            },
            new Plotpoint(){
                tag = "Character Assistance",
                refTag = "CharAssistance",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){79,80},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character assists another Character in some way. This assistance can be anything from coming to their aid in " +
                "battle to giving them a shoulder to cry on."
            },
            new Plotpoint(){
                tag = "Asking for Help",
                refTag = "AskingForHelp",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){81,82},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character approaches another Character to ask for help."
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
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a Character needing to fortify a place of refuge. For instance, a baron must shore " +
                "up his castle defenses against an impending attack, or a generator must be fueled up to increase a force field’s power " +
                "before a meteor storm rains down on the planet surface."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Something needs to be abandoned or has been abandoned already in this Turning Point. For instance, a heavily damaged " +
                "starship is going to explode in two hours and must be evacuated. Or, a Character comes upon an empty village in a forest."
            },
            new Plotpoint(){
                tag = "Find it Or Else",
                refTag = "FindItOrElse",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){81,82},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.NewCharacter,
                special = SpecialType.None,
                details = "Something needs to be found in this Turning Point to help resolve the Plotline. The act of finding the thing could " +
                "take place in this Turning Point, or a Character learns of the need to find something. The thing to be found can be just about " +
                "anything, from an object such as a magic ring to open a portal, to a special person like the lone witness to a crime that proves " +
                "an accused person is innocent"
            },
            new Plotpoint(){
                tag = "Used Against Them",
                refTag = "UsedAgainstThem",
                listAction = new List<int>(){78},
                listTension = new List<int>(){89},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A resource owned or aligned with one Character is somehow turned against them in this Turning Point. For instance, a small " +
                "starship is being pursued by three massive battle cruisers. By skillful piloting, the smaller ship causes the larger ships to collide " +
                "with each other, using their size against them.Or, a wizard may command a powerful golem, but another wizard casts a spell to make the golem attack its master."
            },
            new Plotpoint(){
                tag = "Powerful Person",
                refTag = "PowerfulPerson",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){77},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a powerful person. The Character’s power can be of any nature, from a physically powerful warrior to a government " +
                "figure with a lot of influence. Invoke a Character. If the Character is powerful, then that is the powerful person. If they are not, then the powerful " +
                "person is someone associated with that Character in some way"
            },
            new Plotpoint(){
                tag = "Creepy Tone",
                refTag = "CreepyTone",
                listAction = new List<int>(){},
                listTension = new List<int>(){90,91},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a creepy tone, such as a dark and forbidding place or a Character who is extremely menacing in a disturbing way."
            },
            new Plotpoint(){
                tag = "Welcome to the Plot",
                refTag = "WelcomePlot",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){83},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character learns that they are connected to this Plotline somehow in a personal way. Maybe it involves something from their " +
                "past or someone in their life. For instance, a detective may discover that the crime syndicate he is trying to take down is run by his long lost brother."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point takes place in a traveling vehicle. For instance, a ship at sea, a train, a ship hurtling through space, etc."
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
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character must escort another Character somewhere. For instance, this could be a bodyguard transporting a high " +
                "powered executive to a remote location, or a band of warriors trying to get a princess through a valley full of monsters."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves an agreement made long ago, probably even before this Adventure began. For instance, " +
                "occult investigators researching a mysterious death discover that the deceased person sold his soul to a demon ten years " +
                "ago, and they suspect the death is the demon having come to collect."
            },
            new Plotpoint(){
                tag = "A New Enemy",
                refTag = "NewEnemy",
                listAction = new List<int>(){},
                listTension = new List<int>(){93},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.NewCharacter,
                special = SpecialType.None,
                details = "This Turning Point presents a new threat to a Character. It is a threat that may or may not be directly related to " +
                "any Plotlines but must be dealt with all the same. For instance, explorers deep under the earth are moving through an ancient " +
                "ruin to find their lost comrade when they are beset upon by dinosaurs who nest in the area. This results automatically in a New Character."
            },
            new Plotpoint(){
                tag = "Alliance",
                refTag = "Alliance",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){81,82},
                listPersonal = new List<int>(){},
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.Organisation,
                details = "One group offers to ally with another. This may be a surprise alliance, such as an enemy wanting to join with another enemy " +
                "to take on a common foe, or it could be something less dramatic, such as the FBI offering to assist local law enforcement in solving a " +
                "crime. The “groups” in question can be formal organizations or something looser, such as groups of individuals."
            },
            new Plotpoint(){
                tag = "Power over Others",
                refTag = "PowerOthers",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){83,84},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character has power over other Characters in some way, shape, or form in this Turning Point. This power puts the Character in " +
                "a commanding position in regards to the others. For instance, the lord of a land demands all the peasants pay high taxes or else his men will " +
                "oppress them. Or, the producer of an anti-toxin for a disease that an entire village has demands they give him whatever he wants in order to receive the medicine"
            },
            new Plotpoint(){
                tag = "A Mysterious New Person",
                refTag = " MysteriousPerson",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){85},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.NewCharacter,
                special = SpecialType.None,
                details = "This Turning Point automatically Invokes a New Character, added to the List, whose identity or purpose is not fully known. " +
                "Maybe they are a shadowy visitor at a meeting, or someone who seems to have authority over someone else."
            },
            new Plotpoint(){
                tag = "Frenetic Activity",
                refTag = "FreneticActivity",
                listAction = new List<int>(){80,81},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves action coming fast and furious at a Character. It should be a rapid fire succession of action, " +
                "for instance a series of attackers, an out of control boat rocketing down a rapids approaching peril after peril, running a gauntlet of some kind through a series of traps, etc"
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a rural setting, such as out in the country or at a farm."
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
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a Character who is very likable to another Character. Whoever it is, " +
                "it should be someone who generates sympathy. The Character’s likability should be strong enough to motivate the other Character’s actions. " +
                "For instance, a jaded cop thought he has seen it all, but a kidnapped girl kindles in him a desire to save her and redeem himself."
            },
            new Plotpoint(){
                tag = "Someone is Where they Should Not Be",
                refTag = "ShouldNotBe",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){87,88},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is at a location where they should not normally be. For instance, an ally is seen at the headquarters of an enemy, " +
                "a wealthy socialite is found meeting with a mafia boss at a restaurant, etc."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A barrier needs to be overcome through stealth or dexterity. For instance, a monster lives in a cave that is only " +
                "accessible by climbing a high, treacherous cliffside. Or, there are too many ninjas guarding the villain to fight your way " +
                "through, but you can slip past them unseen if you are skilled enough."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves corruption of a social apparatus of some kind. For instance, a police officer on the take " +
                "from the mob, or the villain of the Adventure turns out to be a local bureaucrat using his position to give smugglers access to a dock at night."
            },
            new Plotpoint(){
                tag = "Vulnerability Exploited",
                refTag = "VulnerableExploit",
                listAction = new List<int>(){},
                listTension = new List<int>(){95},
                listMystery = new List<int>(){89},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a vulnerability of some kind being exploited by a Character. For instance, someone knowing of another’s " +
                "crime and blackmailing them, Characters learning of a starbase’s secret vulnerability that allows it to be destroyed, etc. This Turning Point " +
                "can either involve learning about the vulnerability or actively exploiting it."
            },
            new Plotpoint(){
                tag = "The Promise of Reward",
                refTag = "PromiseOfReward",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){85,86},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a Character faced with a substantial reward for their participation. For instance, maybe a village " +
                "is willing to give a group of adventurers everything they have if they fight off a band of marauding goblins.The reward should be for " +
                "doing something that is considered legitimate or good."
            },
            new Plotpoint(){
                tag = "Fraud",
                refTag = "Fraud",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){90,91},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character is a fraud. Whatever it is they are presenting themselves as, or whatever story they have told of themselves, is false. This result " +
                "differs from Hidden Agenda, where in Hidden Agenda the Character may legitimately have both motives in mind, whereas in Fraud the image or story they are " +
                "presenting is completely fake. For instance, the prince claiming he is the rightful ruler of a kingdom is actually a shapeshifting doppelgänger assuming the role."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves business or commerce in some way. It can either be a business transaction, or a business is involved in the Turning Point. " +
                "For instance, a corporation hires a super hero to protect an important shipment, or a book of antiquity containing a needed spell has to be purchased from an auction house."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves something that began as a just cause but has spiraled into something unjust. For instance, " +
                "a hero takes down a group of orcs terrorizing a town, saving the people, but now the hero has installed himself as the overlord of the town and is demanding tribute."
            },
            new Plotpoint(){
                tag = "Expert Knowledge",
                refTag = "ExpertKnowledge",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){87},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a Character who has very specific and specialized knowledge or skills that come into play during the Turning Point. " +
                "For instance, only the genius of Dr. Rayder can figure out the intricacies of the alien device, or it’s discovered that a killer is murdering people with his knowledge of exotic poisons."
            },
            new Plotpoint(){
                tag = "A Moment of Peace",
                refTag = "MomentOfPeace",
                listAction = new List<int>(){84,85},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever else is going on in this Turning Point, it should overall be a peaceful time for a Character. " +
                "For instance, there is a lull in the war where the combatants have a chance to enjoy a drink together and relax before they must fight again."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves a focus on something mundane and ordinary, such as a person’s living room or a meal. " +
                "This mundane thing may be coupled with something extraordinary in the Turning Point. For instance, a Character is killed when his nightly dinner is poisoned, " +
                "or a family portrait is found to be a cursed item."
            },
            new Plotpoint(){
                tag = "Run Away!",
                refTag = "RunAway",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){90,91},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character flees or has fled. The actual flight may occur in this Turning Point or it may be learned of. " +
                "For instance, a Character runs screaming as a horrible monster appears on the scene, or, a Character who disappeared earlier in the Adventure is learned " +
                "to have left town fearing for his life."
            },
            new Plotpoint(){
                tag = "Beat You To It",
                refTag = "BeatYouToIt",
                listAction = new List<int>(){86,87},
                listTension = new List<int>(){},
                listMystery = new List<int>(){92,93},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "Whatever is happening in this Turning Point that involves arriving at a location for some purpose, " +
                "a Character discovers that someone else has arrived before them. For instance, a Character goes to the morgue to check out a clue and learns that another " +
                "investigator already showed up and took the body."
            },
            new Plotpoint(){
                tag = "Confrontation",
                refTag = "Confrontation",
                listAction = new List<int>(){88,89},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){91},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "This Turning Point involves Characters meeting in a confrontation that may turn physical if things don’t go well. " +
                "For instance, a Character meets the leader of a street gang to get information, but the gang is notoriously twitchy and violent."
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
                numberOfCharacters = 2,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A disagreement between two Characters leads to a conflict in this Turning Point."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "An element of extreme social tension is near the breaking point. This Turning Point involves some aspect of that, " +
                "such as an event that increases the tension or an event that is a result of the tension. For instance, two nations at the brink of war have a " +
                "border skirmish as pressure rises among soldiers."
            },
            new Plotpoint(){
                tag = "Protector",
                refTag = "Protector",
                listAction = new List<int>(){90,91},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){92,93},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character must protect someone or something in this Turning Point. If this is an Action Plot Point, " +
                "the Character must actively protect in this Turning Point from a threat. If it is a Personal Plot Point, then the Character receives the protection duty in this Turning Point."
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
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A series of events that has taken place in this Adventure culminates in this Turning Point. " +
                "If this is early in the Adventure or in this Plotline, then instead the Adventure or Plotline gets off to a fiery start. For instance, " +
                "Characters following clues to track a cult finally discover their lair, resulting in a mass battle. Or, a Plotline about retrieving a stolen gem begins with a very elaborate theft"
            },
            new Plotpoint(){
                tag = "Destroy the Thing",
                refTag = "DestroyThing",
                listAction = new List<int>(){94,95},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character must destroy or try to destroy something in this Turning Point. " +
                "Maybe a party of dungeon delvers reaches the heart of the cavern where they must break a mystic seal."
            },
            new Plotpoint(){
                tag = "Conspiracy Theory",
                refTag = "ConspiracyTheory",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){94},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character believes in a scenario that explains a problem in this Adventure. " +
                "The Character may be right or wrong, but the theory may cause action on the part of the Character. " +
                "For instance, a group is holed up in a mall during a zombie apocalypse. One Character believes it’s just a disease, " +
                "so they encourage the others not to shoot the zombies."
            },
            new Plotpoint(){
                tag = "Servant",
                refTag = "Servant",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){},
                listSocial = new List<int>(){95},
                listPersonal = new List<int>(){94,95},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = " This Turning Point involves a servant or proxy of another Character. Invoke a Character for the servant to represent."
            },
            new Plotpoint(){
                tag = "An Opposing Story",
                refTag = "OpposingStory",
                listAction = new List<int>(){},
                listTension = new List<int>(){},
                listMystery = new List<int>(){95},
                listSocial = new List<int>(){},
                listPersonal = new List<int>(){},
                numberOfCharacters = 1,
                type = PlotPointType.Normal,
                special = SpecialType.None,
                details = "A Character learns of an alternate version of something they already know about from this Adventure. For instance, " +
                "while investigating a starship that had been waylaid by aliens, Characters discover a crewmember who claims the attackers " +
                "were members of a rival guild and not aliens."
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
                type = PlotPointType.Meta,
                special = SpecialType.None,
                details = "This is a special Plot Point category with Plot Points that change the Characters List or combine Plotlines. Go to the Meta " +
                "Plot Points Table and roll 1d100 on it for your Plot Point."
            },
        };
        /*Debug.LogFormat("[Tst] ToolDetails -> InitialisePlotpoints: There are {0} records in the listOfPlotponts{1}", listOfPlotpoints.Count, "\n");*/
        //convert list to dictionary
        for (int i = 0; i < listOfPlotpoints.Count; i++)
        { ToolManager.i.toolDataScript.AddPlotpoint(listOfPlotpoints[i]); }
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
                action = MetaAction.CharacterExits,
                listToRoll = new List<int>(){1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18},
                details = "A Character, who is not a Player Character, is removed from the Characters List completely. Cross out all references to that " +
                "Character on the Characters List. If there are no nonPlayer Characters, then re-roll for another Meta Plot Point.This change can be reflected " +
                "in the activity in this Turning Point or not.For instance, you may explain the Character being removed from the Adventure by having that " +
                "Character die in the Turning Point.Or, you simply remove them from the Characters List and decide that their involvement in the Adventure " +
                "is over.If, when rolling on the Characters List to determine who this Character is, you roll a Player Character or “New Character”, " +
                "then consider it a result of “Choose The Most Logical Character”."
            },
            new MetaPlotpoint(){
                tag = "Character Returns",
                refTag = "CharReturns",
                action = MetaAction.CharacterReturns,
                listToRoll = new List<int>(){19,20,21,22,23,24,25,26,27},
                details = "A Character who previously had been removed from the Adventure returns. Write that Character back into the Characters List with a " +
                "single listing. If there are no Characters to return, then treat this as a “New Character” result and use this Plot Point to introduce a new " +
                "Character into the Turning Point. If there is more than one Character who can return, then choose the most logical Character to return. " +
                "This change can be reflected in the activity in this Turning Point or not."
            },
            new MetaPlotpoint(){
                tag = "Character Steps Up",
                refTag = "CharStepsUp",
                action = MetaAction.CharacterStepsUp,
                listToRoll = new List<int>(){28,29,30,31,32,33,34,35,36},
                details = "A Character becomes more important, gaining another slot on the Characters List even if it pushes them past 3 slots.When you roll on the " +
                "Characters List to see who the Character is, treat a result of “New Character” as “Choose The Most Logical Character”. " +
                "This change can be reflected in the activity in this Turning Point or not."
            },
            new MetaPlotpoint(){
                tag = "Character Steps Down",
                refTag = "CharStepsDown",
                action = MetaAction.CharacterStepsDown,
                listToRoll = new List<int>(){37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55},
                details = "A Character becomes less important, remove them from one slot on the Characters List even if it removes them completely from the List. " +
                "If this would remove a Player Character completely from the List, or if when rolling for the Character you get a result of “New Character”, " +
                "then treat this as a result of “Choose The Most Logical Character”. If there is no possible Character to choose without removing a Player Character " +
                "completely from the List, then roll again on the Meta Plot Points Table.This change can be reflected in the activity in this Turning Point or not"
            },
            new MetaPlotpoint(){
                tag = "Character Downgrade",
                refTag = "CharDowngrade",
                action = MetaAction.CharacterDowngrade,
                listToRoll = new List<int>(){56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73},
                details = "A Character becomes less important, remove them from two slots on the Characters List even if it removes them completely from the List. " +
                "If this would remove a Player Character completely from the List, or if when rolling for the Character you get a result of “New Character”, then " +
                "treat this as a result of “Choose The Most Logical Character”. If there is no possible Character to choose without removing a Player Character " +
                "completely from the List, then roll again on the Meta Plot Points Table.This change can be reflected in the activity in this Turning Point or not."
            },
            new MetaPlotpoint(){
                tag = "Character Upgrade",
                refTag = "CharUpgrade",
                action = MetaAction.CharacterUpgrade,
                listToRoll = new List<int>(){74,75,76,77,78,79,80,81,82},
                details = "A Character becomes more important, gaining 2 slots on the Characters List even if it pushes them past 3 slots.When you roll on the Characters " +
                "List to see who the Character is, treat a result of “New Character” as “Choose The Most Logical Character”. This change can be reflected in the activity " +
                "in this Turning Point or not."
            },
            new MetaPlotpoint(){
                tag = "Plotline Combo",
                refTag = "PlotlineCombo",
                action = MetaAction.PlotLineCombo,
                listToRoll = new List<int>(){83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100},
                details = "This Turning Point is about more than one Plotline at the same time. Roll again on the Plotlines List and add that Plotline to this Turning Point " +
                "along with the original Plotline rolled. If when rolling for an additional Plotline you roll the same Plotline already in use for this Turning Point, " +
                "then treat the result as a “Choose The Most Logical Plotline”. If there are no other Plotlines to choose from, then create a new Plotline as the " +
                "additional Plotline.If a Conclusion is rolled as a Plot Point during this Turning Point, apply it to the Plotline that seems most appropriate. " +
                "If another Conclusion is rolled, continue to apply them to the additional Plotlines in this Turning Point if you can. It is possible with repeated " +
                "results of “Plotline Combo” to have more than two Plotlines combined in this way."
            }
        };
        Dictionary<string, MetaPlotpoint> dictOfMetaPlotpoints = ToolManager.i.toolDataScript.GetDictOfMetaPlotpoints();
        if (dictOfMetaPlotpoints != null)
        {
            /*Debug.LogFormat("[Tst] ToolDetails -> InitialiseMetaPlotpoints: There are {0} records in the listOfMetaPlotponts{1}", listOfMetaPlotpoints.Count, "\n");*/
            //convert list to dictionary
            for (int i = 0; i < listOfMetaPlotpoints.Count; i++)
            { ToolManager.i.toolDataScript.AddMetaPlotpoint(listOfMetaPlotpoints[i]); }
        }
        else { Debug.LogError("Invalid dictOfMetaPlotpoints (Null)"); }
    }
    #endregion

    #region InitialisePlotpointLookup
    /// <summary>
    /// Initialise arrayOfPlotpointLookup table
    /// </summary>
    private void InitialisePlotpointLookup()
    {
        Plotpoint[,] arrayOfPlotpointLookup = ToolManager.i.toolDataScript.GetPlotpointLookup();
        if (arrayOfPlotpointLookup != null)
        {
            //populate array with data from dictOfPlotpoints
            int index;
            Dictionary<string, Plotpoint> dictOfPlotpoints = ToolManager.i.toolDataScript.GetDictOfPlotpoints();
            if (dictOfPlotpoints != null)
            {
                foreach (var plot in dictOfPlotpoints)
                {
                    //action
                    if (plot.Value.listAction.Count > 0)
                    {
                        for (int i = 0; i < plot.Value.listAction.Count; i++)
                        {
                            index = plot.Value.listAction[i] - 1;
                            arrayOfPlotpointLookup[index, (int)ThemeType.Action] = plot.Value;
                        }
                    }
                    //tension
                    if (plot.Value.listTension.Count > 0)
                    {
                        for (int i = 0; i < plot.Value.listTension.Count; i++)
                        {
                            index = plot.Value.listTension[i] - 1;
                            arrayOfPlotpointLookup[index, (int)ThemeType.Tension] = plot.Value;
                        }
                    }
                    //mystery
                    if (plot.Value.listMystery.Count > 0)
                    {
                        for (int i = 0; i < plot.Value.listMystery.Count; i++)
                        {
                            index = plot.Value.listMystery[i] - 1;
                            arrayOfPlotpointLookup[index, (int)ThemeType.Mystery] = plot.Value;
                        }
                    }
                    //social
                    if (plot.Value.listSocial.Count > 0)
                    {
                        for (int i = 0; i < plot.Value.listSocial.Count; i++)
                        {
                            index = plot.Value.listSocial[i] - 1;
                            arrayOfPlotpointLookup[index, (int)ThemeType.Social] = plot.Value;
                        }
                    }
                    //personal
                    if (plot.Value.listPersonal.Count > 0)
                    {
                        for (int i = 0; i < plot.Value.listPersonal.Count; i++)
                        {
                            index = plot.Value.listPersonal[i] - 1;
                            arrayOfPlotpointLookup[index, (int)ThemeType.Personal] = plot.Value;
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid dictOfPlotpoints (Null)"); }
            //data validation
            Plotpoint test;
            int counter = 0;
            for (int inner = 0; inner < arrayOfPlotpointLookup.GetUpperBound(0) + 1; inner++)
            {
                for (int outer = 0; outer < (int)ThemeType.Count; outer++)
                {
                    test = arrayOfPlotpointLookup[inner, outer];
                    if (test == null)
                    { Debug.LogWarningFormat("Invalid Plotpoint (Null) for arrayOfPlotpointLookup[{0},{1}]", inner, outer); }
                    else { counter++; }
                }
            }
            /*Debug.LogFormat("[Tst] ToolDetails.cs -> InitialisePlotpointLookup: arrayOfPlotPointLookup has {0} records{1}", counter, "\n");*/
        }
        else { Debug.LogError("Invalid arrayOfPlotpointLookup (Null)"); }
    }
    #endregion

    #region InitialiseMetaPlotpointLookup
    /// <summary>
    /// Initialise arrayOfMetaPlotpointLookup table
    /// </summary>
    private void InitialiseMetaPlotpointLookup()
    {
        MetaPlotpoint[] arrayOfMetaPlotpointLookup = ToolManager.i.toolDataScript.GetMetaPlotpointLookup();
        if (arrayOfMetaPlotpointLookup != null)
        {
            //populate array with data from dictOfPlotpoints
            int index;
            Dictionary<string, MetaPlotpoint> dictOfMetaPlotpoints = ToolManager.i.toolDataScript.GetDictOfMetaPlotpoints();
            if (dictOfMetaPlotpoints != null)
            {
                foreach (var meta in dictOfMetaPlotpoints)
                {
                    //input data
                    if (meta.Value.listToRoll.Count > 0)
                    {
                        for (int i = 0; i < meta.Value.listToRoll.Count; i++)
                        {
                            index = meta.Value.listToRoll[i] - 1;
                            arrayOfMetaPlotpointLookup[index] = meta.Value;
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid dictOfMetaPlotpoints (Null)"); }
            //data validation
            MetaPlotpoint test;
            int counter = 0;
            for (int i = 0; i < arrayOfMetaPlotpointLookup.Length; i++)
            {
                test = arrayOfMetaPlotpointLookup[i];
                if (test == null)
                { Debug.LogWarningFormat("Invalid MetaPlotpoint (Null) for arrayOfMetaPlotpointLookup[{0}]", i); }
                else { counter++; }
            }
            /*Debug.LogFormat("[Tst] ToolDetails.cs -> InitialiseMetaPlotpointLookup: arrayOfMetaPlotPointLookup has {0} records{1}", counter, "\n");*/
        }
        else { Debug.LogError("Invalid arrayOfMetaPlotpointLookup (Null)"); }
    }
    #endregion

    #region InitaliseCharacterSpecial
    /// <summary>
    /// Initialise Character Special Trait
    /// </summary>
    private void InitialiseCharacterSpecial()
    {
        List<CharacterSpecial> listOfCharacterSpecial = new List<CharacterSpecial>()
        {
            new CharacterSpecial() {
                tag = "Individual",
                special = SpecialType.None,
                listToRoll = new List<int> {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50},
                details = "The Character is an individual, as opposed to an organization or object."
            },
            new CharacterSpecial() {
                tag = "Organisation",
                special = SpecialType.Organisation ,
                listToRoll = new List<int> {51,52,53,54,55,56,57},
                details = "This Character is not a specific individual, but an organization or community. General members of this organization are considered part of the Character as a community"
            },
            new CharacterSpecial() {
                tag = "Object",
                special = SpecialType.Object,
                listToRoll = new List<int> {58,59,60,61,62,63,64},
                details = "This Character is something other than a typical, living individual or group organization. The Character is an object of some kind that could also be " +
                "considered a Character unto itself. Examples might include a spaceship that is old and temperamental, or a city teeming with culture"
            },
            new CharacterSpecial() {
                tag = "Connected",
                special = SpecialType.None,
                listToRoll = new List<int> {65,66,67,68,69,70,71},
                details = "This Character enters the Adventure somehow connected with the Plotline of this Turning Point."
            },
            new CharacterSpecial() {
                tag = "NOT Connected",
                special = SpecialType.None,
                listToRoll = new List<int> {72,73,74,75,76,77,78},
                details = "This Character enters the Adventure not connected to this Turning Point’s Plotline. The Character may become part of the Plotline in the course of this " +
                "Turning Point, but does not start off that way. Examples include bystanders to the main events of a Turning Point or people outside the events of the " +
                "Plotline who get drawn into the Adventure."
            },
            new CharacterSpecial() {
                tag = "Assists Resolution",
                special = SpecialType.None,
                listToRoll = new List<int> {79,80,81,82,83,84,85},
                details = "This Character is someone who can help resolve the current Plotline in some way, likely serving as an aid to the Player Characters"
            },
            new CharacterSpecial() {
                tag = "Hinders Resolution",
                special = SpecialType.None,
                listToRoll = new List<int> {86,87,88,89,90,91,92},
                details = "This Character gets in the way of resolving the current Plotline in some way, likely serving as a complication to the Player Characters"
            },
            new CharacterSpecial() {
                tag = "Connected to Another",
                special = SpecialType.None,
                listToRoll = new List<int> {93,94,95,96,97,98,99,100},
                details = "This Character has some relationship to another, existing Character in this Adventure. Roll on the Characters List to see who. " +
                "A result of New Character is changed to Choose The Most Logical Character. The connection can be anything, from the two Characters are related, " +
                "they know each other, they were former friends, they both work in the same occupation or belong to the same organization, they look or act similarly, " +
                "they have similar skills or equipment, etc. The connection can be as close or as distant as you like."
            },
        };
        CharacterSpecial[] arrayOfSpecial = ToolManager.i.toolDataScript.GetArrayOfCharacterSpecial();
        if (arrayOfSpecial != null)
        {
            int count;
            int index;
            //populate array
            for (int i = 0; i < listOfCharacterSpecial.Count; i++)
            {
                CharacterSpecial special = listOfCharacterSpecial[i];
                if (special != null)
                {
                    count = special.listToRoll.Count;
                    if (count > 0)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            index = special.listToRoll[j] - 1;
                            arrayOfSpecial[index] = special;
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid count (Zero) for characterSpecial \"{0}\"", special.tag); }
                }
                else { Debug.LogWarningFormat("Invalid characterSpecial (Null) for listOfCharacterSpecial[{0}]", "\n"); }
            }
            //data validation
            count = 0;
            for (int i = 0; i < arrayOfSpecial.Length; i++)
            {
                if (arrayOfSpecial[i] == null)
                { Debug.LogWarningFormat("Invalid characterSpecial (Null) for arrayOfSpecial[{0}]", i); }
                else { count++; }
            }
            /*Debug.LogFormat("[Tst] ToolDetails.cs -> InitialiseCharacterSpecial: arrayOfSpecial has {0} records{1}", count, "\n");*/
        }
        else { Debug.LogError("Invalid arrayOfSpecial (Null)"); }

    }
    #endregion

    #region InitialiseCharacterIndentity
    /// <summary>
    /// Character Identity Initialisation
    /// </summary>
    private void InitialiseCharacterIdentity()
    {
        List<CharacterIdentity> listOfCharacterIdentity = new List<CharacterIdentity>() {
            new CharacterIdentity() {
                tag = "RollAgain",
                listToRoll = new List<int> {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33},
                isRollAgain = true
            },
            new CharacterIdentity() {
                tag = "Warrior",
                listToRoll = new List<int> {34},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Healer",
                listToRoll = new List<int> {35},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Protector",
                listToRoll = new List<int> {36},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Assistant",
                listToRoll = new List<int> {37},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Dependant",
                listToRoll = new List<int> {38},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Ruler",
                listToRoll = new List<int> {39},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Administrator",
                listToRoll = new List<int> {40},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Victim",
                listToRoll = new List<int> {41},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Scholar",
                listToRoll = new List<int> {42},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Expert",
                listToRoll = new List<int> {43},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Elite",
                listToRoll = new List<int> {44},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Investigator",
                listToRoll = new List<int> {45},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Criminal",
                listToRoll = new List<int> {46},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Supporter",
                listToRoll = new List<int> {47},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Helpless",
                listToRoll = new List<int> {48},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Outsider",
                listToRoll = new List<int> {49},
                isRollAgain = false
            },

            // - - - 

            new CharacterIdentity() {
                tag = "Mediator",
                listToRoll = new List<int> {50},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Entertainer",
                listToRoll = new List<int> {51},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Socialite",
                listToRoll = new List<int> {52},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Athlete",
                listToRoll = new List<int> {53},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Performer",
                listToRoll = new List<int> {54},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Representative",
                listToRoll = new List<int> {55},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Merchant",
                listToRoll = new List<int> {56},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Trader",
                listToRoll = new List<int> {57},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Creator",
                listToRoll = new List<int> {58},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Artist",
                listToRoll = new List<int> {59},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Servant",
                listToRoll = new List<int> {60},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Laborer",
                listToRoll = new List<int> {61},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Religious",
                listToRoll = new List<int> {62},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Hunter",
                listToRoll = new List<int> {63},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Leader",
                listToRoll = new List<int> {64},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Fighter",
                listToRoll = new List<int> {65},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Crafter",
                listToRoll = new List<int> {66},
                isRollAgain = false
            },

            // - - -
            
            new CharacterIdentity() {
                tag = "Thief",
                listToRoll = new List<int> {67},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Radical",
                listToRoll = new List<int> {68},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Executive",
                listToRoll = new List<int> {69},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Thug",
                listToRoll = new List<int> {70},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Guard",
                listToRoll = new List<int> {71},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Guardian",
                listToRoll = new List<int> {72},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Explorer",
                listToRoll = new List<int> {73},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Hero",
                listToRoll = new List<int> {74},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Villain",
                listToRoll = new List<int> {75},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Deceiver",
                listToRoll = new List<int> {76},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Engineer",
                listToRoll = new List<int> {77},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Scout",
                listToRoll = new List<int> {78},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Fixer",
                listToRoll = new List<int> {79},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Wanderer",
                listToRoll = new List<int> {80},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Subverter",
                listToRoll = new List<int> {81},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Soldier",
                listToRoll = new List<int> {82},
                isRollAgain = false
            },

            new CharacterIdentity() {
                tag = "Law Enforcement",
                listToRoll = new List<int> {83},
                isRollAgain = false
            },

            // - - -

            new CharacterIdentity() {
                tag = "Scientist",
                listToRoll = new List<int> {84},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Gatherer",
                listToRoll = new List<int> {85},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Foreigner",
                listToRoll = new List<int> {86},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Survivor",
                listToRoll = new List<int> {87},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Gambler",
                listToRoll = new List<int> {88},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Rogue",
                listToRoll = new List<int> {89},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Farmer",
                listToRoll = new List<int> {90},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Killer",
                listToRoll = new List<int> {91},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Professional",
                listToRoll = new List<int> {92},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Driver/Pilot",
                listToRoll = new List<int> {93},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Student",
                listToRoll = new List<int> {94},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Organiser",
                listToRoll = new List<int> {95},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Deliverer",
                listToRoll = new List<int> {96},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Lackey",
                listToRoll = new List<int> {97},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Teacher",
                listToRoll = new List<int> {98},
                isRollAgain = false
            },
            new CharacterIdentity() {
                tag = "Exotic",
                listToRoll = new List<int> {99, 100},
                isRollAgain = false
            }
        };

        CharacterIdentity[] arrayOfIdentity = ToolManager.i.toolDataScript.GetArrayOfCharacterIdentity();
        if (arrayOfIdentity != null)
        {
            int count;
            int index;
            //populate array
            for (int i = 0; i < listOfCharacterIdentity.Count; i++)
            {
                CharacterIdentity identity = listOfCharacterIdentity[i];
                if (identity != null)
                {
                    count = identity.listToRoll.Count;
                    if (count > 0)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            index = identity.listToRoll[j] - 1;
                            arrayOfIdentity[index] = identity;
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid count (Zero) for characterIdentity \"{0}\"", identity.tag); }
                }
                else { Debug.LogWarningFormat("Invalid characterIndentity (Null) for listOfCharacterIdentity[{0}]", "\n"); }
            }
            //data validation
            count = 0;
            for (int i = 0; i < arrayOfIdentity.Length; i++)
            {
                if (arrayOfIdentity[i] == null)
                { Debug.LogWarningFormat("Invalid characterIdentity (Null) for arrayOfIdentity[{0}]", i); }
                else { count++; }
            }
            /*Debug.LogFormat("[Tst] ToolDetails.cs -> InitialiseCharacterIndentity: arrayOfIdentity has {0} records{1}", count, "\n");*/
        }
        else { Debug.LogError("Invalid arrayOfIdentity (Null)"); }

    }
    #endregion

    #region InitialiseCharacterDescriptors
    /// <summary>
    /// Character descriptor Initialisation
    /// </summary>
    private void InitialiseCharacterDescriptors()
    {
        List<CharacterDescriptor> listOfCharacterDescriptor = new List<CharacterDescriptor>() {
            new CharacterDescriptor() {
                tag = "RollAgain",
                listToRoll = new List<int> {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21},
                isRollAgain = true
            },
            new CharacterDescriptor() {
                tag = "Ugly",
                listToRoll = new List<int> {22},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Beautiful",
                listToRoll = new List<int> {23},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Foul",
                listToRoll = new List<int> {24},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Sweet",
                listToRoll = new List<int> {25},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Unusual",
                listToRoll = new List<int> {26},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Common",
                listToRoll = new List<int> {27},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Intelligent",
                listToRoll = new List<int> {28},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Ignorant",
                listToRoll = new List<int> {29},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Educated",
                listToRoll = new List<int> {30},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Skilled",
                listToRoll = new List<int> {31},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Trained",
                listToRoll = new List<int> {32},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Rude",
                listToRoll = new List<int> {33},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Polite",
                listToRoll = new List<int> {34},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Fancy",
                listToRoll = new List<int> {35},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Rough",
                listToRoll = new List<int> {36},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Dirty",
                listToRoll = new List<int> {37},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Clean",
                listToRoll = new List<int> {38},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Wealthy",
                listToRoll = new List<int> {39},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Poor",
                listToRoll = new List<int> {40},
                isRollAgain = false
            },

            // - - -

            new CharacterDescriptor() {
                tag = "Small",
                listToRoll = new List<int> {41},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Large",
                listToRoll = new List<int> {42},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Quiet",
                listToRoll = new List<int> {43},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Loud",
                listToRoll = new List<int> {44},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Fast",
                listToRoll = new List<int> {45},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Slow",
                listToRoll = new List<int> {46},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Exotic",
                listToRoll = new List<int> {47},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Uniformed",
                listToRoll = new List<int> {48},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Interesting",
                listToRoll = new List<int> {49},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Colourful",
                listToRoll = new List<int> {50},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Informative",
                listToRoll = new List<int> {51},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Dangerous",
                listToRoll = new List<int> {52},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Inept",
                listToRoll = new List<int> {53},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Clumsy",
                listToRoll = new List<int> {54},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Capable",
                listToRoll = new List<int> {55},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Intrusive",
                listToRoll = new List<int> {56},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Respectful",
                listToRoll = new List<int> {57},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Primitive",
                listToRoll = new List<int> {58},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Sophisticated",
                listToRoll = new List<int> {59},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Elegant",
                listToRoll = new List<int> {60},
                isRollAgain = false
            },

            // - - -

            new CharacterDescriptor() {
                tag = "Armed",
                listToRoll = new List<int> {61},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Different",
                listToRoll = new List<int> {62},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Young",
                listToRoll = new List<int> {63},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Old",
                listToRoll = new List<int> {64},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Difficult",
                listToRoll = new List<int> {65},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Helpful",
                listToRoll = new List<int> {66},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Harmful",
                listToRoll = new List<int> {67},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Disciplined",
                listToRoll = new List<int> {68},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Erratic",
                listToRoll = new List<int> {69},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Wild",
                listToRoll = new List<int> {70},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Crazy",
                listToRoll = new List<int> {71},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Commanding",
                listToRoll = new List<int> {72},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Meek",
                listToRoll = new List<int> {73},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Humorous",
                listToRoll = new List<int> {74},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Frightened",
                listToRoll = new List<int> {75},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Brave",
                listToRoll = new List<int> {76},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Strong",
                listToRoll = new List<int> {77},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Weak",
                listToRoll = new List<int> {78},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Impulsive",
                listToRoll = new List<int> {79},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Strategic",
                listToRoll = new List<int> {80},
                isRollAgain = false
            },

            // - - - 

            new CharacterDescriptor() {
                tag = "Naive",
                listToRoll = new List<int> {81},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Confident",
                listToRoll = new List<int> {82},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Surprising",
                listToRoll = new List<int> {83},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Passive",
                listToRoll = new List<int> {84},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Bold",
                listToRoll = new List<int> {85},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Careless",
                listToRoll = new List<int> {86},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Cautious",
                listToRoll = new List<int> {87},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Sneaky",
                listToRoll = new List<int> {88},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Intimidating",
                listToRoll = new List<int> {89},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Powerful",
                listToRoll = new List<int> {90},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Powerless",
                listToRoll = new List<int> {91},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Hurt",
                listToRoll = new List<int> {92},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Rough",
                listToRoll = new List<int> {93},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Gentle",
                listToRoll = new List<int> {94},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Caring",
                listToRoll = new List<int> {95},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Principled",
                listToRoll = new List<int> {96},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Arrogant",
                listToRoll = new List<int> {97},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Curious",
                listToRoll = new List<int> {98},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Supportive",
                listToRoll = new List<int> {99},
                isRollAgain = false
            },
            new CharacterDescriptor() {
                tag = "Heroic",
                listToRoll = new List<int> {100},
                isRollAgain = false
            },
        };

        CharacterDescriptor[] arrayOfDescriptor = ToolManager.i.toolDataScript.GetArrayOfCharacterDescriptors();
        if (arrayOfDescriptor != null)
        {
            int count;
            int index;
            //populate array
            for (int i = 0; i < listOfCharacterDescriptor.Count; i++)
            {
                CharacterDescriptor descriptor = listOfCharacterDescriptor[i];
                if (descriptor != null)
                {
                    count = descriptor.listToRoll.Count;
                    if (count > 0)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            index = descriptor.listToRoll[j] - 1;
                            arrayOfDescriptor[index] = descriptor;
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid count (Zero) for characterDescriptor \"{0}\"", descriptor.tag); }
                }
                else { Debug.LogWarningFormat("Invalid characterDescriptor (Null) for listOfCharacterDescriptor[{0}]", "\n"); }
            }
            //data validation
            count = 0;
            for (int i = 0; i < arrayOfDescriptor.Length; i++)
            {
                if (arrayOfDescriptor[i] == null)
                { Debug.LogWarningFormat("Invalid characterDescriptor (Null) for arrayOfDescriptor[{0}]", i); }
                else { count++; }
            }
            /*Debug.LogFormat("[Tst] ToolDetails.cs -> InitialiseCharacterDescriptors: arrayOfDescriptor has {0} records{1}", count, "\n");*/
        }
        else { Debug.LogError("Invalid arrayOfDescriptor (Null)"); }
    }
    #endregion

    #region InitialiseCharacterGoal
    /// <summary>
    /// Character Goal Initialisation
    /// </summary>
    private void InitialiseCharacterGoal()
    {
        List<CharacterGoal> listOfCharacterGoal = new List<CharacterGoal>() {
            new CharacterGoal() {
                tag = "Obtain an Object",
                listToRoll = new List<int> {1,2,3},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Make an agreement",
                listToRoll = new List<int> {4,5,6},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Build a relationship",
                listToRoll = new List<int> {7,8,9},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Undermine a relationship",
                listToRoll = new List<int> {10, 11, 12},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Seek a truth",
                listToRoll = new List<int> {13, 14, 15},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Pay a debt",
                listToRoll = new List<int> {16, 17, 18},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Refute a falsehood",
                listToRoll = new List<int> {19, 20, 21},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Harm a rival",
                listToRoll = new List<int> {22, 23, 24},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Cure an ill",
                listToRoll = new List<int> {25, 26, 27},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Find a person",
                listToRoll = new List<int> {28, 29, 30},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Find a home",
                listToRoll = new List<int> {31, 32, 33},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Seize power",
                listToRoll = new List<int> {34, 35, 36},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Restore a relationship",
                listToRoll = new List<int> {37, 38, 39},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Create an item",
                listToRoll = new List<int> {40, 41, 42},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Travel to a place",
                listToRoll = new List<int> {43, 44, 45},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Secure provisions",
                listToRoll = new List<int> {46, 47, 48},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Rebel against power",
                listToRoll = new List<int> {49, 50, 51},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Collect a debt",
                listToRoll = new List<int> {52, 53, 54},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Protect a secret",
                listToRoll = new List<int> {55, 56, 57},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Spread faith",
                listToRoll = new List<int> {58, 59, 60},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Enrich themselves",
                listToRoll = new List<int> {61, 62, 63},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Protect a person",
                listToRoll = new List<int> {64, 65, 66},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Protect the status quo",
                listToRoll = new List<int> {67, 68, 69},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Advance status",
                listToRoll = new List<int> {70, 71, 72},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Defend a place",
                listToRoll = new List<int> {73, 74, 75},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Avenge a wrong",
                listToRoll = new List<int> {76, 77, 78},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Fufill a duty",
                listToRoll = new List<int> {79, 80, 81},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Gain knowledge",
                listToRoll = new List<int> {82, 83, 84},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Prove worthiness",
                listToRoll = new List<int> {85, 86, 87},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Find redemption",
                listToRoll = new List<int> {88, 89, 90},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Escape from something",
                listToRoll = new List<int> {91, 92, 93},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "Resolve a dispute",
                listToRoll = new List<int> {94, 95, 96},
                isRollAgain = false
            },
            new CharacterGoal() {
                tag = "RollAgain",
                listToRoll = new List<int> {97, 98, 99, 100},
                isRollAgain = true
            }
        };

        CharacterGoal[] arrayOfGoal = ToolManager.i.toolDataScript.GetArrayOfCharacterGoals();
        if (arrayOfGoal != null)
        {
            int count;
            int index;
            //populate array
            for (int i = 0; i < listOfCharacterGoal.Count; i++)
            {
                CharacterGoal goal = listOfCharacterGoal[i];
                if (goal != null)
                {
                    count = goal.listToRoll.Count;
                    if (count > 0)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            index = goal.listToRoll[j] - 1;
                            arrayOfGoal[index] = goal;
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid count (Zero) for characterGoal \"{0}\"", goal.tag); }
                }
                else { Debug.LogWarningFormat("Invalid characterGoal (Null) for listOfCharacterGoal[{0}]", "\n"); }
            }
            //data validation
            count = 0;
            for (int i = 0; i < arrayOfGoal.Length; i++)
            {
                if (arrayOfGoal[i] == null)
                { Debug.LogWarningFormat("Invalid characterGoal (Null) for arrayOfGoal[{0}]", i); }
                else { count++; }
            }
            /*Debug.LogFormat("[Tst] ToolDetails.cs -> InitialiseCharacterGoal: arrayOfGoal has {0} records{1}", count, "\n");*/
        }
        else { Debug.LogError("Invalid arrayOfGoals (Null)"); }

    }
    #endregion

    #region InitialiseCharacterMotivation
    /// <summary>
    /// Character Motivation Initialisation
    /// </summary>
    private void InitialiseCharacterMotivation()
    {
        List<CharacterMotivation> listOfCharacterMotivation = new List<CharacterMotivation>() {
            new CharacterMotivation() {
                tag = "Personal Egotism",
                listToRoll = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9},
                isRollAgain = false
            },
            new CharacterMotivation() {
                tag = "Personal Honour",
                listToRoll = new List<int> {10, 11, 12, 13, 14, 15, 16, 17, 18},
                isRollAgain = false
            },
            new CharacterMotivation() {
                tag = "Love of Duty",
                listToRoll = new List<int> {19, 20, 21, 22, 23, 24, 25, 26, 27},
                isRollAgain = false
            },
            new CharacterMotivation() {
                tag = "Pleasure / Excitement",
                listToRoll = new List<int> {28, 29, 30, 31, 32, 33, 34, 35, 36},
                isRollAgain = false
            },
            new CharacterMotivation() {
                tag = "Knowledge",
                listToRoll = new List<int> {37, 38, 39, 40, 41, 42, 43, 44, 45},
                isRollAgain = false
            },
            new CharacterMotivation() {
                tag = "Love",
                listToRoll = new List<int> {46, 47, 48, 49, 50, 51, 52, 53, 54},
                isRollAgain = false
            },
            new CharacterMotivation() {
                tag = "Power",
                listToRoll = new List<int> {55, 56, 57, 58, 59, 60, 61, 62, 63},
                isRollAgain = false
            },
            new CharacterMotivation() {
                tag = "Wealth",
                listToRoll = new List<int> {64, 65, 66, 67, 68, 69, 70, 71, 72},
                isRollAgain = false
            },
            new CharacterMotivation() {
                tag = "Social Status",
                listToRoll = new List<int> {73, 74, 75, 76, 77, 78, 79, 80, 81},
                isRollAgain = false
            },
            new CharacterMotivation() {
                tag = "Vengeance",
                listToRoll = new List<int> {82, 83, 84, 85, 86, 87, 88, 89, 90},
                isRollAgain = false
            },
            new CharacterMotivation() {
                tag = "RollAgain",
                listToRoll = new List<int> {91, 92, 93, 94, 95, 96, 97, 98, 99, 100},
                isRollAgain = true
            }
        };

        CharacterMotivation[] arrayOfMotivation = ToolManager.i.toolDataScript.GetArrayOfCharacterMotivation();
        if (arrayOfMotivation != null)
        {
            int count;
            int index;
            //populate array
            for (int i = 0; i < listOfCharacterMotivation.Count; i++)
            {
                CharacterMotivation motivation = listOfCharacterMotivation[i];
                if (motivation != null)
                {
                    count = motivation.listToRoll.Count;
                    if (count > 0)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            index = motivation.listToRoll[j] - 1;
                            arrayOfMotivation[index] = motivation;
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid count (Zero) for characterMotivation \"{0}\"", motivation.tag); }
                }
                else { Debug.LogWarningFormat("Invalid characterMotivation (Null) for listOfCharacterMotivation[{0}]", "\n"); }
            }
            //data validation
            count = 0;
            for (int i = 0; i < arrayOfMotivation.Length; i++)
            {
                if (arrayOfMotivation[i] == null)
                { Debug.LogWarningFormat("Invalid characterMotivation (Null) for arrayOfMotivation[{0}]", i); }
                else { count++; }
            }
            /*Debug.LogFormat("[Tst] ToolDetails.cs -> InitialiseCharacterMotivation: arrayOfMotivation has {0} records{1}", count, "\n");*/
        }
        else { Debug.LogError("Invalid arrayOfMotivation (Null)"); }
    }
    #endregion

    #region InitialiseCharacterFocus
    /// <summary>
    /// Character Focus Initialisation
    /// </summary>
    private void InitialiseCharacterFocus()
    {
        List<CharacterFocus> listOfCharacterFocus = new List<CharacterFocus>() {
            new CharacterFocus() {
                tag = "Identity",
                listToRoll = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20},
                isRollAgain = false
            },
            new CharacterFocus() {
                tag = "Descriptors",
                listToRoll = new List<int> {21, 22, 23, 24, 25, 26, 27, 28, 29, 30},
                isRollAgain = false
            },
            new CharacterFocus() {
                tag = "Goal",
                listToRoll = new List<int> {31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50},
                isRollAgain = false
            },
            new CharacterFocus() {
                tag = "Motivation",
                listToRoll = new List<int> {51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70},
                isRollAgain = false
            },
            new CharacterFocus() {
                tag = "Trait",
                listToRoll = new List<int> {71, 72, 73, 74, 75, 76, 77, 78, 79, 80},
                isRollAgain = false
            },
            new CharacterFocus() {
                tag = "RollAgain",
                listToRoll = new List<int> {81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100},
                isRollAgain = true
            }
        };

        CharacterFocus[] arrayOfFocus = ToolManager.i.toolDataScript.GetArrayOfCharacterFocus();
        if (arrayOfFocus != null)
        {
            int count;
            int index;
            //populate array
            for (int i = 0; i < listOfCharacterFocus.Count; i++)
            {
                CharacterFocus focus = listOfCharacterFocus[i];
                if (focus != null)
                {
                    count = focus.listToRoll.Count;
                    if (count > 0)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            index = focus.listToRoll[j] - 1;
                            arrayOfFocus[index] = focus;
                        }
                    }
                    else { Debug.LogWarningFormat("Invalid count (Zero) for characterFocus \"{0}\"", focus.tag); }
                }
                else { Debug.LogWarningFormat("Invalid characterFocus (Null) for listOfCharacterFocus[{0}]", "\n"); }
            }
            //data validation
            count = 0;
            for (int i = 0; i < arrayOfFocus.Length; i++)
            {
                if (arrayOfFocus[i] == null)
                { Debug.LogWarningFormat("Invalid characterFocus (Null) for arrayOfFocus[{0}]", i); }
                else { count++; }
            }
            /*Debug.LogFormat("[Tst] ToolDetails.cs -> InitialiseCharacterFocus: arrayOfFocus has {0} records{1}", count, "\n");*/
        }
        else { Debug.LogError("Invalid arrayOfFocus (Null)"); }
    }
    #endregion

    #region InitialiseOrganisationType
    /// <summary>
    /// Organisation type
    /// </summary>
    private void InitialiseOrganisationType()
    {
        List<OrganisationDescriptor> listOfOrganisationType = new List<OrganisationDescriptor>()
        {
            new OrganisationDescriptor(){ tag = "Military"},
            new OrganisationDescriptor(){ tag = "Religious"},
            new OrganisationDescriptor(){ tag = "Scientific"},
            new OrganisationDescriptor(){ tag = "Bureaucratic"},
            new OrganisationDescriptor(){ tag = "Corporate"},
            new OrganisationDescriptor(){ tag = "Political"},
            new OrganisationDescriptor(){ tag = "Workers"},
            new OrganisationDescriptor(){ tag = "Criminal"},
            new OrganisationDescriptor(){ tag = "Off World"},
            new OrganisationDescriptor(){ tag = "Secret Society"},
        };
        ToolManager.i.toolDataScript.SetListOfOrganisationType(listOfOrganisationType);
    }
    #endregion

    #region InitialiseOrganisationOrigin
    /// <summary>
    /// Organisation origin
    /// </summary>
    private void InitialiseOrganisationOrigin()
    {
        List<OrganisationDescriptor> listOfOrganisationOrigin = new List<OrganisationDescriptor>()
        {
            new OrganisationDescriptor(){ tag = "Ancient"},
            new OrganisationDescriptor(){ tag = "New or Recent"},
            new OrganisationDescriptor(){ tag = "Local"},
            new OrganisationDescriptor(){ tag = "Foreign"},
            new OrganisationDescriptor(){ tag = "Alliance"},
            new OrganisationDescriptor(){ tag = "Splinter Group"},
            new OrganisationDescriptor(){ tag = "Revolt"},
            new OrganisationDescriptor(){ tag = "Off World"},
            new OrganisationDescriptor(){ tag = "Prophecy"},
            new OrganisationDescriptor(){ tag = "Unknown"},
        };
        ToolManager.i.toolDataScript.SetListOfOrganisationOrigin(listOfOrganisationOrigin);
    }
    #endregion

    #region InitialiseOrganisationHistory
    /// <summary>
    /// Organisation History -> Major turning point
    /// </summary>
    private void InitialiseOrganisationHistory()
    {
        List<OrganisationDescriptor> listOfOrganisationHistory = new List<OrganisationDescriptor>()
        {
            new OrganisationDescriptor(){ tag = "Accomplishment"},  //Org completed one of it's previous goals
            new OrganisationDescriptor(){ tag = "Allies"},          //Org gained trust and support of another org or important figure
            new OrganisationDescriptor(){ tag = "Betrayal"},        //Org suffered badly when betrayed. Members seek to avenge this event
            new OrganisationDescriptor(){ tag = "Changing Of the Guard"},   //Change in important leadership (natural/unnatural) which caused a change in direction
            new OrganisationDescriptor(){ tag = "Coup"},            //A new leader took power. This has hardened the survivors and solidified the method of choosing new leadership (to prevent another coup or to secure power)
            new OrganisationDescriptor(){ tag = "Dark Days"},       //Org survived a period when nothing went its way. This sobering event was transformational for the org moving forward
            new OrganisationDescriptor(){ tag = "Decline"},         //Org lost power/influence/prestige over a long time period fostering a strong sense of regaining these things and restoring the glory days of old
            new OrganisationDescriptor(){ tag = "Enemies"},         //The Org's greateness is defined by the quality of it's enemies. This new enemy has become a point of pride for the Org's membership
            new OrganisationDescriptor(){ tag = "Foolishness"},     //The Org tries to remember and avoid a particularly stupid action in it's past
            new OrganisationDescriptor(){ tag = "Golden Age"},      //Org experienced a period of greateness, a highwater mark that inspires it's current membership
            new OrganisationDescriptor(){ tag = "Great Leader"},    //Org reveres a past leader as the paragon of all it stands for. Members hope for another leader of a similar calibre
            new OrganisationDescriptor(){ tag = "Growth"},          //Org has experienced growth by a slow and steady expansion over a long period of time
            new OrganisationDescriptor(){ tag = "Prophecy"},        //Org has received information that informs it's expectations about future events (not necessarily a religious turning point, eg. could be an astute analysis)
            new OrganisationDescriptor(){ tag = "Persecuted"},      //Org has endured mistreatment or violence against it, or strongly identifies with another group who has done so
            new OrganisationDescriptor(){ tag = "Rise From the Ashes"}, //Org was shutdown/disbanded/destroyed at some point, yet managed to renew itself, rising from the ruins convincing members of org's near-immortality
            new OrganisationDescriptor(){ tag = "Respected"},       //Org has received accolades from other orgs or individuals from who it harbours great respect
            new OrganisationDescriptor(){ tag = "Revolt"},          //Org's members at some point overthrew the leadership replacing them with their own. This served to forge strong links between leaders and members
            new OrganisationDescriptor(){ tag = "Rivals"},          //Rivalry with a counterpoint, perhaps not overtly, drives the current members to excel, motivating them to outperform the other and prove their superiority
            new OrganisationDescriptor(){ tag = "Triumphant"},      //Org experienced a spectacular success, far beyond their expectations. This shining moment has become a cornerstone of the organisation's pride
            new OrganisationDescriptor(){ tag = "Victory"}          //Everybody wants to be on the winning side. The Org has consistently been so in the past.
        };
        ToolManager.i.toolDataScript.SetListOfOrganisationHistory(listOfOrganisationHistory);
    }
    #endregion

    #region InitialiseOrganisationLeadership
    /// <summary>
    /// Organisation Leadership
    /// </summary>
    private void InitialiseOrganisationLeadership()
    {
        List<OrganisationDescriptor> listOfOrganisationLeadership = new List<OrganisationDescriptor>()
        {
            new OrganisationDescriptor(){ tag = "A higher power"},  //percieved or real
            new OrganisationDescriptor(){ tag = "Dictatorship"},
            new OrganisationDescriptor(){ tag = "Elites"},          //a small group
            new OrganisationDescriptor(){ tag = "Council"},         //elected or self appointed
            new OrganisationDescriptor(){ tag = "Democracy"},
            new OrganisationDescriptor(){ tag = "Hierarchical"},
            new OrganisationDescriptor(){ tag = "Hereditary"},
            new OrganisationDescriptor(){ tag = "Guru"},            //charismatic individual
            new OrganisationDescriptor(){ tag = "A.I"},              //artificial intelligence
            new OrganisationDescriptor(){ tag = "Mysterious"},
        };
        ToolManager.i.toolDataScript.SetListOfOrganisationLeadership(listOfOrganisationLeadership);
    }
    #endregion

    #region InitialiseOrganisationMotivation
    /// <summary>
    /// Organisation Motivation
    /// </summary>
    private void InitialiseOrganisationMotivation()
    {
        List<OrganisationDescriptor> listOfOrganisationMotivation = new List<OrganisationDescriptor>()
        {
            new OrganisationDescriptor(){ tag = "Power"},
            new OrganisationDescriptor(){ tag = "Wealth"},
            new OrganisationDescriptor(){ tag = "Influence"},
            new OrganisationDescriptor(){ tag = "Status Quo"},
            new OrganisationDescriptor(){ tag = "Revolution"},
            new OrganisationDescriptor(){ tag = "Destruction"},
            new OrganisationDescriptor(){ tag = "Anarchy"},
            new OrganisationDescriptor(){ tag = "Evolution"},
            new OrganisationDescriptor(){ tag = "Meglomania"},
            new OrganisationDescriptor(){ tag = "Protection"},
            new OrganisationDescriptor(){ tag = "Visionaries"},
            new OrganisationDescriptor(){ tag = "Terrorism"},
            new OrganisationDescriptor(){ tag = "Takeover"},
            new OrganisationDescriptor(){ tag = "Stability"},
            new OrganisationDescriptor(){ tag = "Revelation"},
            new OrganisationDescriptor(){ tag = "Genocide"},
            new OrganisationDescriptor(){ tag = "Credibility"},
            new OrganisationDescriptor(){ tag = "Counter"},
            new OrganisationDescriptor(){ tag = "Holy Grail"},
            new OrganisationDescriptor(){ tag = "Redemption"},
        };
        ToolManager.i.toolDataScript.SetListOfOrganisationMotivation(listOfOrganisationMotivation);
    }
    #endregion

    #region InitialiseOrganisationMethod
    /// <summary>
    /// Organisation Methodology / Persuasion
    /// </summary>
    private void InitialiseOrganisationMethod()
    {
        List<OrganisationDescriptor> listOfOrganisationMethod = new List<OrganisationDescriptor>()
        {
            new OrganisationDescriptor(){ tag = "Legal"},                   //Within the legal framework of the current culture or society
            new OrganisationDescriptor(){ tag = "Overt Violence"},          //overt violence; war, raids, ceremonial or gladiatorial combat
            new OrganisationDescriptor(){ tag = "Hidden Violence"},         //assassinations, secret raids
            new OrganisationDescriptor(){ tag = "Bribery"},                 //bribery and corruption
            new OrganisationDescriptor(){ tag = "Disharmony"},              //sow distrust and disharmony through backdoor diplomacy
            new OrganisationDescriptor(){ tag = "Manipulation"},            //manipulation or leverage using secret information
            new OrganisationDescriptor(){ tag = "Consensus"},               //negotiate a joint consensus position
            new OrganisationDescriptor(){ tag = "Third Parties"},           //hide behind third parties which are used to achieve their aims, never directly intervene
            new OrganisationDescriptor(){ tag = "PsyOps"},                  //uses all available tools to sway opinions by fair means or foul
            new OrganisationDescriptor(){ tag = "Long View"}               //takes the long view and is willing to take short term setbacks in pursuit of long term goals
        };
        ToolManager.i.toolDataScript.SetListOfOrganisationMethod(listOfOrganisationMethod);
    }
    #endregion

    #region InitialiseOrganisationStrength
    /// <summary>
    /// Organisation Strength
    /// </summary>
    private void InitialiseOrganisationStrength()
    {
        List<OrganisationDescriptor> listOfOrganisationStrength = new List<OrganisationDescriptor>()
        {
            new OrganisationDescriptor(){ tag = "Decisive"},                //Act decisively with no hesitation when an opportunity presents itself
            new OrganisationDescriptor(){ tag = "Planners"},                //Meticulous Planning that accounts for all possibilities and outcomes
            new OrganisationDescriptor(){ tag = "Decentralised"},           //A network of associates, hard to shut down
            new OrganisationDescriptor(){ tag = "Allies"},                  //have supportive and powerful allies
            new OrganisationDescriptor(){ tag = "Resources"},               //have access to valuable resources above and beyond what would be normally expected
            new OrganisationDescriptor(){ tag = "Fanatical"},               //Will stop at nothing to achieve their goals
            new OrganisationDescriptor(){ tag = "Efficient"},               //Highly efficient at whatever they do
            new OrganisationDescriptor(){ tag = "Adaptable"},               //Fast on their feet, can adapt to changing circumstances with ease
            new OrganisationDescriptor(){ tag = "Connections"},             //Have powerful connections that they can draw on
            new OrganisationDescriptor(){ tag = "Reputation"},              //Have a strong reputation (honesty/fear/relentless/etc..
        };
        ToolManager.i.toolDataScript.SetListOfOrganisationStrength(listOfOrganisationStrength);
    }
    #endregion

    #region InitialiseOrganisationWeakness
    /// <summary>
    /// Organisation weakness
    /// </summary>
    private void InitialiseOrganisationWeakness()
    {
        List<OrganisationDescriptor> listOfOrganisationWeakness = new List<OrganisationDescriptor>()
        {
            new OrganisationDescriptor(){ tag = "Ticking Clock"},           //running out of time
            new OrganisationDescriptor(){ tag = "Skill Shortage"},          //need more or specific people
            new OrganisationDescriptor(){ tag = "Resource Shortage"},       //need more of a specific resource
            new OrganisationDescriptor(){ tag = "Spanner in the Works"},    //something unexpected has gone wrong, or not gone to plan
            new OrganisationDescriptor(){ tag = "Arousing Suspicion"},      //there is increasing suspicion of the organisation by the outside world
            new OrganisationDescriptor(){ tag = "Fifth Element"},           //leaks, traitors, spies, informants are white anting the organisation
            new OrganisationDescriptor(){ tag = "Internal Dissension"},     //disputed leadership, faction conflicts, no agreement on direction
            new OrganisationDescriptor(){ tag = "Poor Morale"},             //poor internal morale
            new OrganisationDescriptor(){ tag = "Poor Management"},
            new OrganisationDescriptor(){ tag = "Poor Communication"},
        };
        ToolManager.i.toolDataScript.SetListOfOrganisationWeakness(listOfOrganisationWeakness);
    }
    #endregion

    #region InitialiseOrganisationObstacle
    /// <summary>
    /// Organisation obstacle preventing them from achieving their goal
    /// </summary>
    private void InitialiseOrganisationObstacle()
    {
        List<OrganisationDescriptor> listOfOrganisationObstacle = new List<OrganisationDescriptor>()
        {
            new OrganisationDescriptor(){ tag = "Direct Competition"},      //another org wants the same thing
            new OrganisationDescriptor(){ tag = "Direct Opposition"},       //another org is actively trying to stop them (but doesn't want the same thing)
            new OrganisationDescriptor(){ tag = "Accidental"},              //another org is accidentally interfering and working against them
            new OrganisationDescriptor(){ tag = "Environmental"},           //Something in the enviroment is disrupting them severely (disaster, ongoing effect, etc)
            new OrganisationDescriptor(){ tag = "Political"},               //There is a significant political obstacle
            new OrganisationDescriptor(){ tag = "Vulnerability"},           //Another person or organisation is exploiting their/a weakness for their own purposes
            new OrganisationDescriptor(){ tag = "Financial"},               //There is a significant financial obstacle to overcome
            new OrganisationDescriptor(){ tag = "Committment"},             //A web pf/single alliance/committment/conflicted loyalties is inhibiting them from eaching their goal
            new OrganisationDescriptor(){ tag = "Distractions"},            //They are constantly getting sidetracked fromt their goal
            new OrganisationDescriptor(){ tag = "Unseen"},                  //An unseen and unknown force/org/individual is actively hindering the organisation
        };
        ToolManager.i.toolDataScript.SetListOfOrganisationObstacle(listOfOrganisationObstacle);
    }
    #endregion


    /*
    #region InitialiseOrganisationOrigin
    /// <summary>
    /// Organisation origin
    /// </summary>
    private void InitialiseOrganisationOrigin()
    {
        List<OrganisationDescriptor> listOfOrganisationOrigin = new List<OrganisationDescriptor>()
        {
            new OrganisationDescriptor(){ tag = ""},
            new OrganisationDescriptor(){ tag = ""},
            new OrganisationDescriptor(){ tag = ""},
            new OrganisationDescriptor(){ tag = ""},
            new OrganisationDescriptor(){ tag = ""},
            new OrganisationDescriptor(){ tag = ""},
            new OrganisationDescriptor(){ tag = ""},
            new OrganisationDescriptor(){ tag = ""},
            new OrganisationDescriptor(){ tag = ""},
            new OrganisationDescriptor(){ tag = ""},
        };
        ToolManager.i.toolDataScript.SetListOfOrganisationOrigin(listOfOrganisationOrigin);
    }
    #endregion
    */

    //new methods above here
}
