using modalAPI;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Generic class to provide floating text pop ups of status changes for UI elements (Not really modal, it just sits above everything else in the hierarchy)
/// </summary>
public class ModalPopUp : MonoBehaviour
{
    public GameObject popObject;
    public Transform popTransform;
    public TextMeshProUGUI popText;

    private bool isActive;              //true if displayed on screen
    private Coroutine myCoroutine;
    private Vector3 localScaleDefault;

    private float timerMax = 1.5f;     //maximum time onscreen
    private float moveSpeed = 1.0f;      //y_axis move speed (upwards)
    private float threshold;           //halfway point of popUp life (timerMax * 0.5f)
    private float increaseScale = 1.0f;   //factor to increase size of text
    private float decreaseScale = 1.0f;   //factor to decrease size of text

    static ModalPopUp modalPopUp;

    public void Awake()
    {
        Debug.Assert(popObject != null, "Invalid popObject (Null)");
        Debug.Assert(popTransform != null, "Invalid popTransform (Null)");
        Debug.Assert(popText != null, "Invalid popText (Null)");
        //disable object on startup
        popObject.SetActive(false);
        //threshold
        threshold = timerMax * 0.5f;
        //local scale default
        localScaleDefault = popTransform.localScale;
    }

    /// <summary>
    /// provide a static reference to TextPopUpUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ModalPopUp Instance()
    {
        if (!modalPopUp)
        {
            modalPopUp = FindObjectOfType(typeof(ModalPopUp)) as ModalPopUp;
            if (!modalPopUp)
            { Debug.LogError("There needs to be one active modalPopUp script on a GameObject in your scene"); }
        }
        return modalPopUp;
    }

    /// <summary>
    /// Initialise and activate pop-up text over a given UI position
    /// </summary>
    /// <param name="data"></param>
    public void SetPopUp(ModalPopUpData data)
    {
        if (data != null)
        {
            if (string.IsNullOrEmpty(data.text) == false)
            {
                //check already running
                if (isActive == false)
                {
                    popText.text = data.text;
                    data.position.x += data.x_offset;
                    data.position.y += data.y_offset;
                    popTransform.position = data.position;
                    popObject.SetActive(true);
                    //start coroutine
                    myCoroutine = StartCoroutine("PopUp");
                }
                else
                {
                    //stop coroutine and update with new data before restarting
                    StopCoroutine("PopUp");
                    popText.text = data.text;
                    data.position.x += data.x_offset;
                    data.position.y += data.y_offset;
                    popTransform.position = data.position;
                    popObject.SetActive(true);
                    //start coroutine
                    StartCoroutine("PopUp");
                }
            }
            else { Debug.LogWarning("Invalid ModalPopUpData.text (Null or Empty)"); }
        }
        else { Debug.LogWarning("Invalid ModalPopUpData.position (Null)"); }
    }


    /// <summary>
    /// Coroutine
    /// </summary>
    /// <returns></returns>
    IEnumerator PopUp()
    {
        float elapsedTime = 0f;
        int counter = 0;
        isActive = true;
        popTransform.localScale = localScaleDefault;
        do
        {
            popTransform.position += new Vector3(0, moveSpeed) * Time.deltaTime;
            if (elapsedTime < threshold)
            {
                //first half of popUp lifetime -> grow in size
                popTransform.localScale += Vector3.one * increaseScale * Time.deltaTime;
            }
            else
            {
                //second half of popUp lifetime -> shrink
                popTransform.localScale -= Vector3.one * decreaseScale * Time.deltaTime;
            }
            //increment time
            elapsedTime += Time.deltaTime;
            //fail safe
            counter++;
            if (counter > 1000) { Debug.LogWarning("Counter reached 1000 -> FAILSAFE activated"); break; }
            yield return null;
        }
        while (elapsedTime < timerMax);
        popObject.SetActive(false);
        isActive = false;
    }
}
