using System.IO;
using UnityEngine;

public static class SaveSystem // No need for Monobehaviour here either

{
    private const string FILE_NAME = "save.json";

    public static string SavePath => Path.Combine(Application.persistentDataPath, FILE_NAME);

    public static void Save(SaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Saved -> {SavePath}");
        }
        catch (System.Exception e) 
        {
                Debug.LogError("Save failed " + e.Message);
        }
    }

    public static bool TryLoad(out  SaveData data)
    {
        data = null;

        try
        {
            if (!File.Exists(SavePath))
                return false;

            string json = File.ReadAllText(SavePath);
            data = JsonUtility.FromJson<SaveData>(json);
            return data != null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Load failed " + ex.Message);
            return false;
        }
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath)) 
            { File.Delete(SavePath); }
    }
    
}
