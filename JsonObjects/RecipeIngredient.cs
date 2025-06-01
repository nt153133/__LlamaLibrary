using System;
using ff14bot.Managers;
using LlamaLibrary.Extensions;

namespace LlamaLibrary.JsonObjects;

public class RecipeIngredient : IEquatable<RecipeIngredient>
{
    public uint Item { get; set; }
    public uint Amount { get; set; }
    public IngredientType Type { get; set; }

    public uint ExtraData { get; set; }

    public RecipeIngredient()
    {
    }

    public RecipeIngredient(uint item, uint amount, IngredientType type, uint extraData)
    {
        Item = item;
        Amount = amount;
        Type = type;
        ExtraData = extraData;
    }

    public bool Equals(RecipeIngredient? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Item == other.Item;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((RecipeIngredient)obj);
    }

    public override int GetHashCode()
    {
        return (int)Item;
    }

    public override string ToString()
    {
        return $"Item: {DataManager.GetItem(Item).LocaleName()}, Amount: {Amount}, Type: {Type}, ExtraData: {ExtraData}";
    }
}