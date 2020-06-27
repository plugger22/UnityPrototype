using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR)
/// <summary>
/// GameLoader equivalent for Internal tools
/// </summary>
public class ToolLoader : MonoBehaviour
{

    public GameObject toolManager;
    public GameObject toolEvents;


    // Use this for initialization
    void Awake()
    {
        //Check if a ToolEvents has already been assigned to static variable toolEvents.instance or if it's still null
        if (ToolEvents.i == null)
        {
            //create EventManager instance from prefab
            Instantiate(toolEvents);
        }
        //Check if a toolManager has already been assigned to static variable toolManager.instance or if it's still null
        if (ToolManager.i == null)
        {
            //create GameManager instance from prefab
            Instantiate(toolManager);
        }
    }

}

#endif
