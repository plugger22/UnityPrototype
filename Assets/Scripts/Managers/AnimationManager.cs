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
    public GameObject carSurveil;

    [Header("Materials (Animation")]
    [Tooltip("Place any material in here that would be suitable for background tile animations (tile0 flashing sequence). Materials are randomly chosen from list")]
    public List<Material> listOfMaterials;


    private float connectionSpeed = -1;
    private float connectionDelay = -1;
    private float tileDelay = -1;
    private float trafficWaitTime = -1;
    private float trafficHeightMin = -1;
    private int trafficPauseChance = -1;
    private int trafficCarChance = -1;
    private int trafficNodeDistanceMin = -1;
    private float surveilAltitude = -1;
    private float surveilWaitInterval = -1;
    private int surveilWaitFactor = -1;
    private float surveilHeightStart = -1;

    private Transform carHolder;
    private Vector3 posAirport;
    private Node nodeAirport;

    private Coroutine myCoroutineAnimation;
    private Coroutine myCoroutineConnection;
    private Coroutine myCoroutineTile0;
    private Coroutine myCoroutineTileOthers;
    private Coroutine myCoroutineSignage;
    private Coroutine myCoroutineTraffic;
    private Coroutine myCoroutineSurveil;

    private List<Car> listOfCarsTraffic = new List<Car>();        //holds all active instances of Cars involved in traffic animation
    private List<Car> listOfCarsSurveillance = new List<Car>();   //holds active instance of (single) car involved in surveillance animation
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
        Debug.Assert(carSurveil != null, "Invalid carSurveil (Null)");
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
        trafficWaitTime = GameManager.i.guiScript.trafficWaitTime;
        trafficHeightMin = GameManager.i.guiScript.trafficHeightMin;
        trafficPauseChance = GameManager.i.guiScript.trafficChancePause;
        trafficCarChance = GameManager.i.guiScript.trafficChanceCar;
        trafficNodeDistanceMin = GameManager.i.guiScript.trafficNodeDistanceMin;
        surveilAltitude = GameManager.i.guiScript.surveilAltitude;
        surveilWaitInterval = GameManager.i.guiScript.surveilWaitInterval;
        surveilWaitFactor = GameManager.i.guiScript.surveilWaitFactor;
        surveilHeightStart = GameManager.i.guiScript.surveilHeightStart;
        Debug.Assert(connectionSpeed > -1, "Invalid connectionSpeed (-1)");
        Debug.Assert(connectionDelay > -1, "Invalid connectionDelay (-1)");
        Debug.Assert(tileDelay > -1, "Invalid tileDelay (-1)");
        Debug.Assert(trafficWaitTime > -1, "Invalid trafficWaitTime (-1)");
        Debug.Assert(trafficHeightMin > -1, "Invalid trafficHeightMin (-1)");
        Debug.Assert(trafficPauseChance > -1, "Invalid trafficPauseChance (-1)");
        Debug.Assert(trafficCarChance > -1, "Invalid trafficCarChance (-1)");
        Debug.Assert(trafficNodeDistanceMin > -1, "Invalid trafficNodeDistanceMin (-1)");
        Debug.Assert(surveilAltitude > -1, "Invalid surveilAltitude (-1)");
        Debug.Assert(surveilWaitInterval > -1, "Invalid surveilWaitInterval (-1)");
        Debug.Assert(surveilWaitFactor > -1, "Invalid surveilWaitFactor (-1)");
        Debug.Assert(surveilHeightStart > -1, "Invalid surveilHeightStart (-1)");
    }
    #endregion

    #endregion


    public void Reset()
    {
        //remove any prefab car clones from previous level
        if (carHolder != null)
        {
            if (carHolder.childCount > 0)
            {
                //traffic
                if (listOfCarsTraffic.Count > 0)
                {
                    for (int i = listOfCarsTraffic.Count - 1; i >= 0; i--)
                    { GameManager.i.SafeDestroy(listOfCarsTraffic[i].gameObject); }
                }
                //surveillance
                if (listOfCarsSurveillance.Count > 0)
                {
                    for (int i = listOfCarsSurveillance.Count - 1; i >= 0; i--)
                    { GameManager.i.SafeDestroy(listOfCarsSurveillance[i].gameObject); }
                }
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
        myCoroutineSurveil = StartCoroutine("AnimateSurveillance");
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
        if (myCoroutineSurveil != null) { StopCoroutine(myCoroutineSurveil); }
        ResetTraffic();
        ResetSurveillance();
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
        //each turn has a variable number of cars in flight at any one time to give variety
        int maxNumOfCars = GetRandomTrafficNumber();
        Vector3 startPos = posAirport;
        startPos.y = trafficHeightMin;
        GameObject instanceCar;
        Car car;
        CarData data;
        //parent -> you only want one instance
        if (carHolder == null)
        { carHolder = new GameObject("MasterCar").transform; }
        //initial pause
        yield return new WaitForSeconds(trafficWaitTime);
        while (true)
        {
            if (listOfCarsTraffic.Count < maxNumOfCars)
            {
                //introduce random dead periods -> ignore at start of turn
                if (isWait == true)
                {
                    if (Random.Range(0, 1000) < trafficPauseChance)
                    { yield return new WaitForSeconds(trafficWaitTime); }
                }
                //generate new traffic at random intervals
                if (Random.Range(0, 100) < trafficCarChance)
                {
                    isWait = true;
                    carType = GetCarTypeTraffic();
                    //generate a new car instance if none currently onMap
                    instanceCar = Instantiate(GetCarPrefabTraffic(carType), startPos, Quaternion.identity) as GameObject;
                    instanceCar.SetActive(false);
                    if (instanceCar != null)
                    {
                        //Select node for destination
                        Node node = GameManager.i.dijkstraScript.GetRandomNodeAtMaxDistance(nodeAirport, trafficNodeDistanceMin);
                        if (node != null)
                        {
                            car = instanceCar.GetComponent<Car>();
                            data = GetCarData(carType);
                            car.InitialiseCar(node, data);
                            //add to list
                            AddCarTraffic(car);
                            if (car != null && instanceCar != null)
                            {
                                instanceCar.SetActive(true);
                                //run car animation sequence (moves from airport to destination node with vertical lift/ease at start and finish)
                                car.StartCoroutineTraffic(startPos);
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
    /// Generates a single surveillance car which randomly moves around map (surveillance tiles -> no nodes/no connection overlays), inspecting tiles with a searchlight
    /// </summary>
    /// <returns></returns>
    IEnumerator AnimateSurveillance()
    {
        GameObject instanceCar;
        Car car;
        float waitInterval = surveilWaitInterval * (1 + Random.Range(0, surveilWaitFactor));
        Vector3 startPos = posAirport;
        startPos.y = surveilHeightStart;
        List<Vector3> listOfPositions = GameManager.i.levelScript.GetListOfSurveillanceTiles();
        Vector3 destination;
        if (listOfPositions != null)
        {
            yield return new WaitForSeconds(waitInterval);
            //generate a new car instance if none currently onMap
            instanceCar = Instantiate(carSurveil, startPos, Quaternion.identity) as GameObject;
            instanceCar.SetActive(false);
            if (instanceCar != null)
            {
                car = instanceCar.GetComponent<Car>();
                if (car != null)
                {
                    //add to list
                    listOfCarsSurveillance.Add(car);
                    //flight profile
                    CarData data = GetCarData(CarType.Surveil);
                    //animation sequence
                    instanceCar.SetActive(true);
                    //turn off light
                    car.lightObject.SetActive(false);
                    while (true)
                    {
                        //Select node for destination
                        destination = listOfPositions[Random.Range(0, listOfPositions.Count)];
                        //new destination?
                        if (car.CheckIfCurrentDestination(destination) == false)
                        {
                            //initialise flight plan
                            car.InitialiseCar(destination, data);
                            //move car
                            yield return car.StartCoroutine("MoveCarSurveil", startPos);
                            //activate searchlight
                            yield return car.StartCoroutine("ShowSearchlight");
                        }
                        else
                        {
                            //activate searchlight
                            yield return car.StartCoroutine("ShowSearchlight");

                        }
                        //update startPosition
                        startPos = car.GetCurrentPosition();
                    }
                }
                else
                {
                    Debug.LogError("CarSurveil has an invalid car component (Null)");
                    GameManager.i.SafeDestroy(instanceCar);
                }

            }
        }
        else { Debug.LogError("Invalid listOfSurveillanceTiles (Null)"); }
    }


    //
    // - - - Utilities
    //

    /// <summary>
    /// Reset traffic at end of turn
    /// </summary>
    private void ResetTraffic()
    {
        //Traffic
        if (listOfCarsTraffic.Count > 0)
        {
            //reverse loop, delete all cars
            for (int i = listOfCarsTraffic.Count - 1; i >= 0; i--)
            {
                Car car = listOfCarsTraffic[i];
                if (car != null)
                {
                    //destroy
                    car.StopCoroutineTraffic();
                    GameManager.i.SafeDestroy(car.carObject);
                    /*Debug.LogFormat("[Tst] AnimationManager.cs -> ResetTraffic: car destinationID {0} DESTROYED{1}", car.destinationID, "\n");*/
                }
                else { Debug.LogErrorFormat("Invalid car (Null) for listOfCarsTraffic[{0}]", i); }
            }
            //empty list
            listOfCarsTraffic.Clear();
        }
    }


    /// <summary>
    /// Reset surveillance at end of turn
    /// </summary>
    private void ResetSurveillance()
    {
        //Surveillance
        if (listOfCarsSurveillance.Count > 0)
        {
            //reverse loop, delete all cars
            for (int i = listOfCarsSurveillance.Count - 1; i >= 0; i--)
            {
                Car car = listOfCarsSurveillance[i];
                if (car != null)
                {
                    //destroy
                    car.StopCoroutine("MoveCarSurveil");
                    car.StopCoroutine("ShowSearchlight");
                    GameManager.i.SafeDestroy(car.carObject);
                    /*Debug.LogFormat("[Tst] AnimationManager.cs -> ResetTraffic: car destinationID {0} DESTROYED{1}", car.destinationID, "\n");*/
                }
                else { Debug.LogErrorFormat("Invalid car (Null) for listOfCarsSurveillance[{0}]", i); }
            }
            //empty list
            listOfCarsSurveillance.Clear();
        }
    }

    /// <summary>
    /// Add car to listOfCarsTraffic
    /// </summary>
    /// <param name="car"></param>
    public void AddCarTraffic(Car car)
    {
        if (car != null)
        {
            listOfCarsTraffic.Add(car);
            /*Debug.LogFormat("[Tst] AnimationManager.cs -> AddCar: Car ADDED to list, destinationID {0}{1}", car.destinationID, "\n");*/
        }
    }

    /// <summary>
    /// Remove car from listOfCarsTraffic
    /// </summary>
    /// <param name="nodeID"></param>
    public void DeleteCarTraffic(int nodeID)
    {
        int index = listOfCarsTraffic.FindIndex(x => x.destinationID == nodeID);
        if (index > -1)
        {
            listOfCarsTraffic.RemoveAt(index);
            /*Debug.LogFormat("[Tst] AnimationManager.cs -> RemoveCar: Car REMOVED from list, destinationID {0}{1}", nodeID, "\n");*/
        }
    }

    /// <summary>
    /// Randomly chooses a car type
    /// </summary>
    /// <returns></returns>
    private CarType GetCarTypeTraffic()
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
    private GameObject GetCarPrefabTraffic(CarType carType)
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
                data.verticalSpeed = 0.6f;
                data.horizontalSpeed = 0.5f;
                data.hoverDelay = 1.0f;
                data.isSiren = true;
                break;
            case CarType.Police:
                data.cruiseAltitude = 2.0f;
                data.verticalSpeed = 0.5f;
                data.horizontalSpeed = 0.4f;
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
                data.verticalSpeed = 0.3f;
                data.horizontalSpeed = 0.3f;
                data.hoverDelay = 0.5f;
                data.isSiren = false;
                break;
            case CarType.Surveil:
                data.cruiseAltitude = 2.25f;
                data.surveilAltitude = surveilAltitude;
                data.verticalSpeed = 0.6f;
                data.horizontalSpeed = 0.5f;
                data.hoverDelay = 0.75f;
                data.isSiren = true;
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
