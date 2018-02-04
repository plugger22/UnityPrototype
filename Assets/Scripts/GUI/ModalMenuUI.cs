using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using gameAPI;
using packageAPI;

/// <summary>
/// allows Node tooltip to show with mouseover of Modal Action Menu header (node details)
/// </summary>
public class ModalMenuUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public int NodeID { get; set; }

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
        nodeObject = GameManager.instance.dataScript.GetNodeObject(NodeID);

    }


    /// <summary>
    /// Mouse Over event
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        onMouseFlag = true;
        if (nodeObject != null)
        { StartCoroutine(ShowTooltip()); }
    }

    /// <summary>
    /// Mouse exit, close tooltip
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        onMouseFlag = false;
        StopCoroutine(ShowTooltip());
        GameManager.instance.tooltipNodeScript.CloseTooltip();
    }

    /// <summary>
    /// tooltip coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowTooltip()
    {
        //delay before tooltip kicks in
        yield return new WaitForSeconds(mouseOverDelay);
        //activate tool tip if mouse still over node
        if (onMouseFlag == true)
        {
            //need to regularly update node details, rather than just at game start
            nodeObject = GameManager.instance.dataScript.GetNodeObject(NodeID);
            if (nodeObject != null)
            { node = nodeObject.GetComponent<Node>(); }
            //do once
            while (GameManager.instance.tooltipNodeScript.CheckTooltipActive() == false)
            {

                List<string> activeList = node.GetNodeActors();
                List<string> effectsList = node.GetOngoingEffects();
                List<string> targetList = GameManager.instance.targetScript.GetTargetTooltip(node.targetID);
                List<string> teamList = new List<string>();
                List<Team> listOfTeams = node.GetTeams();
                if (listOfTeams.Count > 0)
                {
                    foreach(Team team in listOfTeams)
                    { teamList.Add(string.Format("{0} team", team.Arc.name.ToUpper())); }
                }
                //adjust position prior to sending
                Vector3 position = transform.position;
                position.x += 100;
                position.y -= 100;
                position = Camera.main.ScreenToWorldPoint(position);
                
                NodeTooltipData dataTooltip = new NodeTooltipData()
                {
                    nodeName = node.nodeName,
                    type = string.Format("{0} ID {1}", node.Arc.name, NodeID),
                    arrayOfStats = new int[] { node.Stability, node.Support, node.Security },
                    listOfActive = activeList,
                    listOfEffects = effectsList,
                    listOfTeams = teamList,
                    listOfTargets = targetList,
                    tooltipPos = position
                    };
                GameManager.instance.tooltipNodeScript.SetTooltip(dataTooltip);
                yield return null;
            }
            //fade in
            float alphaCurrent;
            while (GameManager.instance.tooltipNodeScript.GetOpacity() < 1.0)
            {
                alphaCurrent = GameManager.instance.tooltipNodeScript.GetOpacity();
                alphaCurrent += Time.deltaTime / mouseOverFade;
                GameManager.instance.tooltipNodeScript.SetOpacity(alphaCurrent);
                yield return null;
            }
        }
    }

}
