using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
/*
/// <summary>
/// Scrolls ticker text across the screen in a continuous loop
/// </summary>
public class TickerTextScroller : MonoBehaviour
{
    public GameObject tickerObject;
    public TextMeshProUGUI textMeshComponent;
    public float ScrollSpeed = 10;

    private TextMeshProUGUI cloneTextObject;
    private RectTransform textRectTransform;
    private bool hasTextChanged;
    private static TickerTextScroller ticker;
    private string sourceText;

    /// <summary>
    /// provide a static reference to ticker that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static TickerTextScroller Instance()
    {
        if (!ticker)
        {
            ticker = FindObjectOfType(typeof(TickerTextScroller)) as TickerTextScroller;
            if (!ticker)
            { Debug.LogError("There needs to be one active TickerTextScroller script on a GameObject in your scene"); }
        }
        return ticker;
    }

    private void Awake() 
    {
        textRectTransform = textMeshComponent.GetComponent<RectTransform>();
        cloneTextObject = Instantiate(textMeshComponent) as TextMeshProUGUI;
        RectTransform cloneRectTransform = cloneTextObject.GetComponent<RectTransform>();
        cloneRectTransform.SetParent(textRectTransform);
        cloneRectTransform.anchorMin = new Vector2(1, 0.5f);
        cloneRectTransform.anchorMax = new Vector2(1, 0.5f);
        cloneRectTransform.localScale = new Vector3(1, 1, 1);
    }


    // Use this for initialization
    private IEnumerator Start ()
    {
        //float width = textMeshComponent.preferredWidth * textRectTransform.lossyScale.x;
        float width = textMeshComponent.preferredWidth;
        Debug.LogFormat("[Tst] 0: width -> {0}{1}", width, "\n");
        Vector3 startPosition = textRectTransform.position;

        float scrollPosition = 0;

        while (true)
        {
            //Recompute the width of the RectTransform if the text object has changed
            if (hasTextChanged)
            {
                width = textMeshComponent.preferredWidth;
                cloneTextObject.text = textMeshComponent.text;
            }

            cloneTextObject.rectTransform.position = new Vector3(cloneTextObject.rectTransform.position.x, startPosition.y, cloneTextObject.rectTransform.position.z);
            Debug.LogFormat("[Tst] 1: Clone.position.x {0}{1}", cloneTextObject.rectTransform.position.x, "\n");
            if (cloneTextObject.rectTransform.position.x <= -15)
            { scrollPosition = -cloneTextObject.rectTransform.position.x; }

            //scroll the text across the screen by moving the RectTransform
            textRectTransform.position = new Vector3(-scrollPosition % width, startPosition.y, startPosition.z);

            Debug.LogFormat("[Tst] 2: Normal.position.x -> {0}{1}",textRectTransform.position.x, "\n");
            scrollPosition += ScrollSpeed * 20 * Time.deltaTime;
            Debug.LogFormat("[Tst] 3: ScrollPosition (changed) -> {0}{1}", scrollPosition, "\n");
            yield return null;
        }
	}




    /// <summary>
    /// Set text
    /// </summary>
    /// <param name="text"></param>
    public void SetTickerText(string text)
    {
        if (string.IsNullOrEmpty(text) == false)
        { sourceText = text.ToUpper(); }
        else { Debug.LogWarning("Invalid text (Null or Empty"); }
    }



    /// <summary>
    /// Toggles ticker on/off
    /// </summary>
    public void ToggleTicker()
    {
        if (tickerObject.activeSelf == true)
        { tickerObject.SetActive(false); }
        else
        { tickerObject.SetActive(true); }
    }


    private void OnEnable()
    {
        //subscribe to event fired when text object has been regenerated
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
    }

    private void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
    }
    
    /// <summary>
    /// checks to see if text has changed
    /// </summary>
    /// <param name="obj"></param>
    private void ON_TEXT_CHANGED(Object obj)
    {
        if (obj == textMeshComponent)
        { hasTextChanged = true; }
    }
}
*/