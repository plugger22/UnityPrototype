using packageAPI;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages individual car (traffic) operations
/// </summary>
public class Car : MonoBehaviour
{
    public GameObject carObject;        //master car object
    public GameObject sirenObject;      //siren
    [Tooltip("Searchlight (carSurveil) ignore for all others")]
    public GameObject lightObject;      //searchLight -> not all cars have this, only carSurveil

    private Node nodeDestination;
    private Vector3 destinationPos;
    [HideInInspector] public int destinationID = -1;     //ID used to find item in listOfCars

    private float altitude;             //y coord
    private float flightAltitude;
    private float surveilAltitude = 1.2f;
    private float speedVertical;
    private float speedHorizontal;
    private float hoverDelay;

    private bool isSiren;
    private bool isFlashOn;
    private bool isLampOn;

    private Coroutine myCoroutineCar;


    public void OnEnable()
    {
        Debug.Assert(carObject != null, "Invalid carObject (Null)");
        Debug.Assert(sirenObject != null, "Invalid sirenObject (Null)");
    }


    /// <summary>
    /// Initialise Destination
    /// </summary>
    /// <param name="node"></param>
    public void InitialiseCar(Node node, CarData data)
    {
        if (node != null)
        {
            nodeDestination = node;
            destinationPos = node.nodePosition;
            destinationID = node.nodeID;
            flightAltitude = data.cruiseAltitude;
            speedHorizontal = data.horizontalSpeed;
            speedVertical = data.verticalSpeed;
            hoverDelay = data.hoverDelay;
            isSiren = data.isSiren;
        }
        else { Debug.LogError("Invalid node (Null)"); }
    }

    /// <summary>
    /// Used by carSurveil (no need for node)
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="data"></param>
    public void InitialiseCar(Vector3 pos, CarData data)
    {
        destinationPos = pos;
        destinationID = 555;            //indicates surveil operation
        flightAltitude = data.cruiseAltitude;
        speedHorizontal = data.horizontalSpeed;
        speedVertical = data.verticalSpeed;
        hoverDelay = data.hoverDelay;
        isSiren = data.isSiren;
    }

    /// <summary>
    /// returns true if car has a valid destination
    /// </summary>
    /// <returns></returns>
    public bool CheckDestinationValid()
    { return destinationID > -1 ? true : false; }


    /// <summary>
    /// coroutine to move car from airport to destination node
    /// </summary>
    /// <returns></returns>
    public IEnumerator MoveCar(Vector3 startPosition)
    {
        if (carObject != null)
        {
            //siren
            if (isSiren == true)
            { StartCoroutine("FlashSiren"); }
            float startAltitude = startPosition.y;
            Quaternion quaternionTarget = new Quaternion();
            Vector3 destination = new Vector3(destinationPos.x, flightAltitude, destinationPos.z);
            /*Debug.LogFormat("[Tst] Car.cs -> MoveCar: Destination x_cord {0}, y_cord {1}, z_cord {2}{3}", destination.x, destination.y, destination.z, "\n");*/
            float x_pos, z_pos, step;
            altitude = startPosition.y;
            x_pos = startPosition.x;
            z_pos = startPosition.z;
            //initial lift from airport to flight altitude
            do
            {
                //carPosition.y += liftAmount;
                altitude += Time.deltaTime / speedVertical;
                /*Debug.LogFormat("[Tst] Car.cs -> MoveCar: x_cord {0}, y_cord {1}, z_cord {2}{3}", x_pos, altitude, z_pos, "\n");*/
                carObject.transform.position = new Vector3(x_pos, altitude, z_pos);
                yield return null;
            }
            while (altitude < flightAltitude);

            #region archiveSmoothRotation
            /*//target rotation
            quaternionTarget = Quaternion.LookRotation(destination - carObject.transform.position, Vector3.up);
            //rotate
            quaternion.SetFromToRotation(carObject.transform.position, destination);
            do
            {
                carObject.transform.position = Vector3.Lerp(carObject.transform.position, destination, Time.deltaTime / speedHorizontal);
                carObject.transform.rotation = quaternion * carObject.transform.rotation;
                angle = Quaternion.Angle(carObject.transform.rotation, quaternionTarget);
                Debug.LogFormat("[Tst] Car.cs -> MoveCar: angle {0}{1}", angle, "\n");
                yield return null;
            }
            while (angle > 0);*/
            #endregion

            //target rotation
            quaternionTarget = Quaternion.LookRotation(destination - carObject.transform.position, Vector3.up);
            //rotate car
            carObject.transform.rotation = quaternionTarget;

            //hover for a bit
            yield return new WaitForSeconds(hoverDelay);
            //move towards destination
            do
            {
                step = Time.deltaTime / speedHorizontal;

                carObject.transform.position = Vector3.MoveTowards(carObject.transform.position, destination, step);
                /*Debug.LogFormat("[Tst] Car.cs -> MoveCar: x_cord {0}, y_cord {1}, z_cord {2}{3}", carObject.transform.position.x, carObject.transform.position.y,
                    carObject.transform.position.z, "\n");*/
                yield return null;
                //failsafe check (had a bug, since resolved, have left this in, not needed
                if (carObject == null)
                {
                    Debug.LogWarningFormat("Car.cs -> MoveCar: Invalid carObject (Null) for destinationID {0} (coroutine stopped){1}", destinationID, "\n");
                    //stop coroutine
                    yield break;
                }
            }
            while (carObject.transform.position != destination);
            //hover for a bit
            yield return new WaitForSeconds(hoverDelay);
            //drop down vertically to target destination
            altitude = carObject.transform.position.y;
            x_pos = destination.x;
            z_pos = destination.z;
            do
            {
                altitude -= Time.deltaTime / speedVertical;
                carObject.transform.position = new Vector3(x_pos, altitude, z_pos);
                yield return null;
            }
            while (altitude > startAltitude);
            //remove from list
            GameManager.i.animateScript.DeleteCar(destinationID);
            //destroy car
            GameManager.i.SafeDestroy(carObject);
        }
        else { Debug.LogWarningFormat("Invalid carObject (Null) for destinationID {0}", destinationID); }
    }

