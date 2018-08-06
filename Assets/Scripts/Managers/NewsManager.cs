using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// handles news generation for the Info App ticker tape
/// </summary>
public class NewsManager : MonoBehaviour
{

    /// <summary>
    /// returns
    /// </summary>
    /// <returns></returns>
    public string GetNews()
    {
        string text;
        text = "\tBreaking News: Mayor Cameron pardons Resistance prisoners. Elsewhere riots continue in Commerce and vigilante groups disrupt neighbour tranquility in Gardenna.\t";
        //text = "\tthe world is about to end by this time tommorrow but it isn't as bad as you think.In breaking news riots have broken out across the length and breadth of Gotham city. Mayor Greene issued a statement this afternoon on CNN Live\t";
        return text;
    }

}
