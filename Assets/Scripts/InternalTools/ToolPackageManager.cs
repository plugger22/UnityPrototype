using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public enum ToolModalSubNew { New, Summary }                                        //new adventure sub state
    public enum ThemeType { Action, Tension, Mystery, Social, Personal, Count }   //NOTE: Order matters (ToolDetails.cs)
    public enum StoryStatus { New, Logical, Data }
    public enum ListItemStatus { None, PlotLine, Character }    //what's currently selected on the Aventure/list page
    public enum PlotPointType { Normal, Conclusion, None, RemoveCharacter, NewCharacter, Meta }
    public enum MetaAction { CharacterExits, CharacterReturns, CharacterUpgrade, CharacterDowngrade, CharacterStepsUp, CharacterStepsDown, PlotLineCombo }
    public enum TurningPointType { None, New, Development, Conclusion }
    public enum CharacterSex { None, Male, Female }




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
        public string refTag;
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
        /// <param name="copy"></param>
        public Story(Story copy)
        {
            refTag = copy.refTag;
            tag = copy.tag;
            notes = copy.notes;
            date = copy.date;
            numTurningPoints = copy.numTurningPoints;
            theme = new ThemeData(copy.theme);
            arrays = new StoryArrays(copy.arrays);
            for (int i = 0; i < arrayOfTurningPoints.Length; i++)
            { arrayOfTurningPoints[i] = copy.arrayOfTurningPoints[i]; }
            lists = new StoryLists(copy.lists);
        }

        /// <summary>
        /// Reset data
        /// </summary>
        public void Reset()
        {
            refTag = "";
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
            PopulateArrays();
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
            PopulateArrays();
        }

        /// <summary>
        /// get a Random plotLine ListItem from array. Returns null if a problem
        /// </summary>
        /// <returns></returns>
        public ListItem GetRandomPlotLineFromArray()
        {
            ListItem item = null;
            int rnd = Random.Range(0, 25);
            item = arrayOfPlotLines[rnd];
            return item;
        }

        /// <summary>
        /// get a Random character ListItem from array. Returns null if a problem.
        /// </summary>
        /// <returns></returns>
        public ListItem GetRandomCharacterFromArray()
        {
            ListItem item = null;
            int rnd = Random.Range(0, 25);
            item = arrayOfCharacters[rnd];
            return item;
        }

        /// <summary>
        /// returns number of active data entries (listItem.status = Data, listItem.tag != null or MT)
        /// </summary>
        /// <returns></returns>
        public int CheckDataItemsInArray()
        {
            int count = 0;
            for (int i = 0; i < arrayOfCharacters.Length; i++)
            {
                if (arrayOfCharacters[i].status == StoryStatus.Data)
                {
                    if (string.IsNullOrEmpty(arrayOfCharacters[i].tag) == false)
                    { count++; }
                }
            }
            return count;
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
                        int count = arrayOfCharacters.Where(x => x.tag.Equals(newItem.tag, StringComparison.Ordinal)).Count();
                        if (count < 3)
                        {
                            arrayOfCharacters[i] = newItem;
                            Debug.LogFormat("[Tst] StoryArrays.cs -> AddCharacterToArray: {0}, {1} ADDED to arrayOfCharacters (count now {2}){3}", newItem.tag, newItem.status, count + 1, "\n");
                            Debug.LogFormat("[Tst] StoryArrays.cs -> arrayOfCharacters - - - {0}", "\n");
                            for (int j = 0; j < arrayOfCharacters.Length; j++)
                            {
                                if (arrayOfCharacters[j].status == StoryStatus.Data)
                                { Debug.LogFormat("[Tst] -> index {0} -> \"{1}\"{2}", j, arrayOfCharacters[j].tag, "\n"); }
                            }
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
                        int count = arrayOfPlotLines.Where(x => x.tag.Equals(newItem.tag, StringComparison.Ordinal)).Count();
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
        private void PopulateArrays()
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

        /// <summary>
        /// resets the specific index entry in the arrayOfCharacters back to it's default status (eg. New or Logical)
        /// </summary>
        /// <param name="index"></param>
        public void SetCharacterArrayItemToDefault(int index)
        {
            switch (index)
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
                    arrayOfCharacters[index] = new ListItem() { tag = "", status = StoryStatus.New };
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
                    arrayOfCharacters[index] = new ListItem() { tag = "", status = StoryStatus.Logical };
                    break;
                default: Debug.LogWarningFormat("Unrecognised counter \"{0}\" for Characters", index); break;
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
        /// Returns specific Character based on list index (used for DropDown input)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Character GetCharacterFromList(int index)
        {
            Character character = null;
            if (index > -1 && index < listOfCharacters.Count)
                character = listOfCharacters[index];
            else { Debug.LogErrorFormat("Invalid index \"{0}\" (must be between 0 and {1})", index, listOfCharacters.Count); }
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
                {
                    listOfCharacters.Add(character);
                    Debug.LogFormat("[Tst] StoryLists.cs -> AddCharacterToList: \"{0}\", refTag {1} Added to List{2}", character.tag, character.refTag, "\n");
                    Debug.LogFormat("[Tst] StoryLists.cs -> AddCharacterToList: listOfCharacters - - -{0}", "\n");
                    for (int i = 0; i < listOfCharacters.Count; i++)
                    { Debug.LogFormat("[Tst] StoryLists.cs -> AddCharacterToList: index {0} -> \"{1}\"{2}", i, listOfCharacters[i].tag, "\n"); }
                }
                else { Debug.LogWarningFormat("Character refTag \"{0}\" alread present in list -> Info Only", character.refTag); }
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
                {
                    listOfPlotLines.Add(plotLine);
                    Debug.LogFormat("[Tst] StoryLists.cs -> AddPlotLineToList: \"{0}\", refTag {1} Added to List{2}", plotLine.tag, plotLine.refTag, "\n");
                }
                else { Debug.LogWarningFormat("Plotline refTag \"{0}\" already present in list -> Info Only", plotLine.refTag); }
            }
            else { Debug.LogError("Invalid plotLine (Null)"); }
        }

        /// <summary>
        /// Remove a characer from list
        /// </summary>
        /// <param name="character"></param>
        public void RemoveCharacterFromList(Character character)
        {
            if (character != null)
            {
                //should be exactly one entry on list
                int counter = 0;
                //remove entry from list
                for (int i = listOfCharacters.Count - 1; i >= 0; i--)
                {
                    if (listOfCharacters[i].refTag.Equals(character.refTag, StringComparison.Ordinal) == true)
                    {
                        listOfCharacters.RemoveAt(i);
                        counter++;
                    }
                }
                Debug.LogFormat("[Tst] StoryList.cs -> RemoveCharacterFromList: {0} record{1} of \"{2}\" have been REMOVED from listOfCharacters{3}", counter, counter != 1 ? "s" : "", character.tag, "\n");
                Debug.LogFormat("[Tst] StoryLists.cs -> RemoveCharacterFromList: listOfCharacters - - -{0}", "\n");
                for (int i = 0; i < listOfCharacters.Count; i++)
                { Debug.LogFormat("[Tst] StoryLists.cs -> RemoveCharacterFromList: index {0} -> \"{1}\"{2}", i, listOfCharacters[i].tag, "\n"); }
                if (counter == 0)
                { Debug.LogWarning("Invalid counter (Zero) should be at least one"); }
            }
            else { Debug.LogError("Invalid character (Null)"); }
        }

        /// <summary>
        /// Debug method to display listOfCharacters
        /// </summary>
        public void DebugShowCharacterList()
        {
            Debug.LogFormat("[Tst] StoryLists.cs -> listOfCharacters - - -{0}", "\n");
            for (int i = 0; i < listOfCharacters.Count; i++)
            { Debug.LogFormat("[Tst] StoryLists.cs -> index {0} -> \"{1}\"{2}", i, listOfCharacters[i].tag, "\n"); }
        }

        /// <summary>
        /// Remove a Plotline from list (has been concluded)
        /// </summary>
        /// <param name="plotLine"></param>
        public void RemovePlotLineFromList(string refTag)
        {
            if (string.IsNullOrEmpty(refTag) == false)
            {
                //should be exactly one entry on list
                int counter = 0;
                //remove entry from list
                for (int i = listOfPlotLines.Count - 1; i >= 0; i--)
                {
                    if (listOfPlotLines[i].refTag.Equals(refTag, StringComparison.Ordinal) == true)
                    {
                        listOfPlotLines.RemoveAt(i);
                        counter++;
                    }
                }
                Debug.LogFormat("[Tst] StoryList.cs -> RemovePlotLineFromList: {0} record{1} of \"{2}\" have been REMOVED from listOfPlotLines{3}", counter, counter != 1 ? "s" : "", refTag, "\n");
                Debug.LogFormat("[Tst] StoryLists.cs -> listOfPlotLines - - -{0}", "\n");
                for (int i = 0; i < listOfPlotLines.Count; i++)
                { Debug.LogFormat("[Tst] StoryLists.cs -> index {0} -> \"{1}\"{2}", i, listOfPlotLines[i].tag, "\n"); }
                if (counter == 0)
                { Debug.LogWarning("Invalid counter (Zero) should be at least one"); }
            }
            else { Debug.LogError("Invalid plotLine refTag (Null or Empty)"); }
        }

        #endregion
    }
    #endregion

    #region TurningPoint
    /// <summary>
    /// A story can have up to 5 turningPoints with each having up to 5 plotpoints, Turningpoints are just numbered scenes whereas PlotLines are the named elements
    /// </summary>
    [System.Serializable]
    public class TurningPoint
    {
        public string refTag;                                       //PlotLine refTag
        public string tag;                                          //PlotLine tag
        public string notes;                                        //notes for the plotLine specific to this TurningPoint
        public TurningPointType type;                               //last in the series? (maxCap of 5 if not concluded before)
        /*public bool isConcluded;                                    //if true then Turning point is complete and no more plotpoints can be generated*/
        public PlotDetails[] arrayOfDetails = new PlotDetails[5];

        #region TurnPoint Methods
        /// <summary>
        /// default constructor
        /// </summary>
        public TurningPoint()
        {
            type = TurningPointType.None;
            //initialise arrayOfDetails with blanks
            for (int i = 0; i < arrayOfDetails.Length; i++)
            { arrayOfDetails[i] = new PlotDetails(); }
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy"></param>
        public TurningPoint(TurningPoint copy)
        {
            refTag = copy.refTag;
            tag = copy.tag;
            notes = copy.notes;
            type = copy.type;
            /*isConcluded = copy.isConcluded;*/
            for (int i = 0; i < arrayOfDetails.Length; i++)
            { arrayOfDetails[i] = copy.arrayOfDetails[i]; }
        }

        /// <summary>
        /// Reset
        /// </summary>
        public void Reset()
        {
            refTag = "";
            tag = "";
            notes = "";
            type = TurningPointType.None;
            /*isConcluded = false;*/
            for (int i = 0; i < arrayOfDetails.Length; i++)
            { arrayOfDetails[i].Reset(); }
        }

        /// <summary>
        /// returns number of specified plotPointtype, eg. 'None' in the current turningPoint.arrayOfDetails
        /// </summary>
        /// <param name="plotPointType"></param>
        /// <returns></returns>
        public int CheckNumberOfPlotPointType(PlotPointType plotPointType)
        { return arrayOfDetails.Where(x => x.type == plotPointType).Count(); }

        #endregion
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
        public string plotPoint;                //plotPoint.refTag
        public string notes;                    //notes for plotpoint
        public Character character1;
        public Character character2;
        public PlotPointType type;

        #region PlotDetails Methods
        /// <summary>
        /// default constructor
        /// </summary>
        public PlotDetails() { }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy"></param>
        public PlotDetails(PlotDetails copy)
        {
            isActive = copy.isActive;
            plotPoint = copy.plotPoint;
            notes = copy.notes;
            character1 = copy.character1;
            character2 = copy.character2;
            type = copy.type;
        }

        /// <summary>
        /// Reset
        /// </summary>
        public void Reset()
        {
            isActive = false;
            plotPoint = "";
            notes = "";
            character1 = null;
            character2 = null;
            type = PlotPointType.None;
        }
        #endregion
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
        public List<string> listOfNotes;                    //One entry for each instance of the PlotLine, eg. if 3 turning points refer to this plotLine then 3 sets of notes

        #region PlotLine methods
        /// <summary>
        /// Default constructor
        /// </summary>
        public PlotLine()
        {
            refTag = "";
            tag = "";
            listOfNotes = new List<string>();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy"></param>
        public PlotLine(PlotLine copy)
        {
            refTag = copy.refTag;
            tag = copy.tag;
            listOfNotes.Clear();
            listOfNotes.AddRange(copy.listOfNotes);
        }

        /// <summary>
        /// returns listOfNotes in a single string
        /// </summary>
        /// <returns></returns>
        public string GetNotes()
        {
            string notes = "";
            int count = listOfNotes.Count;
            if (count > 1)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < count; i++)
                {
                    if (builder.Length > 0) { builder.AppendLine(); builder.AppendLine(); }
                    builder.Append(listOfNotes[i]);
                }
                notes = builder.ToString();
            }
            else
            {
                if (count > 0)
                { notes = listOfNotes[0]; }
            }
            return notes;
        }

        #endregion
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
        public CharacterSex sex;
        public List<string> listOfNotes;

        #region Character Methods

        /// <summary>
        /// default constructor
        /// </summary>
        public Character()
        { listOfNotes = new List<string>(); }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="copy"></param>
        public Character(Character copy)
        {
            refTag = copy.refTag;
            tag = copy.tag;
            dataCreated = copy.dataCreated;
            listOfNotes.Clear();
            listOfNotes.AddRange(copy.listOfNotes);
            sex = copy.sex;
        }

        /// <summary>
        /// returns listOfNotes in a single string
        /// </summary>
        /// <returns></returns>
        public string GetNotes()
        {
            string notes = "";
            int count = listOfNotes.Count;
            if (count > 1)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < count; i++)
                {
                    if (builder.Length > 0) { builder.AppendLine(); builder.AppendLine(); }
                    builder.Append(listOfNotes[i]);
                }
                notes = builder.ToString();
            }
            else
            {
                if (count > 0)
                { notes = listOfNotes[0]; }
            }
            return notes;
        }
        #endregion
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

        /// <summary>
        /// default constructor
        /// </summary>
        public Plotpoint() { }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy"></param>
        public Plotpoint(Plotpoint copy)
        {
            refTag = copy.refTag;
            tag = copy.tag;
            details = copy.details;
            type = copy.type;
            numberOfCharacters = copy.numberOfCharacters;
            listAction = new List<int>(copy.listAction);
            listAction = new List<int>(copy.listTension);
            listAction = new List<int>(copy.listMystery);
            listAction = new List<int>(copy.listSocial);
            listAction = new List<int>(copy.listPersonal);
        }

        /// <summary>
        /// Reset
        /// </summary>
        public void Reset()
        {
            refTag = "";
            tag = "";
            details = "";
            type = PlotPointType.None;
            numberOfCharacters = 0;
        }


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
