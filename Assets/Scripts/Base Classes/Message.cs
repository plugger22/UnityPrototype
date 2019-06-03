using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace gameAPI
{
    /// <summary>
    /// Internal Messaging system handles 'Messages'
    /// </summary>
    [System.Serializable]
    public class Message
    {
        public string text;

        public int msgID;
        public int turnCreated;
        public int displayDelay;            //number of turns delay before message is displayed (if isPublic == true)
        public int data0;                   //3 x general purpose data points whose meaning depends on MessageType
        public int data1;
        public int data2;
        public int data3;

        public bool isPublic;               //display if true
        

        /*public GlobalSide side;*/
        public int sideLevel;               //GlobalSide.level
        public MessageType type;            //main category
        public MessageSubType subType;      //sub type of main category


        /// <summary>
        /// Constructor (not a Unity class)
        /// </summary>
        public Message()
        {
            msgID = GameManager.instance.messageScript.messageIDCounter++;
            turnCreated = GameManager.instance.turnScript.Turn;
            //default values
            data0 = -1;
            data1 = -1;
            data2 = -1;
            data3 = -1;
            isPublic = false;
        }


    }
}
