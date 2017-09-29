using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple FPS counter that requires a text component for display
/// </summary>
[RequireComponent(typeof(Text))]
public class FPSCounter : MonoBehaviour
{

    private Text textComponent;
    private int frameCount = 0;
    private float fps = 0;
    private float timeLeft = 0.5f;
    private float timePassed = 0f;
    private float updateInterval = 0.5f;

	void Awake ()
    {
        textComponent = GetComponent<Text>();
        //not necessary (text component auto added by attribute above) but good practice
        if (textComponent == null)
        {
            Debug.LogError("This script needs to be attached to a Text Component!");
            enabled = false;
            return;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        frameCount += 1;
        timeLeft -= Time.deltaTime;
        timePassed += Time.timeScale / Time.deltaTime;
        //FPS calculation each second
        if (timeLeft <= 0f)
        {
            fps = timePassed / frameCount;
            timeLeft = updateInterval;
            timePassed = 0f;
            frameCount = 0;
        }
        //Set the colour of the text
        if (fps < 30) { textComponent.color = Color.red; }
        else if (fps < 60) { textComponent.color = Color.yellow; }
        else if (fps < 120) { textComponent.color = Color.blue; }
        else if (fps < 200) { textComponent.color = Color.cyan; }
        else { textComponent.color = Color.green; }
        //Set text string
        textComponent.text = string.Format("<b>{0:F0} FPS</b>", fps);
	}
}
