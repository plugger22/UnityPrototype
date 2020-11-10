using gameAPI;
using packageAPI;
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
    public GameObject carNormal;
    public GameObject carPolice;
    public GameObject carBus;
    public GameObject carRogue;

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
    private List<int> listOfCarNumbers = new List<int> { 1, 1, 1, 1, 2, 2, 5, 5 };

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
        Debug.Assert(carNormal != null, "Invalid carNormal (Null)");
        Debug.Assert(carPolice != null, "Invalid carPolice (Null)");
        Debug.Assert(carBus != null, "Invalid carBus (Null)");
        Debug.Assert(carRogue != null, "Invalid carRogue (Null)");
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
        CarType carType;
        bool isWait = false;
        //set starting position
        float waitInterval = 2.0f;
        float minTrafficHeight = 1.00f;
        //each turn has a variable number of cars in flight at any one time to give variety
        int maxNumOfCars = GetRandomTrafficNumber();
        Vector3 startPos = posAirport;
        startPos.y = minTrafficHeight;
        GameObject instanceCar;
        Car car;
        CarData data;
        //parent -> you only want one instance
        if (carHolder == null)
        { carHolder = new GameObject("MasterCar").transform; }
        //initial pause
        yield return new WaitForSeconds(waitInterval);
        while (true)
        {
            if (listOfCars.Count < maxNumOfCars)
            {
                //introduce random dead periods -> ignore at start of turn
                if (isWait == true)
                {
                    if (Random.Range(0, 1000) < 3)
                    { yield return new WaitForSeconds(waitInterval); }
                }
                //generate new traffic at random intervals
                if (Random.Range(0, 100) < 1)
                {
                    isWait = true;
                    carType = GetCarType();
                    //generate a new car instance if none currently onMap
                    instanceCar = Instantiate(GetCarPrefab(carType), startPos, Quaternion.identity) as GameObject;
                    instanceCar.SetActive(false);
                    if (instanceCar != null)
                    {
                        //Select node for destination
                        Node node = GameManager.i.dijkstraScript.GetRandomNodeAtMaxDistance(nodeAirport, 4);
                        if (node != null)
                        {
                            car = instanceCar.GetComponent<Car>();
                            data = GetCarData(carType);
                            car.InitialiseCar(node, data);
                            //add to list
                            AddCar(car);
                            if (car != null && instanceCar != null)
                            {
                                instanceCar.SetActive(true);
                                //run car animation sequence (moves from airport to destination node with vertical lift/ease at start and finish)
                                car.StartCoroutines(startPos);
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
            }

            yield return null;
        }
    }

    /// <summary>
    /// Reset traffic at end of turn
    private void ResetTraffic()
    /// </summary>
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
                    car.StopCoroutines();
                    GameManager.i.SafeDestroy(car.carObject);
                    /*Debug.LogFormat("[Tst] AnimationManager.cs -> ResetTraffic: car destinationID {0} DESTROYED{1}", car.destinationID, "\n");*/
                }
                else { Debug.LogErrorFormat("Invalid car (Null) for listOfCars[{0}]", i); }
            }
            //empty list
            listOfCars.Clear();
        }
    }

    /// <summary>
    /// Add car to listOfCars
    /// </summary>
    /// <param name="car"></param>
    public void AddCar(Car car)
    {
        if (car != null)
        {
            listOfCars.Add(car);
            /*Debug.LogFormat("[Tst] AnimationManager.cs -> AddCar: Car ADDED to list, destinationID {0}{1}", car.destinationID, "\n");*/
        }
    }

    /// <summary>
    /// Remove car from listOfCars
    /// </summary>
    /// <param name="nodeID"></param>
    public void DeleteCar(int nodeID)
    {
        int index = listOfCars.FindIndex(x => x.destinationID == nodeID);
        if (index > -1)
        {
            listOfCars.RemoveAt(index);
            /*Debug.LogFormat("[Tst] AnimationManager.cs -> RemoveCar: Car REMOVED from list, destinationID {0}{1}", nodeID, "\n");*/
        }
    }

    /// <summary>
    /// Randomly chooses a car type
    /// </summary>
    /// <returns></returns>
    private CarType GetCarType()
    {
        CarType carType = CarType.Normal;
        int rnd = Random.Range(0, 100);
        if (rnd < 10) { carType = CarType.Police; }
        else if (rnd > 85)
        {
            if (rnd > 95)
            { carType = CarType.Rogue; }
            else { carType = CarType.Bus; }
        }
        return carType;
    }

    /// <summary>
    /// Returns an appropriate CarType prefab
    /// </summary>
    /// <param name="carType"></param>
    /// <returns></returns>
    private GameObject GetCarPrefab(CarType carType)
    {
        GameObject carObject = carNormal;
        switch (carType)
        {
            case CarType.Normal: carObject = carNormal; break;
            case CarType.Police: carObject = carPolice; break;
            case CarType.Bus: carObject = carBus; break;
            case CarType.Rogue: carObject = carRogue; break;
            default: Debug.LogWarningFormat("Unrecognised carType \"{0}\"", carType); break;
        }
        return carObject;
    }

    /// <summary>
    /// returns a data package appropriate for the carType
    /// </summary>
    /// <param name="carType"></param>
    /// <returns></returns>
    private CarData GetCarData(CarType carType)
    {
        CarData data = new CarData();
        switch (carType)
        {
            case CarType.Normal:
                data.cruiseAltitude = 3.0f;
                data.verticalSpeed = 0.5f;
                data.horizontalSpeed = 0.5f;
                data.hoverDelay = 1.0f;
                data.isSiren = true;
                break;
            case CarType.Police:
                data.cruiseAltitude = 2.0f;
                data.verticalSpeed = 0.25f;
                data.horizontalSpeed = 0.25f;
                data.hoverDelay = 0.5f;
                data.isSiren = true;
                break;
            case CarType.Bus:
                data.cruiseAltitude = 2.5f;
                data.verticalSpeed = 0.75f;
                data.horizontalSpeed = 0.75f;
                data.hoverDelay = 1.0f;
                data.isSiren = true;
                break;
            case CarType.Rogue:
                data.cruiseAltitude = 1.5f;
                data.verticalSpeed = 0.25f;
                data.horizontalSpeed = 0.25f;
                data.hoverDelay = 0.5f;
                data.isSiren = false;
                break;
            default:
                Debug.LogWarningFormat("Unrecognised carType \"{0}\"", carType);
                //provide default data
                data.cruiseAltitude = 3.0f;
                data.verticalSpeed = 0.5f;
                data.horizontalSpeed = 0.5f;
                data.hoverDelay = 1.0f;
                break;
        }
        return data;
    }

    /// <summary>
    /// Returns a random number of cars to have airborne at any one time. The list is designed to give extremes rather than a linear sample 
    /// </summary>
    /// <returns></returns>
    private int GetRandomTrafficNumber()
    { return listOfCarNumbers[Random.Range(0, listOfCarNumbers.Count)]; }

    //new methods above here
}
