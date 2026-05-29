using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using FishNet;

/*
Keeps track of current season, Day of the month, Season length
currentSeaon and seasonDay need to be saved
*/
public partial class ServerWorld: MonoBehaviour
{
    #region Variables
    private int seasonDay; //used in Update() in ServerWorld
    private float prePercent = 0f; 
    private float postPercent = 0f;
    public SeasonState CURRENTSEASON {get; private set;}
    private SeasonState newSeason;
    protected List<KeyValuePair<string, SeasonState>> sortedSeasons = new List<KeyValuePair<string, SeasonState>>();
    #endregion

    #region Internal methods
    public void SeasonSort() //sorts all of the season data into a sorted list of KVP for GetAdjacent
    {
        sortedSeasons = _seasonData
            .OrderBy(kvp => kvp.Value.position) //orderby my beloved
            .ToList();
    }

    private (SeasonState previousSeason, SeasonState postSeason) GetAdjacent() //helps navigate seasons, assume sorted list based on position
    {
        int currentSeasonPosition = sortedSeasons.FindIndex(s => s.Value.position == CURRENTSEASON.position); //fixed to handle a list with gaps 
        int count = sortedSeasons.Count;

        int prevSeason = (currentSeasonPosition - 1 + count) % count;
        int postSeason= (currentSeasonPosition + 1) % count;

        return (sortedSeasons[prevSeason].Value, sortedSeasons[postSeason].Value); //return both seasons, DO NOT CALL EVERY FRAME, just once per season
    }

    private void SeasonIncrementer () //double check logic
    {
        seasonDay = 0;
        SeasonState newSeason;
        (_,newSeason)= GetAdjacent(); //only grabs post season
        SetSeason(newSeason);
    }

    public (float weight,bool prev) SeasonInterpolator (int seasonDate) //Helper function that returns weight and determines if its post or pre season
    { 
        float halfSeason = WorldConstants.SEASON_LENGTH / 2f;
        float seasonLength = WorldConstants.SEASON_LENGTH;

        float distance = seasonDate - halfSeason;
        float weight = Math.Abs(distance) / halfSeason; 

        bool prev = distance < 0;
        return (weight , prev); //gives back the weight of the other, non-current, season and labels its direction
    }

    public SeasonState SeasonGrabber(SeasonWrapper data, string targetSeason)//utility
    {
        if(data.season_Profiles.TryGetValue(targetSeason, out SeasonState state))
        {
            return state;
        }
        else
        {
            Debug.Log($"SeasonGrabber:{targetSeason} does not exist");
            return null;
        }
    }

    #endregion
    
    #region Outgoing 

    public void SetSeason(SeasonState newSeason) 
    {
        CURRENTSEASON = newSeason;
        Debug.Log($"The Current Season Is: {CURRENTSEASON}");
        BroadcastSeasonUpdate();
    }

    private void BroadcastSeasonUpdate() 
    {
        if (!global::ServerManager.HasInstance) return;

        var networkManager = InstanceFinder.NetworkManager;
        if (networkManager == null) return;

        SeasonUpdateMessage msg = new SeasonUpdateMessage
        {
            CurrentSeason = CURRENTSEASON
        };

        networkManager.ServerManager.Broadcast(msg, false);
        Debug.Log("Season Broadcasted!");
    }
    #endregion
}
