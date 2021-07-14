using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For ActorConfiguration.SO -> specifies actor arc type and level. Enum class
/// </summary>
[CreateAssetMenu(menuName = "Tutorial / TutorialActorType")]
public class TutorialActorType : ScriptableObject
{

    public ActorArc arc;
    public int level;
    public GlobalSide side;

    [Tooltip("Each actor of the same arc should have the same code, it's for running a quick dupe check in a lineUp. (Anarchist 0, blogger 1, fixer 2, hacker 3, heavy 4, observer 5, operator 6, planner 7, recruiter 8)")]
    public int dupeIndex;
		
	
}
