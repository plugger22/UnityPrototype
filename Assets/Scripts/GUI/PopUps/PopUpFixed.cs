using gameAPI;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// handles all Fixed UI popUps. NOTE: PopUpDynamic is a single dynamic UI popUp that is designed to be used anywhere (not fixed)
/// </summary>
public class PopUpFixed : MonoBehaviour
{
    [Header("Actor Slot0")]
    public GameObject popSlot0Object;
    public Transform popSlot0Transform;
    public TextMeshProUGUI popSlot0Text;
    [Header("Actor Slot1")]
    public GameObject popSlot1Object;
    public Transform popSlot1Transform;
    public TextMeshProUGUI popSlot1Text;
    [Header("Actor Slot2")]
    public GameObject popSlot2Object;
    public Transform popSlot2Transform;
    public TextMeshProUGUI popSlot2Text;
    [Header("Actor Slot3")]
    public GameObject popSlot3Object;
    public Transform popSlot3Transform;
    public TextMeshProUGUI popSlot3Text;
    [Header("Player")]
    public GameObject popPlayerObject;
    public Transform popPlayerTransform;
    public TextMeshProUGUI popPlayerText;
    [Header("Top Bar Left")]
    public GameObject popTopLeftObject;
    public Transform popTopLeftTransform;
    public TextMeshProUGUI popTopLeftText;
    [Header("Top Bar Right")]
    public GameObject popTopRightObject;
    public Transform popTopRightTransform;
    public TextMeshProUGUI popTopRightText;
    [Header("Top Widget Centre")]
    public GameObject popTopCentreObject;
    public Transform popTopCentreTransform;
    public TextMeshProUGUI popTopCentreText;

    private GameObject[] arrayOfObjects;
    private Transform[] arrayOfTransforms;
    private TextMeshProUGUI[] arrayOfTexts;
    private bool[] arrayOfActive;

    private int sizeOfArray;
    private bool isActive;
    private Coroutine myCoroutine;
    private Vector3 localScaleDefault;
    private Color textColorDefault;

    //fast access
    private float timerMax = -1;     //maximum time onscreen
    private float moveSpeed = -1;      //y_axis move speed (upwards)
    private float increaseScale = -1;   //factor to increase size of text
    private float decreaseScale = -1;   //factor to decrease size of text
    private float fadeSpeed = -1;         //factor to fade text
    private float threshold;           //halfway point of popUp life (timerMax * 0.5f)

    static PopUpFixed popUpFixed;

