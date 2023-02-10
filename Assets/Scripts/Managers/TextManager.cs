using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    public Dictionary<string, string> stringData { get; set; }

    public string GetStringData(string key)
    {
        if (!stringData.ContainsKey(key))
            return key;

        return stringData[key];
    }
}
