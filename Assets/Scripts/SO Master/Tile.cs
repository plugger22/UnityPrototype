using System.Collections;
using UnityEngine;

/// <summary>
/// Handles animation matters for background tiles
/// </summary>
public class Tile : MonoBehaviour
{
    [Tooltip("GameObject for animation. Should be called 'Neon0'")]
    public GameObject sphere0;  //animates in a coloured rapid blink sequence
    public GameObject sphere1;  //animates in a on/off toggle between sphere1 & 2
    public GameObject sphere2;  //animates in a on/off toggle between sphere1 & 2
    public GameObject sphere3;  //on all time
    public GameObject sphere4;  //on all time

    private Renderer renderer0;         //need renderer as you're changing materials
    /*private Renderer renderer1;
    private Renderer renderer2;*/
    private bool isOn0;
    private bool isOn1;
    private bool isOn2;

    //fast access 
    private float tileDuration = -1;                    //sphere0 flash durations
    private int tileRepeat = -1;                        //sphere0
    private int tileMinimum = -1;                       //sphere0
    private int tileRandom = -1;                        //sphere0

    public void Awake()
    {
        //disable sphere at start
        if (sphere0 != null)
        {
            isOn1 = false;
            sphere0.SetActive(false);
            renderer0 = sphere0.GetComponent<Renderer>();
            Debug.Assert(renderer0 != null, "Invalid renderer0 (Null)");
        }
        else { Debug.LogError("Invalid sphere0 (Null)"); }
        if (sphere1 != null)
        {
            isOn1 = false;
            sphere1.SetActive(false);
            /*renderer1 = sphere1.GetComponent<Renderer>();
            Debug.Assert(renderer1 != null, "Invalid renderer1 (Null)");*/
        }
        else { Debug.LogError("Invalid sphere1 (Null)"); }
        if (sphere2 != null)
        {
            isOn2 = true;
            sphere2.SetActive(true);
            /*renderer2 = sphere2.GetComponent<Renderer>();
            Debug.Assert(renderer2 != null, "Invalid renderer2 (Null)");*/
        }
        else { Debug.LogError("Invalid sphere2 (Null)"); }
        if (sphere3 != null)
        { sphere3.SetActive(true); }
        else { Debug.LogError("Invalid sphere3 (Null)"); }
        if (sphere4 != null)
        { sphere4.SetActive(true); }
        else { Debug.LogError("Invalid sphere4 (Null)"); }
    }


    public void Start()
    {
        //fast access
        tileRepeat = GameManager.i.guiScript.tileRepeat;
        tileMinimum = GameManager.i.guiScript.tileMinimum;
        tileRandom = GameManager.i.guiScript.tileRandom;
        tileDuration = GameManager.i.guiScript.tileDuration;
        Debug.Assert(tileRepeat > -1, "Invalid tileRepeat (-1)");
        Debug.Assert(tileMinimum > -1, "Invalid tileMinimum (-1)");
        Debug.Assert(tileRandom > -1, "Invalid tileRandom (-1)");
        Debug.Assert(tileDuration > -1, "Invalid tileDuration (-1)");

    }

    /// <summary>
    /// True if Tile animation currently running
    /// </summary>
    /// <returns></returns>
    public bool CheckIsAnimating()
    { return isOn0; }

    /// <summary>
    /// Coroutine to animate tile, sphere 0
    /// </summary>
    /// <returns></returns>
    IEnumerator AnimateTile0(Material material)
    {
        //minimum num of flashes
        int numOfFlashes = tileMinimum + Random.Range(0, tileRandom);
        int timesUsed = 0;
        bool isRepeat = false;
        //light on for 'x' times longer than off
        float onDuration = tileDuration * 3;
        bool isFlashOn = false;
        isOn0 = true;
        if (material != null)
        {
            //assign material
            renderer0.material = material;
            //flash on/off cycle
            do
            {
                do
                {
                    if (isFlashOn == false)
                    {
                        sphere0.gameObject.SetActive(true);
                        isFlashOn = true;
                        yield return new WaitForSeconds(onDuration);
                    }
                    else
                    {
                        sphere0.gameObject.SetActive(false);
                        isFlashOn = false;
                        yield return new WaitForSeconds(tileDuration);
                    }
                    timesUsed++;
                }
                while (timesUsed < numOfFlashes);
                //repeat
                if (Random.Range(0, 100) < tileRepeat)
                {
                    isRepeat = true;
                    numOfFlashes = tileMinimum + Random.Range(0, tileRandom);
                    timesUsed = 0;
                }
                else { isRepeat = false; }
            }
            while (isRepeat == false);
        }
        else { Debug.LogError("Invalid material (Null)"); }
        //switch off
        sphere0.gameObject.SetActive(false);
        isOn0 = false;
    }

    /// <summary>
    /// Toggles sphere's 1 and 2 (one goes off, one goes on)
    /// </summary>
    public void ToggleLights()
    {
        //sphere1
        switch (isOn1)
        {
            case true:
                sphere1.gameObject.SetActive(false);
                isOn1 = false;
                break;
            case false:
                sphere1.gameObject.SetActive(true);
                isOn1 = true;
                break;
        }
        //sphere2
        switch (isOn2)
        {
            case true:
                sphere2.gameObject.SetActive(false);
                isOn2 = false;
                break;
            case false:
                sphere2.gameObject.SetActive(true);
                isOn2 = true;
                break;
        }
    }


}