    /// <summary>
    /// coroutine to move Surveillance Car from current loc to destination node
    /// </summary>
    /// <returns></returns>
    public IEnumerator MoveCarSurveil(Vector3 startPosition)
    {
        if (carObject != null)
        {
            //siren
            if (isSiren == true)
            { StartCoroutine("FlashSiren"); }
            float startAltitude = startPosition.y;
            Quaternion quaternionTarget = new Quaternion();
            Vector3 destination = new Vector3(destinationPos.x, flightAltitude, destinationPos.z);
            /*Debug.LogFormat("[Tst] Car.cs -> MoveCar: Destination x_cord {0}, y_cord {1}, z_cord {2}{3}", destination.x, destination.y, destination.z, "\n");*/
            float x_pos, z_pos, step;
            altitude = startPosition.y;
            x_pos = startPosition.x;
            z_pos = startPosition.z;
            //initial lift from airport to flight altitude
            do
            {
                //carPosition.y += liftAmount;
                altitude += Time.deltaTime / speedVertical;
                /*Debug.LogFormat("[Tst] Car.cs -> MoveCar: x_cord {0}, y_cord {1}, z_cord {2}{3}", x_pos, altitude, z_pos, "\n");*/
                carObject.transform.position = new Vector3(x_pos, altitude, z_pos);
                yield return null;
            }
            while (altitude < flightAltitude);

            //target rotation
            quaternionTarget = Quaternion.LookRotation(destination - carObject.transform.position, Vector3.up);
            //rotate car
            carObject.transform.rotation = quaternionTarget;

            //hover for a bit
            yield return new WaitForSeconds(hoverDelay);
            //move towards destination
            do
            {
                step = Time.deltaTime / speedHorizontal;

                carObject.transform.position = Vector3.MoveTowards(carObject.transform.position, destination, step);
                /*Debug.LogFormat("[Tst] Car.cs -> MoveCar: x_cord {0}, y_cord {1}, z_cord {2}{3}", carObject.transform.position.x, carObject.transform.position.y,
                    carObject.transform.position.z, "\n");*/
                yield return null;
                //failsafe check (had a bug, since resolved, have left this in, not needed
                if (carObject == null)
                {
                    Debug.LogWarningFormat("Car.cs -> MoveCar: Invalid carObject (Null) for destinationID {0} (coroutine stopped){1}", destinationID, "\n");
                    //stop coroutine
                    yield break;
                }
            }
            while (carObject.transform.position != destination);
            //hover for a bit
            yield return new WaitForSeconds(hoverDelay);
            //drop down vertically to target destination
            altitude = carObject.transform.position.y;
            x_pos = destination.x;
            z_pos = destination.z;
            do
            {
                altitude -= Time.deltaTime / speedVertical;
                carObject.transform.position = new Vector3(x_pos, altitude, z_pos);
                yield return null;
            }
            while (altitude > surveilAltitude);
        }
        else { Debug.LogWarningFormat("Invalid carObject (Null) for destinationID {0}", destinationID); }
    }

    /// <summary>
    /// Animate searchlight for carSurveil once at destination
    /// </summary>
    /// <returns></returns>
    public IEnumerator ShowSearchlight()
    {
        float timeElapsed = 0.0f;
        float timeLimit = 6.0f;
        lightObject.SetActive(true);
        do
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        while (timeElapsed < timeLimit);
        lightObject.SetActive(false);
    }

    /// <summary>
    /// Coroutine to flash lights of car
    /// </summary>
    /// <returns></returns>
    private IEnumerator FlashSiren()
    {
        float sirenTime = 0.15f;
        isFlashOn = false;
        while (true)
        {
            if (isFlashOn == true)
            {
                sirenObject.SetActive(false);
                isFlashOn = false;
                yield return new WaitForSeconds(sirenTime);
            }
            else
            {
                sirenObject.SetActive(true);
                isFlashOn = true;
                yield return new WaitForSeconds(sirenTime);
            }
        }
    }


    /// <summary>
    /// Start Coroutines for Traffic animation
    /// </summary>
    /// <param name="startPosition"></param>
    public void StartCoroutineTraffic(Vector3 startPosition)
    {
        if (myCoroutineCar == null)
        { myCoroutineCar = StartCoroutine("MoveCar", startPosition); }
        else { Debug.LogWarning("Invalid myCoroutineCar (should be Null)"); }
    }


    /// <summary>
    /// Stop coroutines for Traffic animation
    /// </summary>
    public void StopCoroutineTraffic()
    {
        StopCoroutine(myCoroutineCar);
        if (isSiren == true)
        { StopCoroutine("FlashSiren"); }
    }

    /// <summary>
    /// Returns true if specified position is current position (used by carSurveil), false otherwise
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool CheckIfCurrentDestination(Vector3 position)
    {
        if (position.x == destinationPos.x && position.z == destinationPos.z)
        { return true; }
        return false;
    }

    /// <summary>
    /// used by carSurveil to get current location prior to moving on to next position
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCurrentPosition()
    {
        Vector3 position = destinationPos;
        position.y = surveilAltitude;
        return position;
    }

}
