﻿using System;
using System.Collections.Generic;
using UnityEngine;

#if(UNITY_EDITOR)
namespace toolsAPI
{

    //
    // - - - Enums
    //

    public enum ToolModal { Menu, Main, New, Lists }
    public enum ToolModalType { Read, Edit }
    public enum ThemeType { Action, Tension, Social, Mystery, Personal, Count }
    public enum StoryStatus { New, Logical, Data }
    public enum ListItemStatus { None, PlotLine, Character }    //what's currently selected on the Aventure/list page



    //
    // - - - Data Packages
    //

    #region Story Master
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
        public StoryList arrays = new StoryList();
        //Collections
        public List<PlotLine> listOfPlotLines = new List<PlotLine>();
        public List<Character> listOfCharacters = new List<Character>();

        #region Story Methods
        /// <summary>
        /// default constructor
        /// </summary>
        public Story()
        { }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="story"></param>
        public Story(Story story)
        {
            tag = story.tag;
            notes = story.notes;
            date = story.date;
            theme = new ThemeData(story.theme);
            lists = new StoryList(story.lists);
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
            lists.Reset();
        }
        #endregion
    }
    #endregion

    #region ThemeData
    /// <summary>
    /// Theme class
    /// </summary>
    [System.Serializable]
    public class ThemeData
    {
        //10 index array for theme table with an extra index for the last index where it's a 50/50 chance of [9] or [10]
        public ThemeType[] arrayOfThemes = new ThemeType[11];

        public ThemeData() { }

        #region Theme Methods
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
        #endregion
    }
    #endregion

    #region StoryList
    /// <summary>
    /// Lists of Plotlines and Characters
    /// </summary>
    [System.Serializable]
    public class StoryLis
    {
        public ListItem[] arrayOfPlotLines;
        public ListItem[] arrayOfCharacters;
        private int size = 25;

        #region StoryList Methods
        /// <summary>
        /// default constructor
        /// </summary>
        public StoryList()
        {
            arrayOfPlotLines = new ListItem[size];
            arrayOfCharacters = new ListItem[size];
            PopulateLists();
        }


        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="data"></param>
        public StoryList(StoryList data)
        {
            //initialise if need be (don't need data as will be copied over)
            if (arrayOfPlotLines == null) { arrayOfPlotLines = new ListItem[size]; }
            if (arrayOfCharacters == null) { arrayOfCharacters = new ListItem[size]; }
            //copy data
            data.arrayOfPlotLines.CopyTo(arrayOfPlotLines, 0);
            data.arrayOfCharacters.CopyTo(arrayOfCharacters, 0);
        }

        /// <summary>
        /// Reset arrays
        /// </summary>
        public void Reset()
        {
            /*Array.Clear(arrayOfPlotLines, 0, arrayOfPlotLines.Length);
            Array.Clear(arrayOfCharacters, 0, arrayOfCharacters.Length);*/
            PopulateLists();
        }

        /// <summary>
        /// Populate lists with default data
        /// </summary>
        private void PopulateLists()
        {
            //populate arrays
            for (int i = 0; i < size; i++)
            {
                //Plotlines
                switch (i)
                {
                    case 0:
                    case 2:
                    case 3:
                    case 4:
                    case 6:
                    case 7:
                    case 8:
                    case 10:
                    case 11:
                    case 12:
                    case 14:
                    case 15:
                    case 16:
                    case 18:
                    case 19:
                    case 20:
                    case 22:
                    case 23:
                    case 24:
                        arrayOfPlotLines[i] = new ListItem() { tag = "", status = StoryStatus.Logical };
                        break;
                    case 1:
                    case 5:
                    case 9:
                    case 13:
                    case 17:
                    case 21:
                        arrayOfPlotLines[i] = new ListItem() { tag = "", status = StoryStatus.New };
                        break;
                    default: Debug.LogWarningFormat("Unrecognised counter \"{0}\" for Plotlines", i); break;
                }
                //characters
                switch (i)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 4:
                    case 5:
                    case 6:
                    case 8:
                    case 9:
                    case 10:
                    case 12:
                    case 16:
                    case 20:
                    case 24:
                        arrayOfCharacters[i] = new ListItem() { tag = "", status = StoryStatus.New };
                        break;
                    case 3:
                    case 7:
                    case 11:
                    case 13:
                    case 14:
                    case 15:
                    case 17:
                    case 18:
                    case 19:
                    case 21:
                    case 22:
                    case 23:
                        arrayOfCharacters[i] = new ListItem() { tag = "", status = StoryStatus.Logical };
                        break;
                    default: Debug.LogWarningFormat("Unrecognised counter \"{0}\" for Characters", i); break;
                }
            }
        }
        #endregion
    }
    #endregion

    #region PlotLine
    /// <summary>
    /// Plotline class (Story.cs)
    /// </summary>
    [System.Serializable]
    public class PlotLine
    {
        public string tag;
        public string dataCreated;                          //generated data
        public string dataMe;                               //my interpretation
    }
    #endregion

    #region Character
    /// <summary>
    /// Character class (Story.cs)
    /// </summary>
    [System.Serializable]
    public class Character
    {
        public string tag;
        public string dataCreated;                          //generated data
        public string dataMe;                               //my interpretation
    }
    #endregion

    #region ListItem
    /// <summary>
    /// List Item (PlotLines and Character references that inhabit lists)
    /// </summary>
    [System.Serializable]
    public class ListItem
    {
        public string tag;                                  //name (tag) of PlotLine or Character
        public StoryStatus status;
    }
    #endregion

}
#endif
