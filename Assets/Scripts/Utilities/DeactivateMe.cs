using UnityEngine;

/// <summary>
/// Add this script to any Object to deactivate it automatically on initialisation
/// NOTE: That the first time the object is activated it will then auto deactivate with this script attached so make sure you only attach the script to objects that are active in the hierarchy otherwise
/// when you first call the object via code to be active, it will run awakke and DeactiveMe and cancel out the activate call.
/// </summary>
public class DeactivateMe : MonoBehaviour
{
    //Deactivate on Initialisation
    private void Awake()
    {
        gameObject.SetActive(false);
    }
}
