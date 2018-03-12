using modalAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles Inventory related UI's, eg. Reserve Actor pool (both sides) and Player Gear (Resistance)
/// </summary>
public class ModalInventoryUI : MonoBehaviour
{

    public GameObject modalInventoryObject;
    public GameObject modalPanelObject;
    public GameObject modalHeaderObject;
    public Image modalPanel;

    public TextMeshProUGUI topText;
    public TextMeshProUGUI middleText;
    public TextMeshProUGUI bottomText;

    public Button buttonCancel;

    public Sprite errorSprite;                              //sprite to display in event of an error in the outcome dialogue

    public GameObject[] arrayOfInventoryOptions;                //place Inventory option UI elements here (up to 4 options)

    private static ModalGenericPicker modalInventoryPicker;

    /*private int optionIDSelected;                             //slot ID (eg arrayOfGenericOptions [index] of selected option
    private string optionTextSelected;                        //used for nested Generic Picker windows, ignore otherwise
    private int nodeIDSelected;
    private int actorSlotIDSelected;
    private EventType defaultReturnEvent;                          //event to trigger once confirmation button is clicked
    private EventType backReturnEvent;                  //event triggered when back button clicked (dynamic -> SetBackButton)
    private ModalActionDetails nestedDetails;           //used only if there are multiple, nested, option windows (dynamic -> InitialiseNestedOptions)*/

    private string colourEffect;
    private string colourSide;
    private string colourTeam;
    private string colourDefault;
    private string colourNormal;
    private string colourGood;
    private string colourBad;
    private string colourEnd;
}
