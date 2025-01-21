using System.IO;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Recipe
{
    public string inputResource;
    public string outputResource;
    public int inputAmount;
    public int outputAmount;
    public float processingTime;

    [System.NonSerialized]
    public ResourceType inputResourceType;

    [System.NonSerialized]
    public ResourceType outputResourceType;
}

[System.Serializable]
public class RecipeList
{
    public List<Recipe> recipes;
}

public class RecipeLoader : MonoBehaviour
{
    public string jsonFilePath;
    public RecipeList recipeList;

    public void Start()
    {
        LoadRecipes();
    }

    public void LoadRecipes()
    {
        TextAsset jsonText = Resources.Load<TextAsset>(jsonFilePath);

        if (jsonText != null)
        {
            recipeList = JsonUtility.FromJson<RecipeList>(jsonText.text);

            // Manually parse the string fields to enums
            foreach (var recipe in recipeList.recipes)
            {
                recipe.inputResourceType = ParseResourceType(recipe.inputResource);
                recipe.outputResourceType = ParseResourceType(recipe.outputResource);
                Debug.Log($"Loaded recipe: {recipe.inputResource} -> {recipe.outputResource}");
            }

            Debug.Log("Recipes Loaded Successfully");
        }
        else
        {
            Debug.LogError("Recipe JSON file not found at: " + jsonFilePath);
        }
    }

    private ResourceType ParseResourceType(string resourceTypeString)
    {
        if (System.Enum.TryParse(resourceTypeString.Trim(), true, out ResourceType result))
        {
            return result;
        }
        else
        {
            Debug.LogError($"Invalid ResourceType: '{resourceTypeString}'");
            return ResourceType.None;
        }
    }

    public Recipe GetRecipeByInput(ResourceType inputResource)
    {
        Debug.Log($"Searching for recipe with input resource: {inputResource}");
        return recipeList.recipes.Find(recipe => recipe.inputResourceType == inputResource);
    }
}
