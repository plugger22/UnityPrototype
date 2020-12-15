using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all UI related sprites
/// </summary>
public class SpriteManager : MonoBehaviour
{

    [Header("Sprites")]
    [Tooltip("Sprite to use for ActorGUI to show that the position is vacant")]
    public Sprite vacantActorSprite;
    [Tooltip("Universal Error sprite")]
    public Sprite errorSprite;
    [Tooltip("Universal Info sprite")]
    public Sprite infoSprite;
    [Tooltip("Alarm (spotted) sprite")]
    public Sprite alarmSprite;
    [Tooltip("Undetected (NOT spotted) sprite")]
    public Sprite undetectedSprite;
    [Tooltip("Used for Target attempts that succeed")]
    public Sprite targetSuccessSprite;
    [Tooltip("Used for Target attempts that fail")]
    public Sprite targetFailSprite;
    [Tooltip("Used for Generic Picker with Planner action to select target (maybe there could be three colour variations of the same sprite to indicate how much intel you will gain?")]
    public Sprite targetInfoSprite;
    [Tooltip("Used for Player or Actor having been captured (152 x 160 png)")]
    public Sprite capturedSprite;
    [Tooltip("Used for Player or Actor being released from captivity (152 x 160 png)")]
    public Sprite releasedSprite;
    [Tooltip("Used for Player being Fired by their HQ")]
    public Sprite firedSprite;
    [Tooltip("Default City Arc sprite (512 x 150 png)")]
    public Sprite cityArcDefaultSprite;
    [Tooltip("Used for message Information Alerts (152 x 160 png)")]
    public Sprite alertInformationSprite;
    [Tooltip("Used for message Warning Alerts (152 x 160 png)")]
    public Sprite alertWarningSprite;
    [Tooltip("Used for message Random (152 x 160 png)")]
    public Sprite alertRandomSprite;
    [Tooltip("Used for ai reboot messages (152 x 160 png)")]
    public Sprite aiRebootSprite;
    [Tooltip("Used for ai countermeasures (152 x 160 png)")]
    public Sprite aiCountermeasureSprite;
    [Tooltip("Used for ai alert status changes (152 x 160 png)")]
    public Sprite aiAlertSprite;
    [Tooltip("Used for Investigations (152 x 160 png")]
    public Sprite investigationSprite;
    [Tooltip("Used for Secrets (152 x 160 png")]
    public Sprite secretSprite;
    [Tooltip("Used for Organisations (152 x 160 png")]
    public Sprite organisationSprite;
    [Tooltip("Used for ongoing effects (152 x 160 png")]
    public Sprite ongoingEffectSprite;
    [Tooltip("Used for node Crisis (152 x 160 png")]
    public Sprite nodeCrisisSprite;
    [Tooltip("Used for city loyalty changes (152 x 160 png)")]
    public Sprite cityLoyaltySprite;
    [Tooltip("Used for action adjustment infoAPP effect notifications")]
    public Sprite actionSprite;
    [Tooltip("Used for Objectives")]
    public Sprite objectiveSprite;
    [Tooltip("Default topic sprite")]
    public Sprite topicDefaultSprite;
    [Tooltip("Default sprite used for Topic UI (normal valid option) if none specified")]
    public Sprite topicOptionNormalValidSprite;
    [Tooltip("Sprite used for a normal invalid topic option")]
    public Sprite topicOptionNormalInvalidSprite;
    [Tooltip("Sprite used for a valid Other format Topic UI option")]
    public Sprite topicOptionOtherValidSprite;
    [Tooltip("Sprite used for a invalid Other format Topic UI option")]
    public Sprite topicOptionOtherInvalidSprite;
    [Tooltip("Sprite used for Topic Reviews")]
    public Sprite topicReviewSprite;
    [Tooltip("Sprite for a Friendly relationship")]
    public Sprite friendSprite;
    [Tooltip("Sprite for an Enemy relationship")]
    public Sprite enemySprite;
    [Tooltip("Sprite for Gold medal")]
    public Sprite medalGoldSprite;
    [Tooltip("Sprite for Silver medal")]
    public Sprite medalSilverSprite;
    [Tooltip("Sprite for Bronze medal")]
    public Sprite medalBronzeSprite;
    [Tooltip("Sprite used for Dead Duck Award")]
    public Sprite medalDuckSprite;
    [Tooltip("ModalTabbedUI male contact sprite")]
    public Sprite tabbedContactMaleSprite;
    [Tooltip("ModalTabbedUI female contact sprite")]
    public Sprite tabbedContactFemaleSprite;

    [Header("Priorities")]
    [Tooltip("Used for itemData priority High in MainInfoUI (20 x 20 artboard with icon being 15 x 15 png)")]
    public Sprite priorityHighSprite;
    [Tooltip("Used for itemData priority Medium in MainInfoUI (20 x 20 artboard with icon being 15 x 15 png)")]
    public Sprite priorityMediumSprite;
    [Tooltip("Used for itemData priority Low in MainInfoUI (20 x 20 artboard with icon being 15 x 15 png)")]
    public Sprite priorityLowSprite;
    [Tooltip("Used for Items that are Inactive (MetaGameUI) (20 x 20 artboard with icon being 15 x 15 png")]
    public Sprite priorityInactiveSprite;

