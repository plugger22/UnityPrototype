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

    [Header("Connections")]
    [Tooltip("Speed at which the 'ball' moves along a connection (higher the number, the slower)")]
    [Range(0f, 5f)] public float connectionSpeed = 5.0f;
    [Tooltip("The delay in seconds between successive connection movement sequences, eg. there is a pause after one connection coroutine finishes and the next starts")]
    [Range(0f, 2f)] public float connectionDelay = 0.25f;
    [Tooltip("The chance (%) of a connection repeating the same movement. If fail roll then end of that connections animation, otherwise will keep repeating while roll keeps succeeding")]
    [Range(0, 100)] public int connectionRepeat = 30;

    [Header("Tiles")]
    [Tooltip("The delay in seconds between successive sphere0 tile animation sequences")]
    [Range(0f, 2f)] public float tileDelay = 0.5f;
    [Tooltip("The chance (%) of a tile sequence repeating")]
    [Range(0, 100)] public int tileRepeat = 30;
    [Tooltip("The minimum number of flashes to occur in a given sphere0 animation sequence")]
    [Range(0, 10)] public int tileMinimum = 5;
    [Tooltip("The random additional number flashes for sphere0 to be added to the minimum. This number will be Random.Range(0, number) + tileMinimum flashes")]
    [Range(0, 20)] public int tileRandom = 20;
    [Tooltip("Flash duration for sphere0 animation sequences")]
    [Range(0, 1f)] public float tileDuration = 0.15f;

    [Header("Signage")]
    [Tooltip("Delay between signage animation sequence flashes (sign0 on/off or all sign on/off)")]
    [Range(0f, 2f)] public float signageDelay = 0.2f;
    [Tooltip("Base number of times that signage animation will run (both types) -> gives the minimum number of sequences")]
    [Range(0, 20)] public int signageMinimum = 10;
    [Tooltip("The random additional number of iterations for the signage animation sequences (both types) that is added to the base")]
    [Range(0, 20)] public int signageRandom = 10;
    [Tooltip("The chance (%) of an initial signage sequence (sign0 animation) morphing into follow on sequence (all of sign toggled on/off")]
    [Range(0, 100)] public int signageRepeat = 50;

    [Header("Traffic")]
    [Tooltip("Initial pause and same for random pauses in generating new cars due animation sequence (seconds)")]
    [Range(0f, 5f)] public float trafficWaitTime = 1.0f;
    [Tooltip("Altitude at which cars are instantiated above a node and destroyed")]
    [Range(0f, 2.0f)] public float trafficHeightMin = 1.0f;
    [Tooltip("Chance (1d1000) of a dead period happening during sequence and nothing happens for trafficWaitTime")]
    [Range(0, 20)] public int trafficChancePause = 3;
    [Tooltip("Basic chance (1d100) of a new car being generated every iteration")]
    [Range(1, 20)] public int trafficChanceCar = 1;
    [Tooltip("Minimum distance (in connection links) that a destination node must be (>=) from Airport before being placed in the selection pool")]
    [Range(1, 10)] public int trafficNodeDistanceMin = 4;
    [Tooltip("Factor that decelerates traffic over time (including CarSurveil). Higher the number, faster the deceleration")]
    [Range(0, 1.0f)] public float decelerationVertical = 0.8f;
    [Tooltip("Factor that decelerates traffic over time (including CarSurveil). Higher the number, faster the deceleration")]
    [Range(0, 1.0f)] public float decelerationHorizontal = 0.05f;
    [Tooltip("Maximum value that actual speed can reach due to deceleration (higher the value the slower the car)")]
    [Range(5f, 20f)] public float speedLimit = 10.0f;
    [Tooltip("Determines how fast a car (includes carSurveil) will rotate towards it's destination. Higher the number, faster the rotation")]
    [Range(0.1f, 1.0f)] public float rotationSpeed = 0.75f;
    [Tooltip("How long (seconds) it takes for a car to scale down to nothing at the completion of it's journey prior to being destroyed")]
    [Range(0.5f, 3.0f)] public float scaleDownTime = 1.5f;

    [Header("Car Siren")]
    [Tooltip("Time (seconds) for siren flash sequence intervals")]
    [Range(0.1f, 1.0f)] public float sirenFlashInterval = 0.15f;

    [Header("Surveillance")]
    [Tooltip("Altitude at which surveillance occus")]
    [Range(0.5f, 1.5f)] public float surveilAltitude = 1.0f;
    [Tooltip("Initial pause (multiplied by a random amount of surveilWaitFactor) prior to sequence commencing at start of turn")]
    [Range(0f, 3.0f)] public float surveilWaitInterval = 1.5f;
    [Tooltip("Factor by which surveilWaitInterval is multiplied by a random1dFactor to get initial wait time prior to starting sequence")]
    [Range(0, 10)] public int surveilWaitFactor = 5;
    [Tooltip("Altitude at which carSurveil is initiated at Airport")]
    [Range(0.5f, 1.5f)] public float surveilHeightStart = 1.0f;

    [Header("Car Searchlight")]
    [Tooltip("Searchlight limit of movement in one direction before reversing (this is a weird number due to Eular/Quaternions and is determined by observation")]
    [Range(0.01f, 0.1f)] public float searchlightLimit = 0.035f;
    [Tooltip("Searchlight sequence time factor (in seconds) (multiplied by searchlightRandom to give a time limit for sequence")]
    [Range(1.0f, 10.0f)] public float searchlightFactor = 6.0f;
    [Tooltip("Searchlight random factor (1dRandom) multiplied by factor above to give total time period for sequence")]
    [Range(1, 5)] public int searchlightRandom = 2;
    [Tooltip("Searchlight speed of movement across the ground (higher the faster)")]
    [Range(1f, 10f)] public float searchlightSpeed = 6.0f;


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
            case GameState.TutorialOptions:
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseLevelStart();
                SubInitialiseAll();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseLevelStart();
                SubInitialiseAll();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseLevelStart();
                SubInitialiseAll();
                break;
            case GameState.LoadGame:
                SubInitialiseAll();
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


    }
    #endregion

    #region SubInitialiseAll
    private void SubInitialiseAll()
    {
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
        //run only during normal play mode
        if (GameManager.i.inputScript.CheckNormalMode() == true)
        {
            myCoroutineConnection = StartCoroutine("AnimateConnections");
            myCoroutineTile0 = StartCoroutine("AnimateTile0");
            myCoroutineTileOthers = StartCoroutine("AnimateTileOthers");
            myCoroutineSignage = StartCoroutine("AnimateSignage");
            myCoroutineTraffic = StartCoroutine("AnimateTraffic");
            myCoroutineSurveil = StartCoroutine("AnimateSurveillance");
        }
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
                    if (Random.Range(0, 1000) < trafficChancePause)
                    { yield return new WaitForSeconds(trafficWaitTime * 2f); }
                }
                //generate new traffic at random intervals
                if (Random.Range(0, 100) < trafficChanceCar)
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
                data.decelerationHorizontal = decelerationHorizontal;
                data.decelerationVertical = decelerationVertical;
                break;
            case CarType.Police:
                data.cruiseAltitude = 2.0f;
                data.verticalSpeed = 0.5f;
                data.horizontalSpeed = 0.4f;
                data.hoverDelay = 0.75f;
                data.isSiren = true;
                data.decelerationHorizontal = decelerationHorizontal;
                data.decelerationVertical = decelerationVertical;
                break;
            case CarType.Bus:
                data.cruiseAltitude = 2.5f;
                data.verticalSpeed = 0.75f;
                data.horizontalSpeed = 0.75f;
                data.hoverDelay = 1.0f;
                data.isSiren = true;
                data.decelerationHorizontal = decelerationHorizontal;
                data.decelerationVertical = decelerationVertical;
                break;
            case CarType.Rogue:
                data.cruiseAltitude = 1.5f;
                data.verticalSpeed = 0.3f;
                data.horizontalSpeed = 0.3f;
                data.hoverDelay = 0.75f;
                data.isSiren = false;
                data.decelerationHorizontal = decelerationHorizontal * 1.5f;
                data.decelerationVertical = decelerationVertical * 3.0f;
                break;
            case CarType.Surveil:
                data.cruiseAltitude = 2.25f;
                data.verticalSpeed = 0.6f;
                data.horizontalSpeed = 0.5f;
                data.hoverDelay = 1.0f;
                data.isSiren = true;
                //specific to surveillance, ignore for the rest
                data.surveilAltitude = surveilAltitude;
                data.searchlightLimit = searchlightLimit;
                data.searchlightFactor = searchlightFactor;
                data.searchlightRandom = searchlightRandom;
                data.searchlightSpeed = searchlightSpeed;
                data.decelerationHorizontal = decelerationHorizontal;
                data.decelerationVertical = decelerationVertical;
                break;
            default:
                Debug.LogWarningFormat("Unrecognised carType \"{0}\"", carType);
                //provide default data
                data.cruiseAltitude = 3.0f;
                data.verticalSpeed = 0.5f;
                data.horizontalSpeed = 0.5f;
                data.hoverDelay = 1.0f;
                data.decelerationHorizontal = decelerationHorizontal;
                data.decelerationVertical = decelerationVertical;
                break;
        }
        //globals
        data.sirenFlashInterval = sirenFlashInterval;
        data.decelerationHorizontal = decelerationHorizontal;
        data.decelerationVertical = decelerationVertical;
        data.speedLimit = speedLimit;
        data.rotationSpeed = rotationSpeed;
        data.scaleDownTime = scaleDownTime;
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
