using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to a game object to control a particle system component
/// </summary>
public class ParticleLauncher : MonoBehaviour
{
    public ParticleSystem particleLauncher;

    private bool isPlaying = false;

    private void Awake()
    {
        //deactivate Launcher at game start
        if (particleLauncher != null)
        { particleLauncher.Stop(false, ParticleSystemStopBehavior.StopEmitting); }
        else { Debug.LogWarning("Invalid particleLauncher (Null)"); }
    }

    /// <summary>
    /// start emitting at a set rate of particles per frame
    /// </summary>
    /// <param name="particlesPerFrame"></param>
    public void StartParticleSystem(int particlesPerFrame)
    {
        if (isPlaying == false)
        {
            //keep input within reasonable parameters
            particlesPerFrame = Mathf.Clamp(particlesPerFrame, 10, 40);
            var emission = particleLauncher.emission;
            emission.rateOverTime = particlesPerFrame;
            particleLauncher.Play(false);
            isPlaying = true;
        }
    }

    /// <summary>
    /// stop emitting
    /// </summary>
    public void StopParticleSystem()
    {
        particleLauncher.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        isPlaying = false;
    }

}
