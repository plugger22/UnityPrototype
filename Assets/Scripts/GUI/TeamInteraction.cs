using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace gameAPI
{
    /// <summary>
    /// class attached to TeamObject prefab to facilitate safe access to child components and handle mouse clicks
    /// </summary>
    public class TeamInteraction : MonoBehaviour, IPointerClickHandler
    {
        public Image highlightImage;
        public Image teamImage;
        public TextMeshProUGUI teamText;

        public int teamArcID;

        private bool isSelected;

        /// <summary>
        /// Internal initialisation
        /// </summary>
        private void Awake()
        {
            isSelected = false;
        }

        /// <summary>
        /// External initialisation
        /// </summary>
        private void Start()
        {
            //event listener
            EventManager.instance.AddListener(EventType.DeselectOtherTeams, OnEvent);
        }

        /// <summary>
        /// Event Handler
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="Sender"></param>
        /// <param name="Param"></param>
        public void OnEvent(EventType eventType, Component Sender, object Param = null)
        {
            //select event type
            switch (eventType)
            {
                case EventType.DeselectOtherTeams:
                    DeSelectTeam();
                    break;
                default:
                    Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                    break;
            }
        }

        /// <summary>
        /// Activates highlight image when clicked
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            Color tempColour = highlightImage.color;
            if (isSelected == true)
            {
                tempColour.a = 0.0f;
                isSelected = false;
                //deactivate Confirm button (only shows when a team is selected)
                EventManager.instance.PostNotification(EventType.ConfirmDeactivate, this);
            }
            else
            {
                //deactivate any other currently selected team and switch Confirm button on
                EventManager.instance.PostNotification(EventType.DeselectOtherTeams, this);
                EventManager.instance.PostNotification(EventType.ConfirmActivate, this, teamArcID);
                //NOTE: Event call must be made BEFORE activating this choice otherwise it'll deactivate it immediately
                tempColour.a = 1.0f;
                isSelected = true;
            }
            //assign new values to Image
            highlightImage.color = tempColour;
        }


        /// <summary>
        /// Deselect team choice (event call) as a result of another team being selected
        /// </summary>
        private void DeSelectTeam()
        {
            Color tempColour = highlightImage.color;
            if (isSelected == true)
            {
                tempColour.a = 0.0f;
                isSelected = false;
            }
            //assign new values to Image
            highlightImage.color = tempColour;
        }

    }

}