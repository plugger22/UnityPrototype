using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour {

    public GameObject gameManager;
    

	// Use this for initialization
	void Awake ()
    {
		//Check if a gameManager has already been assigned to static variable GameManager.instance or if it's still null
        if (GameManager.instance == null)
        {
            //create GameManager instance from prefab
            Instantiate(gameManager);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
