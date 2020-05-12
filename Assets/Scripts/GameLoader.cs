using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameLoader : MonoBehaviour
{

    public GameObject gameManager;
    public GameObject eventManager;
    

	// Use this for initialization
	void Awake ()
    {
		//Check if a gameManager has already been assigned to static variable GameManager.instance or if it's still null
        if (GameManager.i == null)
        {
            //create GameManager instance from prefab
            Instantiate(gameManager);
        }
        //Check if a eventManager has already been assigned to static variable eventManager.instance or if it's still null
        if (EventManager.instance == null)
        {
            //create EventManager instance from prefab
            Instantiate(eventManager);
        }
    }
	
}
