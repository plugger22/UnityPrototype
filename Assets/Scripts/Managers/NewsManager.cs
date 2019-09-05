﻿using gameAPI;
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

    #region Save Compatible Data
    [HideInInspector] public int newsIDCounter = 0;                            //used to sequentially number newsID's
    #endregion


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
                        builder.AppendFormat(" id {0}, t {1}, {2}", item.newsID, item.timer, item.text.Substring(0, 50));
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
                    else { builder.AppendFormat(" id {0}, t {1}, {2}", item.newsID, item.timer, item.text); }
                    builder.AppendLine();
                }
                else { Debug.LogWarningFormat("Invalid newsItem (Null) for listOfNewsItems[{0}]", i); }
            }
        }
        return builder.ToString();
    }

    //new methods above here
}
