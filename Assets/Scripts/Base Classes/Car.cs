using System.Collections;
using UnityEngine;

/// <summary>
/// Manages individual car (traffic) operations
/// </summary>
public class Car : MonoBehaviour
{
    public GameObject carObject;
    public GameObject lightObject;

    private Node nodeDestination;
    private int destinationID = -1;     //ID used to find item in listOfCars

    private bool isFlightAltitude;      //y coord at which initial lift from airport ceases and flight to destination commences
    private float altitude;             //y coord

    private bool isFlashOn;


    public void OnEnable()
    {
        Debug.Assert(carObject != null, "Invalid carObject (Null)");
        Debug.Assert(lightObject != null, "Invalid lightObject (Null)");
    }


    /// <summary>
    /// Initialise Destination
    /// </summary>
    /// <param name="node"></param>
    public void SetDestination(Node node)
    {
        if (node != null)
        {
            nodeDestination = node;
            destinationID = node.nodeID;
        }
        else { Debug.LogError("Invalid node (Null)"); }
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
        StartCoroutine("FlashSiren");
        isFlightAltitude = false;
        float startAltitude = startPosition.y;
        float flightAltitude = 3.0f;
        float speedHorizontal = 1.0f;
        float speedVertical = 1.0f;
        float hoverWaitTime = 1.5f;
        //Vector3 destination = new Vector3(nodeDestination.nodePosition.x, nodeDestination.nodePosition.y, nodeDestination.nodePosition.z);
        Vector3 destination = new Vector3(nodeDestination.nodePosition.x, flightAltitude, nodeDestination.nodePosition.z);
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
        isFlightAltitude = true;
        //hover for a bit
        yield return new WaitForSeconds(hoverWaitTime);
        //move towards destination
        do
        {
            step = Time.deltaTime / speedHorizontal;
            carObject.transform.position = Vector3.MoveTowards(carObject.transform.position, destination, step);

            /*Debug.LogFormat("[Tst] Car.cs -> MoveCar: x_cord {0}, y_cord {1}, z_cord {2}{3}", carObject.transform.position.x, carObject.transform.position.y,
                carObject.transform.position.z, "\n");*/
            yield return null;
        }
        while (carObject.transform.position != destination);
        //hover for a bit
        yield return new WaitForSeconds(hoverWaitTime);
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
                lightObject.SetActive(false);
                isFlashOn = false;
                yield return new WaitForSeconds(sirenTime);
            }
            else
            {
                lightObject.SetActive(true);
                isFlashOn = true;
                yield return new WaitForSeconds(sirenTime);
            }
        }
    }

}
