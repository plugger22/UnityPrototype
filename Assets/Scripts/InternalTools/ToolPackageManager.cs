using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if(UNITY_EDITOR)
namespace toolsAPI
{

    //
    // - - - Enums
    //

    public enum ToolModal { Menu, Main, New }
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
        public string tag;              //stored in dict under this name (use as a reference)
        public string notes;
        public string date;
        public int numTurningPoints;
        //subClasses
        public ThemeData theme = new ThemeData();

        /// <summary>
        /// default constructor
        /// </summary>
        public Story()
        { }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="story"></param>
        public Story (Story story)
        {
            tag = story.tag;
            notes = story.notes;
            date = story.date;
            theme = new ThemeData(story.theme);
        }

        /// <summary>
        /// Reset data
        /// </summary>
        public void Reset()
        {
            tag = "";
            notes = "";
            date = "";
            theme.Reset();
        }
    }

    /// <summary>
    /// Theme class
    /// </summary>
    [System.Serializable]
    public class ThemeData
    {
        //10 index array for theme table with an extra index for the last index where it's a 50/50 chance of [9] or [10]
        public ThemeType[] arrayOfThemes = new ThemeType[11];

        public ThemeData() { }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="data"></param>
        public ThemeData(ThemeData data)
        { data.arrayOfThemes.CopyTo(arrayOfThemes, 0); }

        /// <summary>
        /// Reset array
        /// </summary>
        public void Reset()
        { Array.Clear(arrayOfThemes, 0, arrayOfThemes.Length); }


        /// <summary>
        /// returns ThemeType for a specific priority (1 to 5)
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public ThemeType GetThemePriority(int priority)
        {
            ThemeType themeType = new ThemeType();
            switch (priority)
            {
                case 1: themeType = arrayOfThemes[0]; break;
                case 2: themeType = arrayOfThemes[4]; break;
                case 3: themeType = arrayOfThemes[7]; break;
                case 4: themeType = arrayOfThemes[9]; break;
                case 5: themeType = arrayOfThemes[10]; break;
                default: Debug.LogWarningFormat("Unrecognised priority \"{0}\"", priority); break;
            }
            return themeType;
        }
    }

}
#endif
