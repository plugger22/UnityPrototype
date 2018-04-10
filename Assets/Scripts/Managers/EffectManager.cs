using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using packageAPI;
using System.Text;


/// <summary>
/// handles Effect related matters (Actions and Targets)
/// Effects are assumed to generate text and outcomes for a Modal Outcome window
/// </summary>
public class EffectManager : MonoBehaviour
{
    [Tooltip("How long do ongoing effects last for? Global setting")]
    [Range(3,20)] public int ongoingEffectTimer = 3;

    //colour palette for Modal Outcome
    private string colourGood; //good effect Rebel / bad effect Authority
    private string colourBad; //bad effect Authority / bad effect Rebel
    private string colourNeutral; //used when node is EqualsTo, eg. reset, or for Team Names
    private string colourNormal;
    private string colourDefault;
    private string colourAlert;
    private string colourActor;
    private string colourEnd;

    [HideInInspector] private static int ongoingEffectIDCounter = 0;              //used to sequentially number ongoing Effect ID's

    public void Initialise()
    {
        
        //register listener
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
    }

    /// <summary>
    /// Event Handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //select event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }


    /// <summary>
    /// set colour palette for modal Outcome Window
    /// </summary>
    public void SetColours()
    {
        //output colours for good and bad depend on player side
        switch (GameManager.instance.sideScript.PlayerSide.name)
        {
            case "Resistance":
                colourGood = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
                colourBad = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
                break;
            case "Authority":
                colourGood = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
                colourBad = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", GameManager.instance.sideScript.PlayerSide.name));
                break;
        }
        colourNeutral = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourAlert = GameManager.instance.colourScript.GetColour(ColourType.alertText);
        colourActor = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }

