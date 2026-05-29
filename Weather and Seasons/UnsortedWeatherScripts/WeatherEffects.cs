using UnityEngine;

//referenced in both SeasonData and WeatherData

[System.Serializable]
public class ColorTweaks
{
    //Primitive color values
}

[System.Serializable]
public class LightingTweaks
{
    //primitive lighting values
}

[System.Serializable]
public class GameplayTweaks
{
    //primitive multiplier values (Crop growth, happiness, etc)
}

[System.Serializable]
public class AudioFX
{
    //holds strings to FXregistry audio
}

[System.Serializable]
public class visualFX
{
    //holds strings to FXregistry Visuals
}

public class Payload //WeatherEffects.CS
{
    public ColorTweaks color_tweaks;
    public LightingTweaks lighting_tweaks;
    public GameplayTweaks gameplay_tweaks;
    public AudioFX audio_FX;
    public visualFX visual_FX;
}
