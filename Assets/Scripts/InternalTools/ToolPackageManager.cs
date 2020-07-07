﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

#if (UNITY_EDITOR)
namespace toolsAPI
{

    //
    // - - - Enums
    //

    public enum ToolModal { Menu, Main, New, TurningPoint, Lists }
    public enum ToolModalType { Read, Edit }
    public enum ThemeType { Action, Tension, Mystery, Social, Personal, Count }   //NOTE: Order matters (ToolDetails.cs)
    public enum StoryStatus { New, Logical, Data }
    public enum ListItemStatus { None, PlotLine, Character }    //what's currently selected on the Aventure/list page
    public enum PlotPointType { Normal, Conclusion, None, RemoveCharacter, NewCharacter, Meta }
    public enum MetaAction { CharacterExits, CharacterReturns, CharacterUpgrade, CharacterDowngrade, CharacterStepsUp, CharacterStepsDown, PlotLineCombo }
    public enum TurningPointType { None, New, Development, Conclusion }



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
        public string tag;                  //stored in dict under this name (use as a reference)
        public string notes;
        public string date;
        public int numTurningPoints;        //current active turning points (max cap 5)
        public bool isConcluded;            //if true no more turning points can be added, story is complete
        //subClasses
        public ThemeData theme = new ThemeData();
        public StoryArrays arrays = new StoryArrays();
        public TurningPoint[] arrayOfTurningPoints = new TurningPoint[5];
        public StoryLists lists = new StoryLists();

