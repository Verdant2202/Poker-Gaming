using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;

[System.Serializable]
public class PlayerData
{
    public int Money;
 
    public static PlayerData Load()
    {
        byte[] encoded = File.ReadAllBytes(Path.Combine(Application.persistentDataPath, "Save.dat"));
        for (int i = 0; i < encoded.Length; i++)
        {
            encoded[i] = (byte)(encoded[i] + 4);
        }

        string json = System.Text.Encoding.UTF8.GetString(encoded);
        PlayerData data = JsonConvert.DeserializeObject<PlayerData>(json);
        return data;
    }

    public static void Save(PlayerData data)
    {
        string json = JsonConvert.SerializeObject(data);
        byte[] encoded = System.Text.Encoding.UTF8.GetBytes(json);
        for (int i = 0; i < encoded.Length; i++)
        {
            encoded[i] = (byte)(encoded[i] - 4);
        }
        File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "Save.dat"), encoded);
    }
}