using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using gameAPI;

/// <summary>
/// flashes player's current node while ever mouse over
/// </summary>
public class PlayerHighlightUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool isFlashOn;
    private float flashNodeTime;
    private Coroutine myFlashCoroutine;

    //fast access
    private Material materialNormal;
    private Material materialPlayer;

    /// <summary>
    /// constructor -> needs to be Start as GameManager hasn't initialised prior to this (UI elements initialise before normal gameObjects?)
    /// </summary>
    private void Start()
    {
        //flash
        flashNodeTime = GameManager.instance.guiScript.flashNodeTime;
        Debug.Assert(flashNodeTime > 0, "Invalid flashNodeTime (zero)");
        //materials
        materialNormal = GameManager.instance.nodeScript.GetNodeMaterial(NodeType.Normal);
        materialPlayer = GameManager.instance.nodeScript.GetNodeMaterial(NodeType.Player);
        Debug.Assert(materialNormal != null, "Invalid nodeNormal (Null)");
        Debug.Assert(materialPlayer != null, "Invalid nodePlayer (Null)");
    }

    /// <summary>
    /// Mouse Over event -> show tooltip if Player tooltipStatus > ActorTooltip.None
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Flasher
        Node node = null;
        //get correct node (captured node if captured)
        if (GameManager.instance.sideScript.PlayerSide.level == GameManager.instance.globalScript.sideResistance.level)
        {
            //human resistance player
            node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.GetPlayerNodeID());

            /*switch (GameManager.instance.playerScript.status)
            {
                case ActorStatus.Captured:
                    node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodeCaptured);
                    break;
                default:
                    node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
                    break;
            }*/
        }
        else
        {
            //AI Resistance player
            switch (GameManager.instance.aiRebelScript.status)
            {
                case ActorStatus.Captured:
                    node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodeCaptured);
                    break;
                default:
                    node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
                    break;
            }
        }
        if (node != null)
        {
            isFlashOn = false;
            //need to switch this off as otherwise a NodeRedraw will auto display player node all the time and nothing will flash
            GameManager.instance.nodeScript.SetShowPlayerNode(false);
            myFlashCoroutine = StartCoroutine("FlashingPlayerNode", node);
        }
        else { Debug.LogWarningFormat("Invalid player node (Null) for node ID {0}", GameManager.instance.nodeScript.nodePlayer); }
    }

    /// <summary>
    /// Mouse exit, close tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        //Flasher
        if (myFlashCoroutine != null)
        {
            StopCoroutine(myFlashCoroutine);
            GameManager.instance.nodeScript.SetShowPlayerNode(true);
            //set player node back to correct material
            Node node = null;
            //get correct node (captured node if captured)
            switch (GameManager.instance.playerScript.status)
            {
                case ActorStatus.Active:
                    node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodePlayer);
                    if (node != null)
                    {
                        node.SetPlayerNormal();
                        /*node.SetMaterial(materialPlayer);*/
                        GameManager.instance.nodeScript.NodeRedraw = true;
                    }
                    break;
                case ActorStatus.Captured:
                    node = GameManager.instance.dataScript.GetNode(GameManager.instance.nodeScript.nodeCaptured);
                    if (node != null)
                    {
                        node.SetNormal();
                        /*node.SetMaterial(materialNormal);*/
                        GameManager.instance.nodeScript.NodeRedraw = true;
                    }
                    break;
            }

            /*else { Debug.LogWarningFormat("Invalid player node (Null) for node ID {0}", GameManager.instance.nodeScript.nodePlayer); }*/
            GameManager.instance.tooltipGenericScript.CloseTooltip("PlayerSpriteTooltipUI.cs -> OnPointerExit");
        }
    }


    /// <summary>
    /// flash Player's current node coroutine whilever mouseover player sprite
    /// NOTE: node checked for null by calling procedure
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    IEnumerator FlashingPlayerNode(Node node)
    {
        do
        {
            if (isFlashOn == false)
            {
                node.SetPlayerFlash();
                /*node.SetMaterial(materialPlayer);*/
                GameManager.instance.nodeScript.NodeRedraw = true;
                isFlashOn = true;
                yield return new WaitForSecondsRealtime(flashNodeTime);
            }
            else
            {
                node.SetNormal();
                /*node.SetMaterial(materialNormal);*/
                GameManager.instance.nodeScript.NodeRedraw = true;
                isFlashOn = false;
                yield return new WaitForSecondsRealtime(flashNodeTime);
            }
        }
        while (true);
    }
    //new methods above here
}
