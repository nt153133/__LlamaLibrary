namespace LlamaLibrary.Structs.Housing;

/// <summary>An estate represented in the Timers estate-status payload.</summary>
public enum EstateType
{
    /// <summary>The character's free company estate.</summary>
    FreeCompany,

    /// <summary>The character's personally owned estate.</summary>
    Private,

    /// <summary>The first estate shared with the character as a tenant.</summary>
    SharedEstate1,

    /// <summary>The second estate shared with the character as a tenant.</summary>
    SharedEstate2,
}
