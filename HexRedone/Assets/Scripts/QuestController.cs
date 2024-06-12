using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{
    public string Id { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; } = false;
    public int TotalBuildingsCount { get; set; } = 0;

    // Represents the required count of each building type to complete the quest
    public Dictionary<string, int> Objectives { get; set; } = new Dictionary<string, int>();
}

public class QuestController : MonoBehaviour
{
    public WorldGen worldGen;
    public PlayerController pController;

    public Dictionary<string, Quest> quests = new Dictionary<string, Quest>();


    public void checkQuestCompleted(string questID)
    {
        if (!quests.ContainsKey(questID))
        {
            Debug.Log("Quest " + questID + " does not exist.");
            return;
        }

        var quest = quests[questID];

        int totalBuildings = 0;
        foreach (var objective in quest.Objectives)
        {
            if (!pController.buildingCounts.ContainsKey(objective.Key) || pController.buildingCounts[objective.Key] < objective.Value)
            {
                return;
            }
            totalBuildings += pController.buildingCounts[objective.Key];
        }

        if (totalBuildings < quest.TotalBuildingsCount)
        {
            return;
        }

        Debug.Log("Quest " + questID + " completed!");
        quest.IsCompleted = true;
        // Implement other quest complete logic here
    }

    // Method for creating a new quest
    public void CreateQuest(string ID, string description, Dictionary<string, int> objectives)
    {
        var quest = new Quest
        {
            Id = ID,
            Description = description,
            Objectives = objectives
        };
        quests[ID] = quest;
    }

    public bool hasDistinctValues(int count, Dictionary<string, int> dict)
    {
        if (dict.Count == count)
        {
            return true;
        }
        return false;
    }
}
