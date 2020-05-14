using gameAPI;
using packageAPI;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Generic class to provide dynamic floating text pop ups of status changes for UI elements (Not really modal, it just sits above everything else in the hierarchy)
/// Has a single instance but can be positioned anywhere on any UI element
/// </summary>
public class PopUpDynamic : MonoBehaviour
{
    public GameObject popObject;
    public Transform popTransform;
    public TextMeshProUGUI popText;

    private bool isActive;              //true if displayed on screen
    private Coroutine myCoroutine;
    private Vector3 localScaleDefault;
    private Color textColorDefault;


    private float timerMax = 1.5f;     //maximum time onscreen
    private float moveSpeed = 1.0f;      //y_axis move speed (upwards)
    private float increaseScale = 1.0f;   //factor to increase size of text
    private float decreaseScale = 1.0f;   //factor to decrease size of text
    private float fadeSpeed = 1.0f;         //factor to fade text
    private float threshold;           //halfway point of popUp life (timerMax * 0.5f)

    static PopUpDynamic popUpDynamic;



    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.NewInitialisation:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseSessionStart();
                break;
            case GameState.LoadGame:
            case GameState.FollowOnInitialisation:
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\"", GameManager.i.inputScript.GameState);
                break;
        }
    }

    #region Initialise SubMethods

    #region SubInitialiseSessionStart
    /// <summary>
    /// Start Session
    /// </summary>
    public void SubInitialiseSessionStart()
    {

        Debug.Assert(popObject != null, "Invalid popObject (Null)");
        Debug.Assert(popTransform != null, "Invalid popTransform (Null)");
        Debug.Assert(popText != null, "Invalid popText (Null)");
        //disable object on startup
        popObject.SetActive(false);
        //defaults
        localScaleDefault = popTransform.localScale;
        textColorDefault = popText.color;
    }
    #endregion

    #region SubInitialiseFastAccess
    /// <summary>
    /// Fast access
    /// </summary>
    public void SubInitialiseFastAccess()
    {
        //fast access
        timerMax = GameManager.i.guiScript.timerMax;
        moveSpeed = GameManager.i.guiScript.moveSpeed;
        increaseScale = GameManager.i.guiScript.increaseScale;
        decreaseScale = GameManager.i.guiScript.decreaseScale;
        fadeSpeed = GameManager.i.guiScript.fadeSpeed;
        Debug.Assert(timerMax > -1, "Invalid timerMax (-1)");
        Debug.Assert(moveSpeed > -1, "Invalid moveSpeed (-1)");
        Debug.Assert(increaseScale > -1, "Invalid increaseScale (-1)");
        Debug.Assert(decreaseScale > -1, "Invalid decreaseScale (-1)");
        Debug.Assert(fadeSpeed > -1, "Invalid fadeSpeed (-1)");
        //threshold
        threshold = timerMax * 0.5f;
    }
    #endregion

    #endregion

    /// <summary>
    /// provide a static reference to PopUpDynamic that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static PopUpDynamic Instance()
    {
        if (!popUpDynamic)
        {
            popUpDynamic = FindObjectOfType(typeof(PopUpDynamic)) as PopUpDynamic;
            if (!popUpDynamic)
            { Debug.LogError("There needs to be one active PopUpDynamic script on a GameObject in your scene"); }
        }
        return popUpDynamic;
    }

    /// <summary>
    /// Initialise and activate dynamic pop-up text over a given UI position
    /// </summary>
    /// <param name="data"></param>
    public void ExecuteDynamic(PopUpDynamicData data)
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
        popText.color = textColorDefault;
        Color color = popText.color;
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
                //fade
                color.a -= fadeSpeed * Time.deltaTime;
                popText.color = color;
            }
            //increment time
            elapsedTime += Time.deltaTime;
            //fail safe
            counter++;
            if (counter > 500) { Debug.LogWarning("Counter reached 1000 -> FAILSAFE activated"); break; }
            yield return null;
        }
        while (elapsedTime < timerMax);
        popObject.SetActive(false);
        isActive = false;
    }

    /// <summary>
    /// Stop coroutine
    /// </summary>
    public void StopCoroutine()
    {
        StopCoroutine("PopUp");
        isActive = false;
    }
}
