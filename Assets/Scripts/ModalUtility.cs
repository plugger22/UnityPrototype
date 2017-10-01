﻿using System.Collections;
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
        public string buttonTooltip;
        //public Sprite buttonBackground;
        public UnityAction action;
    }


    /// <summary>
    /// Specific setup for Modal Action Menu
    /// </summary>
    public class ModalPanelDetails
    {
        public string nodeName;
        public string nodeDetails;
        //public Sprite menuBackground;
        //public Sprite menuDivider;
        public List<EventButtonDetails> listOfButtonDetails;          //only the first five are used
        /*public EventButtonDetails button1Details;
        public EventButtonDetails button2Details;
        public EventButtonDetails button3Details;
        public EventButtonDetails button4Details;
        public EventButtonDetails button5Details;*/
        //Button 6 is always 'Cancel'
        public Vector3 nodePos;                             //position of node in world units (transform)
    }


}


    public class ModalUtility : MonoBehaviour
    {
    }


