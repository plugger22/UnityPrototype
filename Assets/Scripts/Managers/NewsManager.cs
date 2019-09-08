using gameAPI;
using packageAPI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


/// <summary>
/// handles news generation for the Info App ticker tape
/// </summary>
public class NewsManager : MonoBehaviour
{
    [Header("News Items")]
    [Tooltip("How many turns a given newsItem will stay in the selection pool before being deleted (if it's selected then it's also deleted)")]
    [Range(1, 10)] public int timerMaxItemTurns = 4;
    [Tooltip("How many newsItems will be selected (if available) per turn")]
    [Range(0, 3)] public int numOfNewsItems = 1;
    [Tooltip("How many adverts will be selected (if available) per turn")]
    [Range(0, 3)] public int numOfAdverts = 1;

    /*#region Save Compatible Data
    [HideInInspector] public int newsIDCounter = 0;                             //used to sequentially number newsID's
    #endregion*/

    private List<string> listOfCurrentNews = new List<string>();                       //news feed cut up into individual News snippets with the last record always being an advert (excludes Adverts)
    private List<string> listOfCurrentAdverts = new List<string>();                    //news feed cut up into individual Advert snippets with the last record always being an advert (excludes News)

    /// <summary>
    /// Initialisation
    /// </summary>
    /// <param name="state"></param>
    public void Initialise(GameState state)
    {
        switch (state)
        {
            case GameState.StartUp:
            case GameState.NewInitialisation:
                SubInitialiseNewGame();
                SubInitialiseFastAccess(); //needs to be first
                SubInitialiseStartUp();
                SubInitialiseLevelStart();
                SubInitialiseEvents();
                break;
            case GameState.FollowOnInitialisation:
                SubInitialiseLevelStart();
                break;
            case GameState.LoadAtStart:
                SubInitialiseFastAccess();
                SubInitialiseStartUp();
                SubInitialiseEvents();
                break;
            case GameState.LoadGame:
                //do nothing
                break;
            default:
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\" (Initialise)", GameManager.instance.inputScript.GameState);
                break;
        }
    }

    #region Initialisation SubMethods

    #region SubInitialiseStartUp
    private void SubInitialiseStartUp()
    {
    }
    #endregion

    #region SubInitialiseNewGame
    /// <summary>
    /// Start new game/campaign
    /// </summary>
    private void SubInitialiseNewGame()
    {
        //set up adverts for ticker text
        GameManager.instance.dataScript.InitialiseAdvertList();
    }
    #endregion

    #region SubInitialiseLevelStart
    private void SubInitialiseLevelStart()
    {
    }
    #endregion

    #region SubInitialiseFastAccess
    private void SubInitialiseFastAccess()
    {
    }
    #endregion

    #region SubInitialiseEvents
    private void SubInitialiseEvents()
    {
        //event listener
        EventManager.instance.AddListener(EventType.StartTurnLate, OnEvent, "NewsManager");
    }
    #endregion

    #endregion


    /// <summary>
    /// Event handler
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EventType eventType, Component Sender, object Param = null)
    {
        //Detect event type
        switch (eventType)
        {
            case EventType.StartTurnLate:
                UpdateNewsItems();
                break;
            default:
                Debug.LogError(string.Format("Invalid eventType {0}{1}", eventType, "\n"));
                break;
        }
    }

    /// <summary>
    /// late turn check that checks listOfNewsItems, decrements timers and deletes any expired items
    /// </summary>
    private void UpdateNewsItems()
    {
        //not if autoRun
        if (GameManager.instance.turnScript.CheckIsAutoRun() == false)
        {
            List<NewsItem> listOfNewsItems = GameManager.instance.dataScript.GetListOfNewsItems();
            if (listOfNewsItems != null)
            {
                int counter = 0;
                //loop in reverse (may have to delete)
                for (int index = listOfNewsItems.Count - 1; index >= 0; index--)
                {
                    NewsItem item = listOfNewsItems[index];
                    item.timer--;
                    if (item.timer <= 0)
                    {
                        counter++;
                        listOfNewsItems.RemoveAt(index);
                    }
                }
                Debug.LogFormat("[New] NewsManager.cs -> UpdateNewsItems: There are {0} newsItems available, {1} have been deleted (expired){2}", listOfNewsItems.Count, counter, "\n");
            }
            else { Debug.LogError("Invalid listOfNewsItems (Null)"); }
        }
    }

    /// <summary>
    /// returns single newsFeed string (splices together selected newsItems)
    /// </summary>
    /// <returns></returns>
    public string GetNews()
    {
        int index, count, limit;
        string newsSnippet;
        string splicer = " ... Updating ... ";
        listOfCurrentNews.Clear();
        listOfCurrentAdverts.Clear();
        StringBuilder builder = new StringBuilder();
        //put a header to prevent the ticker start being already out of view on opening the MainInfoApp
        builder.AppendFormat("{0}Latest News Feed", splicer);
        //
        // - - - News Items
        //
        List<NewsItem> listOfNewsItems = GameManager.instance.dataScript.GetListOfNewsItems();
        if (listOfNewsItems != null)
        {
            count = listOfNewsItems.Count;
            if (count > 0)
            {
                //get num required per turn, capped by number available
                limit = Mathf.Min(count, numOfNewsItems);
                for (int i = 0; i < limit; i++)
                {        
                    //randomly select item from list
                    index = Random.Range(0, count);
                    newsSnippet = listOfNewsItems[index].text;
                    listOfCurrentNews.Add(newsSnippet);
                    if (string.IsNullOrEmpty(newsSnippet) == false)
                    {
                        if (builder.Length > 0) { builder.Append(splicer); }
                        builder.Append(newsSnippet);
                    }
                    else { Debug.LogWarningFormat("Invalid newsItem newsSnippet (Null or Empty) for listOfNewsItem[{0}]", index); }
                    //delete newsItem from list to prevent dupes
                    listOfNewsItems.RemoveAt(index);
                }
            }
            else { builder.AppendFormat("{0}City News Blackout in place. Strikes at the Server Farm", splicer); }
        }
        else { Debug.LogError("Invalid listOfNewsItems (Null)"); }
        //
        // - - - Adverts
        //
        List<string> listOfAdverts = GameManager.instance.dataScript.GetListOfAdverts();
        if (listOfAdverts != null)
        {
            count = listOfAdverts.Count;
            if (count > 0)
            {
                //get num required per turn, capped by number available
                limit = Mathf.Min(count, numOfAdverts);
                for (int i = 0; i < limit; i++)
                {
                    //randomly select item from list
                    index = Random.Range(0, count);
                    newsSnippet = listOfAdverts[index];
                    listOfCurrentAdverts.Add(newsSnippet);
                    if (string.IsNullOrEmpty(newsSnippet) == false)
                    {
                        if (builder.Length > 0) { builder.Append(splicer); }
                        builder.Append(newsSnippet);
                    }
                    else { Debug.LogWarningFormat("Invalid Advert newsSnippet (Null or Empty) for listOfAdverts[{0}]", index); }
                    //delete Advert from list to prevent dupes
                    listOfAdverts.RemoveAt(index);
                }
            }
            //check if list empty and needs to be reinitialised
            if (listOfAdverts.Count == 0)
            { GameManager.instance.dataScript.InitialiseAdvertList(); }
        }
        else { Debug.LogError("Invalid listOfAdverts (Null)"); }
        //fail safe
        if (builder.Length == 0) { builder.Append("News BlackOut in force"); }
        return builder.ToString();
    }


    public List<string> GetListOfCurrentNews()
    { return listOfCurrentNews; }

    public List<string> GetListOfCurrentAdverts()
    { return listOfCurrentAdverts; }

    //
    // - - - Debug - - -
    //

    /// <summary>
    /// Debug display of all news items currently available for selection
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayNewsItems()
    {
        int count, length, subLength;
        List<NewsItem> listOfNewsItems = GameManager.instance.dataScript.GetListOfNewsItems();
        StringBuilder builder = new StringBuilder();
        if (listOfNewsItems != null)
        {
            count = listOfNewsItems.Count;
            builder.AppendFormat("- News Items  ({0} record{1}){2}", count, count != 1 ? "s" : "", "\n");
            for (int i = 0; i < count; i++)
            {
                NewsItem item = listOfNewsItems[i];
                if (item != null)
                {
                    //split text over two lines if overlength
                    length = item.text.Length;
                    subLength = 0;
                    if (length > 50)
                    {
                        builder.AppendFormat(" t {0}, {1}", item.timer, item.text.Substring(0, 50));
                        do
                        {
                            subLength += 50;
                            if ((length - subLength) < 50)
                            {
                                //last segment
                                builder.AppendFormat("{0}  ...{1}", "\n", item.text.Substring(subLength));
                                break;
                            }
                            else { builder.AppendFormat("{0}  ...{1}...", "\n", item.text.Substring(subLength, 50)); }
                        }
                        while (subLength < length);
                    }
                    else { builder.AppendFormat(" t {0}, {1}", item.timer, item.text); }
                    builder.AppendLine();
                }
                else { Debug.LogWarningFormat("Invalid newsItem (Null) for listOfNewsItems[{0}]", i); }
            }
        }
        return builder.ToString();
    }

    //new methods above here
}
