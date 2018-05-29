using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AIDisplayUI : MonoBehaviour
{

    public GameObject aiDisplayObject;
    public Image mainPanel;
    public Image backgroundImage;
    public Image innerPanel;
    public Image subTopPanel;
    public Image subMiddlePanel;
    public Image subBottomPanel;
    public TextMeshProUGUI tabTopText;
    public TextMeshProUGUI tabBottomText;
    public TextMeshProUGUI tabCloseText;
    public TextMeshProUGUI subTopUpper;
    public TextMeshProUGUI subTopLower;
    public TextMeshProUGUI subTopChance;
    public TextMeshProUGUI subMiddleUpper;
    public TextMeshProUGUI subMiddleLower;
    public TextMeshProUGUI subMiddleChance;
    public TextMeshProUGUI subBottomUpper;
    public TextMeshProUGUI subBottomLower;
    public TextMeshProUGUI subBottomChance;

    private static AIDisplayUI aiDisplayUI;


    /// <summary>
    /// provide a static reference to AIDisplayUI that can be accessed from any script
    /// </summary>
    /// <returns></returns>
    public static AIDisplayUI Instance()
    {
        if (!aiDisplayUI)
        {
            aiDisplayUI = FindObjectOfType(typeof(AIDisplayUI)) as AIDisplayUI;
            if (!aiDisplayUI)
            { Debug.LogError("There needs to be one active aiDisplayUI script on a GameObject in your scene"); }
        }
        return aiDisplayUI;
    }


    public void Initialise()
    {
        //set all sub compoponents to Active
        mainPanel.gameObject.SetActive(true);
        backgroundImage.gameObject.SetActive(true);
        innerPanel.gameObject.SetActive(true);
        subTopPanel.gameObject.SetActive(true);
        subMiddlePanel.gameObject.SetActive(true);
        subBottomPanel.gameObject.SetActive(true);
        tabCloseText.gameObject.SetActive(true);
    }
}
