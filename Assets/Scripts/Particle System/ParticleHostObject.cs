using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Debug script to attach to host game object to test particle system in Particle Scene
/// </summary>
public class ParticleHostObject : MonoBehaviour
{

    public GameObject cube;

    [HideInInspector] public ParticleLauncher launcher;


    private void Awake()
    {
        launcher = cube.GetComponent<ParticleLauncher>();
        if (launcher == null)
        { Debug.LogWarning("Invalid launcher component (Null)"); }
    }

    private void Update()
    {
        if (launcher != null)
        {
            //launcher.StartParticleSystem(20);
            //launcher.particleLauncher.Play(false);
            StartCoroutine("TestParticleSystem");
        }
    }


    IEnumerator TestParticleSystem()
    {
        yield return new WaitForSecondsRealtime(3);
        launcher.StartSmoke(25);
        yield return null;
    }
}
