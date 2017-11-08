using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace gameAPI
{
    /// <summary>
    /// class attached to TeamObject prefab to facilitate safe access to child components
    /// </summary>
    public class TeamChoiceUI : MonoBehaviour
    {
        public Image highlightImage;
        public Image teamImage;
        public TextMeshProUGUI name;
    }

}