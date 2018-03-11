using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;
using modalAPI;
using packageAPI;
using System;
using Random = UnityEngine.Random;
using System.Text;
using UnityEngine.Events;

/// <summary>
/// handles Actor related data and methods
/// </summary>
public class ActorManager : MonoBehaviour
{

    [HideInInspector] public int numOfActiveActors;    //Actors who are OnMap and active, eg. not asleep or captured
    [Tooltip("Maxium number of actors (Active or Inactive) that can be onMap (eg. 'Onscreen') at any one time")]
    [Range(1, 4)] public int maxNumOfOnMapActors = 4;      //if you increase this then GUI elements and GUIManager will need to be changed to accomodate it, default value 4
    [Tooltip("Maximum number of actors that can be in the replacement pool (applies to both sides)")]
    [Range(1, 6)] public int maxNumOfReserveActors = 4;    //
    [Tooltip("The maximum number of stats (Qualities) that an actor can have")]
    [Range(2,4)] public int numOfQualities = 3;        //number of qualities actors have (different for each side), eg. "Connections, Invisibility" etc. Map to DataPoint0 -> DataPoint'x'
    [Tooltip("Maximum value of an actor datapoint stat")]
    [Range(2, 4)] public int maxStatValue = 3;
    [Tooltip("Minimum value of an actor datapoint stat")]
    [Range(2, 4)] public int minStatValue = 0;
    [Tooltip("% Chance of an actor in the Reserve Pool becoming unhappy each turn once their unhappyTimer expires")]
    [Range(1, 50)] public int chanceOfUnhappy = 20;
    [Tooltip("Multiplier to the chanceOfUnhappy for an actor who has been promised that they will be recalled within a set time period")]
    [Range(1, 5)] public int unhappyPromiseFactor = 3;
    [Tooltip("Actor sent to Reserves and Player promises to recall them within this number of turns. Their unhappy timer will be set to this number of turns.")]
    [Range(1, 10)] public int promiseReserveTimer = 10;
    [Tooltip("Actor sent to Reserves and Player did NOT promise anything. Their unhappy timer will be set to this number of turns.")]
    [Range(1, 10)] public int noPromiseReserveTimer = 5;
    [Tooltip("Actor sent to Reserves to Rest. Their unhappy timer will be set to this number of turns.")]
    [Range(1, 10)] public int restReserveTimer = 10;
    [Tooltip("Actor Recruited and placed in Reserves. Their unhappy timer will be set to this number of turns.")]
    [Range(1, 10)] public int recruitedReserveTimer = 5;
    [Tooltip("Renown cost for negating a bad gear use roll")]
    [Range(1, 3)] public int renownCostGear = 1;
    [Tooltip("Base Renown cost for carrying out Manage Reserve Actor actions")]
    [Range(1, 5)] public int manageReserveRenown = 1;
    [Tooltip("Base Renown cost for carrying out Manage Dismiss Actor actions")]
    [Range(1, 5)] public int manageDismissRenown = 2;
    [Tooltip("Base Renown cost for carrying out Manage Dispose Actor actions")]
    [Range(1, 5)] public int manageDisposeRenown = 3;
    [Tooltip("Once actor has taken action as a result of being unhappy this is the number of turns warning period you get before they carry out their action")]
    [Range(1, 5)] public int unhappyWarningPeriod = 2;
    [Tooltip("Once actor is unhappy, the chance per turn (1d100) of losing motivation -1")]
    [Range(1, 99)] public int unhappyLoseMotivationChance = 50;
    [Tooltip("Once actor is unhappy and has motivation 0 the chance of them acting on their dissatisfaction / turn")]
    [Range(1, 99)] public int unhappyTakeActionChance = 25;
    [Tooltip("When an unhappy actor in the Reserve pool takes action this is the first check made (ignored if actor has no secrets")]
    [Range(1, 99)] public int unhappyRevealSecretChance = 50;
    [Tooltip("When an unhappy actor in the Reserve pool takes action this is the second check made. Double chance if actor has previously complained")]
    [Range(1, 99)] public int unhappyLeaveChance = 25;
    [Tooltip("When an unhappy actor in the Reserve pool takes action this is the third check made. An actor can only complain once")]
    [Range(1, 99)] public int unhappyComplainChance = 50;


    private static int actorIDCounter = 0;              //used to sequentially number actorID's

    //fast access fields
    private GlobalSide globalAuthority;
    private GlobalSide globalResistance;

    //colour palette for Generic tool tip
    private string colourResistance;
    private string colourAuthority;
    private string colourCancel;
    private string colourInvalid;
    private string colourGoodEffect;
    private string colourNeutralEffect;
    private string colourBadEffect;
    private string colourDefault;
    private string colourNormal;
    private string colourRecruit;
    private string colourArc;
    private string colourEnd;


    public void PreInitialiseActors()
    {
        //number of actors, default 4
        maxNumOfOnMapActors = maxNumOfOnMapActors == 4 ? maxNumOfOnMapActors : 4;
        numOfActiveActors = maxNumOfOnMapActors;
    }


    public void Initialise()
    {
        //fast acess fields
        globalAuthority = GameManager.instance.globalScript.sideAuthority;
        globalResistance = GameManager.instance.globalScript.sideResistance;
        //event listener is registered in InitialiseActors() due to GameManager sequence.
        EventManager.instance.AddListener(EventType.StartTurnLate, OnEvent);
        EventManager.instance.AddListener(EventType.ChangeColour, OnEvent);
        EventManager.instance.AddListener(EventType.RecruitAction, OnEvent);
        EventManager.instance.AddListener(EventType.RecruitDecision, OnEvent);
        EventManager.instance.AddListener(EventType.GenericRecruitActorResistance, OnEvent);
        EventManager.instance.AddListener(EventType.GenericRecruitActorAuthority, OnEvent);
        //create active, OnMap actors
        InitialiseActors(maxNumOfOnMapActors, GameManager.instance.globalScript.sideResistance);
        InitialiseActors(maxNumOfOnMapActors, GameManager.instance.globalScript.sideAuthority);
        //create pool actors
        InitialisePoolActors();

    }

    /// <summary>
    /// handles events
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.ChangeColour:
                SetColours();
                break;
            case EventType.StartTurnLate:
                StartTurnLate();
                break;
            case EventType.RecruitAction:
                ModalActionDetails details = Param as ModalActionDetails;
                InitialiseGenericPickerRecruit(details);
                break;
            case EventType.RecruitDecision:
                RecruitActor((int)Param);
                break;
            case EventType.GenericRecruitActorResistance:
                GenericReturnData returnDataRecruitResistance = Param as GenericReturnData;
                ProcessRecruitChoiceResistance(returnDataRecruitResistance);
                break;
            case EventType.GenericRecruitActorAuthority:
                GenericReturnData returnDataRecruitAuthority = Param as GenericReturnData;
                ProcessRecruitChoiceAuthority(returnDataRecruitAuthority);
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// Pre turn processing
    /// </summary>
    private void StartTurnLate()
    {
        CheckInactiveActors();
        CheckReserveActors();
    }

    /// <summary>
    /// deregisters events
    /// </summary>
    public void OnDisable()
    {
        EventManager.instance.RemoveEvent(EventType.NodeAction);
        EventManager.instance.RemoveEvent(EventType.TargetAction);
    }

    /// <summary>
    /// set colour palette for Generic Tool tip
    /// </summary>
    public void SetColours()
    {
        colourResistance = GameManager.instance.colourScript.GetColour(ColourType.sideRebel);
        colourAuthority = GameManager.instance.colourScript.GetColour(ColourType.sideAuthority);
        colourCancel = GameManager.instance.colourScript.GetColour(ColourType.cancelNormal);
        colourInvalid = GameManager.instance.colourScript.GetColour(ColourType.cancelHighlight);
        colourGoodEffect = GameManager.instance.colourScript.GetColour(ColourType.goodEffect);
        colourNeutralEffect = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourBadEffect = GameManager.instance.colourScript.GetColour(ColourType.badEffect);
        colourDefault = GameManager.instance.colourScript.GetColour(ColourType.defaultText);
        colourNormal = GameManager.instance.colourScript.GetColour(ColourType.normalText);
        colourRecruit = GameManager.instance.colourScript.GetColour(ColourType.neutralEffect);
        colourArc = GameManager.instance.colourScript.GetColour(ColourType.actorArc);
        colourEnd = GameManager.instance.colourScript.GetEndTag();
    }



    /// <summary>
    /// Set up number of required actors (minions supporting play)
    /// </summary>
    /// <param name="num"></param>
    public void InitialiseActors(int num, GlobalSide side)
    {
        if (num > 0)
        {
            //get a list of random actorArcs
            List<ActorArc> tempActorArcs = GameManager.instance.dataScript.GetRandomActorArcs(num, side);
            //Create actors
            for (int i = 0; i < num; i++)
            {
                Actor actor = CreateActor(side, tempActorArcs[i].ActorArcID, 1, ActorStatus.Active, i);
                if (actor != null)
                {
                    Debug.Log(string.Format("Actor added -> {0}, {1} {2}, {3} {4}, {5} {6}, level {7}{8}", actor.arc.actorName,
                        GameManager.instance.dataScript.GetQuality(side, 0), actor.datapoint0, 
                        GameManager.instance.dataScript.GetQuality(side, 1), actor.datapoint1, 
                        GameManager.instance.dataScript.GetQuality(side, 2), actor.datapoint2, actor.level, "\n"));
                }
                else { Debug.LogWarning("Actor not created"); }
            }
        }
        else { Debug.LogWarning("Invalid number of Actors (Zero, or less)"); }
    }

