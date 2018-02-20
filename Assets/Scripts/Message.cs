using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace gameAPI
{
    /// <summary>
    /// Internal Messaging system handles 'Messages'
    /// </summary>
    public class Message
    {
        public string text;

        public int msgID;
        public int turnCreated;
        public int displayDelay;            //number of turns delay before message is displayed (if isPublic == true)
        public int data0;                   //3 x general purpose data points whose meaning depends on MessageType
        public int data1;
        public int data2;

        public bool isPublic;               //display if true

        public GlobalSide side;
        public MessageType type;            //main category
        public MessageSubType subType;      //sub type of main category
        
        


        private static int messageCounter = 0;

        /// <summary>
        /// Constructor (not a Unity class)
        /// </summary>
        public Message()
        {
            msgID = messageCounter++;
            turnCreated = GameManager.instance.turnScript.Turn;
        }


    }
}