    /// <summary>
    /// provide a static reference to PopUpFixed that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static PopUpFixed Instance()
    {
        if (!popUpFixed)
        {
            popUpFixed = FindObjectOfType(typeof(PopUpFixed)) as PopUpFixed;
            if (!popUpFixed)
            { Debug.LogError("There needs to be one active PopUpFixed script on a GameObject in your scene"); }
        }
        return popUpFixed;
    }

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
        //initialise arrays
        sizeOfArray = (int)PopUpPosition.Count;
        arrayOfObjects = new GameObject[sizeOfArray];
        arrayOfTransforms = new Transform[sizeOfArray];
        arrayOfTexts = new TextMeshProUGUI[sizeOfArray];
        arrayOfActive = new bool[sizeOfArray];
        //popUps present
        Debug.Assert(popSlot0Object != null, "Invalid popSlot0Object (Null)");
        Debug.Assert(popSlot0Transform != null, "Invalid popSlot0Transform (Null)");
        Debug.Assert(popSlot0Text != null, "Invalid popSlot0Text (Null)");
        Debug.Assert(popSlot1Object != null, "Invalid popSlot1Object (Null)");
        Debug.Assert(popSlot1Transform != null, "Invalid popSlot1Transform (Null)");
        Debug.Assert(popSlot1Text != null, "Invalid popSlot1Text (Null)");
        Debug.Assert(popSlot2Object != null, "Invalid popSlot2Object (Null)");
        Debug.Assert(popSlot2Transform != null, "Invalid popSlot2Transform (Null)");
        Debug.Assert(popSlot2Text != null, "Invalid popSlot2Text (Null)");
        Debug.Assert(popSlot3Object != null, "Invalid popSlot3Object (Null)");
        Debug.Assert(popSlot3Transform != null, "Invalid popSlot3Transform (Null)");
        Debug.Assert(popSlot3Text != null, "Invalid popSlot3Text (Null)");
        Debug.Assert(popPlayerObject != null, "Invalid popPlayerObject (Null)");
        Debug.Assert(popPlayerTransform != null, "Invalid popPlayerTransform (Null)");
        Debug.Assert(popPlayerText != null, "Invalid popPlayerText (Null)");
        Debug.Assert(popTopLeftObject != null, "Invalid popTopLeftObject (Null)");
        Debug.Assert(popTopLeftTransform != null, "Invalid popTopLeftTransform (Null)");
        Debug.Assert(popTopLeftText != null, "Invalid popTopLeftText (Null)");
        Debug.Assert(popTopRightObject != null, "Invalid popTopRightObject (Null)");
        Debug.Assert(popTopRightTransform != null, "Invalid popTopRightTransform (Null)");
        Debug.Assert(popTopRightText != null, "Invalid popTopRightText (Null)");
        Debug.Assert(popTopCentreObject != null, "Invalid popTopCentreObject (Null)");
        Debug.Assert(popTopCentreTransform != null, "Invalid popTopCentreTransform (Null)");
        Debug.Assert(popTopCentreText != null, "Invalid popTopCentreText (Null)");
        //populate arrays -> Objects
        arrayOfObjects[0] = popSlot0Object;
        arrayOfObjects[1] = popSlot1Object;
        arrayOfObjects[2] = popSlot2Object;
        arrayOfObjects[3] = popSlot3Object;
        arrayOfObjects[4] = popPlayerObject;
        arrayOfObjects[5] = popTopLeftObject;
        arrayOfObjects[6] = popTopRightObject;
        arrayOfObjects[7] = popTopCentreObject;
        //populate arrays -> Transforms
        arrayOfTransforms[0] = popSlot0Transform;
        arrayOfTransforms[1] = popSlot1Transform;
        arrayOfTransforms[2] = popSlot2Transform;
        arrayOfTransforms[3] = popSlot3Transform;
        arrayOfTransforms[4] = popPlayerTransform;
        arrayOfTransforms[5] = popTopLeftTransform;
        arrayOfTransforms[6] = popTopRightTransform;
        arrayOfTransforms[7] = popTopCentreTransform;
        //populate arrays -> Texts
        arrayOfTexts[0] = popSlot0Text;
        arrayOfTexts[1] = popSlot1Text;
        arrayOfTexts[2] = popSlot2Text;
        arrayOfTexts[3] = popSlot3Text;
        arrayOfTexts[4] = popPlayerText;
        arrayOfTexts[5] = popTopLeftText;
        arrayOfTexts[6] = popTopRightText;
        arrayOfTexts[7] = popTopCentreText;
        //defaults (all the same, use first item in arrays as defaults)
        localScaleDefault = arrayOfTransforms[0].localScale;
        textColorDefault = arrayOfTexts[0].color;
        //reset all to default settings
        Reset();
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
    /// Resets relevant arrays ready for next lot of data
    /// </summary>
    public void Reset()
    {
        for (int i = 0; i < sizeOfArray; i++)
        {
            if (arrayOfActive[i] == true)
            { arrayOfObjects[i].SetActive(false); }
            arrayOfTexts[i].text = "";
            arrayOfActive[i] = false;
        }
        isActive = false;
        Debug.LogFormat("[Tst] PopUpFixed.cs -> Reset: RESET{0}", "\n");
    }

    /// <summary>
    /// Input text to display at a specific fixed popUp (won't display until ExecuteFixed is called)
    /// NOTE: text is appended to any existing text (on a new line)
    /// </summary>
    /// <param name="popPos"></param>
    /// <param name="textToDisplay"></param>
    public void SetData(PopUpPosition popPos, string textToDisplay)
    {
        if (string.IsNullOrEmpty(textToDisplay) == false)
        {
            int index = (int)popPos;
            //update text to display and set to active
            arrayOfObjects[index].SetActive(false);
            arrayOfTexts[index].text = string.Format("{0}{1}{2}", arrayOfTexts[index].text, "\n", textToDisplay);
            arrayOfActive[index] = true;
            Debug.LogFormat("[Tst] PopUpFixed.cs -> SetData: {0} -> \"{1}\"{2}", popPos, textToDisplay, "\n");
        }
        else { Debug.LogWarning("Invalid textToDisplay (Null or Empty)"); }
    }

