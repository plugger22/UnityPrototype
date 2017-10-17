using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// Handles all node target related matters
/// </summary>
public class TargetManager : MonoBehaviour
{
    [Range(0, 20)]
    [Tooltip("The % of the total Nodes on the level which commence with a Live target")]
    public int startPercentTargets = 10;
    [Range(20, 50)]
    [Tooltip("The % of the total Nodes on the level that can have a Live target at any one time")]
    public int maxPercentTargets = 25;

    public int StartTargets { get; private set; }
    public int ActiveTargets { get; private set; }
    public int LiveTargets { get; private set; }
    public int MaxTargets { get; private set; }


    /// <summary>
    /// Initial setup
    /// </summary>
    public void Initialise()
    {
        Dictionary<int, Target> dictOfTargets = GameManager.instance.dataScript.GetDictOfTargets();
        if (dictOfTargets != null)
        {
            //calculate limits
            int numOfNodes = GameManager.instance.levelScript.GetNumOfNodes();
            StartTargets = numOfNodes * startPercentTargets / 100;
            MaxTargets = numOfNodes * maxPercentTargets / 100;
            ActiveTargets = MaxTargets - StartTargets;
            ActiveTargets = Mathf.Max(0, ActiveTargets);
            //loop targets
            foreach(var target in dictOfTargets)
            {
                if (GameManager.instance.dataScript.GetNodeInfo(target.Value.nodeArc.NodeArcID, NodeInfo.Number ) > 0)
                {
                    //add to list of Possible targets
                    GameManager.instance.dataScript.AddPossibleTarget(target.Value);
                }
                else
                {
                    Debug.Log(string.Format("TargetManager: {0} has been ignored as there are no required node types present (\"{1}\"){2}", 
                        target.Value.name, target.Value.nodeArc.name, "\n"));
                    continue;
                }
            }
        }
        else
        { Debug.LogError("Invalid dictOfTargets (null) -> Targets not assigned"); }
    }


    
}
