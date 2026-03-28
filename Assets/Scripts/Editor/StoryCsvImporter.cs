using System;
using UnityEditor;
using UnityEngine;

public static partial class StoryCsvImporter
{
    [MenuItem("Tools/Story/Import CSV To JSON")]
    public static void ImportCsvToJson()
    {
        try
        {
            StoryDatabase database = BuildDatabase("Assets/Scripts/GameData/StorySource");
            WriteJson(database, "Assets/Resources/Story/story.json");
            AssetDatabase.Refresh();
            Debug.Log("Story import complete: Assets/Resources/Story/story.json");
        }
        catch (Exception ex)
        {
            Debug.LogError("Story import failed: " + ex.Message + "\n" + ex.StackTrace);
        }
    }
}
