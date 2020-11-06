using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles animation matters for background tiles
/// </summary>
public class Tile : MonoBehaviour
{
    [Tooltip("GameObject for animation. Should be called 'Neon0'")]
    public GameObject animateSphere;

    private bool isAnimating;

    public void Awake()
    {
        //disable sphere at start
        if (animateSphere != null)
        { animateSphere.SetActive(false); }
    }

    /// <summary>
    /// True if Tile animation currently running
    /// </summary>
    /// <returns></returns>
    public bool CheckIsAnimating()
    { return isAnimating; }

    /// <summary>
    /// Coroutine to animate tile
    /// </summary>
    /// <returns></returns>
    IEnumerator AnimateTile()
    {
        
        float flashDuration = 0.25f;
        int numOfFlashes = Random.Range(1, 10);
        int timesUsed = 0;
        bool isFlashOn = false;
        isAnimating = true;
        do
        {
            if (isFlashOn == false)
            {
                animateSphere.gameObject.SetActive(true);
                isFlashOn = true;
                yield return new WaitForSeconds(flashDuration);
            }
            else
            {
                animateSphere.gameObject.SetActive(false);
                isFlashOn = false;
                yield return new WaitForSeconds(flashDuration);
            }
            timesUsed++;
        }
        while (timesUsed < numOfFlashes);
        //switch off
        animateSphere.gameObject.SetActive(false);
        isAnimating = false;
    }
}
