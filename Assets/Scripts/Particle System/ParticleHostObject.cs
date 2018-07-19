using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// test script to attach to host game object
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
            StartCoroutine("TestParticleSystem");
        }
    }


    IEnumerator TestParticleSystem()
    {
        yield return new WaitForSecondsRealtime(5);
        launcher.StartParticleSystem(20);
        yield return new WaitForSecondsRealtime(5);
        launcher.StopParticleSystem();
        yield return null;
    }
}
