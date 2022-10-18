using System;
using System.Collections.Generic;
using UnityEngine;

public static class NameRandomizer
{
    [Serializable]
    private class NamesList
    {
        public List<string> names;
    }

    private static NamesList _namesList;

    private static NamesList CurrentNamesList
    {
        get
        {
            if (_namesList != null) return _namesList;
            var textAsset = Resources.Load("NamesList") as TextAsset;
            if (textAsset != null) _namesList = JsonUtility.FromJson<NamesList>(textAsset.text);
            return _namesList;
        }
    }

    public static string GetRandomName()
    {
        return CurrentNamesList.names[UnityEngine.Random.Range(0, CurrentNamesList.names.Count)];
    }

    public static List<string> GetRandomNames(int nbNames)
    {
        if (nbNames > CurrentNamesList.names.Count)
            throw new Exception("Asking for more random names than there actually are!");
        
        var copy = new NamesList
        {
            names = new List<string>(CurrentNamesList.names)
        };

        var result = new List<string>();

        for (var i = 0; i < nbNames; i++)
        {
            var rnd = UnityEngine.Random.Range(0, copy.names.Count);
            result.Add(copy.names[rnd]);
            copy.names.RemoveAt(rnd);
        }

        return result;
    }
}
