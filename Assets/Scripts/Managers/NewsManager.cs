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
    [Range(1, 10)] public int timerMaxItemTurns = 2;
    [Tooltip("How many newsItems will be selected (if available) per turn. Also gives identical number of Adverts")]
    [Range(0, 3)] public int numOfNewsItems = 1;
    /*[Tooltip("How many adverts will be selected (if available) per turn")]
    [Range(0, 3)] public int numOfAdverts = 1;*/

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
                Debug.LogWarningFormat("Unrecognised GameState \"{0}\" (Initialise)", GameManager.i.inputScript.GameState);
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
        GameManager.i.dataScript.InitialiseAdvertList();
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
        EventManager.i.AddListener(EventType.StartTurnLate, OnEvent, "NewsManager");
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
        if (GameManager.i.turnScript.CheckIsAutoRun() == false)
        {
            List<NewsItem> listOfNewsItems = GameManager.i.dataScript.GetListOfNewsItems();
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
                /*Debug.LogFormat("[New] NewsManager.cs -> UpdateNewsItems: There are {0} newsItems available, {1} have been deleted (expired){2}", listOfNewsItems.Count, counter, "\n");*/
            }
            else { Debug.LogError("Invalid listOfNewsItems (Null)"); }
        }
    }

    /// <summary>
    /// returns single newsFeed string (splices together selected newsItems). Sequence is NewsItem + Advert repeated (1 of each)
    /// </summary>
    /// <returns></returns>
    public string GetNews()
    {
        int indexNews, countNews, limit;
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
        List<NewsItem> listOfNewsItems = GameManager.i.dataScript.GetListOfNewsItems();
        if (listOfNewsItems != null)
        {
            countNews = listOfNewsItems.Count;
            if (countNews > 0)
            {
                //get num required per turn, capped by number available
                limit = Mathf.Min(countNews, numOfNewsItems);
                for (int i = 0; i < limit; i++)
                {
                    //randomly select item from list
                    indexNews = Random.Range(0, listOfNewsItems.Count);
                    newsSnippet = listOfNewsItems[indexNews].text;
                    listOfCurrentNews.Add(newsSnippet);
                    if (string.IsNullOrEmpty(newsSnippet) == false)
                    {
                        if (builder.Length > 0) { builder.Append(splicer); }
                        builder.Append(newsSnippet);

                        /*//get Advert              NOTE: adverts now show on billboards, if you want to rejig you'll need to add the string variable 'advert'
                        advert = GetAdvert();
                        //add to news
                        if (builder.Length > 0) { builder.Append(splicer); }
                        builder.Append(advert);*/

                    }
                    else { Debug.LogWarningFormat("Invalid newsItem newsSnippet (Null or Empty) for listOfNewsItem[{0}]", indexNews); }
                    //delete newsItem from list to prevent dupes
                    listOfNewsItems.RemoveAt(indexNews);
                }
            }
            else
            {
                builder.AppendFormat("{0}City News Blackout in force. Strikes at the Server Farm", splicer);
                listOfCurrentNews.Add("City News Blackout in force. Strikes at the Server Farm");
            }
        }
        else { Debug.LogError("Invalid listOfNewsItems (Null)"); }
        //fail safe
        if (builder.Length == 0) { builder.Append("News BlackOut in place"); }
        return builder.ToString();
    }

    /// <summary>
    /// Returns a random advertisement, deletes from list, reinitialises list if empty
    /// </summary>
    /// <returns></returns>
    public string GetAdvert()
    {
        int countAdvert, indexAdvert;
        string advert = "Unknown";
        List<string> listOfAdverts = GameManager.i.dataScript.GetListOfAdverts();
        if (listOfAdverts != null)
        {
            countAdvert = listOfAdverts.Count;
            if (countAdvert == 0)
            {
                //reinitialise
                GameManager.i.dataScript.InitialiseAdvertList();
                countAdvert = listOfAdverts.Count;
            }
            if (countAdvert > 0)
            {
                indexAdvert = Random.Range(0, listOfAdverts.Count);
                advert = listOfAdverts[indexAdvert];
                listOfCurrentAdverts.Add(advert);
                //check for invalid advert
                if (string.IsNullOrEmpty(advert) == true)
                { Debug.LogWarningFormat("Invalid advert (Null or Empty) for listOfAdverts[{0}]", indexAdvert); }
                //delete Advert from list to prevent dupes
                listOfAdverts.RemoveAt(indexAdvert);
            }
            else { Debug.LogWarning("Invalid listOfAdverts (CountAdvert is Zero AFTER Reinitialising)"); }
        }
        else { Debug.LogError("Invalid listOfAdverts (Null)"); }
        return advert;
    }


    public List<string> GetListOfCurrentNews()
    { return listOfCurrentNews; }

    public List<string> GetListOfCurrentAdverts()
    { return listOfCurrentAdverts; }

    /// <summary>
    /// Replaces text tags on NON-TOPIC News texts. Different to CheckTopicText but uses identical tags. Pass parameters in via a data package. No colour formatting involved. Returns null if a problem
    /// </summary>
    /// <param name="text"></param>
    /// <param name="isColourHighlighting"></param>
    /// <param name="isValidation"></param>
    /// <param name="objectName"></param>
    /// <returns></returns>
    public string CheckNewsText(CheckTextData data)
    {
        string checkedText = null;
        if (data != null)
        {
            if (string.IsNullOrEmpty(data.text) == false)
            {
                checkedText = data.text;
                string tag, replaceText;
                int tagStart, tagFinish, length; //indexes
                                                 //loop while ever tags are present
                while (checkedText.Contains("[") == true)
                {
                    tagStart = checkedText.IndexOf("[");
                    tagFinish = checkedText.IndexOf("]");
                    length = tagFinish - tagStart;
                    tag = checkedText.Substring(tagStart + 1, length - 1);
                    //strip brackets
                    replaceText = null;
                    switch (tag)
                    {
                        case "actor":
                            //actor arc name
                            if (data.isValidate == false)
                            {
                                if (data.actorID > -1)
                                {
                                    Actor actor = GameManager.i.dataScript.GetActor(data.actorID);
                                    if (actor != null)
                                    { replaceText = actor.arc.name; }
                                    else { Debug.LogWarningFormat("Invalid actor (Null) for actorID \"{0}\"", data.actorID); }
                                }
                                else
                                { Debug.LogWarningFormat("Invalid actorID \"{0}\" for tag [actor]", data.actorID); }
                            }
                            break;
                        case "player":
                            if (data.isValidate == false)
                            { replaceText = GameManager.i.playerScript.PlayerName; }
                            break;
                        case "node":
                            if (data.isValidate == false)
                            {
                                if (data.node != null)
                                { replaceText = data.node.nodeName; }
                            }
                            break;
                        case "nodeArc":
                            if (data.isValidate == false)
                            {
                                if (data.node != null)
                                { replaceText = data.node.Arc.name; }
                            }
                            break;
                        case "npc":
                            if (data.isValidate == false)
                            {
                                if (Random.Range(0, 100) < 50) { replaceText = GameManager.i.cityScript.GetCity().country.nameSet.firstFemaleNames.GetRandomRecord(); }
                                else { replaceText = GameManager.i.cityScript.GetCity().country.nameSet.firstMaleNames.GetRandomRecord(); }
                                replaceText += " " + GameManager.i.cityScript.GetCity().country.nameSet.lastNames.GetRandomRecord();
                            }
                            break;
                        case "npcIs":
                            //npc is/was something
                            if (data.isValidate == false)
                            { replaceText = GameManager.i.topicScript.textListNpcSomething.GetRandomRecord(); }
                            break;
                        case "handicap":
                            //npc has a handicap
                            if (data.isValidate == false)
                            { replaceText = GameManager.i.topicScript.textListHandicap.GetRandomRecord(); }
                            break;
                        case "job":
                            //Random job name appropriate to node arc
                            if (data.isValidate == false)
                            {
                                string job = "Unknown";
                                if (data.node != null)
                                {
                                    ContactType contactType = data.node.Arc.GetRandomContactType();
                                    if (contactType != null)
                                    { job = contactType.pickList.GetRandomRecord(); }
                                    else
                                    { Debug.LogWarningFormat("Invalid contactType (Null) for node {0}, {1}, {2} for object {3}", data.node.nodeName, data.node.Arc.name, data.node.nodeID, data.objectName); }
                                }
                                else { Debug.LogWarningFormat("Invalid node (Null) for [job] tag for object \"{0}\"", data.objectName); }
                                replaceText = job;
                            }
                            break;
                        case "genLoc":
                            //Random Generic Location
                            if (data.isValidate == false)
                            {
                                string location = "Unknown";
                                location = GameManager.i.topicScript.textlistGenericLocation.GetRandomRecord();
                                replaceText = location;
                            }
                            break;
                        case "mayor":
                            //mayor + first name
                            if (data.isValidate == false)
                            { replaceText = GameManager.i.cityScript.GetCity().mayor.mayorName; }
                            break;
                        case "city":
                            //city name
                            if (data.isValidate == false)
                            { replaceText = GameManager.i.cityScript.GetCity().name; }
                            break;
                        case "citys":
                            //city name plural
                            if (data.isValidate == false)
                            { replaceText = string.Format("{0}'s", GameManager.i.cityScript.GetCity().name); }
                            break;
                        case "who":
                            //My '[best friend]'s [crazy] [sister]' 
                            if (data.isValidate == false)
                            {
                                replaceText = string.Format("{0}'s {1} {2}", GameManager.i.topicScript.textListWho0.GetRandomRecord(), GameManager.i.topicScript.textListCondition.GetRandomRecord(),
                                  GameManager.i.topicScript.textListWho1.GetRandomRecord());
                            }
                            break;
                        default:
                            if (data.isValidate == false)
                            { Debug.LogWarningFormat("Unrecognised tag \"{0}\"", tag); }
                            else { Debug.LogFormat("[Val] NewsManager.cs -> CheckNewsText: Unrecognised tag \"{0}\" for object {1}", tag, data.objectName); }
                            break;
                    }
                    //catch all
                    if (replaceText == null) { replaceText = "Unknown"; }
                    //swap tag for text
                    checkedText = checkedText.Remove(tagStart, length + 1);
                    checkedText = checkedText.Insert(tagStart, replaceText);
                }
            }
            else { Debug.LogWarning("Invalid text (Null or Empty)"); }
        }
        else { Debug.LogError("Invalid data (Null)"); }
        return checkedText;
    }

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
        List<NewsItem> listOfNewsItems = GameManager.i.dataScript.GetListOfNewsItems();
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

    /// <summary>
    /// debug display of listOfAdverts
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayAdverts()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- listOfAdverts{0}", "\n");
        List<string> listOfAdverts = GameManager.i.dataScript.GetListOfAdverts();
        if (listOfAdverts != null)
        {
            int count = listOfAdverts.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                { builder.AppendFormat(" {0}{1}", listOfAdverts[i], "\n"); }
            }
            else { builder.AppendFormat("{0} no records found", "\n"); }
        }
        else { Debug.LogWarning("Invalid listOfAdverts (Null)"); }
        return builder.ToString();
    }

    /// <summary>
    /// debug display of listOfBillboards (billboard pool yet to be displayed)
    /// </summary>
    /// <returns></returns>
    public string DebugDisplayBillboards()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("- listOfBillboardSeen{0}", "\n");
        List<string> listOfBillboards = GameManager.i.dataScript.GetListOfBillboardsSeen();
        if (listOfBillboards != null)
        {
            int count = listOfBillboards.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                { builder.AppendFormat(" {0}{1}", listOfBillboards[i], "\n"); }
            }
            else { builder.AppendFormat("{0} no records found", "\n"); }
        }
        else { Debug.LogWarning("Invalid listOfBillboardSeen (Null)"); }
        return builder.ToString();
    }

    //new methods above here
}
