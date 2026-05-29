using System.Collections.Generic;
using UnityEngine;

//Just because its listed here does not mean every section is used in the JSON

[System.Serializable]
public class WeatherWrapper //data storage
{
    public Dictionary<string, WeatherState> weather_profiles;
}

[System.Serializable]
public class WeatherState //Actual Weather state (prevents recursion this way)
{
    public Payload effects;
    public SubWeatherState subWeather;
}

[System.Serializable]
public class SubWeatherState //inherits from base weather state
{
    public string subWeatherID; //name of subWeather
    public float subWeatherChance; //Odds, out of 100, of this weather
    public Payload effectOverrides; //use a merge function in .payload
}