        #region Story Methods
        /// <summary>
        /// default constructor
        /// </summary>
        public Story()
        {
            //initialise turning Point array with blank turning points
            for (int i = 0; i < arrayOfTurningPoints.Length; i++)
            { arrayOfTurningPoints[i] = new TurningPoint(); }
        }

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
            arrays = new StoryArrays(story.arrays);
            for (int i = 0; i < arrayOfTurningPoints.Length; i++)
            { arrayOfTurningPoints[i] = story.arrayOfTurningPoints[i]; }
            /*lists = new StoryLists(story.lists);*/
        }

        /// <summary>
        /// Reset data
        /// </summary>
        public void Reset()
        {
            tag = "";
            notes = "";
            date = "";
            isConcluded = false;
            theme.Reset();
            arrays.Reset();
            for (int i = 0; i < arrayOfTurningPoints.Length; i++)
            { arrayOfTurningPoints[i].Reset(); }
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
        public ThemeType GetThemeType(int priority)
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

    #region StoryArrays
    /// <summary>
    /// Arrays of Plotlines and Characters (ListItem arrays)
    /// </summary>
    [System.Serializable]
    public class StoryArrays
    {
        public ListItem[] arrayOfPlotLines;
        public ListItem[] arrayOfCharacters;
        private int size = 25;

        #region StoryArray Methods

        /// <summary>
        /// default constructor
        /// </summary>
        public StoryArrays()
        {
            arrayOfPlotLines = new ListItem[size];
            arrayOfCharacters = new ListItem[size];
            PopulateLists();
        }


        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="data"></param>
        public StoryArrays(StoryArrays data)
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
        /// get a plotLine ListItem from array. Returns null if a problem
        /// </summary>
        /// <returns></returns>
        public ListItem GetPlotLineFromArray()
        {
            ListItem item = null;
            int rnd = Random.Range(0, 25);
            item = arrayOfPlotLines[rnd];
            return item;
        }

        /// <summary>
        /// get a character ListItem from array. Returns null if a problem.
        /// </summary>
        /// <returns></returns>
        public ListItem GetCharacterFromArray()
        {
            ListItem item = null;
            int rnd = Random.Range(0, 25);
            item = arrayOfCharacters[rnd];
            return item;
        }

        /// <summary>
        /// Adds a new character to the next vacant, non-DATA slot in the array. Returns true if successful
        /// </summary>
        /// <param name="newItem"></param>
        public bool AddCharacterToArray(ListItem newItem)
        {
            if (newItem != null)
            {
                for (int i = 0; i < arrayOfCharacters.Length; i++)
                {
                    if (arrayOfCharacters[i].status != StoryStatus.Data)
                    {
                        //check how many are already present
                        int count = arrayOfCharacters.Select(x => x.tag.Equals(newItem.tag, StringComparison.Ordinal)).Count();
                        if (count < 3)
                        {
                            arrayOfCharacters[i] = newItem;
                            Debug.LogFormat("[Tst] StoryArrays.cs -> AddCharacterToArray: {0}, {1} ADDED to arrayOfCharacters{2}", newItem.tag, newItem.status, "\n");
                            return true;
                        }
                        else
                        {
                            Debug.LogFormat("[Tst] StoryArrays.cs -> AddCharacterToArray:{0}, {1} NOT added to arrayOfCharacters (has {2} of identical already){3}",
                                newItem.tag, newItem.status, count, "\n");
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid newItem (Null)"); }
            return false;
        }

        /// <summary>
        /// Adds a new PlotLine to the next vacant, non-DATA slot in the array. Returns true if successful
        /// </summary>
        /// <param name="newItem"></param>
        public bool AddPlotLineToArray(ListItem newItem)
        {
            if (newItem != null)
            {
                for (int i = 0; i < arrayOfPlotLines.Length; i++)
                {
                    if (arrayOfPlotLines[i].status != StoryStatus.Data)
                    {
                        //check how many are already present
                        int count = arrayOfPlotLines.Select(x => x.tag.Equals(newItem.tag, StringComparison.Ordinal)).Count();
                        if (count < 3)
                        {
                            arrayOfPlotLines[i] = newItem;
                            Debug.LogFormat("[Tst] StoryArrays.cs -> AddPlotLineToArray: {0}, {1} ADDED to arrayOfPlotLines{2}", newItem.tag, newItem.status, "\n");
                            return true;
                        }
                        else
                        {
                            Debug.LogFormat("[Tst] StoryArrays.cs -> AddPlotLineToArray:{0}, {1} NOT added to arrayOfPlotLines (has {2} of identical already){3}",
                                newItem.tag, newItem.status, count, "\n");
                        }
                    }
                }
            }
            else { Debug.LogError("Invalid newItem (Null)"); }
            return false;
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

    #region StoryLists
    /// <summary>
    /// Lists of PlotLines and Characters
    /// </summary>
    [System.Serializable]
    public class StoryLists
    {
        public List<PlotLine> listOfPlotLines = new List<PlotLine>();
        public List<Character> listOfCharacters = new List<Character>();
        public List<Character> listOfRemovedCharacters = new List<Character>();

        #region StoryLists Methods

        /// <summary>
        /// default constructor
        /// </summary>
        public StoryLists() { }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="data"></param>
        public StoryLists(StoryLists data)
        {
            listOfPlotLines.Clear();
            listOfCharacters.Clear();
            listOfRemovedCharacters.Clear();
            listOfPlotLines.AddRange(data.listOfPlotLines);
            listOfCharacters.AddRange(data.listOfCharacters);
            listOfRemovedCharacters.AddRange(data.listOfRemovedCharacters);
        }

        /// <summary>
        /// Reset
        /// </summary>
        public void Reset()
        {
            listOfPlotLines.Clear();
            listOfCharacters.Clear();
            listOfRemovedCharacters.Clear();
        }

        /// <summary>
        /// Returns specific Character from list (based on array refTag), null if not found
        /// </summary>
        /// <param name="charRef"></param>
        /// <returns></returns>
        public Character GetCharacterFromList(string charRef)
        {
            Character character = null;
            if (string.IsNullOrEmpty(charRef) == false)
            { character = listOfCharacters.Find(x => x.refTag.Equals(charRef, StringComparison.Ordinal)); }
            else { Debug.LogError("Invalid charRef (Null or Empty)"); }
            return character;
        }

        /// <summary>
        /// Returns specific Plotline from list (base on array refTag), null if not found
        /// </summary>
        /// <param name="plotRef"></param>
        /// <returns></returns>
        public PlotLine GetPlotLineFromList(string plotRef)
        {
            PlotLine plotLine = null;
            if (string.IsNullOrEmpty(plotRef) == false)
            { plotLine = listOfPlotLines.Find(x => x.refTag.Equals(plotRef, StringComparison.Ordinal)); }
            else { Debug.LogError("Invalid plotRef (Null or Empty)"); }
            return plotLine;
        }

        /// <summary>
        /// Add character to listOfCharacters
        /// </summary>
        /// <param name="character"></param>
        public void AddCharacterToList(Character character)
        {
            if (character != null)
            {
                //check not already in list
                if (listOfCharacters.Exists(x => x.refTag.Equals(character.refTag, StringComparison.Ordinal)) == false)
                { listOfCharacters.Add(character); }
                else { Debug.LogWarningFormat("Character refTag \"{0}\" alread present in list", character.refTag); }
            }
            else { Debug.LogError("Invalid character (Null)"); }
        }

        /// <summary>
        /// Add PlotLine to listOfPlotLines
        /// </summary>
        /// <param name="plotLine"></param>
        public void AddPlotLineToList(PlotLine plotLine)
        {
            if (plotLine != null)
            {
                //check not already in list
                if (listOfPlotLines.Exists(x => x.refTag.Equals(plotLine.refTag, StringComparison.Ordinal)) == false)
                { listOfPlotLines.Add(plotLine); }
                else { Debug.LogWarningFormat("Plotline refTag \"{0}\" alread present in list", plotLine.refTag); }
            }
            else { Debug.LogError("Invalid plotLine (Null)"); }
        }


        #endregion
    }
    #endregion

    #region TurningPoint
    /// <summary>
    /// A story can have up to 5 turningPoints with each having up to 5 plotpoints
    /// </summary>
    [System.Serializable]
    public class TurningPoint
    {
        public string refTag;
        public string tag;
        public string notes;
        public TurningPointType type;
        public bool isConcluded;                                    //if true then Turning point is complete and no more plotpoints can be generated
        public PlotDetails[] arrayOfDetails = new PlotDetails[5];

        public TurningPoint()
        {
            //initialise arrayOfDetails with blanks
            for (int i = 0; i < arrayOfDetails.Length; i++)
            { arrayOfDetails[i] = new PlotDetails(); }
        }


        public void Reset()
        {
            refTag = "";
            tag = "";
            notes = "";
            type = TurningPointType.None;
            isConcluded = false;
            for (int i = 0; i < arrayOfDetails.Length; i++)
            { arrayOfDetails[i].Reset(); }
        }
    }
    #endregion

    #region PlotDetails
    /// <summary>
    /// Each turning point consists of up to five PlotDetails which are all relevant details combined
    /// </summary>
    [System.Serializable]
    public class PlotDetails
    {
        public bool isActive;                   //quick check to see if active plotpoint
        public string turningPoint;             //turningPoint.refTag
        public string plotPoint;                //plotPoint.refTag
        public string plotPointNotes;
        public Character character1;
        public Character character2;
        public string notes;


        public void Reset()
        {
            isActive = false;
            turningPoint = "";
            plotPoint = "";
            plotPointNotes = "";
            notes = "";
            character1 = null;
            character2 = null;
        }
    }
    #endregion

    #region PlotLine
    /// <summary>
    /// Plotline class (Story.cs) A plotline is an ongoing Theme running through the adventure
    /// </summary>
    [System.Serializable]
    public class PlotLine
    {
        public string refTag;                               //plotLine name with no spaces (auto generated)
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
        public string refTag;
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

    #region Plotpoint
    /// <summary>
    /// Individual Plotpoint (a Turning Point scene is made up of multiple plotpoints)
    /// NOTE: Not Serializable (only the refTag is saved elsewhere as a key to the dictOfPlotLines that is hardcoded and loaded at start
    /// </summary>
    public class Plotpoint
    {
        public string refTag;                       //single string reference tag used for dictionaries, lookup tables, etc
        public string tag;
        public string details;
        public PlotPointType type;
        public int numberOfCharacters;              //number of characters involved
        public List<int> listAction;                //die roll numbers, leave list empty for none
        public List<int> listTension;
        public List<int> listMystery;
        public List<int> listSocial;
        public List<int> listPersonal;
    }
    #endregion

    #region MetaPlotpoint
    /// <summary>
    /// Meta Plotpoint (a Turning Point scene is made up of multiple plotpoints)
    /// NOTE: Not Serializable (only the refTag is saved elsewhere as a key to the dictOfPlotLines that is hardcoded and loaded at start
    /// </summary>
    public class MetaPlotpoint
    {
        public string refTag;                       //single string reference tag used for dictionaries, lookup tables, etc
        public string tag;
        public string details;
        public MetaAction action;
        public List<int> listToRoll;                //die roll numbers, leave list empty for none
    }
    #endregion

    #region CharacterIndentity
    /// <summary>
    /// Character identity
    /// </summary>
    public class CharacterIdentity
    {
        public string tag;
        public List<int> listToRoll;
        public bool isRollAgain;                  //if true then roll twice
    }
    #endregion

    #region CharacterDescriptor
    /// <summary>
    /// Character Descriptor
    /// </summary>
    public class CharacterDescriptor
    {
        public string tag;
        public List<int> listToRoll;
        public bool isRollAgain;                  //if true then roll twice
    }
    #endregion

    #region CharacterSpecial
    /// <summary>
    /// Character Special Trait
    /// </summary>
    public class CharacterSpecial
    {
        public string tag;
        public List<int> listToRoll;
        public string details;
    }
    #endregion
}
#endif
