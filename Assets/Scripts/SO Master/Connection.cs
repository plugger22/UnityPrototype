using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

public class Connection : MonoBehaviour {

    private ConnectionType securityLevel;

    private int v1;                                 //vertice nodeID's for either end of the connection
    private int v2;

    public int connID;                              //unique connectionID 
    public int VerticeOne { get { return v1; } }
    public int VerticeTwo { get { return v2; } }

    public void InitialiseConnection(int v1, int v2)
    {
        this.v1 = v1;
        this.v2 = v2;
    }


    /// <summary>
    /// adjust security level and change connection to the appropriate color
    /// </summary>
    /// <param name="secLvl"></param>
    public void ChangeSecurityLevel(ConnectionType secLvl)
    {
        if (secLvl != securityLevel)
        {
            securityLevel = secLvl;
            Renderer renderer = GetComponent<Renderer>();
            //don't need a component reference for GameManager as it's a static Object available to all
            renderer.material = GameManager.instance.connScript.GetConnectionMaterial(secLvl);
        }
    }

    public int GetNode1()
    { return v1; }

    public int GetNode2()
    { return v2; }
}
