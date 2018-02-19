using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gameAPI;

/// <summary>
/// handles all MetaLevel matters
/// </summary>
public class MetaManager : MonoBehaviour
{
    [HideInInspector] public GlobalMeta metaLevel;

    //used for quick reference -> Meta Levels where metaBottom is the lowest level and metaTop is the highest
    [HideInInspector] public GlobalMeta metaBottom;
    [HideInInspector] public GlobalMeta metaMiddle;
    [HideInInspector] public GlobalMeta metaTop;

    public void Initialise()
    {
        //initialise fast access fields -> GlobalMeta
        Dictionary<string, GlobalMeta> dictOfGlobalMeta = GameManager.instance.dataScript.GetDictOfGlobalMeta();
        if (dictOfGlobalMeta != null)
        {
            foreach (var meta in dictOfGlobalMeta)
            {
                //pick out and assign the ones required for fast acess, ignore the rest. 
                //Also dynamically assign GlobalMeta.level values (0/1/2). 
                switch (meta.Key)
                {
                    case "Local":
                        metaBottom = meta.Value;
                        meta.Value.level = 0;
                        break;
                    case "State":
                        metaMiddle = meta.Value;
                        meta.Value.level = 1;
                        break;
                    case "National":
                        metaTop = meta.Value;
                        meta.Value.level = 2;
                        break;
                }
            }
            //error check
            if (metaBottom == null) { Debug.LogError("Invalid metaBottom (Null)"); }
            if (metaMiddle == null) { Debug.LogError("Invalid metaMiddle (Null)"); }
            if (metaTop == null) { Debug.LogError("Invalid metaTop (Null)"); }
            //set state
            metaLevel = metaBottom;
        }
    }

    /// <summary>
    /// returns official authority title
    /// </summary>
    /// <returns></returns>
    public AuthorityActor GetAuthorityTitle()
    { return (AuthorityActor)(metaLevel.level); }

}
