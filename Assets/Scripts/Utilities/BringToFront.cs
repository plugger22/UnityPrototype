using UnityEngine;

/// <summary>
/// attach to UI gameObject. Will pop to front on enable
/// </summary>
public class BringToFront : MonoBehaviour
{
    void OnEnable()
    {
        transform.SetAsLastSibling();
    }

}
