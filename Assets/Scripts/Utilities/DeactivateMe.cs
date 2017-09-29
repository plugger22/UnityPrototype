using UnityEngine;

/// <summary>
/// Add this script to any Object to deactivate it automatically on initialisation
/// </summary>
public class DeactivateMe : MonoBehaviour
{
    //Deactivate on Initialisation
    private void Awake()
    {
        gameObject.SetActive(false);
    }
}
