using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    class Quest
    {
        public string Name { get; }
        private List<Objective> objectives;

        public Quest(string name)
        {
            Name = name;
            objectives = new List<Objective>();
        }

        public void AddObjective(Func<bool> objectiveFunction, string description)
        {
            Objective objective = new Objective(objectiveFunction, description);
            objectives.Add(objective);
        }

        public bool CheckObjectives()
        {
            foreach (var objective in objectives)
            {
                if (!objective.Function())
                {
                    return false;
                }
            }
            return true;
        }
    }

    class Objective
    {
        public Func<bool> Function { get; }
        public string Description { get; }

        public Objective(Func<bool> objectiveFunction, string description)
        {
            Function = objectiveFunction;
            Description = description;
        }
    }

    class Main
    {
        static bool CompleteObjectives()
        {
            return true;
        }

        static bool CollectItem() // Special items
        {
            return true;
        }

        static bool CollectItems() // For normal resources
        {
            return true;
        }

        public void Update()
        {
            // Create a quest
            Quest myQuest = new Quest("Collections!");

            // Add objectives to the quest
            myQuest.AddObjective(CollectItems, "Collect 50 stone");

            // Check if all objectives are complete
            if (myQuest.CheckObjectives())
            {
                Console.WriteLine($"Quest '{myQuest.Name}' is complete!");
            }
            else
            {
                Console.WriteLine($"Quest '{myQuest.Name}' is not complete yet.");
            }
        }
    }
}
