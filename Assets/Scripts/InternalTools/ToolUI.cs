using UnityEngine;

#if (UNITY_EDITOR)

/// <summary>
/// handles main tool screen and menu UI
/// </summary>
public class ToolUI : MonoBehaviour
{

    #region Main
    public Canvas menuCanvas;
    public ToolButtonInteraction adventureInteraction;

    //static reference
    private static ToolUI toolUI;
    #endregion

    /// <summary>
    /// provide a static reference to ToolUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static ToolUI Instance()
    {
        if (!toolUI)
        {
            toolUI = FindObjectOfType(typeof(ToolUI)) as ToolUI;
            if (!toolUI)
            { Debug.LogError("There needs to be one active toolUI script on a GameObject in your scene"); }
        }
        return toolUI;
    }

    /// <summary>
    /// Initialisation
    /// </summary>
    public void Initialise()
    {
        //error checks
        Debug.Assert(menuCanvas != null, "Invalid menuCanvas (Null)");
        Debug.Assert(adventureInteraction != null, "Invalid adventureInteraction (Null)");
        //button assignments
        adventureInteraction.SetButton(ToolEventType.OpenAdventureUI);
        //turn on
        menuCanvas.gameObject.SetActive(true);
    }


    /// <summary>
    /// Open main menu
    /// </summary>
    public void OpenMainMenu()
    {
        menuCanvas.gameObject.SetActive(true);
    }

    /// <summary>
    /// close main menu
    /// </summary>
    public void CloseMainMenu()
    {
        menuCanvas.gameObject.SetActive(false);
    }

    //new methods above here
}

#endif
