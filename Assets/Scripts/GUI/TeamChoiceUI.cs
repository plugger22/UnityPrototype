using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace gameAPI
{
    /// <summary>
    /// class attached to TeamObject prefab to facilitate safe access to child components and handle mouse clicks
    /// </summary>
    public class TeamChoiceUI : MonoBehaviour
    {
        public Image highlightImage;
        public Image teamImage;
        public TextMeshProUGUI name;

        private bool isSelected;

        private void Awake()
        {
            isSelected = false;
            
        }

        /// <summary>
        /// Activates highlight image when clicked
        /// </summary>
        public void OnMouseDown()
        {
            Color tempColour = highlightImage.color;
            if (isSelected == true)
            {
                tempColour.a = 0.0f;
                isSelected = false;
            }
            else
            {
                tempColour.a = 1.0f;
                isSelected = true;
            }
        }

    }

}