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
    [HideInInspector] public string gearName;
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
        mouseOverDelay = GameManager.i.guiScript.tooltipDelay;
        mouseOverFade = GameManager.i.guiScript.tooltipFade;
        //need to get nodeObject details at start to accomodate OnPointerEnter & node component details later in coroutine
        nodeObject = GameManager.i.dataScript.GetNodeObject(nodeID);

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
                if (string.IsNullOrEmpty(gearName) == false)
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
        {
            StopCoroutine(myCoroutine);
            myCoroutine = null;
        }
        switch (menuType)
        {
            case ActionMenuType.Node:
            case ActionMenuType.NodeGear:
                GameManager.i.tooltipNodeScript.CloseTooltip("ModalMenuUI.cs -> OnPointerExit");
                break;
            case ActionMenuType.Actor:
                GameManager.i.tooltipActorScript.CloseTooltip("ModalMenuUI.cs -> OnPointerExit");
                break;
            case ActionMenuType.Player:
                GameManager.i.tooltipPlayerScript.CloseTooltip("ModalMenuUI.cs -> OnPointerExit");
                break;
            case ActionMenuType.Gear:
                GameManager.i.tooltipGenericScript.CloseTooltip("ModalMenuUI.cs -> OnPointerExit");
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
                    nodeObject = GameManager.i.dataScript.GetNodeObject(nodeID);
                    if (nodeObject != null)
                    { node = nodeObject.GetComponent<Node>(); }
                    //do once
                    while (GameManager.i.tooltipNodeScript.CheckTooltipActive() == false)
                    {
                        List<string> activeList = new List<string>();
                        switch (GameManager.i.turnScript.currentSide.level)
                        {
                            case 1:
                                //Authority 
                                activeList = GameManager.i.dataScript.GetActiveContactsAtNodeAuthority(nodeID);
                                break;
                            case 2:
                                activeList = GameManager.i.dataScript.GetActiveContactsAtNodeResistance(nodeID);
                                break;
                        }
                        List<EffectDataTooltip> effectsList = node.GetListOfOngoingEffectTooltips();
                        List<string> targetList = GameManager.i.targetScript.GetTargetTooltip(node.targetName, node.isTargetKnown);
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
                        GameManager.i.tooltipNodeScript.SetTooltip(nodeTooltip);
                        yield return null;
                    }
                    //fade in
                    while (GameManager.i.tooltipNodeScript.GetOpacity() < 1.0)
                    {
                        alphaCurrent = GameManager.i.tooltipNodeScript.GetOpacity();
                        alphaCurrent += Time.deltaTime / mouseOverFade;
                        GameManager.i.tooltipNodeScript.SetOpacity(alphaCurrent);
                        yield return null;
                    }
                    break;
                case ActionMenuType.Gear:
                    Gear gear = GameManager.i.dataScript.GetGear(gearName);
                    if (gear != null)
                    {
                        GenericTooltipDetails details = GameManager.i.gearScript.GetGearTooltip(gear);
                        if (details != null)
                        {
                            //do once
                            while (GameManager.i.tooltipGenericScript.CheckTooltipActive() == false)
                            {
                                //adjust position prior to sending
                                Vector3 positionGear = rectTransform.position;
                                GenericTooltipData data = new GenericTooltipData()
                                { screenPos = positionGear, main = details.textMain, header = details.textHeader, details = details.textDetails};
                                GameManager.i.tooltipGenericScript.SetTooltip(data);
                                yield return null;
                                //fade in
                                while (GameManager.i.tooltipGenericScript.GetOpacity() < 1.0)
                                {
                                    alphaCurrent = GameManager.i.tooltipGenericScript.GetOpacity();
                                    alphaCurrent += Time.deltaTime / mouseOverFade;
                                    GameManager.i.tooltipGenericScript.SetOpacity(alphaCurrent);
                                    yield return null;
                                }
                            }
                        }
                    }
                    break;
                case ActionMenuType.Actor:
                    GlobalSide side = GameManager.i.sideScript.PlayerSide;
                    Actor actor = GameManager.i.dataScript.GetCurrentActor(actorSlotID, side);
                    if (actor != null)
                    {
                        //do once
                        while (GameManager.i.tooltipActorScript.CheckTooltipActive() == false)
                        {
                            //adjust position prior to sending
                            Vector3 positionActor = rectTransform.position;
                            positionActor.x += 70;
                            positionActor.y -= 100;
                            ActorTooltipData actorTooltip = actor.GetTooltipData(positionActor);
                            GameManager.i.tooltipActorScript.SetTooltip(actorTooltip, actor.slotID);
                            yield return null;
                            //fade in
                            while (GameManager.i.tooltipActorScript.GetOpacity() < 1.0)
                            {
                                alphaCurrent = GameManager.i.tooltipActorScript.GetOpacity();
                                alphaCurrent += Time.deltaTime / mouseOverFade;
                                GameManager.i.tooltipActorScript.SetOpacity(alphaCurrent);
                                yield return null;
                            }
                        }
                    }
                    break;
                case ActionMenuType.Player:
                        //do once
                        while (GameManager.i.tooltipPlayerScript.CheckTooltipActive() == false)
                        {
                            //adjust position prior to sending
                            Vector3 positionPlayer = rectTransform.position;
                            positionPlayer.x += 70;
                            positionPlayer.y -= 100;
                            GameManager.i.tooltipPlayerScript.SetTooltip(positionPlayer);
                            yield return null;
                            //fade in
                            while (GameManager.i.tooltipPlayerScript.GetOpacity() < 1.0)
                            {
                                alphaCurrent = GameManager.i.tooltipPlayerScript.GetOpacity();
                                alphaCurrent += Time.deltaTime / mouseOverFade;
                                GameManager.i.tooltipPlayerScript.SetOpacity(alphaCurrent);
                                yield return null;
                            }
                        }
                    break;
            }
        }
    }

}
