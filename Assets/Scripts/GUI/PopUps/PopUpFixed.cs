using gameAPI;
using System.Collections;
using System.Collections.Generic;
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
    private Coroutine myCoroutine;
    private Vector3 localScaleDefault;
    private Color textColorDefault;


    private float timerMax = 1.5f;     //maximum time onscreen
    private float moveSpeed = 1.0f;      //y_axis move speed (upwards)
    private float increaseScale = 1.0f;   //factor to increase size of text
    private float decreaseScale = 1.0f;   //factor to decrease size of text
    private float fadeSpeed = 1.0f;         //factor to fade text
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


    public void Awake()
    {
        //threshold
        threshold = timerMax * 0.5f;
        //defaults (all the same, use first item in arrays as defaults)
        localScaleDefault = arrayOfTransforms[0].localScale;
        textColorDefault = arrayOfTexts[0].color;
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
    }

    /// <summary>
    /// Resets relevant arrays ready for next turn
    /// </summary>
    public void Reset()
    {
        for (int i = 0; i < sizeOfArray; i++)
        {
            arrayOfTexts[i].text = "";
            arrayOfActive[i] = false;
        }

    }

    /// <summary>
    /// Input text to display at a specific fixed popUp (won't display until ExecuteFixed is called)
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
            arrayOfTexts[index].text = textToDisplay;
            arrayOfActive[index] = true;
            
        }
        else { Debug.LogWarning("Invalid textToDisplay (Null or Empty)"); }
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
    /// display any active fixed texts
    /// </summary>
    public void ExecuteFixed()
    {
        
    }

    //new methods above here
}
