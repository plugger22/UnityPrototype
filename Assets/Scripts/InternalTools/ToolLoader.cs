using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR)
/// <summary>
/// GameLoader equivalent for Internal tools
/// </summary>
public class ToolLoader : MonoBehaviour
{

    public GameObject toolEvents;
    public GameObject toolManager;


    // Use this for initialization
    void Awake()
    {
        Debug.Assert(toolEvents != null, "Invalid toolEvents (Null)");
        Debug.Assert(toolManager != null, "Invalid toolManager (Null)");
        //Check if a ToolEvents has already been assigned to static variable toolEvents.instance or if it's still null
        if (ToolEvents.i == null)
        {
            //create EventManager instance from prefab
            Instantiate(toolEvents);
        }
        //Check if a toolManager has already been assigned to static variable toolManager.instance or if it's still null
        if (ToolManager.i == null)
        {
            //create Managers instances from prefab
            Instantiate(toolManager);
        }        
    }

}

#endif