    /// <summary>
    /// Overloaded method that takes an actors slotID for PopUptext (won't display until ExecuteFixed is called)
    /// NOTE: text is appended to any existing text (on a new line)
    /// </summary>
    /// <param name="actorSlotID"></param>
    /// <param name="textToDisplay"></param>
    public void SetData(int actorSlotID, string textToDisplay)
    {
        PopUpPosition pop = PopUpPosition.Count;
        switch (actorSlotID)
        {
            case 0: pop = PopUpPosition.ActorSlot0; break;
            case 1: pop = PopUpPosition.ActorSlot1; break;
            case 2: pop = PopUpPosition.ActorSlot2; break;
            case 3: pop = PopUpPosition.ActorSlot3; break;
        }
        if (pop != PopUpPosition.Count)
        { SetData(pop, textToDisplay); }
    }

    /// <summary>
    /// returns true if any fixed popUp has text waiting to display, false if none
    /// </summary>
    /// <returns></returns>
    public bool CheckIfActive()
    {
        for (int i = 0; i < sizeOfArray; i++)
        {
            if (arrayOfActive[i] == true)
            { return true; }
        }
        return false;
    }

    /// <summary>
    /// display any active fixed texts. Optional time delay in seconds prior to start of animation
    /// </summary>
    public void ExecuteFixed(float timeDelay = 0f)
    {
        if (isActive == false)
        {
            //run only during main game, eg. not during metaGame
            if (GameManager.i.inputScript.GameState == GameState.PlayGame)
            {
                //run only if data present to display
                if (CheckIfActive() == true)
                { myCoroutine = StartCoroutine("PopUp", timeDelay); }
                else { Debug.LogFormat("[Tst] PopUpFixed.cs -> ExecuteFixed: CheckIfActive FALSE{0}", "\n"); }
            }
            else { Debug.LogFormat("[Tst] PopUpFixed.cs -> ExecuteFixed: GameState \"{0}\" (NOT PlayGame){1}", GameManager.i.inputScript.GameState, "\n"); }
        }
        else
        {
            Debug.LogFormat("[Tst] PopUpFixed.cs -> ExecuteFixed: isActive FALSE{0}", "\n");
            StopCoroutine("PopUp");
        }
    }

    /// <summary>
    /// Coroutine -> batch processes all active popUps
    /// </summary>
    /// <returns></returns>
    IEnumerator PopUp(float timeDelay)
    {
        float elapsedTime = 0f;
        int counter = 0;
        isActive = true;
        Color color = textColorDefault;
        //optional time delay prior to commencing animation
        if (timeDelay > 0)
        { yield return new WaitForSeconds(timeDelay); }
        //set defaults prior to animation
        for (int i = 0; i < sizeOfArray; i++)
        {
            if (arrayOfActive[i] == true)
            {
                counter++;
                arrayOfTexts[i].color = textColorDefault;
                arrayOfTransforms[i].localScale = localScaleDefault;
                arrayOfObjects[i].SetActive(true);
            }
        }
        //should always at least one popUp to display
        if (counter > 0)
        {
            counter = 0;
            //animation loop -> text grows in size, in place, then at halfway time point, beings fading and shrinking
            do
            {
                for (int i = 0; i < sizeOfArray; i++)
                {
                    if (arrayOfActive[i] == true)
                    {
                        arrayOfTransforms[i].position += new Vector3(0, moveSpeed) * Time.deltaTime;
                        if (elapsedTime < threshold)
                        {
                            //first half of popUp lifetime -> grow in size
                            arrayOfTransforms[i].localScale += Vector3.one * increaseScale * Time.deltaTime;
                        }
                        else
                        {
                            //second half of popUp lifetime -> shrink
                            arrayOfTransforms[i].localScale -= Vector3.one * decreaseScale * Time.deltaTime;
                            //fade
                            color.a -= fadeSpeed * Time.deltaTime;
                            arrayOfTexts[i].color = color;
                        }
                    }
                }
                //increment time
                elapsedTime += Time.deltaTime;
                //fail safe
                counter++;
                if (counter > 500) { Debug.LogWarning("Counter reached 1000 -> FAILSAFE activated"); break; }
                yield return null;
            }
            while (elapsedTime < timerMax);
            //deactive objects once done
            Reset();
        }
        else { Debug.LogWarning("PopUpFixed coroutine running but there is NO data to display"); }
    }


    /// <summary>
    /// Stop coroutine
    /// </summary>
    public void StopCoroutine()
    {
        StopCoroutine("PopUp");
        Reset();
        isActive = false;
    }

    //new methods above here
}
