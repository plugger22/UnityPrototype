using gameAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all in-turn background animation
/// </summary>
public class AnimationManager : MonoBehaviour
{
    [Header("Car Prefab")]
    [Tooltip("Standard car prefab")]
    public GameObject carPrefab;

    [Header("Materials (Animation")]
    [Tooltip("Place any material in here that would be suitable for background tile animations (tile0 flashing sequence). Materials are randomly chosen from list")]
    public List<Material> listOfMaterials;


    private float connectionSpeed = -1;
    private float connectionDelay = -1;
    private float tileDelay = -1;

    private Transform carHolder;
    private Vector3 posAirport;
    private Node nodeAirport;

    private Coroutine myCoroutineAnimation;
    private Coroutine myCoroutineConnection;
    private Coroutine myCoroutineTile0;
    private Coroutine myCoroutineTileOthers;
    private Coroutine myCoroutineSignage;
    private Coroutine myCoroutineTraffic;

    private List<Car> listOfCars = new List<Car>();        //holds all active instances of Cars

    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseLevelStart();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseLevelStart();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseLevelStart();
                break;
            case GameState.LoadGame:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    #region Initialise SubMethods

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
        Debug.Assert(carPrefab != null, "Invalid carPrefab (Null)");
        //Get airport position
        int nodeID = GameManager.i.cityScript.airportDistrictID;
        nodeAirport = GameManager.i.dataScript.GetNode(nodeID);
        if (nodeAirport != null)
        {
            posAirport = nodeAirport.nodePosition;
        }
        else { Debug.LogErrorFormat("Invalid nodeAirport (Null) for nodeID \"{0}\"", nodeID); }
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
        Debug.Assert(listOfMaterials != null && listOfMaterials.Count > 0, "Invalid listOfMaterials (Null or Empty)");
        //fast access
        connectionSpeed = GameManager.i.guiScript.connectionSpeed;
        connectionDelay = GameManager.i.guiScript.connectionDelay;
        tileDelay = GameManager.i.guiScript.tileDelay;
        Debug.Assert(connectionSpeed > -1, "Invalid connectionSpeed (-1)");
        Debug.Assert(connectionDelay > -1, "Invalid connectionDelay (-1)");
        Debug.Assert(tileDelay > -1, "Invalid tileDelay (-1)");
    }
    #endregion

    #endregion


    public void Reset()
    {
        //remove any prefab car clones from previous level
        if (carHolder != null)
        {
            if (carHolder.childCount > 0 && listOfCars.Count > 0)
            {
                for (int i = listOfCars.Count - 1; i >= 0; i--)
                { GameManager.i.SafeDestroy(listOfCars[i].gameObject); }
            }
        }
    }

    /// <summary>
    /// Starts all in-game background animations (connections, flashing tiles, flying vehicles) at beginning of the turn
    /// Activated from GUIManager.cs -> IEnumerator.MainInfoApp
    /// </summary>
    public void StartAnimations()
    {
        myCoroutineConnection = StartCoroutine("AnimateConnections");
        myCoroutineTile0 = StartCoroutine("AnimateTile0");
        myCoroutineTileOthers = StartCoroutine("AnimateTileOthers");
        myCoroutineSignage = StartCoroutine("AnimateSignage");
        myCoroutineTraffic = StartCoroutine("AnimateTraffic");
    }

    /// <summary>
    /// Ceases all on-map animations at the end of a turn
    /// </summary>
    public void StopAnimations()
    {
        if (myCoroutineConnection != null) { StopCoroutine(myCoroutineConnection); }
        if (myCoroutineTile0 != null) { StopCoroutine(myCoroutineTile0); }
        if (myCoroutineTileOthers != null) { StopCoroutine(myCoroutineTileOthers); }
        if (myCoroutineSignage != null) { StopCoroutine(myCoroutineSignage); }
        if (myCoroutineTraffic != null) { StopCoroutine(myCoroutineTraffic); }
        ResetTraffic();
    }

    /// <summary>
    /// run a continuous series of animated connections (one connection at a time)
    /// </summary>
    /// <returns></returns>
    IEnumerator AnimateConnections()
    {
        while (true)
        {
            yield return new WaitForSeconds(connectionDelay);
            Connection connection = GameManager.i.dataScript.GetRandomConnection();
            if (connection != null)
            {
                if (connection.CheckBallMoving() == false)
                { yield return connection.StartCoroutine("MoveBall", connectionSpeed); }
            }
            else { Debug.LogError("Invalid random Connection (Null)"); }
            yield return null;
        }
    }

    /// <summary>
    /// run a continuous series of animated tiles (one tile at a time) for sphere0 on all tiles (flash individually)s
    /// </summary>
    /// <returns></returns>
    IEnumerator AnimateTile0()
    {
        Material material;
        int numOfMaterials = listOfMaterials.Count;
        while (true)
        {
            yield return new WaitForSeconds(tileDelay);
            Tile tile = GameManager.i.dataScript.GetRandomTile();
            material = listOfMaterials[Random.Range(0, numOfMaterials)];
            if (tile != null)
            {
                if (tile.CheckIsAnimating() == false)
                { yield return tile.StartCoroutine("AnimateTile0", material); }
            }
            else { Debug.LogError("Invalid random Tile (Null)"); }
            yield return null;
        }
    }

    /// <summary>
    /// Animates sphere's 1 and 2 on all suitable tiles. Blinks them on/off
    /// </summary>
    /// <returns></returns>
    IEnumerator AnimateTileOthers()
    {
        Tile tile;
        int index;
        List<Tile> listOfTiles = GameManager.i.dataScript.GetListOfTiles();
        if (listOfTiles != null)
        {
            int numOfTiles = listOfTiles.Count;
            while (true)
            {
                index = Random.Range(0, numOfTiles);
                tile = listOfTiles[index];
                if (tile != null)
                {
                    tile.ToggleLights();
                    yield return new WaitForSeconds(0.1f);
                }
                else { Debug.LogErrorFormat("Invalid tile (Null) in listOfTiles[{0}]", index); }
            }
        }
        else { Debug.LogError("Invalid listOfTiles (Null)"); }
    }

    /// <summary>
    /// Animates signage on districts
    /// </summary>
    /// <returns></returns>
    IEnumerator AnimateSignage()
    {
        int index;
        Node node;
        List<Node> listOfNodes = GameManager.i.dataScript.GetListOfAllNodes();
        if (listOfNodes != null)
        {
            int numOfNodes = listOfNodes.Count;
            while (true)
            {
                index = Random.Range(0, numOfNodes);
                node = listOfNodes[index];
                if (node != null)
                { yield return node.FlashSignage(); }
                else { Debug.LogErrorFormat("Invalid node (Null) in listOfNodes[{0}]", index); }
            }
        }
        else { Debug.LogError("Invalid listOfNodes (Null)"); }
    }

    /// <summary>
    /// Animates aerial traffic
    /// </summary>
    /// <returns></returns>
    IEnumerator AnimateTraffic()
    {
        //set starting position
        float minTrafficHeight = 1.00f;
        Vector3 startPos = posAirport;
        startPos.y = minTrafficHeight;
        GameObject instanceCar;
        Car car;
        //parent -> you only want one instance
        if (carHolder == null)
        { carHolder = new GameObject("MasterCar").transform; }

        while (true)
        {
            if (listOfCars.Count == 0)
            {
                //generate a new car instance if none currently onMap
                instanceCar = Instantiate(carPrefab, startPos, Quaternion.identity) as GameObject;
                instanceCar.SetActive(false);
                if (instanceCar != null)
                {
                    //Select node for destination
                    Node node = GameManager.i.dijkstraScript.GetRandomNodeAtMaxDistance(nodeAirport, 5);
                    if (node != null)
                    {
                        //add to list
                        listOfCars.Add(instanceCar.GetComponent<Car>());
                        car = instanceCar.GetComponent<Car>();
                        if (car != null)
                        {
                            instanceCar.SetActive(true);
                            car.SetDestination(node);
                            //run car animation sequence (moves from airport to destination node with vertical lift/ease at start and finish)
                            yield return new WaitForSeconds(2.0f);
                            yield return car.MoveCar(startPos);
                            GameManager.i.SafeDestroy(instanceCar);
                            listOfCars.Clear();
                        }
                        else
                        {
                            Debug.LogError("Invalid car component (Null)");
                            GameManager.i.SafeDestroy(instanceCar);
                        }
                    }
                    else
                    {
                        Debug.LogError("Invalid random destination node (Null)");
                        GameManager.i.SafeDestroy(instanceCar);
                    }
                }
                else { Debug.LogWarning("Invalid car (Null)"); }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Reset traffic at end of turn
    /// </summary>
    private void ResetTraffic()
    {
        if (listOfCars.Count > 0)
        {
            //reverse loop, delete all cars
            for (int i = listOfCars.Count - 1; i >= 0; i--)
            {
                Car car = listOfCars[i];
                if (car != null)
                {
                    //destroy
                    GameManager.i.SafeDestroy(car.carObject);
                }
                else { Debug.LogErrorFormat("Invalid car (Null) for listOfCars[{0}]", i); }
            }
            //empty list
            listOfCars.Clear();
        }
    }

    //new methods above here
}