    /// <summary>
    /// checks whether effect criteria is valid. Returns "Null" if O.K and a tooltip explanation string if not. 
    /// Can be used independently of Effects provided you have a listOfCriteria
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public string CheckCriteria(CriteriaDataInput data)
    {
        int val;
        StringBuilder result = new StringBuilder();
        string compareTip = null;
        Node node = null;
        bool errorFlag = false;
        Actor actor = null;
        TeamArc teamArc = null;
        if (data.listOfCriteria != null && data.listOfCriteria.Count > 0)
        {
            //
            // - - - access necessary data prior to loop
            //
            //Get node if required (eg., if "CurrentActor" then not)
            if (data.nodeID > -1)
            {
                node = GameManager.instance.dataScript.GetNode(data.nodeID);
                if (node == null)
                { Debug.LogError("Invalid node (null)"); errorFlag = true; }
            }
            //Get actor if required
            if (data.actorSlotID != -1)
            {
                //get actor
                actor = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, GameManager.instance.sideScript.PlayerSide);
                if (actor == null)
                {
                    Debug.LogError("Invalid actorSlotID -> Criteria Check cancelled");
                    errorFlag = true;
                }
            }
            //Get TeamArc if required
            if (data.teamArcID > -1)
            {
                //get team
                teamArc = GameManager.instance.dataScript.GetTeamArc(data.teamArcID);
                if (teamArc == null)
                {
                    Debug.LogError(string.Format("Invalid TeamArc (null) for teamArcID \"{0}\" -> Criteria check cancelled", data.teamArcID));
                    errorFlag = true;
                }
            }
            //O.K to proceed?
            if (errorFlag == false)
            {
                foreach (Criteria criteria in data.listOfCriteria)
                {
                    if (criteria != null)
                    {
                        if (criteria.apply != null)
                        {
                            switch (criteria.apply.name)
                            {
                                //apply to current Node
                                case "NodeCurrent":
                                    if (node != null)
                                    {
                                        if (criteria.effectCriteria != null)
                                        {
                                            switch (criteria.effectCriteria.name)
                                            {
                                                case "NodeSecurityMin":
                                                    val = GameManager.instance.nodeScript.minNodeValue;
                                                    compareTip = ComparisonCheck(val, node.Security, criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Security " + compareTip); }
                                                    break;
                                                case "NodeStabilityMin":
                                                    val = GameManager.instance.nodeScript.minNodeValue;
                                                    compareTip = ComparisonCheck(val, node.Stability, criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Stability " + compareTip); }
                                                    break;
                                                case "NodeSupportMax":
                                                    val = GameManager.instance.nodeScript.maxNodeValue;
                                                    compareTip = ComparisonCheck(val, node.Support, criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Support " + compareTip); }
                                                    break;
                                                case "NumTeamsMin":
                                                    val = GameManager.instance.teamScript.minTeamsAtNode;
                                                    compareTip = ComparisonCheck(val, node.CheckNumOfTeams(), criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "no Teams present"); }
                                                    break;
                                                case "NumTeamsMax":
                                                    val = GameManager.instance.teamScript.maxTeamsAtNode;
                                                    compareTip = ComparisonCheck(val, node.CheckNumOfTeams(), criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Max teams present"); }
                                                    break;
                                                case "NumTracers":
                                                    if (node.isTracer == true)
                                                    { BuildString(result, "Tracer already present"); }
                                                    break;
                                                case "TargetInfoMax":
                                                    val = GameManager.instance.targetScript.maxTargetInfo;
                                                    compareTip = ComparisonCheck(val, node.targetID, criteria.comparison);
                                                    if (compareTip != null)
                                                    { BuildString(result, "Full Info already"); }
                                                    break;
                                                case "TargetPresent":
                                                    //check that a target is present at the node
                                                    if (node.targetID < 0)
                                                    { BuildString(result, "No Target present"); }
                                                    break;
                                                case "TeamActorAbility":
                                                    //actor can only have a number of teams OnMap equal to their ability at any time
                                                    if (actor != null)
                                                    {
                                                        if (actor.CheckNumOfTeams() >= actor.datapoint2)
                                                        { BuildString(result, "Actor Ability exceeded"); }
                                                    }
                                                    else
                                                    { Debug.LogError(string.Format("Invalid Actor (Null) for criteria \"{0}\"", criteria.name)); errorFlag = true; }
                                                    break;
                                                case "TeamIdentical":
                                                    //there can only be one team of a type at a node
                                                    if (node.CheckTeamPresent(data.teamArcID) > -1)
                                                    { BuildString(result, string.Format(" {0} Team already present", teamArc.name)); }
                                                    break;
                                                case "TeamPreferred":
                                                    //there must be a spare team in the reserve pool of the actors preferred type
                                                    if (teamArc != null)
                                                    {
                                                        if (GameManager.instance.dataScript.CheckTeamInfo(data.teamArcID, TeamInfo.Reserve) < 1)
                                                        { BuildString(result, string.Format("No {0} Team available", teamArc.name)); }
                                                    }
                                                    else
                                                    { Debug.LogError(string.Format("Invalid teamArc (null) for criteria \"{0}\"", criteria.name)); errorFlag = true; }
                                                    break;
                                                case "TeamAny":
                                                    //there must be a spare team of any type in the reserve pool
                                                    if (teamArc != null)
                                                    {
                                                        if (GameManager.instance.dataScript.CheckTeamPoolCount(TeamPool.Reserve) < 1)
                                                        { BuildString(result, string.Format("No Teams available", teamArc.name)); }
                                                    }
                                                    else
                                                    { Debug.LogError(string.Format("Invalid teamArc (null) for criteria \"{0}\"", criteria.name)); errorFlag = true; }
                                                    break;
                                                default:
                                                    BuildString(result, "Error!");
                                                    Debug.LogWarning(string.Format("NodeCurrent: Invalid criteriaEffect \"{0}\"", criteria.effectCriteria.name));
                                                    errorFlag = true;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogError(string.Format("Invalid criteria.effectCriteria (Null) for criteria {0}", criteria.name));
                                            errorFlag = true;
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError(string.Format("Invalid node (null) for criteria \"{0}\"", criteria.name));
                                        errorFlag = true;
                                    }
                                    break;
                                //Apply to neighbouring nodes
                                case "NodeNeighbours":
                                    if (node != null)
                                    {
                                        if (criteria.effectCriteria != null)
                                        {
                                            switch (criteria.effectCriteria.name)
                                            {
                                                case "NodeStabilityMin":
                                                    //at least one neighbouring node must have stability > 0
                                                    val = GameManager.instance.nodeScript.minNodeValue;
                                                    List<Node> listOfNeighbouringNodes = node.GetNeighbouringNodes();
                                                    if (listOfNeighbouringNodes != null)
                                                    {
                                                        bool atLeastOneOK = false;
                                                        //loop neighbouring nodes (current node excluded)
                                                        foreach (Node nearNode in listOfNeighbouringNodes)
                                                        {
                                                            compareTip = ComparisonCheck(val, nearNode.Stability, criteria.comparison);
                                                            if (compareTip == null)
                                                            { atLeastOneOK = true; break; }
                                                        }
                                                        if (atLeastOneOK == false)
                                                        { BuildString(result, "Near Nodes Stability > 0"); }
                                                    }
                                                    else { Debug.LogError("Invalid listOfNeighbouringNodes (Null)"); errorFlag = true; }
                                                    break;
                                                default:
                                                    BuildString(result, "Error!");
                                                    Debug.LogWarning(string.Format("NodeNeighbours: Invalid effect.criteriaEffect \"{0}\"", criteria.effectCriteria.name));
                                                    errorFlag = true;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogError(string.Format("Invalid criteria.effectCriteria (Null) for criteria {0}", criteria.name));
                                            errorFlag = true;
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError(string.Format("Invalid node (null) for criteria \"{0}\"", criteria.name));
                                        errorFlag = true;
                                    }
                                    break;
                                //Current Actor or Player
                                case "ActorCurrent":
                                    if (criteria.effectCriteria != null)
                                    {
                                        int playerRenown;
                                        switch (criteria.effectCriteria.name)
                                        {
                                            /*case "ActionsRemainingYes":
                                                //player has at least one action remaining
                                                if (GameManager.instance.turnScript.CheckRemainingActions() == false)
                                                { BuildString(result, "All Actions used this turn"); }
                                                break;*/
                                            case "ConditionStressedYes":
                                                //actor has the 'Stressed' condition
                                                if (actor != null)
                                                {
                                                    Condition condition = GameManager.instance.dataScript.GetCondition("STRESSED");
                                                    if (condition != null)
                                                    {
                                                        if (actor.CheckConditionPresent(condition) == false)
                                                        { BuildString(result, string.Format(" {0} needs to be STRESSED", actor.actorName)); }
                                                    }
                                                    else { Debug.LogError("Invalid condition (Null) for STRESSED"); errorFlag = true; }
                                                }
                                                else
                                                { Debug.LogError(string.Format("Invalid actor (Null) for criteria \"{0}\"", criteria.name)); errorFlag = true; }
                                                break;
                                            case "ConditionCorruptYes":
                                                if (actor != null)
                                                {
                                                    Condition condition = GameManager.instance.dataScript.GetCondition("CORRUPT");
                                                    if (condition != null)
                                                    {
                                                        if (actor.CheckConditionPresent(condition) == false)
                                                        { BuildString(result, string.Format(" {0} needs to be CORRUPT", actor.actorName)); }
                                                    }
                                                    else { Debug.LogError("Invalid condition (Null) for CORRUPT"); errorFlag = true; }
                                                }
                                                else
                                                { Debug.LogError(string.Format("Invalid actor (Null) for criteria \"{0}\"", criteria.name)); errorFlag = true; }
                                                break;
                                            case "ConditionIncompetentYes":
                                                if (actor != null)
                                                {
                                                    Condition condition = GameManager.instance.dataScript.GetCondition("INCOMPETENT");
                                                    if (condition != null)
                                                    {
                                                        if (actor.CheckConditionPresent(condition) == false)
                                                        { BuildString(result, string.Format(" {0} needs to be INCOMPETENT", actor.actorName)); }
                                                    }
                                                    else { Debug.LogError("Invalid condition (Null) for INCOMPETENT"); errorFlag = true; }
                                                }
                                                else
                                                { Debug.LogError(string.Format("Invalid actor (Null) for criteria \"{0}\"", criteria.name)); errorFlag = true; }
                                                break;
                                            case "ConditionQuestionableYes":
                                                if (actor != null)
                                                {
                                                    Condition condition = GameManager.instance.dataScript.GetCondition("QUESTIONABLE");
                                                    if (condition != null)
                                                    {
                                                        if (actor.CheckConditionPresent(condition) == false)
                                                        { BuildString(result, string.Format(" {0} needs to have QUESTIONABLE loyalty", actor.actorName)); }
                                                    }
                                                    else { Debug.LogError("Invalid condition (Null) for QUESTIONABLE"); errorFlag = true; }
                                                }
                                                else
                                                { Debug.LogError(string.Format("Invalid actor (Null) for criteria \"{0}\"", criteria.name)); errorFlag = true; }
                                                break;
                                            case "ConditionStarYes":
                                                if (actor != null)
                                                {
                                                    Condition condition = GameManager.instance.dataScript.GetCondition("STAR");
                                                    if (condition != null)
                                                    {
                                                        if (actor.CheckConditionPresent(condition) == false)
                                                        { BuildString(result, string.Format(" {0} needs to be a STAR", actor.actorName)); }
                                                    }
                                                    else { Debug.LogError("Invalid condition (Null) for STAR"); errorFlag = true; }
                                                }
                                                else
                                                { Debug.LogError(string.Format("Invalid actor (Null) for criteria \"{0}\"", criteria.name)); errorFlag = true; }
                                                break;
                                            case "RenownReserveMin":
                                                int renownReserve = GameManager.instance.actorScript.manageReserveRenown;
                                                playerRenown = GameManager.instance.playerScript.Renown;
                                                if (playerRenown < renownReserve)
                                                { BuildString(result, string.Format("You need at least {0} Renown (currently {1})", renownReserve, playerRenown)); }
                                                break;
                                            case "RenownDismissMin":
                                                int renownDismiss = GameManager.instance.actorScript.manageDismissRenown;
                                                playerRenown = GameManager.instance.playerScript.Renown;
                                                if (playerRenown < renownDismiss)
                                                { BuildString(result, string.Format("You need at least {0} Renown (currently {1})", renownDismiss, playerRenown)); }
                                                break;
                                            case "RenownDisposeMin":
                                                int renownDispose = GameManager.instance.actorScript.manageDisposeRenown;
                                                playerRenown = GameManager.instance.playerScript.Renown;
                                                if (playerRenown < renownDispose)
                                                { BuildString(result, string.Format("You need at least {0} Renown (currently {1})", renownDispose, playerRenown)); }
                                                break;
                                            case "NumRecruitsCurrent":
                                                //check max. number of recruits in reserve pool not exceeded
                                                val = GameManager.instance.dataScript.CheckNumOfActorsInReserve();
                                                compareTip = ComparisonCheck(GameManager.instance.actorScript.maxNumOfReserveActors, val, criteria.comparison);
                                                if (compareTip != null)
                                                { BuildString(result, "maxxed Recruit allowance"); }
                                                break;
                                            case "NumGearMax":
                                                //Note: effect criteria value is ignored in this case
                                                val = GameManager.instance.gearScript.maxNumOfGear;
                                                compareTip = ComparisonCheck(val, GameManager.instance.playerScript.CheckNumOfGear(), criteria.comparison);
                                                if (compareTip != null)
                                                { BuildString(result, "maxxed Gear Allowance"); }
                                                break;
                                            case "GearAvailability":
                                                //checks to see if at least 1 piece of unused common gear is available
                                                List<int> tempCommonGear = new List<int>(GameManager.instance.dataScript.GetListOfGear(GameManager.instance.gearScript.gearCommon));
                                                if (tempCommonGear.Count > 0)
                                                {
                                                    //remove from lists any gear that the player currently has
                                                    List<int> tempPlayerGear = new List<int>(GameManager.instance.playerScript.GetListOfGear());
                                                    int gearID;
                                                    if (tempPlayerGear.Count > 0)
                                                    {
                                                        for (int i = 0; i < tempPlayerGear.Count; i++)
                                                        {
                                                            gearID = tempPlayerGear[i];
                                                            if (tempCommonGear.Exists(id => id == gearID) == true)
                                                            { tempCommonGear.Remove(gearID); }
                                                        }
                                                    }
                                                    if (tempCommonGear.Count == 0)
                                                    { BuildString(result, "No Gear available"); }
                                                }
                                                else { BuildString(result, "No Gear available"); }
                                                break;
                                            case "RebelCauseMin":
                                                val = GameManager.instance.rebelScript.resistanceCauseMin;
                                                compareTip = ComparisonCheck(val, GameManager.instance.rebelScript.resistanceCause, criteria.comparison);
                                                if (compareTip != null)
                                                { BuildString(result, "Rebel Cause  " + compareTip); }
                                                break;
                                            default:
                                                BuildString(result, "Error!");
                                                Debug.LogWarning(string.Format("ActorCurrent: Invalid effect.criteriaEffect \"{0}\"", criteria.effectCriteria.name));
                                                errorFlag = true;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError(string.Format("Invalid criteria.effectCriteria (Null) for criteria {0}", criteria.name));
                                        errorFlag = true;
                                    }
                                    break;
                                default:
                                    BuildString(result, "Error!");
                                    Debug.LogWarning(string.Format("Invalid effect.criteria.apply \"{0}\"", criteria.apply.name));
                                    errorFlag = true;
                                    break;
                            }
                        }
                        else
                        {
                            Debug.LogError(string.Format("Invalid criteria.apply (null) for criteria {0}", criteria.name));
                            errorFlag = true;
                        }
                    }
                    else
                    {
                        Debug.LogError("Invalid criteria (Null)");
                        errorFlag = true;
                    }
                    //exit on error
                    if (errorFlag == true)
                    { break; }
                }
            }
        }
        if (result.Length > 0)
        { return result.ToString(); }
        else { return null; }
    }

    /// <summary>
    /// subMethod to handle stringbuilder admin for CheckEffectCriteria()
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="text1"></param>
    private void BuildString(StringBuilder builder, string text1)
    {
        if (string.IsNullOrEmpty(text1) == false)
        {
            if (builder.Length > 0)
            { builder.AppendLine(); }
            builder.Append(text1);
        }
    }

    /// <summary>
    /// returns null if all O.K and a tool tip string if not giving criteria, eg. "< 1"
    /// </summary>
    /// <param name="criteriaValue"></param>
    /// <param name="actualValue"></param>
    /// <param name="comparison"></param>
    /// <returns></returns>
    private string ComparisonCheck(int criteriaValue, int actualValue, EffectOperator comparison)
    {
        string result = null;
        switch (comparison.name)
        {
            case "LessThan":
                if (actualValue >= criteriaValue)
                { result = string.Format("< {0}, currently {1}", criteriaValue, actualValue); }
                break;
            case "GreaterThan":
                if (actualValue <= criteriaValue)
                { result = string.Format("> {0}, currently {1}", criteriaValue, actualValue); }
                break;
            case "EqualTo":
                if (criteriaValue != actualValue)
                { result = string.Format("{0}, currently {1}", criteriaValue, actualValue); }
                break;
            default:
                result = "Error!";
                Debug.LogError(string.Format("Invalid Comparison \"{0}\"", comparison.name));
                break;
        }
        return result;
    }


    /// <summary>
    /// Processes effects and returns results in a class. 
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="node"></param>
    /// <param name="dataInput">data package from calling method</param>
    /// <param name="actor">if Resistance and Player effects then go with default actor = null, otherwise supply an actor</param>
    /// <returns></returns>
    public EffectDataReturn ProcessEffect(Effect effect, Node node, EffectDataInput dataInput, Actor actor = null)
    {
        int teamID, teamArcID;
        EffectDataReturn effectReturn = new EffectDataReturn();
        //set default values
        effectReturn.errorFlag = false;
        effectReturn.topText = "";
        effectReturn.bottomText = "";
        effectReturn.isAction = false;
        //valid effect?
        if (effect != null)
        {
            //sort out colour based on type (which is effect benefit from POV of Resistance)
            string colourEffect = colourDefault;
            string colourText = colourDefault;
            if (effect.typeOfEffect != null)
            {
                switch (effect.typeOfEffect.name)
                {
                    case "Good":
                        colourEffect = colourGood;
                        break;
                    case "Neutral":
                        colourEffect = colourNeutral;
                        colourText = colourNeutral;
                        break;
                    case "Bad":
                        colourEffect = colourBad;
                        colourText = colourAlert;
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name));
                        break;
                }
            }
            else { Debug.LogWarning(string.Format("Invalid typeOfEffect (Null) for \"{0}\"", effect.name)); }
            //what's affected?
            switch (effect.outcome.name)
            {
                //
                // - - - Node Data effects (both sides)
                //
                case "StatusSpiders":
                case "StatusTracers":
                case "StatusTeams":
                case "StatusContacts":
                case "NodeSecurity":
                case "NodeStability":
                case "NodeSupport":
                    if (node != null)
                    {
                        EffectDataResolve resolve = ResolveNodeData(effect, node, dataInput);
                        if (resolve.isError == true)
                        { effectReturn.errorFlag = true; }
                        else
                        {
                            effectReturn.topText = resolve.topText;
                            effectReturn.bottomText = resolve.bottomText;
                            effectReturn.isAction = true;
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                        effectReturn.errorFlag = true;
                    }
                    break;
                //
                // - - - Conditions (both sides) - - -
                //
                case "ConditionStressed":
                case "ConditionIncompetent":
                case "ConditionCorrupt":
                case "ConditionQuestionable":
                case "ConditionStar":
                case "ConditionGroupBad":
                case "ConditionGroupGood":
                    if (node != null)
                    {

                        //it's OK to pass a null actor provided it's a player condition
                        EffectDataResolve resolve = ResolveConditionData(effect, node, actor);
                        if (resolve.isError == true)
                        { effectReturn.errorFlag = true; }
                        else
                        {
                            effectReturn.topText = resolve.topText;
                            effectReturn.bottomText = resolve.bottomText;
                            effectReturn.isAction = true;
                        }

                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                        effectReturn.errorFlag = true;
                    }
                    break;
                //
                // - - - Player Actions - - -
                //
                case "PlayerActions":
                    EffectDataResolve resolvePlayer = ResolvePlayerData(effect);
                    if (resolvePlayer.isError == true)
                    { effectReturn.errorFlag = true; }
                    else
                    {
                        effectReturn.topText = resolvePlayer.topText;
                        effectReturn.bottomText = resolvePlayer.bottomText;
                        effectReturn.isAction = true;
                    }
                    break;
                //
                // - - - Manage - - -
                //
                case "ActorDismissed":
                case "ActorDisposedOff":
                case "ActorPromoted":
                case "ActorToReserves":
                case "ManageDismissRenown":
                case "ManageDisposeRenown":
                case "ManageReserveRenown":
                case "UnhappyTimerNoPromise":
                case "UnhappyTimerPromise":
                case "UnhappyTimerRest":
                    if (actor != null)
                    {
                        EffectDataResolve resolve = ResolveManageData(effect, actor);
                        if (resolve.isError == true)
                        { effectReturn.errorFlag = true; }
                        else
                        {
                            effectReturn.topText = resolve.topText;
                            effectReturn.bottomText = resolve.bottomText;
                            effectReturn.isAction = true;
                        }
                    }
                    break;
                //
                // - - - Other - - -
                //
                case "Recruit":
                    //no effect, handled directly elsewhere (check ActorManager.cs -> GetNodeActions
                    break;
                case "ConnectionSecurity":
                    if (node != null)
                    {
                        EffectDataResolve resolve = ResolveConnectionData(effect, node, dataInput);
                        if (resolve.isError == true)
                        { effectReturn.errorFlag = true; }
                        else
                        {
                            effectReturn.topText = resolve.topText;
                            effectReturn.bottomText = resolve.bottomText;
                            effectReturn.isAction = true;
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                        effectReturn.errorFlag = true;
                    }
                    break;
                case "Motivation":
                    switch (effect.operand.name)
                    {
                        case "Add":
                            actor.datapoint1 += effect.value;
                            actor.datapoint1 = Mathf.Min(GameManager.instance.actorScript.maxStatValue, actor.datapoint1);
                            effectReturn.bottomText = string.Format("{0}{1} {2}{3}", colourEffect, actor.actorName, effect.textTag, colourEnd);
                            break;
                        case "Subtract":
                            actor.datapoint1 -= effect.value;
                            actor.datapoint1 = Mathf.Max(0, actor.datapoint1);
                            effectReturn.bottomText = string.Format("{0}{1} {2}{3}", colourEffect, actor.actorName, effect.textTag, colourEnd);
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                            effectReturn.errorFlag = true;
                            break;
                    }
                    
                    break;
                //
                // - - - Resistance effects
                //
                case "RebelCause":
                    int rebelCause = GameManager.instance.rebelScript.resistanceCause;
                    int maxCause = GameManager.instance.rebelScript.resistanceCauseMax;
                    switch (effect.operand.name)
                    {
                        case "Add":
                            rebelCause += effect.value;
                            rebelCause = Mathf.Min(maxCause, rebelCause);
                            GameManager.instance.rebelScript.resistanceCause = rebelCause;
                            effectReturn.topText = string.Format("{0}The Rebel Cause gains traction{1}", colourText, colourEnd);
                            effectReturn.bottomText = string.Format("{0}Rebel Cause +{1}{2}", colourEffect, effect.value, colourEnd);
                            break;
                        case "Subtract":
                            rebelCause -= effect.value;
                            rebelCause = Mathf.Max(0, rebelCause);
                            GameManager.instance.rebelScript.resistanceCause = rebelCause;
                            effectReturn.topText = string.Format("{0}The Rebel Cause is losing ground{1}", colourText, colourEnd);
                            effectReturn.bottomText = string.Format("{0}Rebel Cause -{1}{2}", colourEffect, effect.value, colourEnd);
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                            effectReturn.errorFlag = true;
                            break;
                    }
                    effectReturn.isAction = true;
                    break;

                case "Tracer":
                    if (node != null)
                    {
                        node.isTracer = true;
                        node.isTracerActive = true;
                        //neighbouring nodes made active
                        List<Node> listOfNeighbours = node.GetNeighbouringNodes();
                        if (listOfNeighbours != null)
                        {
                            foreach (Node nodeNeighbour in listOfNeighbours)
                            {
                                nodeNeighbour.isTracerActive = true;
                                if (nodeNeighbour.isSpider == true)
                                { nodeNeighbour.isSpiderKnown = true; }
                            }
                            effectReturn.isAction = true;
                        }
                        else { Debug.LogError("Invalid listOfNeighbours (Null)"); }
                        //dialogue
                        effectReturn.topText = string.Format("{0}A Tracer has been successfully inserted{1}", colourText, colourEnd);
                        effectReturn.bottomText = string.Format("{0}All Spiders within a one node radius are revealed{1}", colourEffect, colourEnd);
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                        effectReturn.errorFlag = true;
                    }
                    break;
                case "Gear":
                    //no effect, handled directly elsewhere (check ActorManager.cs -> GetNodeActions
                    break;
                case "TargetInfo":
                    //TO DO
                    break;
                case "NeutraliseTeam":
                    //no effect, handled directly elsewhere (check ActorManager.cs -> GetNodeActions
                    break;
                case "Invisibility":
                    //raise/lower invisibility of actor or Player
                    if (node != null)
                    {

                        if (node.nodeID == GameManager.instance.nodeScript.nodePlayer)
                        {
                            //
                            // - - - Player Invisibility effect - - -
                            //
                            switch (effect.operand.name)
                            {
                                case "Add":
                                    if (GameManager.instance.playerScript.invisibility < GameManager.instance.actorScript.maxStatValue)
                                    {
                                        //adds a variable amount
                                        int invis = GameManager.instance.playerScript.invisibility;
                                        invis += effect.value;
                                        invis = Mathf.Min(GameManager.instance.actorScript.maxStatValue, invis);
                                        GameManager.instance.playerScript.invisibility = invis;
                                    }
                                    effectReturn.bottomText = string.Format("{0}Player {1}{2}", colourEffect, effect.textTag, colourEnd);
                                    break;
                                case "Subtract":
                                    //does player have any invisibility type gear?
                                    int gearID = GameManager.instance.playerScript.CheckGearTypePresent(GameManager.instance.gearScript.typeInvisibility);
                                    if (gearID > -1)
                                    {
                                        //gear present -> No drop in Invisibility
                                        Gear gear = GameManager.instance.dataScript.GetGear(gearID);
                                        if (gear != null)
                                        {
                                            int chance = GameManager.instance.gearScript.GetChanceOfCompromise(gearID);
                                            if (Random.Range(0, 100) <= chance)
                                            {
                                                //gear compromised
                                                string text = string.Format("{0} used to stay Invisible ", gear.name);
                                                effectReturn.bottomText = string.Format("{0}{1}(Compromised!){2}", colourEffect, text, colourEnd);
                                                Message message = GameManager.instance.messageScript.GearCompromised(string.Format("{0}(Obtain Gear)", text), 
                                                    node.nodeID, gear.gearID);
                                                GameManager.instance.dataScript.AddMessage(message);
                                                //remove gear
                                                GameManager.instance.playerScript.RemoveGear(gearID);
                                            }
                                            else
                                            {
                                                //gear O.K
                                                effectReturn.bottomText = string.Format("{0}{1} used to stay Invisible (still O.K){2}", colourEffect, gear.name,
                                                    colourEnd);
                                            }
                                            //special invisibility gear effects
                                            switch (gear.data)
                                            {
                                                case 1:
                                                    //negates invisibility and increases it by +1 at the same time
                                                    if (GameManager.instance.playerScript.invisibility < GameManager.instance.actorScript.maxStatValue)
                                                    { GameManager.instance.playerScript.invisibility++; }
                                                    effectReturn.bottomText = string.Format("{0}{1}{2}{3}Invisibility +1 ({4}){5}", effectReturn.bottomText,
                                                        "\n", "\n", colourEffect, gear.name, colourEnd);
                                                    break;
                                            }
                                        }
                                        else { Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", gearID)); }
                                    }
                                    else
                                    {
                                        //No gear present
                                        int invisibility = GameManager.instance.playerScript.invisibility;
                                        //double effect if spider is present
                                        if (node.isSpider == true)
                                        {
                                            invisibility -= 2;
                                            effectReturn.bottomText = string.Format("{0}Player Invisibility -2 (Spider) (Now {1}){2}", colourEffect,
                                                invisibility, colourEnd);
                                        }
                                        else
                                        {
                                            invisibility -= 1;
                                            effectReturn.bottomText = string.Format("{0}Player {1} (Now {2}){3}", colourEffect, effect.textTag,
                                                invisibility, colourEnd);
                                        }
                                        //mincap zero
                                        invisibility = Mathf.Max(0, invisibility);
                                        GameManager.instance.playerScript.invisibility = invisibility;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            //
                            // - - - Actor Invisibility effect - - -
                            //
                            if (actor != null)
                            {
                                switch (effect.operand.name)
                                {
                                    case "Add":
                                        if (actor.datapoint2 < GameManager.instance.actorScript.maxStatValue)
                                        {
                                            //adds a variable amount
                                            actor.datapoint2 += effect.value;
                                            actor.datapoint2 = Mathf.Min(GameManager.instance.actorScript.maxStatValue, actor.datapoint2);
                                        }
                                        effectReturn.bottomText = string.Format("{0}{1} {2} (Now {3}){4}", colourEffect, actor.arc.name, effect.textTag,
                                            actor.datapoint2, colourEnd);
                                        break;
                                    case "Subtract":
                                        int invisibility = actor.datapoint2;
                                        //double effect if spider is present
                                        if (node.isSpider == true)
                                        {
                                            invisibility -= 2;
                                            effectReturn.bottomText = string.Format("{0}{1} Invisibility -2 (Spider) (Now {2}){3}", colourEffect, actor.arc.name,
                                                invisibility, colourEnd);
                                        }
                                        else
                                        {
                                            invisibility -= 1;
                                            effectReturn.bottomText = string.Format("{0}{1} {2} (Now {3}){4}", colourEffect, actor.arc.name, effect.textTag,
                                                invisibility, colourEnd);
                                        }
                                        //mincap zero
                                        invisibility = Mathf.Max(0, invisibility);
                                        actor.datapoint2 = invisibility;
                                        break;
                                }
                            }
                            else
                            {
                                Debug.LogError(string.Format("Invalid Actor (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                                effectReturn.errorFlag = true;
                            }

                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                        effectReturn.errorFlag = true;
                    }
                    break;
                //Note: Renown can be in increments of > 1
                case "Renown":
                    if (node != null)
                    {

                        if (node.nodeID == GameManager.instance.nodeScript.nodePlayer)
                        {
                            int playerRenown = GameManager.instance.playerScript.Renown;
                            //Player effect
                            switch (effect.operand.name)
                            {
                                case "Add":
                                    playerRenown += effect.value;
                                    effectReturn.bottomText = string.Format("{0}Player {1} (Now {2}){3}", colourGood, effect.textTag,
                                        playerRenown, colourEnd);
                                    break;
                                case "Subtract":
                                    playerRenown -= effect.value;
                                    playerRenown = Mathf.Max(0, playerRenown);
                                    effectReturn.bottomText = string.Format("{0}Player {1} (Now {2}){3}", colourBad, effect.textTag,
                                        playerRenown, colourEnd);
                                    break;
                            }
                            GameManager.instance.playerScript.Renown = playerRenown;
                        }
                        else
                        {
                            //Actor effect
                            if (actor != null)
                            {
                                switch (effect.operand.name)
                                {
                                    case "Add":
                                        actor.renown += effect.value;
                                        effectReturn.bottomText = string.Format("{0}{1} {2} (Now {3}){4}", colourBad, actor.arc.name, effect.textTag,
                                            actor.renown, colourEnd);
                                        break;
                                    case "Subtract":
                                        actor.renown -= effect.value;
                                        actor.renown = Mathf.Max(0, actor.renown);
                                        effectReturn.bottomText = string.Format("{0}{1} {2} (Now {3}){4}", colourGood, actor.arc.name, effect.textTag,
                                            actor.renown, colourEnd);
                                        break;
                                }
                            }
                            else
                            {
                                Debug.LogError(string.Format("Invalid Actor (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                                effectReturn.errorFlag = true;
                            }

                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Invalid Node (null) for EffectOutcome \"{0}\"", effect.outcome.name));
                        effectReturn.errorFlag = true;
                    }
                    break;
                //
                // - - - Authority Effects
                //
                case "AnyTeam":
                    // Not needed as handled by a different process, keep for reference
                    break;
                case "CivilTeam":
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("CIVIL");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    //insert team
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.actorSlotID, node);
                    //return texts
                    effectReturn.topText = SetTopTeamText(teamID);
                    effectReturn.bottomText = SetBottomTeamText(actor);
                    //action
                    effectReturn.isAction = true;
                    break;
                case "ControlTeam":
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("CONTROL");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.actorSlotID, node);
                    //return texts
                    effectReturn.topText = SetTopTeamText(teamID);
                    effectReturn.bottomText = SetBottomTeamText(actor);
                    effectReturn.isAction = true;
                    break;
                case "DamageTeam":
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("DAMAGE");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.actorSlotID, node);
                    //return texts
                    effectReturn.topText = SetTopTeamText(teamID);
                    effectReturn.bottomText = SetBottomTeamText(actor);
                    effectReturn.isAction = true;
                    break;
                case "ErasureTeam":
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("ERASURE");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.actorSlotID, node);
                    //return texts
                    effectReturn.topText = SetTopTeamText(teamID);
                    effectReturn.bottomText = SetBottomTeamText(actor);
                    effectReturn.isAction = true;
                    break;
                case "MediaTeam":
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("MEDIA");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.actorSlotID, node);
                    //return texts
                    effectReturn.topText = SetTopTeamText(teamID);
                    effectReturn.bottomText = SetBottomTeamText(actor);
                    effectReturn.isAction = true;
                    break;
                case "ProbeTeam":
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("PROBE");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.actorSlotID, node);
                    //return texts
                    effectReturn.topText = SetTopTeamText(teamID);
                    effectReturn.bottomText = SetBottomTeamText(actor);
                    effectReturn.isAction = true;
                    break;
                case "SpiderTeam":
                    teamArcID = GameManager.instance.dataScript.GetTeamArcID("SPIDER");
                    teamID = GameManager.instance.dataScript.GetTeamInPool(TeamPool.Reserve, teamArcID);
                    GameManager.instance.teamScript.MoveTeam(TeamPool.OnMap, teamID, actor.actorSlotID, node);
                    //return texts
                    effectReturn.topText = SetTopTeamText(teamID);
                    effectReturn.bottomText = SetBottomTeamText(actor);
                    effectReturn.isAction = true;
                    break;

                default:
                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                    effectReturn.errorFlag = true;
                    break;
            }
        }
        else
        {
            //No effect paramater
            Debug.LogError("Invalid Effect (null)");
            effectReturn = null;
        }
        return effectReturn;
    }

    /// <summary>
    /// Formats string for details.TopText (Authority), returns "unknown" if a problem. Also used by ModalTeamPicker.cs -> ProcessTeamChoice and TeamNanager.cs
    /// Can be used for both Inserting and Recalling a team
    /// </summary>
    /// <param name="team"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public string SetTopTeamText(int teamID, bool isInserted = true)
    {
        //get team
        Team team = GameManager.instance.dataScript.GetTeam(teamID);
        if (team != null)
        {
            //get node
            Node node = GameManager.instance.dataScript.GetNode(team.nodeID);
            if (node != null)
            {
                string operation = "inserted";
                if (isInserted == false) { operation = "recalled"; }
                return string.Format("{0}{1}{2}{3} {4} have been {5} at {6}{7}{8}{9}{10} {11}{12}", 
                    colourNeutral, team.arc.name, colourEnd, 
                    colourNormal, team.teamName, operation, colourEnd,  
                    colourBad, node.Arc.name, colourEnd, colourNormal, node.nodeName, colourEnd);
            }
            else
            {
                Debug.LogError(string.Format("Invalid node (Null) for team.NodeID {0}", team.nodeID));
                return "Unknown";
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid team (Null) for teamID {0}", teamID));
            return "Unknown";
        }

    }

    /// <summary>
    /// Formats string for details.BottomText (Authority), returns "unknown" if parameter is null. Also used by ModalTeamPicker.cs -> ProcessTeamChoice
    /// </summary>
    /// <returns></returns>
    public string SetBottomTeamText(Actor actor)
    {
        if (actor != null)
        {
            string colourNumbers = colourGood;
            if (actor.CheckNumOfTeams() == actor.datapoint2)
            { colourNumbers = colourBad; }
            return string.Format("{0}, {1} of {2}{3}{4} has now deployed {5}{6}{7} of {8}{9}{10} teams",
                actor.actorName, GameManager.instance.metaScript.GetAuthorityTitle(), colourActor, actor.arc.name, colourEnd,
                colourNumbers, actor.CheckNumOfTeams(), colourEnd, colourNumbers, actor.datapoint2, colourEnd);
        }
        else
        {
            Debug.LogError("Invalid actor (Null)");
            return "Unknown";
        }
    }

    /// <summary>
    /// Sub method to process Node Stability/Security/Support
    /// Note: Effect and Node checked for null in calling method
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveNodeData(Effect effect, Node node, EffectDataInput effectInput)
    {
        int value = 0;
        //sort out colour based on type (which is effect benefit from POV of Resistance)
        string colourEffect = colourDefault;
        string colourText = colourDefault;
        if (effect.typeOfEffect != null)
        {
            switch (effect.typeOfEffect.name)
            {
                case "Good":
                    colourEffect = colourGood;
                    break;
                case "Neutral":
                    colourEffect = colourNeutral;
                    colourText = colourNeutral;
                    break;
                case "Bad":
                    colourEffect = colourBad;
                    colourText = colourAlert;
                    break;
                default:
                    Debug.LogError(string.Format("Invalid effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name));
                    break;
            }
        }
        else { Debug.LogWarning(string.Format("Invalid typeOfEffect (Null) for \"{0}\"", effect.name)); }
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //data package to send to node for field processing
        EffectDataProcess effectProcess = new EffectDataProcess() { outcome = effect.outcome };
        //Extent of effect
        switch (effect.apply.name)
        {
            //Current Node only
            case "NodeCurrent":
                if (effect.operand != null)
                {
                    switch (effect.operand.name)
                    {
                        case "Add":
                            value = effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}The security system has been swept and strengthened{1}", colourText, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Law Enforcement teams have stabilised the situation{1}", colourText, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}There is a surge of support for the Rebels{1}", colourText, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}The Node security system has been scanned for intruders{1}", colourText, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}A Spider has been covertly inserted into the Node security system{1}", colourText, colourEnd);
                                    break;
                                case "StatusContacts":
                                    effectResolve.topText = string.Format("{0}Listening bots have been deployed to the Node{1}", colourText, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}The local Node grapevine is alive and well{1}", colourText, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        case "Subtract":
                            value = -1 * effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}The security system has been successfully hacked{1}", colourText, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Civil unrest and instability is spreading throughout{1}", colourText, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}The Rebels are losing popularity{1}", colourText, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}The Node security system has been spoofed to conceal Tracers{1}", colourText, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}ICE has been inserted into the Node security system to conceal Spiders{1}", colourText, colourEnd);
                                    break;
                                case "StatusContacts":
                                    effectResolve.topText = string.Format("{0}Listening bots have been muted to conceal Contacts{1}", colourText, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}The local Node grapevine has been shut down to conceal Teams{1}", colourText, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                            effectResolve.isError = true;
                            break;
                    }
                }
                else
                {
                    //null operand -> Reveal 'x' operation
                    Debug.LogWarning(string.Format("Invalid operand (Null) \"{0}\"", effect.outcome.name));
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, value); }
                //Process Node effect
                node.ProcessNodeEffect(effectProcess);
                break;
            //Neighbouring Nodes
            case "NodeNeighbours":
                if (effect.operand != null)
                {
                    switch (effect.operand.name)
                    {
                        case "Add":
                            value = effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}Neighbouring security systems have been swept and strengthened{1}", colourText, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Law Enforcement teams have stabilised neighbouring nodes{1}", colourText, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}There is a surge of support for the Rebels in neighbouring nodes{1}", colourText, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}The neighbouring security systems have been scanned for intruders{1}", colourText, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}A Tracer has been covertly inserted into the neighbouring security systems{1}", colourText, colourEnd);
                                    break;
                                case "StatusContacts":
                                    effectResolve.topText = string.Format("{0}Listening bots have been deployed{1}", colourText, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}The local neighbourhood grapevine is alive and well{1}", colourText, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        case "Subtract":
                            value = -1 * effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}Neighbouring security systems have been successfully hacked{1}", colourText, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Civil unrest and instability is spreading throughout the neighbouring nodes{1}", colourText, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}The Rebels are losing popularity in neighbouring nodes{1}", colourText, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}The neighbouring security systems have been spoofed to hide Tracers{1}", colourText, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}ICE has been inserted into the neighbouring security systems to conceal Spiders{1}", colourText, colourEnd);
                                    break;
                                case "StatusContacts":
                                    effectResolve.topText = string.Format("{0}Listening bots have been muted to conceal Contacts{1}", colourText, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}The local neighbourhood grapevine has been shut down to conceal Teams{1}", colourText, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                            effectResolve.isError = true;
                            break;
                    }
                }
                else
                {
                    //null operand -> Reveal 'x' operation
                    Debug.LogWarning(string.Format("Invalid operand (Null) \"{0}\"", effect.outcome.name));
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, value); }
                //Process Node effect for current node
                node.ProcessNodeEffect(effectProcess);
                //Process Node effect for all neighbouring nodes
                List<Node> listOfNeighbours = node.GetNeighbouringNodes();
                if (listOfNeighbours != null)
                {
                    foreach (Node nodeTemp in listOfNeighbours)
                    { nodeTemp.ProcessNodeEffect(effectProcess); }
                }
                else { Debug.LogError(string.Format("Invalid listOfNeighbours (Null) for nodeID {0}, \"{1}\"", node.nodeID, node.nodeName)); }
                break;
            //All Nodes
            case "NodeAll":
                if (effect.operand != null)
                {
                    switch (effect.operand.name)
                    {
                        case "Add":
                            value = effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}All security systems have been swept and strengthened{1}", colourText, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Law Enforcement teams have stabilised the city wide situation{1}", colourText, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}There is a surge of support for the Rebels throughout the city{1}", colourText, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}The cities security systems have been scanned for intruders{1}", colourText, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}A Tracer has been covertly inserted into the cities security system{1}", colourText, colourEnd);
                                    break;
                                case "StatusContacts":
                                    effectResolve.topText = string.Format("{0}Listening bots have been deployed throughout the city{1}", colourText, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}The city grapevine is alive and well{1}", colourText, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        case "Subtract":
                            value = -1 * effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}All security systems have been successfully hacked{1}", colourText, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Civil unrest and instability is spreading throughout the city{1}", colourText, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}The Rebels are losing popularity throughout the city{1}", colourText, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}Countermeasures have been deployed to conceal all Tracers within the City{1}", colourText, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}ICE has been deployed to conceal all Spiders within the City{1}", colourText, colourEnd);
                                    break;
                                case "StatusContacs":
                                    effectResolve.topText = string.Format("{0}Countermeasures have been deployed to conceal all Contacts within the City{1}", colourText, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}ICE has been deployed to conceal all Teams within the City{1}", colourText, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                            effectResolve.isError = true;
                            break;
                    }
                }
                else
                {
                    //null operand -> Reveal 'x' operation
                    Debug.LogWarning(string.Format("Invalid operand (Null) \"{0}\"", effect.outcome.name));
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, value); }
                //Process Node effect for current node
                node.ProcessNodeEffect(effectProcess);
                //Process Node effect for all nodes
                Dictionary<int, Node> dictOfAllNodes = GameManager.instance.dataScript.GetAllNodes();
                if (dictOfAllNodes != null)
                {
                    foreach (var nodeTemp in dictOfAllNodes)
                    {
                        //exclude current node
                        if (nodeTemp.Value.nodeID != node.nodeID)
                        { nodeTemp.Value.ProcessNodeEffect(effectProcess); }
                    }
                }
                else { Debug.LogError("Invalid dictOfAllNodes (Null)"); }
                break;
            //Nodes of the Same Arc type
            case "NodeSameArc":
                if (effect.operand != null)
                {
                    switch (effect.operand.name)
                    {
                        case "Add":
                            value = effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}Security systems in {1} nodes have been swept and strengthened{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Law Enforcement teams have stabilised the {1} situation{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}There is a surge of support for the Rebels in {1} nodes{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}{1} security systems have been scanned for intruders{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}A Tracer has been covertly inserted into {1} security systems{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusContacts":
                                    effectResolve.topText = string.Format("{0}Listening bots have been deployed throughout {1} nodes{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}The {1} grapevine is alive and well{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        case "Subtract":
                            value = -1 * effect.value;
                            switch (effect.outcome.name)
                            {
                                case "NodeSecurity":
                                    effectResolve.topText = string.Format("{0}Security systems in {1} nodes have been successfully hacked{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "NodeStability":
                                    effectResolve.topText = string.Format("{0}Civil unrest and instability is spreading throughout {1} nodes{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "NodeSupport":
                                    effectResolve.topText = string.Format("{0}The Rebels are losing popularity throughout {1} nodes{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusTracers":
                                    effectResolve.topText = string.Format("{0}{1} security systems have been spoofed to conceal Tracers{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusSpiders":
                                    effectResolve.topText = string.Format("{0}ICE has been inserted into {1} security systems to conceal Spiders{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusContacts":
                                    effectResolve.topText = string.Format("{0}Countermeasures have been taken in {1} nodes to conceal Contacts{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                case "StatusTeams":
                                    effectResolve.topText = string.Format("{0}The {1} grapevine has been shut down to conceal Teams{2}", colourText, node.Arc.name, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effectOutcome \"{0}\"", effect.outcome.name));
                                    effectResolve.isError = true;
                                    break;
                            }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                            effectResolve.isError = true;
                            break;
                    }
                }
                else
                {
                    //null operand -> Reveal 'x' operation
                    Debug.LogWarning(string.Format("Invalid operand (Null) \"{0}\"", effect.outcome.name));
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, value); }
                //Process Node effect for current node
                node.ProcessNodeEffect(effectProcess);
                //Process Node effect for all neighbouring nodes
                Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetAllNodes();
                if (dictOfNodes != null)
                {
                    foreach (var nodeTemp in dictOfNodes)
                    {
                        //same node Arc type and not the current node?
                        if (nodeTemp.Value.Arc.nodeArcID == node.Arc.nodeArcID && nodeTemp.Value.nodeID != node.nodeID)
                        { nodeTemp.Value.ProcessNodeEffect(effectProcess); }
                    }
                }
                else { Debug.LogError("Invalid dictOfSameNodes (Null)"); }
                break;
            default:
                Debug.LogError(string.Format("Invalid effect.apply \"{0}\"", effect.apply.name));
                break;
        }
        //bottom text
        effectResolve.bottomText = string.Format("{0}{1}{2}", colourEffect, effect.textTag, colourEnd);
        //return data to calling method (ProcessEffect)
        return effectResolve;
    }

    /// <summary>
    /// Sub method to process Node Stability/Security/Support
    /// Note: effect and node checked for Null by calling method
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="node"></param>
    /// <param name="effectInput"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveConnectionData(Effect effect, Node node, EffectDataInput effectInput)
    {
        int value = 0;
        //sort out colour based on type (which is effect benefit from POV of Resistance)
        string colourEffect = colourDefault;
        string colourText = colourDefault;
        if (effect.typeOfEffect != null)
        {
            switch (effect.typeOfEffect.name)
            {
                case "Good":
                    colourEffect = colourGood;
                    break;
                case "Neutral":
                    colourEffect = colourNeutral;
                    colourText = colourNeutral;
                    break;
                case "Bad":
                    colourEffect = colourBad;
                    colourText = colourAlert;
                    break;
                default:
                    Debug.LogError(string.Format("Invalid effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name));
                    break;
            }
        }
        else { Debug.LogWarning(string.Format("Invalid typeOfEffect (Null) for \"{0}\"", effect.name)); }
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //data package to send to node for field processing
        EffectDataProcess effectProcess = new EffectDataProcess() { outcome = effect.outcome };

        //Extent of effect
        switch (effect.apply.name)
        {
            //Current Node only
            case "ConnectionCurrent":
                switch (effect.operand.name)
                {
                    case "Add":
                        value = effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}Immediate Connections have raised their security{1}", colourText, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security +{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    case "Subtract":
                        value = -1 * effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}Immediate Connections have lowered their security{1}", colourText, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security -{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                        effectResolve.isError = true;
                        break;
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                { ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, value); }
                //Process Connection effect for current node
                node.ProcessConnectionEffect(effectProcess);
                break;
            //Neighbouring Nodes
            case "ConnectionNeighbours":
                switch (effect.operand.name)
                {
                    case "Add":
                        value = effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}Neighbouring Connections have raised their security{1}", colourText, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security +{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    case "Subtract":
                        value = -1 * effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}Neighbouring Connections have lowered their security{1}", colourText, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security -{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                        effectResolve.isError = true;
                        break;
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                {
                    ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, value);
                    /*//DEBUG -> remove when finished with testing
                    effectProcess.effectOngoing.ongoingID = GetOngoingEffectID();*/
                }
                //Process Connection effect
                List<Node> listOfNeighbours = node.GetNeighbouringNodes();
                if (listOfNeighbours != null)
                {
                    //clear all connection flags first to prevent double dipping
                    GameManager.instance.connScript.SetAllFlagsToFalse();
                    //process current node
                    node.ProcessConnectionEffect(effectProcess);
                    //process all neighbouring nodes
                    foreach (Node nodeTemp in listOfNeighbours)
                    { nodeTemp.ProcessConnectionEffect(effectProcess); }
                }
                else { Debug.LogError("Invalid listOfNeighours (Null)"); }
                break;
            //Same Node Arc
            case "ConnectionSameArc":
                switch (effect.operand.name)
                {
                    case "Add":
                        value = effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}{1} Connections have raised their security{2}", colourText, node.Arc.name, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security +{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    case "Subtract":
                        value = -1 * effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}{1} Connections have lowered their security{2}", colourText, node.Arc.name, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security -{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                        effectResolve.isError = true;
                        break;
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                {
                    ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, value);
                    /*//DEBUG -> remove when finished with testing
                    effectProcess.effectOngoing.ongoingID = GetOngoingEffectID();
                    GameManager.instance.dataScript.AddOngoingIDToDict(effectProcess.effectOngoing.ongoingID, "Connection Security");*/
                }
                //Process Connection effect
                Dictionary<int, Node> dictOfAllNodes = GameManager.instance.dataScript.GetAllNodes();
                if (dictOfAllNodes != null)
                {
                    //clear all connection flags first to prevent double dipping
                    GameManager.instance.connScript.SetAllFlagsToFalse();
                    //current node
                    node.ProcessConnectionEffect(effectProcess);
                    foreach (var nodeTemp in dictOfAllNodes)
                    {
                        //same node Arc type and not the current node?
                        if (nodeTemp.Value.Arc.nodeArcID == node.Arc.nodeArcID && nodeTemp.Value.nodeID != node.nodeID)
                        { nodeTemp.Value.ProcessConnectionEffect(effectProcess); }
                    }
                }
                else { Debug.LogError("Invalid dictOfAllNodes (Null)"); }
                break;
            //All Nodes
            case "ConnectionAll":
                switch (effect.operand.name)
                {
                    case "Add":
                        value = effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}All Connections have raised their security{1}", colourText, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security +{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    case "Subtract":
                        value = -1 * effect.value;
                        switch (effect.outcome.name)
                        {
                            case "ConnectionSecurity":
                                effectResolve.topText = string.Format("{0}All Connections have lowered their security{1}", colourText, colourEnd);
                                effectResolve.bottomText = string.Format("{0}Connection Security -{1}{2}", colourEffect, effect.value, colourEnd);
                                break;
                        }
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effectOperator \"{0}\"", effect.operand.name));
                        effectResolve.isError = true;
                        break;
                }
                effectProcess.value = value;
                //Ongoing effect
                if (effect.duration.name.Equals("Ongoing"))
                {
                    ProcessOngoingEffect(effect, effectProcess, effectResolve, effectInput, value);
                }
                //Process Connection effect
                Dictionary<int, Node> dictOfNodes = GameManager.instance.dataScript.GetAllNodes();
                if (dictOfNodes != null)
                {                
                    //clear all connection flags first to prevent double dipping
                    GameManager.instance.connScript.SetAllFlagsToFalse();
                    foreach (var nodeTemp in dictOfNodes)
                    { nodeTemp.Value.ProcessConnectionEffect(effectProcess); }
                }
                else { Debug.LogError("Invalid dictOfNodes (Null)"); }
                break;
        }
        //return data to calling method (ProcessEffect)
        return effectResolve;
    }

    /// <summary>
    /// Sub method to process Actor Condition. Can pass a Null actor (default) provided it is a Player condition
    /// Note: effect and actor checked for null by the calling method.
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveConditionData(Effect effect, Node node, Actor actor = null)
    {
        //sort out colour based on type (which is effect benefit from POV of Resistance)
        string colourEffect = colourDefault;
        string colourText = colourDefault;
        Condition condition = null;
        if (effect.typeOfEffect != null)
        {
            switch (effect.typeOfEffect.name)
            {
                case "Good":
                    colourEffect = colourGood;
                    break;
                case "Neutral":
                    colourEffect = colourNeutral;
                    colourText = colourNeutral;
                    break;
                case "Bad":
                    colourEffect = colourBad;
                    colourText = colourAlert;
                    break;
                default:
                    Debug.LogError(string.Format("Invalid effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name));
                    break;
            }
        }
        else { Debug.LogWarning(string.Format("Invalid typeOfEffect (Null) for \"{0}\"", effect.name)); }
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();

        //choose big picture (Random / Any) or specific condition
        switch (effect.outcome.name)
        {
            //specific Conditions
            case "ConditionStressed":
            case "ConditionIncompetent":
            case "ConditionCorrupt":
            case "ConditionQuestionable":
            case "ConditionStar":
                //get condition
                switch (effect.outcome.name)
                {
                    case "ConditionStressed":
                        condition = GameManager.instance.dataScript.GetCondition("STRESSED");
                        break;
                    case "ConditionIncompetent":
                        condition = GameManager.instance.dataScript.GetCondition("INCOMPETENT");
                        break;
                    case "ConditionCorrupt":
                        condition = GameManager.instance.dataScript.GetCondition("CORRUPT");
                        break;
                    case "ConditionQuestionable":
                        condition = GameManager.instance.dataScript.GetCondition("QUESTIONABLE");
                        break;
                    case "ConditionStar":
                        condition = GameManager.instance.dataScript.GetCondition("STAR");
                        break;
                    default:
                        Debug.LogError(string.Format("Invalid effect.outcome \"{0}\"", effect.outcome.name));
                        break;
                }
                //resolve effect outcome
                if (condition != null)
                {
                    //assign condition to player if at their node, otherwise actor
                    if (node.nodeID == GameManager.instance.nodeScript.nodePlayer)
                    {
                        //Player Condition
                        switch (effect.operand.name)
                        {
                            case "Add":
                                //only add condition if NOT already present
                                if (GameManager.instance.playerScript.CheckConditionPresent(condition) == false)
                                {
                                    GameManager.instance.playerScript.AddCondition(condition);

                                    effectResolve.bottomText = string.Format("{0}Player gains condition {1}{2}", colourEffect, condition.name, colourEnd);
                                }
                                break;
                            case "Subtract":
                                //only remove  condition if present
                                if (GameManager.instance.playerScript.CheckConditionPresent(condition) == true)
                                {
                                    GameManager.instance.playerScript.RemoveCondition(condition);
                                    effectResolve.bottomText = string.Format("{0}Player condition {1} removed{2}", colourEffect, condition.name, colourEnd);
                                }
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name));
                                break;
                        }

                    }
                    else
                    {
                        if (actor != null)
                        {
                            //Actor Condition
                            switch (effect.operand.name)
                            {
                                case "Add":
                                    //only add condition if NOT already present
                                    if (actor.CheckConditionPresent(condition) == false)
                                    {
                                        actor.AddCondition(condition);
                                        effectResolve.bottomText = string.Format("{0}{1} condition gained{2}", colourEffect, condition.name, colourEnd);
                                    }
                                    break;
                                case "Subtract":
                                    //only remove  condition if present
                                    if (actor.CheckConditionPresent(condition) == true)
                                    {
                                        actor.RemoveCondition(condition);
                                        effectResolve.bottomText = string.Format("{0}{1} condition removed{2}", colourEffect, condition.name, colourEnd);
                                    }
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name));
                                    break;
                            }
                        }
                        else { Debug.LogError("Invalid actor (Null)"); }
                    }
                }
                else { Debug.LogError(string.Format("Invalid condition (Null) for outcome \"{0}\"", effect.outcome.name)); }
                break;

            //add or subtract a group condition
            case "ConditionGroupGood":
            case "ConditionGroupBad":
                List<Condition> listOfConditions;
                Condition conditionRandom;
                string effectType = "Good";
                string colourConditionAdd = colourGood;
                string colourConditionRemove = colourBad;
                GlobalType type = GameManager.instance.globalScript.typeGood;
                //get type
                switch (effect.outcome.name)
                {
                    case "ConditionGroupGood":
                        //default condition, no need to reassign
                        break;
                    case "ConditionGroupBad":
                        type = GameManager.instance.globalScript.typeBad;
                        effectType = "Bad";
                        colourConditionAdd = colourBad;
                        colourConditionRemove = colourGood;
                        break;
                    default:
                        Debug.LogWarning(string.Format("Invalid outcome name \"{0}\"", effect.outcome.name));
                        break;
                }
                //Player
                if (node.nodeID == GameManager.instance.nodeScript.nodePlayer)
                {
                    if (GameManager.instance.playerScript.CheckNumOfConditions() > 0)
                    {
                        listOfConditions = GameManager.instance.playerScript.GetListOfConditions();
                        switch (effect.apply.name)
                        {
                            case "ConditionRandom":
                                switch (effect.operand.name)
                                {
                                    case "Add":
                                        //add a new condition
                                        conditionRandom = GetRandomCondition(listOfConditions, type, effect.operand.name);
                                        if (conditionRandom != null)
                                        {
                                            GameManager.instance.playerScript.AddCondition(conditionRandom);
                                            effectResolve.bottomText = string.Format("{0}{1} condition gained{2}", colourConditionAdd, conditionRandom.name, colourEnd);
                                        }
                                        else
                                        { effectResolve.bottomText = string.Format("{0}There are no {1} conditions present{2}", colourAlert, effectType, colourEnd); }
                                        break;
                                    case "Subtract":
                                        //remove an existing condition
                                        conditionRandom = GetRandomCondition(listOfConditions, type, effect.operand.name);
                                        if (conditionRandom != null)
                                        {
                                            //remove condition
                                            GameManager.instance.playerScript.RemoveCondition(conditionRandom);
                                            effectResolve.bottomText = string.Format("{0}{1} condition removed{2}", colourConditionRemove, conditionRandom.name, colourEnd);
                                        }
                                        else
                                        { effectResolve.bottomText = string.Format("{0}There are no {1} conditions present{2}", colourAlert, effectType, colourEnd); }
                                        break;
                                    default:
                                        Debug.LogError(string.Format("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name));
                                        break;
                                }
                                break;
                            case "ConditionAll":
                                string typeCompare;
                                int counter = 0;
                                switch (effect.outcome.name)
                                {
                                    case "ConditionGroupGood":
                                        //remove all good conditions
                                        if (listOfConditions.Count > 0)
                                        {
                                            typeCompare = GameManager.instance.globalScript.typeGood.name;
                                            for (int i = listOfConditions.Count - 1; i >= 0; i--)
                                            {
                                                if (listOfConditions[i].type.name.Equals(typeCompare) == true)
                                                { listOfConditions.RemoveAt(i); counter++; }
                                            }
                                            effectResolve.bottomText = string.Format("{0}All ({1}) Good conditions removed{2}", colourBad, counter, colourEnd);
                                        }
                                        break;
                                    case "ConditionGroupBad":
                                        //remove all bad conditions
                                        if (listOfConditions.Count > 0)
                                        {
                                            typeCompare = GameManager.instance.globalScript.typeBad.name;
                                            for (int i = listOfConditions.Count - 1; i >= 0; i--)
                                            {
                                                if (listOfConditions[i].type.name.Equals(typeCompare) == true)
                                                { listOfConditions.RemoveAt(i); counter++; }
                                            }
                                            effectResolve.bottomText = string.Format("{0}All ({1}) Bad conditions removed{2}", colourGood, counter, colourEnd);
                                        }
                                        break;
                                    default:
                                        Debug.LogError(string.Format("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name));
                                        break;
                                }
                                break;
                            default:
                                Debug.LogError(string.Format("Invalid effect.apply \"{0}\" for effect outcome \"{1}\"", effect.apply.name, effect.outcome.name));
                                break;
                        }
                    }
                    else
                    { effectResolve.bottomText = string.Format("{0}There are no {1} conditions present{2}", colourAlert, effectType, colourEnd); }
                }
                //Actor
                else
                {
                    if (actor != null)
                    {
                        if (actor.CheckNumOfConditions() > 0)
                        {
                            listOfConditions = actor.GetListOfConditions();
                            switch (effect.apply.name)
                            {
                                case "ConditionRandom":
                                    switch (effect.operand.name)
                                    {
                                        case "Add":
                                            //add a new condition
                                            conditionRandom = GetRandomCondition(listOfConditions, type, effect.operand.name);
                                            if (conditionRandom != null)
                                            {
                                                actor.AddCondition(conditionRandom);
                                                effectResolve.bottomText = string.Format("{0}{1} condition gained{2}", colourConditionAdd, conditionRandom.name, colourEnd);
                                            }
                                            else
                                            { effectResolve.bottomText = string.Format("{0}There are no {1} conditions present{2}", colourAlert, effectType, colourEnd); }
                                            break;
                                        case "Subtract":
                                            //remove an existing condition
                                            conditionRandom = GetRandomCondition(listOfConditions, type, effect.operand.name);
                                            if (conditionRandom != null)
                                            {
                                                //remove condition
                                                actor.RemoveCondition(conditionRandom);
                                                effectResolve.bottomText = string.Format("{0}{1} condition removed{2}", colourConditionRemove, conditionRandom.name, colourEnd);
                                            }
                                            else
                                            { effectResolve.bottomText = string.Format("{0}There are no {1} conditions present{2}", colourAlert, effectType, colourEnd); }
                                            break;
                                        default:
                                            Debug.LogError(string.Format("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name));
                                            break;
                                    }
                                    break;
                                case "ConditionAll":
                                    string typeCompare;
                                    int counter = 0;
                                    switch (effect.outcome.name)
                                    {
                                        case "ConditionGroupGood":
                                            //remove all good conditions
                                            if (listOfConditions.Count > 0)
                                            {
                                                typeCompare = GameManager.instance.globalScript.typeGood.name;
                                                for (int i = listOfConditions.Count - 1; i >= 0; i--)
                                                {
                                                    if (listOfConditions[i].type.name.Equals(typeCompare) == true)
                                                    { listOfConditions.RemoveAt(i); counter++; }
                                                }
                                                effectResolve.bottomText = string.Format("{0}All ({1}) Good conditions removed{2}", colourBad, counter, colourEnd);
                                            }
                                            break;
                                        case "ConditionGroupBad":
                                            //remove all bad conditions
                                            if (listOfConditions.Count > 0)
                                            {
                                                typeCompare = GameManager.instance.globalScript.typeBad.name;
                                                for (int i = listOfConditions.Count - 1; i >= 0; i--)
                                                {
                                                    if (listOfConditions[i].type.name.Equals(typeCompare) == true)
                                                    { listOfConditions.RemoveAt(i); counter++; }
                                                }
                                                effectResolve.bottomText = string.Format("{0}All ({1}) Bad conditions removed{2}", colourGood, counter, colourEnd);
                                            }
                                            break;
                                        default:
                                            Debug.LogError(string.Format("Invalid operand \"{0}\" for effect outcome \"{1}\"", effect.operand.name, effect.outcome.name));
                                            break;
                                    }
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effect.apply \"{0}\" for effect outcome \"{1}\"", effect.apply.name, effect.outcome.name));
                                    break;
                            }
                        }
                        else
                        { effectResolve.bottomText = string.Format("{0}There are no {1} conditions present{2}", colourAlert, effectType, colourEnd); }
                    }
                    else { Debug.LogError("Invalid actor (Null)"); }
                }
                break;

            default:
                Debug.LogError(string.Format("Invalid effect.outcome \"{0}\"", effect.outcome.name));
                break;
        }
        return effectResolve;
    }

    /// <summary>
    /// Given a list of conditions it chooses a random one of the specified type (Good/Bad). Returns null if not found. Sub method for ResolveConditionData
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private Condition GetRandomCondition(List<Condition> listOfConditions, GlobalType type, string operand)
    {
        Condition conditionSelected = null;
        if (listOfConditions != null && listOfConditions.Count > 0)
        {
            if (string.IsNullOrEmpty(operand) == false)
            {
                //create a new list by value, not reference
                List<Condition> tempList = new List<Condition>();
                switch (operand)
                {
                    case "Remove":
                        //randomly select a condition of the required type to remove from the provided list
                        foreach (Condition condition in listOfConditions)
                        {
                            //add to list if the correct type
                            if (condition.type.name.Equals(type.name) == true)
                            { tempList.Add(condition); }
                        }
                        //select a condition
                        int numOfConditions = tempList.Count;
                        if (numOfConditions > 0)
                        { conditionSelected = tempList[Random.Range(0, numOfConditions)]; }
                        break;
                    case "Add":
                        //randomly select a new condition of the required type that is NOT on the provided list
                        Dictionary<string, Condition> dictOfConditionsByType = GameManager.instance.dataScript.GetDictOfConditionsByType(type);
                        if (dictOfConditionsByType.Count > 0)
                        {
                            //loop listOfConditions and remove any identical entries in the temp dict
                            foreach(Condition condition in listOfConditions)
                            {
                                if (dictOfConditionsByType.ContainsKey(condition.name))
                                { dictOfConditionsByType.Remove(condition.name); }
                            }
                            //place remaining conditions (in temp dict) in a list
                            if (dictOfConditionsByType.Count > 0)
                            {
                                foreach (var condTemp in dictOfConditionsByType)
                                { tempList.Add(condTemp.Value); }
                                //randomly select a condition
                                conditionSelected = tempList[Random.Range(0, tempList.Count)];
                            }
                        }
                        else { Debug.LogWarning(string.Format("dictOfConditionByType returns Empty for type \"{0}\"", type.name)); }
                        break;
                    default:
                        Debug.LogWarning(string.Format("Invalid operand \"{0}\"", operand));
                        break;
                }
            }
            else { Debug.LogWarning("Invalid operand (Null or Empty)"); }
        }
        else { Debug.LogWarning("Invalid listOfConditios (Null or Empty)"); }
        return conditionSelected;
    }

    /// <summary>
    /// subMethod to handle all manage effects
    /// Note: Actor has been checked for null by the calling method
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="actor"></param>
    /// <returns></returns>
    private EffectDataResolve ResolveManageData(Effect effect, Actor actor)
    {
        int data;
        //sort out colour based on type (which is effect benefit from POV of Resistance)
        string colourEffect = colourDefault;
        if (effect.typeOfEffect != null)
        {
            switch (effect.typeOfEffect.name)
            {
                case "Good":
                    colourEffect = colourGood;
                    break;
                case "Neutral":
                    colourEffect = colourNeutral;
                    break;
                case "Bad":
                    colourEffect = colourBad;
                    break;
                default:
                    Debug.LogError(string.Format("Invalid effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name));
                    break;
            }
        }
        else { Debug.LogWarning(string.Format("Invalid typeOfEffect (Null) for \"{0}\"", effect.name)); }
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        //get condition
        switch (effect.outcome.name)
        {
            case "ActorToReserves":
                effectResolve.bottomText = string.Format("{0}{1} moved to the Reserves{2}", colourEffect, actor.actorName, colourEnd);
                break;
            case "ActorPromoted":
                effectResolve.bottomText = string.Format("{0}{1} promoted and will join {2} HQ{3}", colourEffect, actor.actorName, 
                    GameManager.instance.factionScript.GetCurrentFaction(), colourEnd);
                break;
            case "ActorDismissed":
                effectResolve.bottomText = string.Format("{0}{1} dismissed{2}", colourEffect, actor.actorName, colourEnd);
                break;
            case "ActorDisposedOff":
                effectResolve.bottomText = string.Format("{0}{1} killed{2}", colourEffect, actor.actorName, colourEnd);
                break;
            case "ManageReserveRenown":
                data = GameManager.instance.actorScript.manageReserveRenown;
                GameManager.instance.playerScript.Renown -= data;
                effectResolve.bottomText = string.Format("{0}Player Renown -{1}{2}", colourEffect, data, colourEnd);
                break;
            case "ManageDismissRenown":
                data = GameManager.instance.actorScript.manageDismissRenown;
                GameManager.instance.playerScript.Renown -= data;
                effectResolve.bottomText = string.Format("{0}Player Renown -{1}{2}", colourEffect, data, colourEnd);
                break;
            case "ManageDisposeRenown":
                data = GameManager.instance.actorScript.manageDisposeRenown;
                GameManager.instance.playerScript.Renown -= data;
                effectResolve.bottomText = string.Format("{0}Player Renown -{1}{2}", colourEffect, data, colourEnd);
                break;
            case "UnhappyTimerRest":
                data = GameManager.instance.actorScript.restReserveTimer;
                actor.unhappyTimer = data;
                effectResolve.bottomText = string.Format("{0}{1}'s Unhappy Timer set to {2} turn{3}{4}", colourEffect, actor.actorName, data,
                    data != 1 ? "s" : "", colourEnd);
                break;
            case "UnhappyTimerPromise":
                data = GameManager.instance.actorScript.promiseReserveTimer;
                actor.unhappyTimer = data;
                effectResolve.bottomText = string.Format("{0}{1}'s Unhappy Timer set to {2} turn{3}{4}", colourEffect, actor.actorName, data,
                    data != 1 ? "s" : "", colourEnd);
                break;
            case "UnhappyTimerNoPromise":
                data = GameManager.instance.actorScript.noPromiseReserveTimer;
                actor.unhappyTimer = data;
                effectResolve.bottomText = string.Format("{0}{1}'s Unhappy Timer set to {2} turn{3}{4}", colourEffect, actor.actorName, data, 
                    data != 1 ? "s" : "", colourEnd);
                break;
            default:
                Debug.LogError(string.Format("Invalid effect.outcome \"{0}\"", effect.outcome.name));
                break;
        }
        return effectResolve;
    }

    /// <summary>
    /// subMethod to process special player effects
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    private EffectDataResolve ResolvePlayerData(Effect effect)
    {
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        //sort out colour based on type (which is effect benefit from POV of Resistance)
        string colourEffect = colourDefault;
        bool errorFlag = false;            
        //data package to return to the calling methods
        EffectDataResolve effectResolve = new EffectDataResolve();
        if (effect.typeOfEffect != null)
        {
            switch (effect.typeOfEffect.name)
            {
                case "Good":
                    colourEffect = colourGood;
                    break;
                case "Neutral":
                    colourEffect = colourNeutral;
                    break;
                case "Bad":
                    colourEffect = colourBad;
                    break;
                default:
                    Debug.LogError(string.Format("Invalid effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name));
                    break;
            }

            //get condition
            switch (effect.outcome.name)
            {
                case "PlayerActions":
                    //single or ongoing
                    ActionAdjustment actionAdjustment = new ActionAdjustment();
                    actionAdjustment.side = side;
                    switch (effect.duration.name)
                    {
                        case "Single":
                            //once off effect -> Note: timer is 2 because it will be immediately knocked down to 1 at the end of this turn
                            actionAdjustment.timer = 2;
                            switch (effect.operand.name)
                            {
                                case "Add":
                                    actionAdjustment.value = effect.value;
                                    GameManager.instance.dataScript.AddActionAdjustment(actionAdjustment);
                                    effectResolve.bottomText = string.Format("{0}Player gains {1}{2}{3}{4}{5} extra action{6} {7}{8}NEXT TURN{9}", colourEffect, colourEnd,
                                        colourNeutral, effect.value, colourEnd, colourEffect, effect.value != 1 ? "s" : "", colourEnd, colourNeutral, colourEnd);
                                    break;
                                case "Subtract":
                                    actionAdjustment.value = effect.value * -1;
                                    GameManager.instance.dataScript.AddActionAdjustment(actionAdjustment);
                                    effectResolve.bottomText = string.Format("{0}Player loses {1}{2}{3}{4}{5} extra action{6} {7}{8}NEXT TURN{9}", colourEffect, colourEnd,
                                        colourNeutral, effect.value, colourEnd, colourEffect, effect.value != 1 ? "s" : "", colourEnd, colourNeutral, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effect.operand \"{0}\"", effect.operand.name)); errorFlag = true;
                                    break;
                            }
                            break;
                        case "Ongoing":
                            //NOTE: Ongoing effects are handled differently here than a standard ongoing effect (there is also an extra +1 due to decrement at end of turn)
                            actionAdjustment.timer = GameManager.instance.effectScript.ongoingEffectTimer + 1;
                            switch (effect.operand.name)
                            {
                                case "Add":
                                    actionAdjustment.value = effect.value;
                                    GameManager.instance.dataScript.AddActionAdjustment(actionAdjustment);
                                    effectResolve.bottomText = string.Format("{0}Player gains {1}{2}{3}{4}{5} extra action{6} for {7}{8}{9}{10}{11} turns commencing {12}{13}NEXT TURN{14}", 
                                        colourEffect, colourEnd, colourNeutral, effect.value, colourEnd, colourEffect, effect.value != 1 ? "s" : "", colourEnd, colourNeutral,
                                        actionAdjustment.timer - 1, colourEnd, colourEffect, colourEnd, colourNeutral, colourEnd);
                                    break;
                                case "Subtract":
                                    actionAdjustment.value = effect.value * -1;
                                    GameManager.instance.dataScript.AddActionAdjustment(actionAdjustment);
                                    effectResolve.bottomText = string.Format("{0}Player loses {1}{2}{3}{4}{5} extra action{6} for {7}{8}{9}{10}{11} turns commencing {12}{13}NEXT TURN{14}",
                                        colourEffect, colourEnd, colourNeutral, effect.value, colourEnd, colourEffect, effect.value != 1 ? "s" : "", colourEnd, colourNeutral,
                                        actionAdjustment.timer - 1, colourEnd, colourEffect, colourEnd, colourNeutral, colourEnd);
                                    break;
                                default:
                                    Debug.LogError(string.Format("Invalid effect.operand \"{0}\"", effect.operand.name)); errorFlag = true;
                                    break;
                            }
                            break;
                        default:
                            Debug.LogError(string.Format("Invalid effect.duration \"{0}\"", effect.duration.name)); errorFlag = true;
                            break;
                    }
                    break;
                default:
                    Debug.LogError(string.Format("Invalid effect.outcome \"{0}\"", effect.outcome.name)); errorFlag = true;
                    break;
            }
        }
        else { Debug.LogWarning(string.Format("Invalid typeOfEffect (Null) for \"{0}\"", effect.name)); errorFlag = true; }


        return effectResolve;
    }

    /// <summary>
    /// subMethod to handle Ongoing effects
    /// </summary>
    private void ProcessOngoingEffect(Effect effect, EffectDataProcess effectProcess, EffectDataResolve effectResolve, EffectDataInput effectInput, int value)
    {
        EffectDataOngoing effectOngoing = new EffectDataOngoing();
        effectOngoing.outcome = effect.outcome;
        effectOngoing.ongoingID = effectInput.ongoingID;
        effectOngoing.value = value;
        effectOngoing.type = effect.typeOfEffect;
        effectOngoing.apply = effect.apply;
        effectOngoing.side = effectInput.side;
        //descriptor
        switch (effect.outcome.name)
        {
            case "NodeSecurity":
            case "NodeStability":
            case "NodeSupport":
                effectOngoing.text = string.Format("{0} {1}{2} ({3})", effect.outcome.name, value, value > 0 ? "+" : "", effectInput.ongoingText);
                break;
            case "StatusTracers":
            case "StatusSpiders":
            case "StatusTeams":
            case "StatusContacts":
            case "ConnectionSecurity":
                effectOngoing.text = string.Format("{0}", effect.textTag);
                break;
            default:
                effectOngoing.text = string.Format("{0} ({1})", effect.outcome.name, effectInput.ongoingText);
                break;
        }
        //add to effectProcess
        effectProcess.effectOngoing = effectOngoing;
    }

    /// <summary>
    /// gets a unique ID for ongoing effects. All
    /// </summary>
    /// <returns></returns>
    public int GetOngoingEffectID()
    { return ongoingEffectIDCounter++; }

    //place methods above here
}
