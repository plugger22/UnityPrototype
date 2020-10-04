using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// End of turn Billboard UI prior to infoPipeline
/// </summary>
public class BillboardUI : MonoBehaviour
{
    public Canvas billCanvas;
    public GameObject billObject;
    public Image billLeft;
    public Image billRight;
    public TextMeshProUGUI billText;

    private RectTransform billTransformLeft;
    private RectTransform billTransformRight;

    private float halfScreenWidth;
    /*private float width;*/
    private int speed;
    private float distance;

    private static BillboardUI billboardUI;

    /// <summary>
    /// Static instance so the billboardUI can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static BillboardUI Instance()
    {
        if (!billboardUI)
        {
            billboardUI = FindObjectOfType(typeof(BillboardUI)) as BillboardUI;
            if (!billboardUI)
            { Debug.LogError("There needs to be one active billboardUI script on a GameObject in your scene"); }
        }
        return billboardUI;
    }

    public void OnEnable()
    {
        //asserts
        Debug.Assert(billCanvas != null, "Invalid billCanvas (Null)");
        Debug.Assert(billObject != null, "Invalid billObject (Null)");
        Debug.Assert(billLeft != null, "Invalid billLeft (Null)");
        Debug.Assert(billRight != null, "Invalid billRight (Null)");
        Debug.Assert(billText != null, "Invalid billText (Null)");
        //initialise components
        billTransformLeft = billLeft.GetComponent<RectTransform>();
        billTransformRight = billRight.GetComponent<RectTransform>();
        Debug.Assert(billTransformLeft != null, "Invalid billTransformLeft (Null)");
        Debug.Assert(billTransformRight != null, "Invalid billTransformRight (Null)");
        InitialiseBillboard();
    }

    /// <summary>
    /// Set billboard parameters and start position
    /// </summary>
    private void InitialiseBillboard()
    {
        //measurements
        halfScreenWidth = Screen.width / 2;
        /*width = billTransformLeft.rect.width;*/
        distance = halfScreenWidth;
        speed = 15;
        /*Debug.LogFormat("[Tst] BillboardUI.cs -> halfScreenWidth {0}, panelWidth {1}, distance {2}{3}", halfScreenWidth, width, distance, "\n");*/
        //activate
        billCanvas.gameObject.SetActive(true);
        //Reset panels at start
        Reset();
    }

    /// <summary>
    /// reset panels just off screen
    /// </summary>
    public void Reset()
    {
        billText.gameObject.SetActive(false);
        billLeft.transform.localPosition = new Vector3(-distance, 0, 0);
        billRight.transform.localPosition = new Vector3(distance, 0, 0);
    }

    /// <summary>
    /// Main billboard controller
    /// </summary>
    public void RunBillboard()
    {
        StartCoroutine("Slide");
    }

    /// <summary>
    /// coroutine to slide panels together
    /// </summary>
    /// <returns></returns>
    private IEnumerator Slide()
    {
        Reset();
        int counter = 0;
        GameManager.i.inputScript.SetModalState(new ModalStateData() { mainState = gameAPI.ModalSubState.Billboard });
        while (counter < halfScreenWidth)
        {
            counter += speed;
            billLeft.transform.localPosition = new Vector3(-distance + counter, 0, 0);
            billRight.transform.localPosition = new Vector3(distance - counter, 0, 0);
            yield return null;
        }
        billText.gameObject.SetActive(true);
    }

    //events above here
}