    /// <summary>
    /// populate the pools to recruit from (one full set of actor arcs for each side and each level)
    /// </summary>
    public void InitialisePoolActors()
    {
        int numOfArcs;
        //Authority
        List<ActorArc> listOfArcs = GameManager.instance.dataScript.GetActorArcs(globalAuthority);
        if (listOfArcs != null)
        {
            numOfArcs = listOfArcs.Count;
            for (int i = 0; i < numOfArcs; i++)
            {
                //level one actor
                Actor actorOne = CreateActor(globalAuthority, listOfArcs[i].ActorArcID, 1, ActorStatus.Pool);
                if (actorOne != null)
                {
                    //NOTE: need to add to Dictionary BEFORE adding to Pool (Debug.Assert checks dictOfActors.Count in AddActorToPool)
                    GameManager.instance.dataScript.AddActorToDict(actorOne);
                    GameManager.instance.dataScript.AddActorToPool(actorOne.actorID, 1, globalAuthority);
                }
                else { Debug.LogWarning(string.Format("Invalid Authority actorOne (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
                //level two actor
                Actor actorTwo = CreateActor(globalAuthority, listOfArcs[i].ActorArcID, 2, ActorStatus.Pool);
                if (actorTwo != null)
                {
                    GameManager.instance.dataScript.AddActorToDict(actorTwo);
                    GameManager.instance.dataScript.AddActorToPool(actorTwo.actorID, 2, globalAuthority);
                }
                else { Debug.LogWarning(string.Format("Invalid Authority actorTwo (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
                //level three actor
                Actor actorThree = CreateActor(globalAuthority, listOfArcs[i].ActorArcID, 3, ActorStatus.Pool);
                if (actorThree != null)
                {
                    GameManager.instance.dataScript.AddActorToDict(actorThree);
                    GameManager.instance.dataScript.AddActorToPool(actorThree.actorID, 3, globalAuthority);
                }
                else { Debug.LogWarning(string.Format("Invalid Authority actorThree (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
            }
        }
        else { Debug.LogError("Invalid list of Authority Actor Arcs (Null)"); }
        //Resistance
        listOfArcs = GameManager.instance.dataScript.GetActorArcs(globalResistance);
        if (listOfArcs != null)
        {
            numOfArcs = listOfArcs.Count;
            for (int i = 0; i < numOfArcs; i++)
            {
                //level one actor
                Actor actorOne = CreateActor(globalResistance, listOfArcs[i].ActorArcID, 1, ActorStatus.Pool);
                if (actorOne != null)
                {
                    //NOTE: need to add to Dictionary BEFORE adding to Pool (Debug.Assert checks dictOfActors.Count in AddActorToPool)
                    GameManager.instance.dataScript.AddActorToDict(actorOne);
                    GameManager.instance.dataScript.AddActorToPool(actorOne.actorID, 1, globalResistance);
                }
                else { Debug.LogWarning(string.Format("Invalid Resistance actorOne (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
                //level two actor
                Actor actorTwo = CreateActor(globalResistance, listOfArcs[i].ActorArcID, 2, ActorStatus.Pool);
                if (actorTwo != null)
                {
                    GameManager.instance.dataScript.AddActorToDict(actorTwo);
                    GameManager.instance.dataScript.AddActorToPool(actorTwo.actorID, 2, globalResistance);
                }
                else { Debug.LogWarning(string.Format("Invalid Resistance actorTwo (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
                //level three actor
                Actor actorThree = CreateActor(globalResistance, listOfArcs[i].ActorArcID, 3, ActorStatus.Pool);
                if (actorThree != null)
                {
                    GameManager.instance.dataScript.AddActorToDict(actorThree);
                    GameManager.instance.dataScript.AddActorToPool(actorThree.actorID, 3, globalResistance);
                }
                else { Debug.LogWarning(string.Format("Invalid Resistance actorThree (Null) for actorArcID \"{0}\"", listOfArcs[i].ActorArcID)); }
            }
        }
        else { Debug.LogError("Invalid list of Resistance Actor Arcs (Null)"); }
    }

    /// <summary>
    /// creates a new actor of the specified actor arc type, side and level (1 worst -> 3 best). SlotID (0 to 3) for current actor, default '-1' for reserve pool actor.
    /// adds actor to dataManager.cs ArrayOfActors if OnMap (slot ID > -1). NOT added to dictOfActors if slotID -1 (do so after actor is chosen in generic picker)
    /// returns null if a problem
    /// </summary>
    /// <param name="arc"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public Actor CreateActor(GlobalSide side, int actorArcID, int level, ActorStatus status, int slotID = -1)
    {
        Debug.Assert(level > 0 && level < 4, "Invalid level (must be between 1 and 3)");
        Debug.Assert(slotID >= -1 && slotID <= maxNumOfOnMapActors, "Invalid slotID (must be -1 (default) or between 1 and 3");
        //check a valid actor Arc parameter
        ActorArc arc = GameManager.instance.dataScript.GetActorArc(actorArcID);
        if (arc != null)
        {
            //check actor arc is the correct side
            if (arc.side.level == side.level)
            {
                //create new actor
                Actor actor = new Actor();
                //data
                actor.actorID = actorIDCounter++;
                actor.actorSlotID = slotID;
                actor.level = level;
                actor.side = side;
                actor.arc = arc;
                actor.actorName = arc.actorName;
                actor.trait = GameManager.instance.dataScript.GetRandomTrait();
                actor.Status = status;
                //level -> range limits
                int limitLower = 1;
                if (level == 3) { limitLower = 2; }
                int limitUpper = Math.Min(4, level + 2);
                //level -> assign
                actor.datapoint0 = Random.Range(limitLower, limitUpper); //connections and influence
                actor.datapoint1 = Random.Range(limitLower, limitUpper); //motivation and support
                if (side.level == GameManager.instance.globalScript.sideResistance.level)
                {
                    //invisibility -> Level 3 100% Invis 3, level 2 25% Invis 2, 75% Invis 3, level 1 50% Invis 2, 50% Invis 3
                    switch(actor.level)
                    {
                        case 3: actor.datapoint2 = 3; break;
                        case 2: 
                            if (Random.Range(0,100) <= 25) { actor.datapoint2 = 2; }
                            else { actor.datapoint2 = 3; }
                            break;
                        case 1:
                            if (Random.Range(0, 100) <= 50) { actor.datapoint2 = 2; }
                            else { actor.datapoint2 = 3; }
                            break;
                    }
                    
                }
                else if (side.level == GameManager.instance.globalScript.sideAuthority.level)
                { actor.datapoint2 = Random.Range(limitLower, limitUpper); /*Ability*/}
                //OnMap actor
                if (slotID > -1)
                {
                    /*actor.isLive = true;*/

                    //add to data collections
                    GameManager.instance.dataScript.AddCurrentActor(side, actor, slotID);
                    GameManager.instance.dataScript.AddActorToDict(actor);
                }
                
                /*//Reserve pool actor or perhaps an actor for a generic pick list
                else
                { actor.isLive = false; }*/

                //return actor
                return actor;
            }
            else { Debug.LogError(string.Format("ActorArc (\"{0}\") is the wrong side for actorArcID {1}", arc.name, actorArcID)); }
        }
        else { Debug.LogError(string.Format("Invalid ActorArc (Null) for actorArcID {0}", actorArcID)); }
        return null;
    }


    /// <summary>
    /// Returns a list of all relevant Actor Actions for the  node to enable a ModalActionMenu to be put together (one button per action). 
    /// Resistance -> Max 4 Actor + 1 Target actions with an additional 'Cancel' buttnn added last automatically -> total six buttons (hardwired into GUI design)
    /// Authority -> Insert and Recall Teams
    /// </summary>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public List<EventButtonDetails> GetNodeActions(int nodeID)
    {
        string sideColour;
        string cancelText = null;
        string effectCriteria;
        bool proceedFlag;
        int actionID;
        Actor[] arrayOfActors;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"ss
        if ( playerSide.level == globalAuthority.level)
        { sideColour = colourAuthority; }
        else { sideColour = colourResistance; }
        List<EventButtonDetails> tempList = new List<EventButtonDetails>();
        //string builder for cancel button (handles all no go cases
        StringBuilder infoBuilder = new StringBuilder();
        //player's current node
        int playerID = GameManager.instance.nodeScript.nodePlayer;
        //Get Node
        Node node = GameManager.instance.dataScript.GetNode(nodeID);
        if (node != null)
        {
            List<Effect> listOfEffects = new List<Effect>();
            Action tempAction;
            EventButtonDetails details;
            //
            // - - - Resistance - - -
            //
            if (playerSide.level == globalResistance.level)
            {
                //'Cancel' button tooltip)
                if (nodeID == playerID)
                { cancelText = "You are present at the node"; }
                else { cancelText = "You are NOT present at the node"; }
                //
                // - - -  Target - - -
                //
                //Does the Node have a target attached? -> added first
                if (node.targetID >= 0)
                {
                    Target target = GameManager.instance.dataScript.GetTarget(node.targetID);
                    if (target != null)
                    {
                        bool targetProceed = false;
                        //can tackle target if Player at node or target specified actor is present in line-up
                        if (nodeID == playerID) { targetProceed = true; }
                        else if (GameManager.instance.dataScript.CheckActorArcPresent(target.actorArc, globalResistance) == true) { targetProceed = true; }
                        //target live and dancing
                        if (targetProceed == true)
                        {
                            string targetHeader = string.Format("{0}{1}{2}{3}{4}{5}{6}", sideColour, target.name, colourEnd, "\n", colourDefault, 
                                target.description, colourEnd);
                            //button target details
                            EventButtonDetails targetDetails = new EventButtonDetails()
                            {
                                buttonTitle = "Attempt Target",
                                buttonTooltipHeader = targetHeader,
                                buttonTooltipMain = GameManager.instance.targetScript.GetTargetFactors(node.targetID),
                                buttonTooltipDetail = GameManager.instance.targetScript.GetTargetEffects(node.targetID),
                                //use a Lambda to pass arguments to the action
                                action = () => { EventManager.instance.PostNotification(EventType.TargetAction, this, nodeID); }
                            };
                            tempList.Add(targetDetails);
                        }
                        else
                        {
                            //invalid target (Player not present, specified actor Arc not in line up)
                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                            infoBuilder.Append(string.Format("{0}Target invalid{1}{2}{3}(No Player, No {4}){5}",
                                colourInvalid, "\n", colourEnd, colourBadEffect, target.actorArc.name, colourEnd));
                        }
                    }
                    else { Debug.LogError(string.Format("Invalid TargetID \"{0}\" (Null){1}", node.targetID, "\n")); }
                }
                //
                // - - - Actors - - - 
                //
                //loop actors currently in game -> get Node actions (1 per Actor, if valid criteria)
                arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
                foreach (Actor actor in arrayOfActors)
                {
                    //if invalid actor or vacant position then ignore
                    if (actor != null)
                    {
                        proceedFlag = true;
                        details = null;
                        //correct side?
                        if (actor.side.level == playerSide.level)
                        {
                            //actor active?
                            if (actor.Status == ActorStatus.Active)
                            {
                                //active node for actor or player at node
                                if (GameManager.instance.levelScript.CheckNodeActive(node.nodeID, playerSide, actor.actorSlotID) == true ||
                                    nodeID == playerID)
                                {
                                    //get node action
                                    tempAction = actor.arc.nodeAction;

                                    if (tempAction != null)
                                    {
                                        //effects
                                        StringBuilder builder = new StringBuilder();
                                        listOfEffects = tempAction.listOfEffects;
                                        if (listOfEffects.Count > 0)
                                        {
                                            string colourEffect = colourDefault;
                                            for (int i = 0; i < listOfEffects.Count; i++)
                                            {
                                                Effect effect = listOfEffects[i];
                                                //colour code effects according to type
                                                if (effect.typeOfEffect != null)
                                                {
                                                    switch (effect.typeOfEffect.name)
                                                    {
                                                        case "Good":
                                                            colourEffect = colourGoodEffect;
                                                            break;
                                                        case "Neutral":
                                                            colourEffect = colourNeutralEffect;
                                                            break;
                                                        case "Bad":
                                                            colourEffect = colourBadEffect;
                                                            break;
                                                    }
                                                }
                                                //check effect criteria is valid
                                                CriteriaDataInput criteriaInput = new CriteriaDataInput()
                                                {
                                                    nodeID = nodeID,
                                                    listOfCriteria = effect.listOfCriteria
                                                };
                                                effectCriteria = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                                                if (effectCriteria == null)
                                                {
                                                    //Effect criteria O.K -> tool tip text
                                                    if (builder.Length > 0) { builder.AppendLine(); }
                                                    if (effect.outcome.name.Equals("Renown") == false && effect.outcome.name.Equals("Invisibility") == false)
                                                    { builder.Append(string.Format("{0}{1}{2}", colourEffect, effect.textTag, colourEnd)); }
                                                    else
                                                    {
                                                        //handle renown & invisibility situatiosn - players or actors?
                                                        if (nodeID == playerID)
                                                        {
                                                            //player affected (good for renown, bad for invisibility)
                                                            if (effect.outcome.name.Equals("Renown"))
                                                            { builder.Append(string.Format("{0}Player {1}{2}", colourGoodEffect, effect.textTag, colourEnd)); }
                                                            else
                                                            {
                                                                builder.Append(string.Format("{0}Player {1}{2}", colourBadEffect, effect.textTag, colourEnd));
                                                            }
                                                        }
                                                        else
                                                        {
                                                            //actor affected
                                                            builder.Append(string.Format("{0}{1} {2}{3}", colourBadEffect, actor.arc.name, effect.textTag, colourEnd));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    //invalid effect criteria -> Action cancelled
                                                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                                    infoBuilder.Append(string.Format("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                        colourInvalid, actor.arc.name, "\n", colourEnd,
                                                        colourAuthority, effectCriteria, colourEnd));
                                                    proceedFlag = false;
                                                }
                                            }
                                        }
                                        else
                                        { Debug.LogWarning(string.Format("Action \"{0}\" has no effects", tempAction)); }
                                        if (proceedFlag == true)
                                        {
                                            //Details to pass on for processing via button click
                                            ModalActionDetails actionDetails = new ModalActionDetails() { };

                                            actionDetails.side = globalResistance;
                                            actionDetails.nodeID = nodeID;
                                            actionDetails.actorSlotID = actor.actorSlotID;
                                            //pass all relevant details to ModalActionMenu via Node.OnClick()
                                            if (actor.arc.nodeAction.special == null)
                                            {
                                                details = new EventButtonDetails()
                                                {
                                                    buttonTitle = tempAction.name,
                                                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                    buttonTooltipMain = tempAction.tooltipText,
                                                    buttonTooltipDetail = builder.ToString(),
                                                    //use a Lambda to pass arguments to the action
                                                    action = () => { EventManager.instance.PostNotification(EventType.NodeAction, this, actionDetails); }
                                                };
                                            }
                                            //special case, Resistance node actions only
                                            else
                                            {
                                                switch (actor.arc.nodeAction.special.name)
                                                {
                                                    case "NeutraliseTeam":
                                                        details = new EventButtonDetails()
                                                        {
                                                            buttonTitle = tempAction.name,
                                                            buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                            buttonTooltipMain = tempAction.tooltipText,
                                                            buttonTooltipDetail = builder.ToString(),
                                                            action = () => { EventManager.instance.PostNotification(EventType.NeutraliseTeamAction, this, actionDetails); }
                                                        };
                                                        break;
                                                    case "GetGear":
                                                        details = new EventButtonDetails()
                                                        {
                                                            buttonTitle = tempAction.name,
                                                            buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                            buttonTooltipMain = tempAction.tooltipText,
                                                            buttonTooltipDetail = builder.ToString(),
                                                            action = () => { EventManager.instance.PostNotification(EventType.GearAction, this, actionDetails); }
                                                        };
                                                        break;
                                                    case "GetRecruit":
                                                        details = new EventButtonDetails()
                                                        {
                                                            buttonTitle = tempAction.name,
                                                            buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                                            buttonTooltipMain = tempAction.tooltipText,
                                                            buttonTooltipDetail = builder.ToString(),
                                                            action = () => { EventManager.instance.PostNotification(EventType.RecruitAction, this, actionDetails); }
                                                        };
                                                        break;
                                                    default:
                                                        Debug.LogError(string.Format("Invalid actor.Arc.nodeAction.special \"{0}\"", actor.arc.nodeAction.special.name));
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //actor has no connections at node
                                    if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                    infoBuilder.Append(string.Format("{0} has no connections", actor.arc.name));
                                }
                            }
                            else
                            {
                                //actor isn't Active
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                switch (actor.Status)
                                {
                                    case ActorStatus.Inactive:
                                        infoBuilder.Append(string.Format("{0} is lying low and unavailable", actor.arc.name));
                                        break;
                                    case ActorStatus.Captured:
                                        infoBuilder.Append(string.Format("{0} has been captured", actor.arc.name));
                                        break;
                                    default:
                                        infoBuilder.Append(string.Format("{0} is unavailable", actor.arc.name));
                                        break;
                                }
                            }

                            //add to list
                            if (details != null)
                            { tempList.Add(details); }
                        }
                    }
                }
            }
            //
            // - - - Authority - - -
            //
            else if (playerSide.level == globalAuthority.level)
            {
                int teamArcID;
                bool isAnyTeam;
                string tooltipMain;
                //'Cancel' button tooltip)
                int numOfTeams = node.CheckNumOfTeams();
                cancelText = string.Format("There {0} {1} team{2} present at the node", numOfTeams == 1 ? "is" : "are", numOfTeams,
                    numOfTeams != 1 ? "s" : "");
                //
                // - - -  Recall Team - - -
                //
                //Does the Node have any teams present? -> added first
                if (node.CheckNumOfTeams() > 0)
                {
                    //get list of teams
                    List<Team> listOfTeams = node.GetTeams();
                    if (listOfTeams != null)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach(Team team in listOfTeams)
                        {
                            if (builder.Length > 0) { builder.AppendLine(); }
                            builder.Append(string.Format("{0}{1} {2}{3}", colourNeutralEffect, team.arc.name, team.teamName, colourEnd));
                        }
                        //button details
                        EventButtonDetails recallDetails = new EventButtonDetails()
                        {
                            buttonTitle = "Recall Team",
                            buttonTooltipHeader = string.Format("{0}Recall Team{1}", sideColour, colourEnd),
                            buttonTooltipMain = "The following teams can be withdrawn early",
                            buttonTooltipDetail = builder.ToString(),
                            //use a Lambda to pass arguments to the action
                            action = () => { EventManager.instance.PostNotification(EventType.RecallTeamAction, this, node.nodeID); }
                        };
                        tempList.Add(recallDetails);
                    }
                    else { Debug.LogError(string.Format("Invalid listOfTeams (Null) for Node {0} \"{1}\", ID {2}", node.Arc.name, node.nodeName, node.nodeID)); }
                }

                //get a list pre-emptively as it's computationally expensive to do so on demand
                List<string> tempTeamList = GameManager.instance.dataScript.GetAvailableReserveTeams(node);
                arrayOfActors = GameManager.instance.dataScript.GetCurrentActors(globalAuthority);
                //loop actors currently in game -> get Node actions (1 per Actor, if valid criteria)
                foreach (Actor actor in arrayOfActors)
                {
                    proceedFlag = true;
                    details = null;
                    isAnyTeam = false;
                    //correct side?
                    if (actor.side.level == playerSide.level)
                    {
                        //actor active?
                        if (actor.Status == ActorStatus.Active)
                        {
                            //assign preferred team as default (doesn't matter if actor has ANY Team action)
                            teamArcID = actor.arc.preferredTeam.TeamArcID;
                            tempAction = null;
                            //active node for actor
                            if (GameManager.instance.levelScript.CheckNodeActive(node.nodeID, GameManager.instance.sideScript.PlayerSide, actor.actorSlotID) == true)
                            {
                                //get ANY TEAM node action
                                actionID = GameManager.instance.dataScript.GetActionID("Any Team");
                                if (actionID > -1)
                                {
                                    tempAction = GameManager.instance.dataScript.GetAction(actionID);
                                    isAnyTeam = true;
                                }
                            }
                            //actor not live at node -> Preferred team
                            else
                            { tempAction = actor.arc.nodeAction; }
                            //default main tooltip text body
                            tooltipMain = tempAction.tooltipText;
                            //tweak if ANY TEAM
                            if (isAnyTeam == true)
                            { tooltipMain = string.Format("{0} as the {1} has Influence here", tooltipMain, GameManager.instance.metaScript.GetAuthorityTitle()); }
                            //valid action?
                            if (tempAction != null)
                            {
                                //effects
                                StringBuilder builder = new StringBuilder();
                                listOfEffects = tempAction.listOfEffects;
                                if (listOfEffects.Count > 0)
                                {
                                    for (int i = 0; i < listOfEffects.Count; i++)
                                    {
                                        Effect effect = listOfEffects[i];
                                        //check effect criteria is valid
                                        CriteriaDataInput criteriaInput = new CriteriaDataInput()
                                        {
                                            nodeID = nodeID,
                                            teamArcID = teamArcID,
                                            actorSlotID = actor.actorSlotID,
                                            listOfCriteria = effect.listOfCriteria
                                        };
                                        effectCriteria = GameManager.instance.effectScript.CheckCriteria(criteriaInput);
                                        if (effectCriteria == null)
                                        {
                                            //Effect criteria O.K -> tool tip text
                                            if (builder.Length > 0) { builder.AppendLine(); }
                                            if (effect.outcome.name.Equals("Renown") == false)
                                            {
                                                //sort out colour, remember effect.typeOfEffect is from POV of resistance so good will be bad for authority
                                                if (effect.typeOfEffect != null)
                                                {
                                                    switch (effect.typeOfEffect.name)
                                                    {
                                                        case "Good":
                                                            builder.Append(string.Format("{0}{1}{2}", colourBadEffect, effect.textTag, colourEnd));
                                                            break;
                                                        case "Neutral":
                                                            builder.Append(string.Format("{0}{1}{2}", colourNeutralEffect, effect.textTag, colourEnd));
                                                            break;
                                                        case "Bad":
                                                            builder.Append(string.Format("{0}{1}{2}", colourGoodEffect, effect.textTag, colourEnd));
                                                            break;
                                                        default:
                                                            builder.Append(string.Format("{0}{1}{2}", colourDefault, effect.textTag, colourEnd));
                                                            Debug.LogError(string.Format("Invalid effect.typeOfEffect \"{0}\"", effect.typeOfEffect.name));
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    builder.Append(effect.textTag);
                                                    Debug.LogWarning(string.Format("Invalid effect.typeOfEffect (Null) for \"{0}\"", effect.name));
                                                }
                                                //if an ANY TEAM action then display available teams
                                                if (isAnyTeam == true)
                                                {
                                                    foreach (string teamName in tempTeamList)
                                                    {
                                                        builder.AppendLine();
                                                        builder.Append(string.Format("{0}{1}{2}", colourNeutralEffect, teamName, colourEnd));
                                                    }
                                                }
                                            }
                                            //actor automatically accumulates renown for their faction
                                            else
                                            { builder.Append(string.Format("{0}{1} {2}{3}", colourAuthority, actor.arc.name, effect.textTag, colourEnd)); }

                                        }
                                        else
                                        {
                                            //invalid effect criteria -> Action cancelled
                                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                            infoBuilder.Append(string.Format("{0}{1} action invalid{2}{3}{4}({5}){6}",
                                                colourInvalid, actor.arc.name, "\n", colourEnd,
                                                colourAuthority, effectCriteria, colourEnd));
                                            proceedFlag = false;
                                        }
                                    }
                                }
                                else
                                { Debug.LogWarning(string.Format("Action \"{0}\" has no effects", tempAction)); }
                                if (proceedFlag == true)
                                {
                                    //Details to pass on for processing via button click
                                    ModalActionDetails actionDetails = new ModalActionDetails() { };
                                    actionDetails.side = globalAuthority;
                                    actionDetails.nodeID = nodeID;
                                    actionDetails.actorSlotID = actor.actorSlotID;
                                    //Node action is standard but other actions are possible
                                    UnityAction clickAction = null;
                                    //Team action
                                    if (isAnyTeam)
                                    { clickAction = () => { EventManager.instance.PostNotification(EventType.InsertTeamAction, this, actionDetails); };  }
                                    //Node action
                                    else
                                    { clickAction = () => { EventManager.instance.PostNotification(EventType.NodeAction, this, actionDetails); };  }
                                    //pass all relevant details to ModalActionMenu via Node.OnClick()
                                    details = new EventButtonDetails()
                                    {
                                        buttonTitle = tempAction.name,
                                        buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, actor.arc.name, colourEnd),
                                        buttonTooltipMain = tooltipMain,
                                        buttonTooltipDetail = builder.ToString(),
                                        //use a Lambda to pass arguments to the action
                                        action = clickAction
                                    };
                                }
                            }
                            else
                            {
                                Debug.LogError(string.Format("{0}, slotID {1} has no valid action", actor.arc.name, actor.actorSlotID));
                                if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                                infoBuilder.Append(string.Format("{0} is having a bad day", actor.arc.name));
                            }
                        }
                        else
                        {
                            //actor gone silent
                            if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                            infoBuilder.Append(string.Format("{0} is lying low and unavailale", actor.arc.name));
                        }
                        //add to list
                        if (details != null)
                        { tempList.Add(details); }
                    }
                }
            }
            //
            // - - - Cancel
            //
            //Cancel button is added last
            EventButtonDetails cancelDetails = null;
            if (infoBuilder.Length > 0)
            {
                cancelDetails = new EventButtonDetails()
                {
                    buttonTitle = "CANCEL",
                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                    buttonTooltipMain = cancelText,
                    buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, infoBuilder.ToString(), colourEnd),
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this); }
                };
            }
            else
            {
                //necessary to prevent color tags triggering the bottom divider in TooltipGeneric
                cancelDetails = new EventButtonDetails()
                {
                    buttonTitle = "CANCEL",
                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                    buttonTooltipMain = cancelText,
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this); }
                };
            }
            //add Cancel button to list
            tempList.Add(cancelDetails);
        }
        else { Debug.LogError(string.Format("Invalid Node (null), ID {0}{1}", nodeID, "\n")); }
        return tempList;
    }


    /// <summary>
    /// Returns a list of all relevant Actor Actions for the actor to enable a ModalActionMenu to be put together (one button per action). 
    /// Resitance -> up to 3 x 'Give Gear to Actor', 1 x'Activate' / Lie Low', 1 x 'Manage' and an automatic Cancel button (6 total)
    /// </summary>
    /// <param name="actorSlotID"></param>
    /// <returns></returns>
    public List<EventButtonDetails> GetActorActions(int actorSlotID)
    {
        string sideColour, tooltipText, title;
        string cancelText = null;
        int benefit;
        bool isResistance;
        //return list of button details
        List<EventButtonDetails> tempList = new List<EventButtonDetails>();
        //Cancel button tooltip (handles all no go cases)
        StringBuilder infoBuilder = new StringBuilder();
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        //color code for button tooltip header text, eg. "Operator"ss
        if (playerSide.level == globalAuthority.level)
        { sideColour = colourAuthority; isResistance = false; }
        else { sideColour = colourResistance; isResistance = true; }
        //get actor
        Actor actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, playerSide);
        //if actor is Null, a single button (Cancel) menu is still provided
        if (actor != null)
        {
            title = string.Format("{0}", isResistance ? "" : GameManager.instance.metaScript.GetAuthorityTitle().ToString() + " ");
            cancelText = string.Format("{0} {1}", actor.arc.name, actor.actorName);
            switch (actor.Status)
            {
                case ActorStatus.Active:
                    //
                    // - - - Manage (both sides) - - - 
                    //
                    ModalActionDetails manageActionDetails = new ModalActionDetails() { };
                    manageActionDetails.side = playerSide;
                    manageActionDetails.actorSlotID = actor.actorSlotID;
                    tooltipText = string.Format("Select to choose what to do with {0} (send to the Reserve Pool, Dismiss or Dispose Off)", actor.actorName);
                    EventButtonDetails dismissDetails = new EventButtonDetails()
                    {
                        buttonTitle = "MANAGE",
                        buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                        buttonTooltipMain = string.Format("Unfortunately {0}{1}{2} {3}{4}  isn't working out", colourNeutralEffect, actor.arc.name, colourEnd, 
                        title, actor.actorName),
                        buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, tooltipText, colourEnd),
                        //use a Lambda to pass arguments to the action
                        action = () => { EventManager.instance.PostNotification(EventType.ManageActorAction, this, manageActionDetails); }
                    };
                    //add Lie Low button to list
                    tempList.Add(dismissDetails);
                    //
                    // - - - Resistance - - -
                    //
                    if (isResistance == true)
                    {
                        //
                        // - - - Lie Low - - -
                        //
                        //Invisibility must be less than max
                        if (actor.datapoint2 < maxStatValue)
                        {
                            ModalActionDetails lielowActionDetails = new ModalActionDetails() { };
                            lielowActionDetails.side = playerSide;
                            lielowActionDetails.actorSlotID = actor.actorSlotID;
                            int numOfTurns = 3 - actor.datapoint2;
                            tooltipText = string.Format("{0} will regain Invisibility and automatically reactivate in {1} turn{2}", actor.actorName, numOfTurns,
                                numOfTurns != 1 ? "s" : "");
                            EventButtonDetails lielowDetails = new EventButtonDetails()
                            {
                                buttonTitle = "Lie Low",
                                buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                                buttonTooltipMain = string.Format("{0} {1} will be asked to keep a low profile and stay out of sight", actor.arc.name, actor.actorName),
                                buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, tooltipText, colourEnd),
                                //use a Lambda to pass arguments to the action
                                action = () => { EventManager.instance.PostNotification(EventType.LieLowAction, this, lielowActionDetails); }
                            };
                            //add Lie Low button to list
                            tempList.Add(lielowDetails);
                        }
                        else
                        {
                            //actor invisiblity at max
                            infoBuilder.Append(string.Format("{0} Invisibility at Max and can't Lie Low", actor.arc.name));
                        }
                        //
                        // - - - Give Gear - - -
                        //
                        int numOfGear = GameManager.instance.playerScript.GetNumOfGear();
                        if (numOfGear > 0)
                        {
                            List<int> listOfGear = GameManager.instance.playerScript.GetListOfGear();
                            if (listOfGear != null)
                            {
                                //loop gear and create a button for each
                                for (int i = 0; i < numOfGear; i++)
                                {
                                    Gear gear = GameManager.instance.dataScript.GetGear(listOfGear[i]);
                                    if (gear != null)
                                    {
                                        ModalActionDetails gearActionDetails = new ModalActionDetails() { };
                                        gearActionDetails.side = playerSide;
                                        gearActionDetails.actorSlotID = actor.actorSlotID;
                                        gearActionDetails.gearID = gear.gearID;
                                        //get actor's preferred gear
                                        GearType preferredGear = actor.arc.preferredGear;
                                        if (preferredGear != null)
                                        {
                                            switch (gear.rarity.name)
                                            {
                                                case "Common": benefit = GameManager.instance.gearScript.gearBenefitCommon; break;
                                                case "Rare": benefit = GameManager.instance.gearScript.gearBenefitRare; break;
                                                case "Unique": benefit = GameManager.instance.gearScript.gearBenefitUnique; break;
                                                default:
                                                    benefit = 0;
                                                    Debug.LogError(string.Format("Invalid gear rarity \"{0}\"", gear.rarity.name));
                                                    break;
                                            }
                                            if (preferredGear.name.Equals(gear.type.name) == true)
                                            { tooltipText = string.Format("Preferred Gear for {0}{1}{2}{3} motivation +{4}{5}Transfer {6} renown to Player from {7}{8}", 
                                                actor.arc.name, "\n", colourGoodEffect, actor.actorName, benefit, "\n", benefit, actor.actorName, colourEnd); }
                                            else
                                            { tooltipText = string.Format("NOT Preferred Gear (prefers {0}{1}{2}){3}{4}{5} Motivation +{6}{7}", colourNeutralEffect, 
                                                preferredGear.name, colourEnd, "\n", colourGoodEffect, actor.actorName, benefit, colourEnd); }
                                        }
                                        else
                                        {
                                            tooltipText = "Unknown Preferred Gear";
                                            Debug.LogError(string.Format("Invalid preferredGear (Null) for actor Arc {0}", actor.arc.name));
                                        }
                                        EventButtonDetails gearDetails = new EventButtonDetails()
                                        {
                                            buttonTitle = string.Format("Give {0}", gear.name),
                                            buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                                            buttonTooltipMain = string.Format("Give {0} ({1}{2}{3}) to {4} {5}", gear.name, colourNeutralEffect, gear.type.name, 
                                            colourEnd, actor.arc.name, actor.actorName),
                                            buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, tooltipText, colourEnd),
                                            //use a Lambda to pass arguments to the action
                                            action = () => { EventManager.instance.PostNotification(EventType.GiveGearAction, this, gearActionDetails); }
                                        };
                                        //add Lie Low button to list
                                        tempList.Add(gearDetails);
                                    }
                                    else { Debug.LogError(string.Format("Invalid gear (Null) for gearID {0}", listOfGear[i])); }
                                }
                            }
                            else { Debug.LogError("Invalid listOfGear (Null)"); }
                        }
                        if (infoBuilder.Length > 0) { infoBuilder.AppendLine(); }
                        infoBuilder.Append("You have no Gear to give");
                    }

                    break;
                //
                // - - - Actor Inactive - - -
                //
                case ActorStatus.Inactive:
                    //
                    // - - - Resistance - - -
                    //
                    if (isResistance == true)
                    {
                        //
                        // - - - Activate - - -
                        //
                        ModalActionDetails activateActionDetails = new ModalActionDetails() { };
                        activateActionDetails.side = playerSide;
                        activateActionDetails.actorSlotID = actor.actorSlotID;
                        int numOfTurns = 3 - actor.datapoint2;
                        tooltipText = string.Format("{0} is Lying Low and will automatically return in {1} turn{2} if not Activated", actor.actorName, numOfTurns,
                            numOfTurns != 1 ? "s" : "");
                        EventButtonDetails activateDetails = new EventButtonDetails()
                        {
                            buttonTitle = "Activate",
                            buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                            buttonTooltipMain = string.Format("{0} {1} will be Immediately Recalled", actor.arc.name, actor.actorName),
                            buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, tooltipText, colourEnd),
                            //use a Lambda to pass arguments to the action
                            action = () => { EventManager.instance.PostNotification(EventType.ActivateAction, this, activateActionDetails); }
                        };
                        //add Lie Low button to list
                        tempList.Add(activateDetails);
                        //cancel tooltips
                        infoBuilder.Append("Gear cannot be given when Lying Low");
                    }
                    break;
                //
                // - - - Actor Captured or other - - -
                //
                default:
                    cancelText = string.Format("{0}Actor is \"{1}\" and out of contact{2}", colourBadEffect, actor.Status, colourEnd);
                    break;
            }

        }
        else { Debug.LogError(string.Format("Invalid actor (Null) for actorSlotID {0}", actorSlotID)); }

        //Debug
        if (string.IsNullOrEmpty(cancelText)) { cancelText = "Unknown"; }
        if (infoBuilder.Length == 0) { infoBuilder.Append("Test data"); }

        //
        // - - - Cancel - - - (both sides)
        //
        //Cancel button is added last
        EventButtonDetails cancelDetails = null;
        if (actor != null)
        {
            if (infoBuilder.Length > 0)
            {
                cancelDetails = new EventButtonDetails()
                {
                    buttonTitle = "CANCEL",
                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                    buttonTooltipMain = cancelText,
                    buttonTooltipDetail = string.Format("{0}{1}{2}", colourCancel, infoBuilder.ToString(), colourEnd),
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this); }
                };
            }
            else
            {
                //necessary to prevent color tags triggering the bottom divider in TooltipGeneric
                cancelDetails = new EventButtonDetails()
                {
                    buttonTitle = "CANCEL",
                    buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                    buttonTooltipMain = cancelText,
                    //use a Lambda to pass arguments to the action
                    action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this); }
                };
            }
        }
        else
        {
            //Null actor -> invalid menu creation
            cancelDetails = new EventButtonDetails()
            {
                buttonTitle = "CANCEL",
                buttonTooltipHeader = string.Format("{0}{1}{2}", sideColour, "INFO", colourEnd),
                buttonTooltipMain = string.Format("{0}Invalid Actor{1}",colourBadEffect, colourEnd),
                //use a Lambda to pass arguments to the action
                action = () => { EventManager.instance.PostNotification(EventType.CloseActionMenu, this); }
            };
        }
        //add Cancel button to list
        tempList.Add(cancelDetails);

        return tempList;
    }

    /// <summary>
    /// call this to recruit an actor of a specified level (1 to 3) from a Decision
    /// </summary>
    /// <param name="level"></param>
    public void RecruitActor(int level = 0)
    {
        Debug.Assert(level > 0 && level <= 3, string.Format("Invalid level {0}", level));
        ModalActionDetails details = new ModalActionDetails();
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        //ignore node and actorSlotID
        details.side = side;
        details.level = level;
        details.nodeID = -1;
        details.actorSlotID = -1;
        //get event
        switch (side.name)
        {
            case "Authority":
                details.eventType = EventType.GenericRecruitActorAuthority;
                break;
            case "Resistance":
                details.eventType = EventType.GenericRecruitActorResistance;
                break;
            default:
                Debug.LogError(string.Format("Invalid side \"{0}\"", side.name));
                break;
        }
        InitialiseGenericPickerRecruit(details);
    }

    /// <summary>
    /// Choose Recruit (Both sides): sets up ModalGenericPicker class and triggers event: ModalGenericEvent.cs -> SetGenericPicker()
    /// </summary>
    /// <param name="details"></param>
    private void InitialiseGenericPickerRecruit(ModalActionDetails details)
    {
        bool errorFlag = false;
        int index, countOfRecruits;
        GenericPickerDetails genericDetails = new GenericPickerDetails();
        Node node = null;
        if (details.side.level == GameManager.instance.globalScript.sideResistance.level)
        {
            node = GameManager.instance.dataScript.GetNode(details.nodeID);
            if (node != null)
            {
                //check for player/actor being captured
                int actorID = 999;
                if (node.nodeID != GameManager.instance.nodeScript.nodePlayer)
                {
                    Actor actor = GameManager.instance.dataScript.GetCurrentActor(details.actorSlotID, globalResistance);
                    if (actor != null)
                    { actorID = actor.actorID; }
                    else
                    {
                        Debug.LogError(string.Format("Invalid actor (Null) for details.ActorSlotID {0}", details.actorSlotID));
                        errorFlag = true;
                    }
                }
                //check capture provided no errors
                if (errorFlag == false)
                {
                    AIDetails aiDetails = GameManager.instance.captureScript.CheckCaptured(node.nodeID, actorID);
                    if (aiDetails != null)
                    {
                        //capture happened, abort recruitment
                        aiDetails.effects = string.Format("{0}The Recruiting mission was a wipe{1}", colourNeutralEffect, colourEnd);
                        EventManager.instance.PostNotification(EventType.Capture, this, aiDetails);
                        return;
                    }
                }
                else
                {
                    //reset flag to the default state prior to recruitments
                    errorFlag = false;
                }
            }
        }
        if (details.side.level == globalAuthority.level || node != null)
        {
            if (details.side.level == globalResistance.level) { genericDetails.returnEvent = EventType.GenericRecruitActorResistance; }
            else if (details.side.level == globalAuthority.level) { genericDetails.returnEvent = EventType.GenericRecruitActorAuthority; }
            else { Debug.LogError(string.Format("Invalid side \"{0}\"", details.side)); }
            genericDetails.side = details.side;
            if (details.side.level == globalResistance.level)
            { genericDetails.nodeID = details.nodeID; }
            else { genericDetails.nodeID = -1; }
            genericDetails.actorSlotID = details.actorSlotID;
            //picker text
            genericDetails.textTop = string.Format("{0}Recruits{1} {2}available{3}", colourNeutralEffect, colourEnd, colourNormal, colourEnd);
            genericDetails.textMiddle = string.Format("{0}Recruit will be assigned to your reserve list{1}",
                colourNormal, colourEnd);
            genericDetails.textBottom = "Click on a Recruit to Select. Press CONFIRM to hire Recruit. Mouseover recruit for more information.";
            //
            //generate temp list of Recruits to choose from and a list of ones for the picker
            //
            int numOfOptions;
            List<int> listOfPoolActors = new List<int>();
            List<int> listOfPickerActors = new List<int>();
            List<int> listOfCurrentArcIDs = new List<int>(GameManager.instance.dataScript.GetAllCurrentActorArcIDs(details.side));
            //selection methodology varies for each side -> need to populate 'listOfPoolActors'
            if (details.side.level == globalResistance.level)
            {
                if (node.nodeID == GameManager.instance.nodeScript.nodePlayer)
                {
                    //player at node, select from 3 x level 1 options, different from current OnMap actor types
                    listOfPoolActors.AddRange(GameManager.instance.dataScript.GetActorPool(1, details.side));
                    //loop backwards through pool of actors and remove any that match the curent OnMap types
                    for (int i = listOfPoolActors.Count - 1; i >= 0; i--)
                    {
                        Actor actor = GameManager.instance.dataScript.GetActor(listOfPoolActors[i]);
                        if (actor != null)
                        {
                            if (listOfCurrentArcIDs.Exists(x => x == actor.arc.ActorArcID))
                            { listOfPoolActors.RemoveAt(i); }
                        }
                        else { Debug.LogWarning(string.Format("Invalid actor (Null) for actorID {0}", listOfPoolActors[i])); }
                    }
                }
                else
                {
                    //actor at node, select from 3 x level 2 options (random types, could be the same as currently OnMap)
                    listOfPoolActors.AddRange(GameManager.instance.dataScript.GetActorPool(2, details.side));
                }
            }
            //Authority
            else if (details.side.level == globalAuthority.level)
            {
                //placeholder -> select from 3 x specified level options (random types, could be the same as currently OnMap)
                listOfPoolActors.AddRange(GameManager.instance.dataScript.GetActorPool(details.level, details.side));
            }
            else { Debug.LogError(string.Format("Invalid side \"{0}\"", details.side)); }
            //
            //select three actors for the picker
            //
            numOfOptions = Math.Min(3, listOfPoolActors.Count);
            for (int i = 0; i < numOfOptions; i++)
            {
                index = Random.Range(0, listOfPoolActors.Count);
                listOfPickerActors.Add(listOfPoolActors[index]);
                //remove actor from pool to prevent duplicate types
                listOfPoolActors.RemoveAt(index);
            }
            //check there is at least one recruit available
            countOfRecruits = listOfPickerActors.Count;
            if (countOfRecruits < 1)
            {
                //OUTCOME -> No recruits available
                Debug.LogWarning("ActorManager: No recruits available in InitaliseGenericPickerRecruits");
                errorFlag = true;
            }
            else
            {
                //
                //loop actorID's that have been selected and package up ready for ModalGenericPicker
                //
                for (int i = 0; i < countOfRecruits; i++)
                {
                    Actor actor = GameManager.instance.dataScript.GetActor(listOfPickerActors[i]);
                    if (actor != null)
                    {
                        //option details
                        GenericOptionDetails optionDetails = new GenericOptionDetails();
                        optionDetails.optionID = actor.actorID;
                        optionDetails.text = actor.arc.name;
                        optionDetails.sprite = actor.arc.baseSprite;
                        //tooltip
                        GenericTooltipDetails tooltipDetails = new GenericTooltipDetails();
                        //arc type and name
                        tooltipDetails.textHeader = string.Format("{0}{1}{2}{3}{4}{5}{6}", colourRecruit, actor.arc.name, colourEnd,
                            "\n", colourNormal, actor.actorName, colourEnd);
                        //stats
                        string[] arrayOfQualities = GameManager.instance.dataScript.GetQualities(details.side);
                        StringBuilder builder = new StringBuilder();
                        if (arrayOfQualities.Length > 0)
                        {
                            builder.Append(string.Format("{0}  {1}{2}{3}{4}", arrayOfQualities[0], GameManager.instance.colourScript.GetValueColour(actor.datapoint0), 
                                actor.datapoint0, colourEnd,"\n"));
                            builder.Append(string.Format("{0}  {1}{2}{3}{4}", arrayOfQualities[1], GameManager.instance.colourScript.GetValueColour(actor.datapoint1), 
                                actor.datapoint1, colourEnd, "\n"));
                            builder.Append(string.Format("{0}  {1}{2}{3}", arrayOfQualities[2], GameManager.instance.colourScript.GetValueColour(actor.datapoint2), 
                                actor.datapoint2, colourEnd));
                            tooltipDetails.textMain = string.Format("{0}{1}{2}", colourNormal, builder.ToString(), colourEnd);
                        }
                        //trait and action
                        tooltipDetails.textDetails = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}", "<font=\"Bangers SDF\">", 
                            GameManager.instance.colourScript.GetValueColour(1 + actor.trait.typeOfTrait.level), 
                            "<cspace=0.6em>", actor.trait.name, "</cspace>", colourEnd, "</font>", "\n", colourNormal, actor.arc.nodeAction.name, colourEnd);
                        //add to master arrays
                        genericDetails.arrayOfOptions[i] = optionDetails;
                        genericDetails.arrayOfTooltips[i] = tooltipDetails;
                    }
                    else { Debug.LogError(string.Format("Invalid actor (Null) for gearID {0}", listOfPickerActors[i])); }
                }
            }
        }
        else
        {
            Debug.LogError(string.Format("Invalid Node (null) for nodeID {0}", details.nodeID));
            errorFlag = true;
        }
        //final processing, either trigger an event for GenericPicker or go straight to an error based Outcome dialogue
        if (errorFlag == true)
        {
            //create an outcome window to notify player
            ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
            outcomeDetails.side = globalResistance;
            outcomeDetails.textTop = "There has been a Snafu in communication and no recruits can be found.";
            outcomeDetails.textBottom = "Heads will roll!";
            EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
        }
        else
        {
            //activate Generic Picker window
            EventManager.instance.PostNotification(EventType.OpenGenericPicker, this, genericDetails);
        }
     
    }

    /// <summary>
    /// Processes choice of Recruit Resistance Actor
    /// </summary>
    /// <param name="returnDetails"></param>
    private void ProcessRecruitChoiceResistance(GenericReturnData data)
    {
        bool successFlag = true;
        StringBuilder builderTop = new StringBuilder();
        StringBuilder builderBottom = new StringBuilder();
        Sprite sprite = GameManager.instance.outcomeScript.errorSprite;
        GlobalSide playerSide = GameManager.instance.sideScript.PlayerSide;
        if (data != null)
        {
            if (data.optionID > -1)
            {
                //find actor
                Actor actorCurrent = GameManager.instance.dataScript.GetCurrentActor(data.actorSlotID, playerSide);
                if (actorCurrent != null)
                {
                    Actor actorRecruited = GameManager.instance.dataScript.GetActor(data.optionID);
                    if (actorRecruited != null)
                    {
                        //add actor to reserve pool
                        if (GameManager.instance.dataScript.AddActorToReserve(actorRecruited.actorID, playerSide) == true)
                        {
                            //change actor's status
                            actorRecruited.Status = ActorStatus.Reserve;
                            //remove actor from appropriate pool list
                            GameManager.instance.dataScript.RemoveActorFromPool(actorRecruited.actorID, actorRecruited.level, playerSide);
                            //sprite of recruited actor
                            sprite = actorRecruited.arc.baseSprite;
                            //initiliase unhappy timer
                            actorRecruited.unhappyTimer = recruitedReserveTimer;
                            //actor successfully recruited
                            builderTop.Append(string.Format("{0}The interview went well!{1}", colourNormal, colourEnd));
                            builderBottom.Append(string.Format("{0}{1}{2}, {3}\"{4}\", has been recruited and is available in the Reserve List{5}", colourArc,
                                actorRecruited.arc.name, colourEnd, colourNormal, actorRecruited.actorName, colourEnd));
                            //message
                            string textMsg = string.Format("{0}, {1}, ID {2} has been recruited", actorRecruited.actorName, actorRecruited.arc.name,
                                actorRecruited.actorID);
                            Message message = GameManager.instance.messageScript.ActorRecruited(textMsg, data.nodeID, actorRecruited.actorID,
                                GameManager.instance.globalScript.sideResistance);
                            if (message != null) { GameManager.instance.dataScript.AddMessage(message); }
                            //Process any other effects, if acquisition was successfull, ignore otherwise
                            Action action = actorCurrent.arc.nodeAction;
                            List<Effect> listOfEffects = action.GetEffects();
                            if (listOfEffects.Count > 0)
                            {
                                EffectDataInput dataInput = new EffectDataInput();
                                Node node = GameManager.instance.dataScript.GetNode(data.nodeID);
                                if (node != null)
                                {
                                    foreach (Effect effect in listOfEffects)
                                    {
                                        if (effect.ignoreEffect == false)
                                        {
                                            EffectDataReturn effectReturn = GameManager.instance.effectScript.ProcessEffect(effect, node, dataInput, actorCurrent);
                                            if (effectReturn != null)
                                            {
                                                builderTop.AppendLine();
                                                builderTop.Append(effectReturn.topText);
                                                builderBottom.AppendLine();
                                                builderBottom.AppendLine();
                                                builderBottom.Append(effectReturn.bottomText);
                                                //exit effect loop on error
                                                if (effectReturn.errorFlag == true) { break; }
                                            }
                                            else { Debug.LogError("Invalid effectReturn (Null)"); }
                                        }
                                    }
                                }
                                else { Debug.LogWarning(string.Format("Invalid node (Null) for nodeID {0}", data.nodeID)); }
                            }
                        }
                        else
                        {
                            //some issue prevents actor being added to reserve pool (full? -> probably not as a criteria checks this)
                            successFlag = false;
                        }
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("Invalid Recruited Actor (Null) for actorID {0}", data.optionID));
                        successFlag = false;
                    }
                }
                else
                {
                    Debug.LogWarning(string.Format("Invalid Current Actor (Null) for actorSlotID {0}", data.actorSlotID));
                    successFlag = false;
                }
            }
            else
            { Debug.LogWarning(string.Format("Invalid optionID {0}", data.optionID)); }
        }
        else
        { Debug.LogError("Invalid GenericReturnData (Null)"); successFlag = false; }
        //failed outcome
        if (successFlag == false)
        {
            builderTop.Append("Something has gone wrong. Our Recruit has gone missing");
            builderBottom.Append("This is a matter of concern");
        }
        //
        // - - - Outcome - - - 
        //
        ModalOutcomeDetails outcomeDetails = new ModalOutcomeDetails();
        outcomeDetails.textTop = builderTop.ToString();
        outcomeDetails.textBottom = builderBottom.ToString();
        outcomeDetails.sprite = sprite;
        outcomeDetails.side = playerSide;
        //action expended automatically for recruit actor
        if (successFlag == true)
        { outcomeDetails.isAction = true; }
        //generate a create modal window event
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, outcomeDetails);
    }

    /// <summary>
    /// Processes choice of Recruit Authority Actor
    /// </summary>
    /// <param name="returnDetails"></param>
    private void ProcessRecruitChoiceAuthority(GenericReturnData data)
    {
        bool successFlag = true;
        StringBuilder builderTop = new StringBuilder();
        StringBuilder builderBottom = new StringBuilder();
        Sprite sprite = GameManager.instance.outcomeScript.errorSprite;
        GlobalSide side = GameManager.instance.sideScript.PlayerSide;
        if (data != null)
        {
            if (data.optionID > -1)
            {
                //find actor
                Actor actorRecruited = GameManager.instance.dataScript.GetActor(data.optionID);
                if (actorRecruited != null)
                {
                    //add actor to reserve pool
                    if (GameManager.instance.dataScript.AddActorToReserve(actorRecruited.actorID, side) == true)
                    {
                        //change actor's status
                        actorRecruited.Status = ActorStatus.Reserve;
                        //remove actor from appropriate pool list
                        GameManager.instance.dataScript.RemoveActorFromPool(actorRecruited.actorID, actorRecruited.level, side);
                        //sprite of recruited actor
                        sprite = actorRecruited.arc.baseSprite;
                        //initiliase unhappy timer
                        actorRecruited.unhappyTimer = recruitedReserveTimer;
                        //message
                        string textMsg = string.Format("{0}, {1}, ID {2} has been recruited", actorRecruited.actorName, actorRecruited.arc.name,
                            actorRecruited.actorID);
                        Message message = GameManager.instance.messageScript.ActorRecruited(textMsg, data.nodeID, actorRecruited.actorID,
                            GameManager.instance.globalScript.sideAuthority);
                        if (message != null) { GameManager.instance.dataScript.AddMessage(message); }
                        //actor successfully recruited
                        builderTop.Append(string.Format("{0}The interview went well!{1}", colourNormal, colourEnd));
                        builderBottom.Append(string.Format("{0}{1}{2}, {3}\"{4}\", has been recruited and is available in the Reserve List{5}", colourArc,
                            actorRecruited.arc.name, colourEnd, colourNormal, actorRecruited.actorName, colourEnd));
                    }
                    else
                    {
                        //some issue prevents actor being added to reserve pool (full? -> probably not as a criteria checks this)
                        successFlag = false;
                    }
                }
                else
                {
                    Debug.LogWarning(string.Format("Invalid Recruited Actor (Null) for actorID {0}", data.optionID));
                    successFlag = false;
                }
            }
            else
            { Debug.LogWarning(string.Format("Invalid optionID {0}", data.optionID)); }
        }
        else
        { Debug.LogError("Invalid GenericDataReturn (Null)"); successFlag = false; }
        //failed outcome
        if (successFlag == false)
        {
            builderTop.Append("Something has gone wrong. Our Recruit has gone missing");
            builderBottom.Append("This is a matter of concern");
        }
        //
        // - - - Outcome - - - 
        //
        ModalOutcomeDetails details = new ModalOutcomeDetails();
        details.textTop = builderTop.ToString();
        details.textBottom = builderBottom.ToString();
        details.sprite = sprite;
        details.side = side;
        if (successFlag == true)
        { details.isAction = true; }
        EventManager.instance.PostNotification(EventType.OpenOutcomeWindow, this, details);
    }

    /// <summary>
    /// Debug method to display actor pools (both sides)
    /// </summary>
    /// <returns></returns>
    public string DisplayPools()
    {
        List<int> listOfActors = new List<int>();
        StringBuilder builder = new StringBuilder();
        //Resistance
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorPool(1, globalResistance), " ResistanceActorPoolLevelOne"));
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorPool(2, globalResistance), " ResistanceActorPoolLevelTwo"));
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorPool(3, globalResistance), " ResistanceActorPoolLevelThree"));
        //Authority
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorPool(1, globalAuthority), " AuthorityActorPoolLevelOne"));
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorPool(2, globalAuthority), " AuthorityActorPoolLevelTwo"));
        builder.Append(DisplaySubPool(GameManager.instance.dataScript.GetActorPool(3, globalAuthority), " AuthorityActorPoolLevelThree"));
        return builder.ToString();
    }

    /// <summary>
    /// submethod for DisplayPools
    /// </summary>
    /// <param name="listOfActors"></param>
    /// <param name="header"></param>
    /// <returns></returns>
    private string DisplaySubPool(List<int> listOfActors, string header)
    {
        StringBuilder subBuilder = new StringBuilder();
        subBuilder.Append(header);
        subBuilder.AppendLine();
        for (int i = 0; i < listOfActors.Count; i++)
        {
            Actor actor = GameManager.instance.dataScript.GetActor(listOfActors[i]);
            subBuilder.Append(string.Format(" actID {0}, {1}, L{2}, {3}-{4}-{5}{6}", actor.actorID, actor.arc.name.ToLower(), actor.level,
                actor.datapoint0, actor.datapoint1, actor.datapoint2, "\n"));
        }
        subBuilder.AppendLine();
        return subBuilder.ToString();
    }

    /// <summary>
    /// Checks all OnMap Inactive Resistance actors, increments invisibility and returns any at max value back to Active status
    /// </summary>
    private void CheckInactiveActors()
    {
        // Resistance actors only
        Actor[] arrayOfActorsResistance = GameManager.instance.dataScript.GetCurrentActors(globalResistance);
        if (arrayOfActorsResistance != null)
        {
            for (int i = 0; i < arrayOfActorsResistance.Length; i++)
            {
                //check actor is present in slot (not vacant)
                if (GameManager.instance.dataScript.CheckActorSlotStatus(i, globalResistance) == true)
                {
                    Actor actor = arrayOfActorsResistance[i];
                    if (actor != null)
                    {
                        if (actor.Status == ActorStatus.Inactive)
                        {
                            if (actor.datapoint2 >= maxStatValue)
                            {
                                //actor has recovered from lying low, needs to be activated
                                actor.datapoint2 = Mathf.Min(maxStatValue, actor.datapoint2);
                                actor.Status = ActorStatus.Active;
                                GameManager.instance.guiScript.UpdateActorAlpha(actor.actorSlotID, GameManager.instance.guiScript.alphaActive);
                                string text = string.Format("{0} {1} has automatically reactivated", actor.arc.name, actor.actorName);
                                Message message = GameManager.instance.messageScript.ActorStatus(text, actor.actorID, GameManager.instance.sideScript.PlayerSide, true);
                                GameManager.instance.dataScript.AddMessage(message);
                            }
                            else
                            {
                                //increment invisibility -> Must be AFTER reactivation check otherwise it will take 1 turn less than it should
                                actor.datapoint2++;
                            }
                        }
                    }
                    else { Debug.LogError(string.Format("Invalid Resistance actor (Null), index {0}", i)); }
                }
            }
        }
        else { Debug.LogError("Invalid arrayOfActorsResistance (Null)"); }
    }


    /// <summary>
    /// Checks all reserve pool actors (both sides), decrements unhappy timers and takes appropriate action if any have reached zero
    /// </summary>
    private void CheckReserveActors()
    {
        List<int> listOfActors = null;
        //
        // - - - Resistance - - -
        //
        listOfActors = GameManager.instance.dataScript.GetActorList(globalResistance, ActorList.Reserve);
        if (listOfActors != null)
        {
            //loop list of reserve actors
            for (int i = 0; i < listOfActors.Count; i++)
            {
                Actor actor = GameManager.instance.dataScript.GetActor(listOfActors[i]);
                if (actor != null)
                {
                    //Decrement unhappy timer if not yet zero
                    if (actor.unhappyTimer > 0)
                    {
                        actor.unhappyTimer--;
                        Debug.Log(string.Format("CheckReserveActors: Resistance {0} {1} unhappy timer now {2}{3}", actor.arc.name, actor.actorName, actor.unhappyTimer, "\n"));
                        //if timer now zero, gain condition "Unhappy"
                        if (actor.unhappyTimer == 0)
                        {
                            Condition condition = GameManager.instance.dataScript.GetCondition("UNHAPPY");
                            actor.AddCondition(condition);
                        }
                    }
                    else
                    {
                        //unhappy timer has reached zero. Is actor's motivation > 0?
                        if (actor.datapoint1 > 0)
                        {
                            //chance of decrementing motivation each turn till it reaches zero
                            if (Random.Range(0, 100) < unhappyLoseMotivationChance)
                            {
                                actor.datapoint1--;
                                Debug.Log(string.Format("CheckReserveActors: Resistance {0} {1} UNHAPPY, Motivation now {2}{3}", actor.arc.name, actor.actorName, 
                                    actor.datapoint1, "\n"));
                            }
                        }
                        else
                        {
                            //actor is Unhappy and has 0 motivation. Do they take action?
                            if (Random.Range(0, 100) < unhappyTakeActionChance)
                            {
                                Debug.Log(string.Format("CheckReserveActors: Resistance {0} {1} takes ACTION {3}", actor.arc.name, actor.actorName, "\n"));
                                TakeAction(actor);
                            }
                        }
                    }
                }
                else { Debug.LogError(string.Format("Invalid Resitance actor (Null) for actorID {0}", listOfActors[i])); }
            }
        }
        else { Debug.LogError("Invalid listOfActors -> Resistance (Null)"); }
        //
        // - - - Authority - - -
        //
        listOfActors = GameManager.instance.dataScript.GetActorList(globalAuthority, ActorList.Reserve);
        if (listOfActors != null)
        {
            //loop list of reserve actors
            for (int i = 0; i < listOfActors.Count; i++)
            {
                Actor actor = GameManager.instance.dataScript.GetActor(listOfActors[i]);
                if (actor != null)
                {
                    //Decrement unhappy timer if not yet zero
                    if (actor.unhappyTimer > 0)
                    {
                        actor.unhappyTimer--;
                        Debug.Log(string.Format("CheckReserveActors: Authority {0} {1} unhappy timer now {2}{3}", actor.arc.name, actor.actorName, actor.unhappyTimer, "\n"));
                        //if timer now zero, gain condition "Unhappy"
                        if (actor.unhappyTimer == 0)
                        {
                            Condition condition = GameManager.instance.dataScript.GetCondition("UNHAPPY");
                            actor.AddCondition(condition);
                        }
                    }
                    else
                    {
                        //unhappy timer has reached zero. Is actor's motivation > 0?
                        if (actor.datapoint1 > 0)
                        {
                            //chance of decrementing motivation each turn till it reaches zero
                            if (Random.Range(0, 100) < unhappyLoseMotivationChance)
                            {
                                actor.datapoint1--;
                                Debug.Log(string.Format("CheckReserveActors: Authority {0} {1} UNHAPPY, Motivation now {2}{3}", actor.arc.name, actor.actorName, 
                                    actor.datapoint1, "\n"));
                            }
                        }
                        else
                        {
                            //actor is Unhappy and has 0 motivation. Do they take action?
                            if (Random.Range(0, 100) < unhappyTakeActionChance)
                            {
                                Debug.Log(string.Format("CheckReserveActors: Authority {0} {1} takes ACTION {3}", actor.arc.name, actor.actorName, "\n"));
                                TakeAction(actor);
                            }
                        }
                    }
                }
                else { Debug.LogError(string.Format("Invalid Authority actor (Null) for actorID {0}", listOfActors[i])); }
            }
        }
        else { Debug.LogError("Invalid listOfActors -> Authority (Null)"); }
    }

    /// <summary>
    /// An unhappy actor takes action (both sides)
    /// NOTE: Actor is assumed to be Null checked by the calling method
    /// </summary>
    /// <param name="actor"></param>
    private void TakeAction(Actor actor)
    {
        //Check for secrets first
        if (actor.CheckNumOfSecrets() > 0)
        {
            if (Random.Range(0, 100) < unhappyRevealSecretChance)
            {

                //TO DO
                Debug.Log(string.Format("Unhappy Actor: {0} {1} Threatens to reveal a Secret{2}", actor.arc.name, actor.actorName, "\n"));
                return;
            }
        }
        //Check for Leaving second
        if (actor.hasComplained == false)
        {
            if (Random.Range(0, 100) < (unhappyLeaveChance))
            {

                //TO DO
                Debug.Log(string.Format("Unhappy Actor: {0} {1} Threatens to Leave{2}", actor.arc.name, actor.actorName, "\n"));
                return;
            }
        }
        else
        {
            //double chance if actor has already complained
            if (Random.Range(0, 100) < (unhappyLeaveChance * 2))
            {

                //TO DO
                Debug.Log(string.Format("Unhappy Actor: {0} {1} Threatens to Leave{2}", actor.arc.name, actor.actorName, "\n"));
                return;
            }
        }
        //Check for Complaint third (skip check if actor has already complained)
        if (actor.hasComplained == false)
        {
            if (Random.Range(0, 100) < unhappyComplainChance)
            {

                //TO DO
                Debug.Log(string.Format("Unhappy Actor: {0} {1} Threatens to Complain{2}", actor.arc.name, actor.actorName, "\n"));
                actor.hasComplained = true;
                return;
            }
        }


    }

    //new methods above here
}
