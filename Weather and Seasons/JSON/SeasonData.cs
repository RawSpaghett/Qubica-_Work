using System.Collections.Generic;
using UnityEngine;

//Just because its listed here does not mean every section is used in the JSON

[System.Serializable]
public class SeasonWrapper
{
    public Dictionary<string, SeasonState> season_Profiles;
}

[System.Serializable]
public class SeasonState // SUBCATEGORIES NOW HANDLED IN WeatherEffects.cs
{
    public int position;
    public Payload seasonTweaks;
    public List<CurrentWeatherState> weather_matrix;
}

//handles the weather options, chances, and their matrices
[System.Serializable]
public class WeatherOptions //holds the data for the weather options and chances
{
    public string weatherOption; //one of the possibilities in the matrix for the current weather
    public float weatherChance;//the preceding weathers chance
}

[System.Serializable]
public class CurrentWeatherState //the header for the weatheroptions
{
    public string currentWeather;
    public List<WeatherOptions> matrices;
}


