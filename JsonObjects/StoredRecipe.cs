using System.Collections.Generic;
using System.Linq;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Extensions;

namespace LlamaLibrary.JsonObjects;

public class StoredRecipe
{
    public uint ItemId{ get; set; }
    public ushort RecipeId{ get; set; }
    public HashSet<RecipeIngredient> Ingredients { get; set; } = new HashSet<RecipeIngredient>();
    public ClassJobType CraftingClass { get; set; }

    public StoredRecipe(uint itemId, ushort recipeId, ClassJobType craftingClass)
    {
        ItemId = itemId;
        RecipeId = recipeId;
        CraftingClass = craftingClass;
    }

    public StoredRecipe()
    {
    }

    public void AddIngredient(uint itemId, uint amount, IngredientType type, uint extraData = 0)
    {
        var ingredient = new RecipeIngredient(itemId, amount, type, extraData);

        if (Ingredients.Contains(ingredient))
        {
            Ingredients.First(i => i.Equals(ingredient)).Amount += amount;
        }
        else
        {
            Ingredients.Add(ingredient);
        }
    }

    public List<RecipeIngredient> GetRequiredIngredients(int amount = 1)
    {
        List<RecipeIngredient> ingredients = new List<RecipeIngredient>(Ingredients.Count);

        foreach (var ingredient in Ingredients)
        {
            ingredients.Add(new RecipeIngredient(ingredient.Item, (uint)(ingredient.Amount * amount), ingredient.Type, ingredient.ExtraData));
        }

        return ingredients;
    }

    public override string ToString()
    {
        return $"Item: {DataManager.GetItem(ItemId).LocaleName()}, RecipeId: {RecipeId}, CraftingClass: {CraftingClass} Ingredients: \n\t{string.Join("\n\t", Ingredients.Select(i=> i.ToString()))}";
    }
}