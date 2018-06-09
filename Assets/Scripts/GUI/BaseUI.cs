using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// Base panel button that sits under all GUI elements and is there to pick up Right Mouse clicks (that don't happen when pointer blocked by a UI element)
/// </summary>
public class BaseUI : MonoBehaviour, IPointerClickHandler
{

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.LogFormat("[UI] BaseUI.cs -> OnPointerClick: Left Mouse Button clicked -> {0} object{1}", eventData.pointerPress.name, "\n");
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.LogFormat("[UI] BaseUI.cs -> OnPointerClick: Right Mouse Button clicked -> {0} object{1}", eventData.pointerPress.name, "\n");
        }
    }
}
