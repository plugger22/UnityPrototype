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

    /*private Node nodeDestination;*/
    private Vector3 destinationPos;
    private Transform carTransform;
    private Transform lightTransform;                   //only used if lightObject present (carSurveil), ignored otherwise
    [HideInInspector] public int destinationID = -1;     //ID used to find item in listOfCars

    private float altitude;             //y coord
    private float flightAltitude;
    private float surveilAltitude;
    private float speedVertical;
    private float speedHorizontal;
    private float hoverDelay;
    private float sirenFlashInterval;
    private float searchlightLimit;
    private float searchlightFactor;
    private float searchlightRandom;
    private float searchlightSpeed;
    private float decelerationHorizontal;
    private float decelerationVertical;
    private float speedLimit;
    private float rotationSpeed;
    private float scaleTime;

    private bool isSiren;
    private bool isFlashOn;
    private bool isLampOn;

    private Coroutine myCoroutineCar;


    public void OnEnable()
    {
        //car
        if (carObject != null)
        {
            carTransform = carObject.GetComponent<Transform>();
            Debug.Assert(carTransform != null, "Invalid carTransform (Null)");
        }
        else { Debug.LogError("Invalid carObject (Null)"); }
        //siren
        Debug.Assert(sirenObject != null, "Invalid sirenObject (Null)");
        //light -> not all cars have light objects, no need for an error condition here
        if (lightObject != null)
        {
            lightTransform = lightObject.GetComponent<Transform>();
            Debug.Assert(lightTransform != null, "Invalid lightTransform (Null)");
        }
    }


    /// <summary>
    /// Initialise Destination
    /// </summary>
    /// <param name="node"></param>
    public void InitialiseCar(Node node, CarData data)
    {
        if (node != null)
        {
            /*nodeDestination = node;*/
            destinationPos = node.nodePosition;
            destinationID = node.nodeID;
            flightAltitude = data.cruiseAltitude;
            speedHorizontal = data.horizontalSpeed;
            speedVertical = data.verticalSpeed;
            hoverDelay = data.hoverDelay;
            isSiren = data.isSiren;
            sirenFlashInterval = data.sirenFlashInterval;
            decelerationHorizontal = data.decelerationHorizontal;
            decelerationVertical = data.decelerationVertical;
            speedLimit = data.speedLimit;
            rotationSpeed = data.rotationSpeed;
            scaleTime = data.scaleDownTime;
        }
        else { Debug.LogError("Invalid node (Null)"); }
    }

    /// <summary>
    /// Used by carSurveil (additional data)
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="data"></param>
    public void InitialiseCar(Vector3 pos, CarData data)
    {
        destinationPos = pos;
        destinationID = 555;            //indicates surveil operation
        flightAltitude = data.cruiseAltitude;
        surveilAltitude = data.surveilAltitude;
        speedHorizontal = data.horizontalSpeed;
        speedVertical = data.verticalSpeed;
        hoverDelay = data.hoverDelay;
        isSiren = data.isSiren;
        sirenFlashInterval = data.sirenFlashInterval;
        searchlightLimit = data.searchlightLimit;
        searchlightFactor = data.searchlightFactor;
        searchlightRandom = data.searchlightRandom;
        searchlightSpeed = data.searchlightSpeed;
        decelerationHorizontal = data.decelerationHorizontal;
        decelerationVertical = data.decelerationVertical;
        speedLimit = data.speedLimit;
        rotationSpeed = data.rotationSpeed;
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
            float speedActual;
            //siren
            if (isSiren == true)
            { StartCoroutine("FlashSiren"); }
            //create at this altitude (clear of underlying district)
            float startAltitude = startPosition.y;
            Quaternion quaternionTarget = new Quaternion();
            Vector3 destination = new Vector3(destinationPos.x, flightAltitude, destinationPos.z);
            /*Debug.LogFormat("[Tst] Car.cs -> MoveCar: Destination x_cord {0}, y_cord {1}, z_cord {2}{3}", destination.x, destination.y, destination.z, "\n");*/
            float x_pos, z_pos, step;
            altitude = startPosition.y;
            x_pos = startPosition.x;
            z_pos = startPosition.z;
            //initial lift from airport to flight altitude
            speedActual = speedVertical;
            do
            {
                if (speedActual < speedLimit)
                {
                    //adjust speed -> decelerate
                    speedActual += Time.deltaTime * decelerationVertical;
                }
                //adjust altitude
                altitude += Time.deltaTime / speedActual;
                /*Debug.LogFormat("[Tst] Car.cs -> MoveCar: x_cord {0}, y_cord {1}, z_cord {2}{3}", x_pos, altitude, z_pos, "\n");*/
                carTransform.position = new Vector3(x_pos, altitude, z_pos);
                yield return null;
            }
            while (altitude < flightAltitude);
            //target rotation
            quaternionTarget = Quaternion.LookRotation(destination - carTransform.position, Vector3.up);
            //rotate car gradually
            yield return StartCoroutine("RotateTo", quaternionTarget);
            //hover for a bit
            yield return new WaitForSeconds(hoverDelay);
            //move towards destination
            speedActual = speedHorizontal;
            do
            {
                step = Time.deltaTime / speedActual;
                //adjust position
                carTransform.position = Vector3.MoveTowards(carTransform.position, destination, step);
                if (speedActual < speedLimit)
                {
                    //adjust speed -> decelerate
                    speedActual += Time.deltaTime * decelerationHorizontal;
                }
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
            while (carTransform.position != destination);
            //hover for a bit
            yield return new WaitForSeconds(hoverDelay);
            //drop down vertically to target destination
            altitude = carTransform.position.y;
            x_pos = destination.x;
            z_pos = destination.z;
            speedActual = speedVertical;
            do
            {
                if (speedActual < speedLimit)
                {
                    //adjust speed -> decelerate
                    speedActual += Time.deltaTime * decelerationVertical;
                }
                //adjust altitude
                altitude -= Time.deltaTime / speedActual;
                carTransform.position = new Vector3(x_pos, altitude, z_pos);
                yield return null;
            }
            while (altitude > startAltitude);
            //scale down car
            float currentTime = 0.0f;
            Vector3 originalScale = carTransform.localScale;
            Vector3 destinationScale = new Vector3(0.1f, 0.1f, 0.1f);
            do
            {
                carTransform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime / scaleTime);
                currentTime += Time.deltaTime;
                yield return null;
            }
            while (currentTime <= scaleTime);
            //remove from list
            GameManager.i.animateScript.DeleteCarTraffic(destinationID);
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
            float speedActual;
            //siren
            if (isSiren == true)
            { StartCoroutine("FlashSiren"); }
            Quaternion quaternionTarget = new Quaternion();
            Vector3 destination = new Vector3(destinationPos.x, flightAltitude, destinationPos.z);
            /*Debug.LogFormat("[Tst] Car.cs -> MoveCar: Destination x_cord {0}, y_cord {1}, z_cord {2}{3}", destination.x, destination.y, destination.z, "\n");*/
            float x_pos, z_pos, step;
            altitude = startPosition.y;
            x_pos = startPosition.x;
            z_pos = startPosition.z;
            //initial lift from airport to flight altitude
            speedActual = speedVertical;
            do
            {
                if (speedActual < speedLimit)
                {
                    //adjust speed -> decelerate
                    speedActual += Time.deltaTime * decelerationVertical;
                }
                //adjust altitude
                altitude += Time.deltaTime / speedActual;
                /*Debug.LogFormat("[Tst] Car.cs -> MoveCar: x_cord {0}, y_cord {1}, z_cord {2}{3}", x_pos, altitude, z_pos, "\n");*/
                carTransform.position = new Vector3(x_pos, altitude, z_pos);
                yield return null;
            }
            while (altitude < flightAltitude);

            //target rotation
            quaternionTarget = Quaternion.LookRotation(destination - carTransform.position, Vector3.up);
            //rotate car gradually
            yield return StartCoroutine("RotateTo", quaternionTarget);
            //hover for a bit
            yield return new WaitForSeconds(hoverDelay);
            //move towards destination
            speedActual = speedHorizontal;
            do
            {
                if (speedActual < speedLimit)
                {
                    //adjust speed -> decelerate
                    speedActual += Time.deltaTime * decelerationHorizontal;
                }
                //adjust position
                step = Time.deltaTime / speedActual;
                carTransform.position = Vector3.MoveTowards(carTransform.position, destination, step);
                /*Debug.LogFormat("[Tst] Car.cs -> MoveCar: x_cord {0}, y_cord {1}, z_cord {2}{3}", carTransform.position.x, carTransform.position.y,
                    carTransform.position.z, "\n");*/
                yield return null;
                //failsafe check (had a bug, since resolved, have left this in, not needed
                if (carObject == null)
                {
                    Debug.LogWarningFormat("Car.cs -> MoveCar: Invalid carObject (Null) for destinationID {0} (coroutine stopped){1}", destinationID, "\n");
                    //stop coroutine
                    yield break;
                }
            }
            while (carTransform.position != destination);
            //hover for a bit
            yield return new WaitForSeconds(hoverDelay);
            //drop down vertically to target destination
            altitude = carTransform.position.y;
            x_pos = destination.x;
            z_pos = destination.z;
            speedActual = speedVertical;
            do
            {
                if (speedActual < speedLimit)
                {
                    //adjust speed -> decelerate
                    speedActual += Time.deltaTime * decelerationVertical;
                }
                //adjust altitude
                altitude -= Time.deltaTime / speedActual;
                carTransform.position = new Vector3(x_pos, altitude, z_pos);
                yield return null;
            }
            while (altitude > surveilAltitude);
        }
        else { Debug.LogWarningFormat("Invalid carObject (Null) for destinationID {0}", destinationID); }
    }

    /// <summary>
    /// Rotates car towards a target direction
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    IEnumerator RotateTo(Quaternion target)
    {
        Quaternion from = carTransform.rotation;
        for (float t = 0; t < 1f; t+= rotationSpeed * Time.deltaTime)
        {
            carTransform.rotation = Quaternion.Lerp(from, target, t);
            yield return null;
        }
        carTransform.rotation = target;
    }

    /// <summary>
    /// Animate searchlight for carSurveil once at destination
    /// </summary>
    /// <returns></returns>
    public IEnumerator ShowSearchlight()
    {
        float timeElapsed = 0.0f;
        float timeLimit = (1 + Random.Range(0, searchlightRandom)) * searchlightFactor;
        float adjust = 0f;
        bool isGrowing = true;
        //retain original rotation so it can be restored at end
        Quaternion originalRotation = lightTransform.rotation;
        lightObject.SetActive(true);
        float difference;
        float start = Mathf.Abs(lightTransform.rotation.x);
        do
        {
            timeElapsed += Time.deltaTime;
            adjust = Time.deltaTime * searchlightSpeed;

            if (isGrowing == true)
            {
                //rotate light to the rear
                lightTransform.Rotate(Vector3.right, adjust);
                difference = Mathf.Abs(lightTransform.rotation.x) - start;
                if (Mathf.Abs(difference) > searchlightLimit)
                {
                    isGrowing = false;
                    difference = 0.0f;
                    start = Mathf.Abs(lightTransform.rotation.x);
                }
            }
            else
            {
                //rotate light forward
                lightTransform.Rotate(Vector3.right * -1, adjust);
                difference = Mathf.Abs(lightTransform.rotation.x) - start;
                if (Mathf.Abs(difference) > searchlightLimit)
                {
                    isGrowing = true;
                    difference = 0.0f;
                    start = Mathf.Abs(lightTransform.rotation.x);
                }
            }
            /*Debug.LogFormat("[Tst] Car.cs -> ShowSearchlight: isGrowing {0}, rot.x {1}, diff {2}, start {3}{4}", isGrowing, lightTransform.rotation.x, difference, start, "\n");*/
            yield return null;
        }
        while (timeElapsed < timeLimit);
        //switch off light
        lightObject.SetActive(false);
        //restore original rotation
        lightTransform.rotation = originalRotation;
        /*Debug.LogFormat("[Tst] Car.cs -> ShowSearchlight: End coroutine - - - - - - {0}", "\n");*/
    }

    /// <summary>
    /// Coroutine to flash lights of car
    /// </summary>
    /// <returns></returns>
    private IEnumerator FlashSiren()
    {
        isFlashOn = false;
        while (true)
        {
            if (isFlashOn == true)
            {
                sirenObject.SetActive(false);
                isFlashOn = false;
                yield return new WaitForSeconds(sirenFlashInterval);
            }
            else
            {
                sirenObject.SetActive(true);
                isFlashOn = true;
                yield return new WaitForSeconds(sirenFlashInterval);
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
