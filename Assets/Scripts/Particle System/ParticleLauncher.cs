using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to a game object to control a particle system component
/// </summary>
public class ParticleLauncher : MonoBehaviour
{
    public ParticleSystem particleLauncher;

    private void Update()
    {
        //deactivate Launcher at game start
        if (particleLauncher != null)
        { particleLauncher.gameObject.SetActive(false); }
        else { Debug.LogWarning("Invalid particleLauncher (Null)"); }
    }

    /// <summary>
    /// start emitting at a set rate of particles per frame
    /// </summary>
    /// <param name="particlesPerFrame"></param>
    public void StartParticleSystem(int particlesPerFrame)
    {
        //keep input within reasonable parameters
        particlesPerFrame = Mathf.Clamp(particlesPerFrame, 10, 40);
        particleLauncher.gameObject.SetActive(true);
        particleLauncher.Emit(particlesPerFrame);
    }

    /// <summary>
    /// stop emitting
    /// </summary>
    public void StopParticleSystem()
    {
        particleLauncher.gameObject.SetActive(false);
    }

}