    [Header("Moods")]
    [Tooltip("Player mood 0 star")]
    public Sprite moodStar0;
    [Tooltip("Player mood 1 star")]
    public Sprite moodStar1;
    [Tooltip("Player mood 2 star")]
    public Sprite moodStar2;
    [Tooltip("Player mood 3 star")]
    public Sprite moodStar3;


    public void Awake()
    {
        //Check sprites are present
        Debug.Assert(vacantActorSprite != null, "Invalid vacantActorSprite (Null)");
        Debug.Assert(errorSprite != null, "Invalid errorSprite (Null)");
        Debug.Assert(infoSprite != null, "Invalid infoSprite (Null)");
        Debug.Assert(alarmSprite != null, "Invalid alarmSprite (Null)");
        Debug.Assert(undetectedSprite != null, "Invalid undetectedSprite (Null)");
        Debug.Assert(targetSuccessSprite != null, "Invalid targetSuccessSprite (Null)");
        Debug.Assert(targetFailSprite != null, "Invalid targetFailSprite (Null)");
        Debug.Assert(capturedSprite != null, "Invalid capturedSprite (Null)");
        Debug.Assert(releasedSprite != null, "Invalid releasedSprite (Null)");
        Debug.Assert(firedSprite != null, "Invalid firedSprite (Null)");
        Debug.Assert(cityArcDefaultSprite != null, "Invalid cityArcDefaultSprite (Null)");
        Debug.Assert(alertInformationSprite != null, "Invalid alertInformationSprite (Null)");
        Debug.Assert(alertWarningSprite != null, "Invalid alertWarningSprite (Null)");
        Debug.Assert(alertRandomSprite != null, "Invalid alertRandomSprite (Null)");
        Debug.Assert(aiRebootSprite != null, "Invalid aiRebootSpirte (Null)");
        Debug.Assert(aiCountermeasureSprite != null, "Invalid aiCountermeasureSprite (Null)");
        Debug.Assert(aiAlertSprite != null, "Invalid aiAlertSprite (Null)");
        Debug.Assert(investigationSprite != null, "Invalid investigationSprite (Null)");
        Debug.Assert(secretSprite != null, "Invalid secretSprite (Null)");
        Debug.Assert(organisationSprite != null, "Invalid organisationSprite (Null)");
        Debug.Assert(ongoingEffectSprite != null, "Invalid ongoingEffectSprite (Null)");
        Debug.Assert(nodeCrisisSprite != null, "Invalid nodeCrisisSprite (Null)");
        Debug.Assert(cityLoyaltySprite != null, "Invalid cityLoyaltySprite (Null)");
        Debug.Assert(priorityHighSprite != null, "Invalid priorityHighSprite (Null)");
        Debug.Assert(priorityMediumSprite != null, "Invalid priorityMediumSprite (Null)");
        Debug.Assert(priorityLowSprite != null, "Invalid priorityLowSprite (Null)");
        Debug.Assert(priorityInactiveSprite != null, "Invalid priorityInactiveSprite (Null)");
        Debug.Assert(actionSprite != null, "Invalid actionSprite (Null)");
        Debug.Assert(objectiveSprite != null, "Invalid objectiveSprite (Null)");
        Debug.Assert(topicDefaultSprite != null, "Invalid topicDefaultSprite (Null)");
        Debug.Assert(topicOptionNormalValidSprite != null, "Invalid topicOptionNormalValidSprite (Null)");
        Debug.Assert(topicOptionNormalInvalidSprite != null, "Invalid topicOptionNormalInvalidSprite (Null)");
        Debug.Assert(topicOptionOtherValidSprite != null, "Invalid topicOptionOtherValidSprite (Null)");
        Debug.Assert(topicOptionOtherInvalidSprite != null, "Invalid topicOptionOtherInvalidSprite (Null)");
        Debug.Assert(topicReviewSprite != null, "Invalid topicReviewSprite (Null)");
        Debug.Assert(friendSprite != null, "Invalid friendSprite (Null)");
        Debug.Assert(enemySprite != null, "Invalid enemySprite (Null)");
        Debug.Assert(moodStar0 != null, "Invalid moodStar0 (Null)");
        Debug.Assert(moodStar1 != null, "Invalid moodStar1 (Null)");
        Debug.Assert(moodStar2 != null, "Invalid moodStar2 (Null)");
        Debug.Assert(moodStar3 != null, "Invalid moodStar3 (Null)");
        Debug.Assert(medalGoldSprite != null, "Invalid medalGoldSprite (Null)");
        Debug.Assert(medalSilverSprite != null, "Invalid medalSilverSprite (Null)");
        Debug.Assert(medalBronzeSprite != null, "Invalid medalBronzeSprite (Null)");
        Debug.Assert(medalDuckSprite != null, "Invalid medalDuckSprite (Null)");
        Debug.Assert(tabbedContactMaleSprite != null, "Invalid tabbedContactMaleSprite (Null)");
        Debug.Assert(tabbedContactFemaleSprite != null, "Invalid tabbedContactFemaleSprite (Null)");

    }
}
