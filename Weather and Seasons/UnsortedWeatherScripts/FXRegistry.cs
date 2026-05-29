using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public abstract class FXentry {public string key;}

[System.Serializable]
public class AudioData: FXentry {public AudioClip audio;}

[System.Serializable]
public class ParticleData: FXentry {public ParticleSystem particles;}

/*
    Could also do On-screen effects like vignette
*/

[CreateAssetMenu(fileName = "FXRegistry", menuName = "Scripting/FX Registry")]
public class FXRegistry: ScriptableObject
{
    public List<AudioData> AudioAssets = new List<AudioData>();
    public List<ParticleData> ParticleFX = new List<ParticleData>();

    private Dictionary<string, object> masterDictionary = new Dictionary<string, object>();

    public void IntializeDictionary() //emptys and fills the masterDictionary for fast lookup
    {
        masterDictionary.Clear(); 

        foreach (var a in AudioAssets) masterDictionary[a.key] = a;
        foreach (var p in ParticleFX) masterDictionary[p.key] = p;
    }
    public T GetAsset<T>(string key) where T: class
    {
        if( masterDictionary.Count == 0)
        {
            IntializeDictionary(); //if its empty, wipe it and rebuild it
        }
        if (masterDictionary.TryGetValue(key,out object asset)) //if found return asset
        {
            return asset as T;
        }
        Debug.Log($"Asset {key} not found"); //if not found
        return null;
    }







}
