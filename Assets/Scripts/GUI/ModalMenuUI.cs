using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using gameAPI;
using packageAPI;
using modalAPI;

/// <summary>
/// allows Node / Actor / Gear tooltip to show with mouseover of Modal Action Menu header
/// </summary>
public class ModalMenuUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //data needed for tooltips and passed by ModalActionMenu.cs -> SetActionMenu
    [HideInInspector] public int nodeID;
    [HideInInspector] public int gearID;
    [HideInInspector] public int actorSlotID;
    [HideInInspector] public ActionMenuType menuType;

    private float mouseOverDelay;
    private float mouseOverFade;
    private bool onMouseFlag;
    private Coroutine myCoroutine;
    private RectTransform rectTransform;
    private GameObject nodeObject;
    private Node node;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// constructor -> needs to be Start as GameManager hasn't initialised prior to this (UI elements initialise before normal gameObjects?)
    /// </summary>
    private void Start()
    {
        mouseOverDelay = GameManager.instance.tooltipScript.tooltipDelay;
        mouseOverFade = GameManager.instance.tooltipScript.tooltipFade;
        //need to get nodeObject details at start to accomodate OnPointerEnter & node component details later in coroutine
        nodeObject = GameManager.instance.dataScript.GetNodeObject(nodeID);

    }


    /// <summary>
    /// Mouse Over event
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        onMouseFlag = true;
        switch (menuType)
        {
            case ActionMenuType.Node:
            case ActionMenuType.NodeGear:
                if (nodeObject != null)
                { myCoroutine = StartCoroutine("ShowTooltip"); }
                break;
            case ActionMenuType.Actor:
                if (actorSlotID > -1)
                { myCoroutine = StartCoroutine("ShowTooltip"); }
                break;
            case ActionMenuType.Player:
                myCoroutine = StartCoroutine("ShowTooltip");
                break;
            case ActionMenuType.Gear:
                if (gearID > -1)
                { myCoroutine = StartCoroutine("ShowTooltip"); }
                break;
        }
    }

    /// <summary>
    /// Mouse exit, close tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        onMouseFlag = false;
        if (myCoroutine != null)
        { StopCoroutine(myCoroutine); }
        switch (menuType)
        {
            case ActionMenuType.Node:
            case ActionMenuType.NodeGear:
                GameManager.instance.tooltipNodeScript.CloseTooltip("ModalMenuUI.cs -> OnPointerExit");
                break;
            case ActionMenuType.Actor:
                GameManager.instance.tooltipActorScript.CloseTooltip("ModalMenuUI.cs -> OnPointerExit");
                break;
            case ActionMenuType.Player:
                GameManager.instance.tooltipPlayerScript.CloseTooltip("ModalMenuUI.cs -> OnPointerExit");
                break;
            case ActionMenuType.Gear:
                GameManager.instance.tooltipGenericScript.CloseTooltip("ModalMenuUI.cs -> OnPointerExit");
                break;
        }
    }

    /// <summary>
    /// tooltip coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowTooltip()
    {
        float alphaCurrent;
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over node
        if (onMouseFlag == true)
        {
            switch (menuType)
            {
                case ActionMenuType.Node:
                case ActionMenuType.NodeGear:
                    //need to regularly update node details, rather than just at game start
                    nodeObject = GameManager.instance.dataScript.GetNodeObject(nodeID);
                    if (nodeObject != null)
                    { node = nodeObject.GetComponent<Node>(); }
                    //do once
                    while (GameManager.instance.tooltipNodeScript.CheckTooltipActive() == false)
                    {
                        List<string> activeList = new List<string>();
                        switch (GameManager.instance.turnScript.currentSide.level)
                        {
                            case 1:
                                //Authority 
                                activeList = GameManager.instance.dataScript.GetActiveContactsAtNodeAuthority(nodeID);
                                break;
                            case 2:
                                activeList = GameManager.instance.dataScript.GetActiveContactsAtNodeResistance(nodeID);
                                break;
                        }
                        /*List<string> activeList = node.GetNodeContacts();*/
                        List<EffectDataTooltip> effectsList = node.GetListOfOngoingEffectTooltips();
                        List<string> targetList = GameManager.instance.targetScript.GetTargetTooltip(node.targetID, node.isTargetKnown);
                        List<string> teamList = new List<string>();
                        List<Team> listOfTeams = node.GetListOfTeams();
                        if (listOfTeams.Count > 0)
                        {
                            foreach (Team team in listOfTeams)
                            { teamList.Add(string.Format("{0} team", team.arc.name)); }
                        }
                        //adjust position prior to sending (rectTransform is in World units)
                        Vector3 positionNode = rectTransform.position;
                        positionNode.x += 150;
                        positionNode.y -= 125;
                        positionNode = Camera.main.ScreenToWorldPoint(positionNode);

                        NodeTooltipData nodeTooltip = new NodeTooltipData()
                        {
                            nodeName = node.nodeName,
                            type = string.Format("{0} ID {1}", node.Arc.name, nodeID),
                            arrayOfStats = new int[] { node.Stability, node.Support, node.Security },
                            listOfContactsCurrent = activeList,
                            listOfEffects = effectsList,
                            listOfTeams = teamList,
                            listOfTargets = targetList,
                            tooltipPos = positionNode
                        };
                        GameManager.instance.tooltipNodeScript.SetTooltip(nodeTooltip);
                        yield return null;
                    }
                    //fade in
                    while (GameManager.instance.tooltipNodeScript.GetOpacity() < 1.0)
                    {
                        alphaCurrent = GameManager.instance.tooltipNodeScript.GetOpacity();
                        alphaCurrent += Time.deltaTime / mouseOverFade;
                        GameManager.instance.tooltipNodeScript.SetOpacity(alphaCurrent);
                        yield return null;
                    }
                    break;
                case ActionMenuType.Gear:
                    Gear gear = GameManager.instance.dataScript.GetGear(gearID);
                    if (gear != null)
                    {
                        GenericTooltipDetails details = GameManager.instance.gearScript.GetGearTooltip(gear);
                        if (details != null)
                        {
                            //do once
                            while (GameManager.instance.tooltipGenericScript.CheckTooltipActive() == false)
                            {
                                //adjust position prior to sending
                                Vector3 positionGear = rectTransform.position;
                                GenericTooltipData data = new GenericTooltipData()
                                { screenPos = positionGear, main = details.textMain, header = details.textHeader, details = details.textDetails};
                                GameManager.instance.tooltipGenericScript.SetTooltip(data);
                                yield return null;
                                //fade in
                                while (GameManager.instance.tooltipGenericScript.GetOpacity() < 1.0)
                                {
                                    alphaCurrent = GameManager.instance.tooltipGenericScript.GetOpacity();
                                    alphaCurrent += Time.deltaTime / mouseOverFade;
                                    GameManager.instance.tooltipGenericScript.SetOpacity(alphaCurrent);
                                    yield return null;
                                }
                            }
                        }
                    }
                    break;
                case ActionMenuType.Actor:
                    GlobalSide side = GameManager.instance.sideScript.PlayerSide;
                    Actor actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, side);
                    if (actor != null)
                    {
                        /*Gear gearActor = null;
                        int gearID = actor.GetGearID();
                        if (gearID > -1)
                        { gearActor = GameManager.instance.dataScript.GetGear(actor.GetGearID()); }*/

                        //do once
                        while (GameManager.instance.tooltipActorScript.CheckTooltipActive() == false)
                        {
                            //adjust position prior to sending
                            Vector3 positionActor = rectTransform.position;
                            positionActor.x += 70;
                            positionActor.y -= 100;

                            /*ActorTooltipData actorTooltip = new ActorTooltipData()
                            {
                                tooltipPos = positionActor,
                                actor = actor,
                                action = GameManager.instance.dataScript.GetActorAction(actorSlotID, side),
                                gear = gearActor,
                                listOfSecrets = actor.GetSecretsTooltipList(),
                                arrayOfQualities = GameManager.instance.dataScript.GetQualities(side),
                                arrayOfStats = GameManager.instance.dataScript.GetActorStats(actorSlotID, side)
                            };*/

                            ActorTooltipData actorTooltip = actor.GetTooltipData(positionActor);

                            GameManager.instance.tooltipActorScript.SetTooltip(actorTooltip);
                            yield return null;
                            //fade in
                            while (GameManager.instance.tooltipActorScript.GetOpacity() < 1.0)
                            {
                                alphaCurrent = GameManager.instance.tooltipActorScript.GetOpacity();
                                alphaCurrent += Time.deltaTime / mouseOverFade;
                                GameManager.instance.tooltipActorScript.SetOpacity(alphaCurrent);
                                yield return null;
                            }
                        }
                    }
                    break;
                case ActionMenuType.Player:
                        //do once
                        while (GameManager.instance.tooltipPlayerScript.CheckTooltipActive() == false)
                        {
                            //adjust position prior to sending
                            Vector3 positionPlayer = rectTransform.position;
                            positionPlayer.x += 70;
                            positionPlayer.y -= 100;
                            GameManager.instance.tooltipPlayerScript.SetTooltip(positionPlayer);
                            yield return null;
                            //fade in
                            while (GameManager.instance.tooltipPlayerScript.GetOpacity() < 1.0)
                            {
                                alphaCurrent = GameManager.instance.tooltipPlayerScript.GetOpacity();
                                alphaCurrent += Time.deltaTime / mouseOverFade;
                                GameManager.instance.tooltipPlayerScript.SetOpacity(alphaCurrent);
                                yield return null;
                            }
                        }
                    break;
            }
        }
    }

}
