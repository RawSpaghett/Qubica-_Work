using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using FishNet;
using System.Linq;
using UnityEngine.Assertions.Must;

//Handles the logic behind the weather calculations and broadcasts it to the client, also keeps track of current days weather

//NOTE TO SELF, replace Orderby()
public partial class ServerWorld: MonoBehaviour
{
    //access weather and season dicts from ModManager
    #region  Variables
    private WeatherState CURRENTWEATHER;
    private WeatherState nextWeather;
    Dictionary<string,float> WeighedOdds = new System.Collections.Generic.Dictionary<string,float>();
    Dictionary<string,float> NormalizedOdds = new System.Collections.Generic.Dictionary<string,float>();

    #endregion

    #region internal
   public float randomFloat() //gets random float for markovs
   {
        float randomValue = UnityEngine.Random.Range(0.01f, 0.99f); //change depending on final values
        return randomValue;
   }

    public void WeatherLinearInterpolator(float weight,SeasonState adjacent,string currentWeather, SeasonState currentSeason) // Interpolates the the weather matrix between two seasons IF they share the same weather
    {
        //holds references to the data we need, important to not change
        var currentMatch = currentSeason.weather_matrix.Find(w => w.currentWeather == currentWeather); 
        var adjacentMatch = adjacent.weather_matrix.Find(w => w.currentWeather == currentWeather); 

        if (currentMatch != null && adjacentMatch != null) //if the weather exists in both seasons
        {
            float interpolatedChance;
            float total = 0f;


            for (int i = 0; i < currentMatch.matrices.Count; i++)//loop through data in current season (more important)
            {
                var curData = currentMatch.matrices[i];
                var adjData = adjacentMatch.matrices.Find(w => w.weatherOption == curData.weatherOption);

                if (curData != null && adjData != null)//if the option exists in both
                {
                    float curChance = curData.weatherChance;
                    float adjChance = adjData.weatherChance;

                    interpolatedChance = Mathf.Lerp(curChance,adjChance,weight);
                    total += interpolatedChance;
                    WeighedOdds.Add(curData.weatherOption,interpolatedChance);
                }
                else //if it only exists in curData
                {
                    Debug.Log("Weather Option only exists in current season");
                    total += curData.weatherChance;
                    WeighedOdds.Add(curData.weatherOption,curData.weatherChance); //unweighed
                }
            }
            foreach(var chance in WeighedOdds) //keep under 1f
            {
                float normalized = chance.Value/total;
                NormalizedOdds.Add(chance.Key,normalized);
            }
        }
        else //if the weather only exists in one season, we dont need to interpolate anything, in theory we could but it is unneccesary for the scale of the project
        {
            return;
        }
    }

    public (WeatherState,SubWeatherState) MarkovsFunction(SeasonState currentSeason, string currentWeather, int seasonDate) //handle the actual math behind the season matrices, https://github.com/RawSpaghett/Justins-Weather-System (loosely based)
    {
        SeasonState markovSeason;
        SubWeatherState subWeather = null;

        (float weight, bool prev) = SeasonInterpolator(seasonDate); //grabs weight of the adjacent season (accordingly) and direction

        //grab appropriate season
        if(prev == true)
        {
            (_,markovSeason) = GetAdjacent();
        }
        else
        {
            (markovSeason,_) = GetAdjacent();
        }
       
        WeatherLinearInterpolator(weight,markovSeason,currentWeather,currentSeason); //create the weighed matrix
        float rand = randomFloat();

        string determinedWeatherStr = NormalizedOdds.OrderBy(v => Math.Abs(v.Value - rand)).First().Key; //finds the closest number to the random number via abs distance and extracts the key/weather name
        
        WeatherState determinedWeather = WeatherGrabber(_weatherData,determinedWeatherStr);

        //now we roll for subweather
        if(determinedWeather.subWeather != null) //each weather gets ONE subweather
        {
            if( rand < determinedWeather.subWeather.subWeatherChance)
            {
                subWeather = determinedWeather.subWeather;
            }
        }

        //wipe the Dictionaries
        WeighedOdds.Clear();
        NormalizedOdds.Clear();

        return (determinedWeather, subWeather); //payload will take this data and handle it accordingly
    }  

    public WeatherState WeatherGrabber(WeatherWrapper data, string targetWeather)//utility
    {
        if(data.weather_profiles.TryGetValue(targetWeather, out WeatherState state))
        {
            return state;
        }
        else
        {
            Debug.Log($"WeatherGrabber:{targetWeather} does not exist");
            return null;
        }
    }
        
    #endregion

    #region Outgoing

    public void SetWeather(WeatherState newWeather) 
    {
        CURRENTWEATHER = newWeather;
        Debug.Log($"The Current Weather Is: {CURRENTWEATHER}");
        BroadcastWeatherUpdate();
    }

    private void BroadcastWeatherUpdate() 
    {
        if (!global::ServerManager.HasInstance) return;

        var networkManager = InstanceFinder.NetworkManager;
        if (networkManager == null) return;

        WeatherUpdateMessage msg = new WeatherUpdateMessage
        {
            CurrentWeather = CURRENTWEATHER
        };

        networkManager.ServerManager.Broadcast(msg, false);
        Debug.Log("Season Broadcasted!");
    }

    #endregion
}
