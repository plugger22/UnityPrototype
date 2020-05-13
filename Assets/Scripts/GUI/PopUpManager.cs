using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// handles all Fixed UI popUps. NOTE: ModalPopUp is a single dynamic UI popUp that is designed to be used anywhere (not fixed)
/// </summary>
public class PopUpManager : MonoBehaviour
{
    [Header("Actor Slot0")]
    public GameObject popSlot0Object;
    public Transform popSlot0Transform;
    public TextMeshProUGUI popSlot0Text;
    [Header("Actor Slot1")]
    public GameObject popSlot1Object;
    public Transform popSlot1Transform;
    public TextMeshProUGUI popSlot1Text;
    [Header("Actor Slot2")]
    public GameObject popSlot2Object;
    public Transform popSlot2Transform;
    public TextMeshProUGUI popSlot2Text;
    [Header("Actor Slot3")]
    public GameObject popSlot3Object;
    public Transform popSlot3Transform;
    public TextMeshProUGUI popSlot3Text;
    [Header("Player")]
    public GameObject popPlayerObject;
    public Transform popPlayerTransform;
    public TextMeshProUGUI popPlayerText;
    [Header("Top Bar Left")]
    public GameObject popTopLeftObject;
    public Transform popTopLeftTransform;
    public TextMeshProUGUI popTopLeftText;
    [Header("Top Bar Right")]
    public GameObject popTopRightObject;
    public Transform popTopRightTransform;
    public TextMeshProUGUI popTopRightText;
    [Header("Top Widget Centre")]
    public GameObject popTopCentreObject;
    public Transform popTopCentreTransform;
    public TextMeshProUGUI popTopCentreText;


}
