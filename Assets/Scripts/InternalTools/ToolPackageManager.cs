using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if(UNITY_EDITOR)
namespace toolsAPI
{

    //
    // - - - Enums
    //

    public enum ThemeType { Action, Tension, Social, Mystery, Personal, Count}



    //
    // - - - Data Packages
    //

    /// <summary>
    /// Master story class
    /// </summary>
    [System.Serializable]
    public class Story
    {
        public string tag;
        public string notes;
        public string date;
        public int numTurningPoints;
        //subClasses
        public ThemeData theme;
    }

    /// <summary>
    /// Theme class
    /// </summary>
    [System.Serializable]
    public class ThemeData
    {
        //10 index array for theme table with an extra index for the last index where it's a 50/50 chance of [9] or [10]
        public ThemeType[] arrayOfThemes = new ThemeType[11];
    }

}
#endif
