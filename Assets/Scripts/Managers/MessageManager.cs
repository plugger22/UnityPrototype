using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// handles all message matters
/// </summary>
public class MessageManager : MonoBehaviour
{

    /// <summary>
    /// Message -> player movement from one node to another. Returns null if text invalid.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="nodeID"></param>
    /// <returns></returns>
    public Message PlayerMove(string text, int nodeID)
    {
        if (string.IsNullOrEmpty(text) == false)
        {
            Message message = new Message();
            message.text = text;
            message.type = MessageType.MOVEMENT;
            message.side = Side.Resistance;
            message.isPublic = false;
            message.data0 = nodeID;
            return message;
        }
        else { Debug.LogWarning("Invalid text (Null or Empty)"); }
        return null;
    }
}
