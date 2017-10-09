using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace modalAPI
{
    /// <summary>
    /// Standard Event button details
    /// </summary>
    public class EventButtonDetails
    {
        public string buttonTitle;
        public string buttonTooltipHeader;
        public string buttonTooltipMain;
        public string buttonTooltipDetail;
        public UnityAction action;
    }


    /// <summary>
    /// Specific setup for Modal Action Menu
    /// </summary>
    public class ModalPanelDetails
    {
        public string nodeName;
        public string nodeDetails;
        public int nodeID;
        public List<EventButtonDetails> listOfButtonDetails;            //only the first five are used (Target + 1 action / actor)
        public Vector3 nodePos;                                         //position of node in world units (transform)
    }

    /// <summary>
    /// data fed to ActionManager.cs -> ProcessNodeAction to facilitate processing the node action
    /// </summary>
    public class ModalActionDetails
    {
        public int NodeID { get; set; }
        public int ActorSlotID { get; set; }
        public EventType EventType { get; set; }
    }


    /// <summary>
    /// data fed from ProcessNodeAction -> ModalOutcome.cs -> SetModalOutcome to populate the window with data
    /// </summary>
    public class ModalOutcomeDetails
    {
        public string textTop;
        public string textBottom;
        public Sprite sprite;
    }


}


    /*public class ModalUtility : MonoBehaviour
    {
    }*/


