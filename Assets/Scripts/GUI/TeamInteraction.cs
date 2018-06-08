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

        //[HideInInspector] public int teamArcID;             //assigned in ModalTeamPicker.cs -> Initialise()
        [HideInInspector] public int teamID;                //teamID of next available team in Reserve pool that matches type -> passed back by event on 'Confirm' click

        private bool isSelected;                            //has the team been selected
        [HideInInspector]
        public bool isActive;                               //is the team a valid choice option or is it greyed out? (can't be selected)

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
            EventManager.instance.AddListener(EventType.DeselectOtherTeams, OnEvent, "TeamInteraction");
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
            //check option is a viable choice
            if (isActive == true)
            {
                Color tempColour = highlightImage.color;
                if (isSelected == true)
                {
                    tempColour.a = 0.0f;
                    isSelected = false;
                    //deactivate Confirm button (only shows when a team is selected)
                    EventManager.instance.PostNotification(EventType.ConfirmTeamDeactivate, this, null, "TeamInteraction.cs -> OnPointerClick");
                }
                else
                {
                    //deactivate any other currently selected team and switch Confirm button on
                    EventManager.instance.PostNotification(EventType.DeselectOtherTeams, this, null, "TeamInteraction.cs -> OnPointerClick");
                    EventManager.instance.PostNotification(EventType.ConfirmTeamActivate, this, teamID, "TeamInteraction.cs -> OnPointerClick");
                    //NOTE: Event call must be made BEFORE activating this choice otherwise it'll deactivate it immediately
                    tempColour.a = 1.0f;
                    isSelected = true;
                }
                //assign new values to Image
                highlightImage.color = tempColour;
            }
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