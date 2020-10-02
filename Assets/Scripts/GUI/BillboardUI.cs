using System.Collections;
using System.Collections.Generic;
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

    private RectTransform billTransformLeft;
    private RectTransform billTransformRight;

    private int halfScreenWidth;
    private int midPoint;

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
        midPoint = 500;
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
        billLeft.transform.localPosition = new Vector3(-midPoint - halfScreenWidth, 0, 0);
        billRight.transform.localPosition = new Vector3(midPoint + halfScreenWidth, 0, 0);
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
        int speed = 5;
        int distance = midPoint + halfScreenWidth;
        while (counter < halfScreenWidth)
        {
            counter += speed;
            billLeft.transform.localPosition = new Vector3(-distance + counter, 0, 0);
            billRight.transform.localPosition = new Vector3(distance - counter, 0, 0);
            yield return null;
        }
        GameManager.i.guiScript.waitUntilDone = true;
    }

    //events above here
}
