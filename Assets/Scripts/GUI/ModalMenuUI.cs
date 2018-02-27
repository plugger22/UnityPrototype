﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using gameAPI;
using packageAPI;

/// <summary>
/// allows Node / Actor tooltip to show with mouseover of Modal Action Menu header
/// </summary>
public class ModalMenuUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //data needed for tooltips and passed by ModalActionMenu.cs -> SetActionMenu
    [HideInInspector] public int nodeID;
    [HideInInspector] public int actorSlotID;
    [HideInInspector] public ActionMenuType menuType;

    private int offset;
    private float mouseOverDelay;
    private float mouseOverFade;
    private bool onMouseFlag;
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
        //halve fade in time as a canvas tool tip appears to be a lot slower than a scene one
        //mouseOverFade /= 2;
        offset = GameManager.instance.tooltipScript.tooltipOffset;
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
                if (nodeObject != null)
                { StartCoroutine(ShowTooltip()); }
                break;
            case ActionMenuType.Actor:
                StartCoroutine(ShowTooltip());
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
        StopCoroutine(ShowTooltip());
        switch (menuType)
        {
            case ActionMenuType.Node:
                GameManager.instance.tooltipNodeScript.CloseTooltip();
                break;
            case ActionMenuType.Actor:
                GameManager.instance.tooltipActorScript.CloseTooltip();
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
                    //need to regularly update node details, rather than just at game start
                    nodeObject = GameManager.instance.dataScript.GetNodeObject(nodeID);
                    if (nodeObject != null)
                    { node = nodeObject.GetComponent<Node>(); }
                    //do once
                    while (GameManager.instance.tooltipNodeScript.CheckTooltipActive() == false)
                    {

                        List<string> activeList = node.GetNodeActors();
                        List<EffectDataTooltip> effectsList = node.GetOngoingEffects();
                        List<string> targetList = GameManager.instance.targetScript.GetTargetTooltip(node.targetID);
                        List<string> teamList = new List<string>();
                        List<Team> listOfTeams = node.GetTeams();
                        if (listOfTeams.Count > 0)
                        {
                            foreach (Team team in listOfTeams)
                            { teamList.Add(string.Format("{0} team", team.Arc.name)); }
                        }
                        //adjust position prior to sending
                        Vector3 positionNode = transform.position;
                        positionNode.x += 100;
                        positionNode.y -= 100;
                        positionNode = Camera.main.ScreenToWorldPoint(positionNode);

                        NodeTooltipData nodeTooltip = new NodeTooltipData()
                        {
                            nodeName = node.nodeName,
                            type = string.Format("{0} ID {1}", node.Arc.name, nodeID),
                            arrayOfStats = new int[] { node.Stability, node.Support, node.Security },
                            listOfActive = activeList,
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
                case ActionMenuType.Actor:
                    GlobalSide side = GameManager.instance.globalScript.sideResistance;
                    Actor actor = GameManager.instance.dataScript.GetCurrentActor(actorSlotID, side);
                    if (actor != null)
                    {
                        //do once
                        while (GameManager.instance.tooltipActorScript.CheckTooltipActive() == false)
                        {
                            //adjust position prior to sending
                            Vector3 positionActor = transform.position;
                            positionActor.x += 100;
                            positionActor.y += 500;
                            positionActor = Camera.main.ScreenToWorldPoint(positionActor);

                            ActorTooltipData actorTooltip = new ActorTooltipData()
                            {
                                tooltipPos = positionActor,
                                actor = actor,
                                action = GameManager.instance.dataScript.GetActorAction(actorSlotID, side),
                                arrayOfQualities = GameManager.instance.dataScript.GetQualities(side),
                                arrayOfStats = GameManager.instance.dataScript.GetActorStats(actorSlotID, side)
                            };
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
            }
        }
    }

}
