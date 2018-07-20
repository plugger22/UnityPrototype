using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to a game object to control a particle system component
/// </summary>
public class ParticleLauncher : MonoBehaviour
{
    public ParticleSystem launcher;

    private bool isPlaying = false;

    private void Awake()
    {
        //deactivate Launcher at game start
        if (launcher != null)
        {
            launcher.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
        else { Debug.LogWarning("Invalid particleLauncher (Null)"); }
    }

    /// <summary>
    /// start emitting at a set rate of particles per frame
    /// </summary>
    /// <param name="particlesPerFrame"></param>
    public void StartSmoke(int particlesPerFrame)
    {
        if (isPlaying == false)
        {
            //keep input within reasonable parameters
            particlesPerFrame = Mathf.Clamp(particlesPerFrame, 10, 40);
            var emission = launcher.emission;
            emission.enabled = true;
            emission.rateOverTime = particlesPerFrame;
            launcher.Play(false);
            isPlaying = true;
        }
    }

    /// <summary>
    /// stop emitting
    /// </summary>
    public void StopSmoke()
    {
        if (isPlaying == true)
        {
            launcher.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            var emission = launcher.emission;
            emission.enabled = false;
            isPlaying = false;
        }
    }

}
