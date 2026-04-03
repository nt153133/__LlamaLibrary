using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LlamaLibrary.Helpers.CharacterSwitching;

/// <summary>
/// Provides delegates and helpers for switching characters and populating the available character list.
/// </summary>
public static class CharacterSwitcher
{
    /// <summary>
    /// Gets the observable collection of registered character tasks.
    /// </summary>
    public static ReadOnlyObservableCollection<CharacterTask> CharacterTasks => CharacterTaskManager.Tasks;

    /// <summary>
    /// Asynchronous delegate used to populate the character list.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that yields <c>true</c> if the character list was filled successfully; otherwise <c>false</c>.
    /// </returns>
    public delegate Task<bool> FillCharacterListDelegate();

    /// <summary>
    /// Delegate to retrieve the currently available characters.
    /// </summary>
    /// <returns>
    /// A <see cref="List{T}"/> of <see cref="CharacterAvatar"/> representing available characters. May be empty but not null.
    /// </returns>
    public delegate List<CharacterAvatar> GetCharacterListDelegate();

    /// <summary>
    /// Asynchronous delegate used to switch character by providing a <see cref="CharacterAvatar"/>.
    /// </summary>
    /// <param name="characterAvatar">The target character avatar to switch to.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that yields <c>true</c> if the switch was successful; otherwise <c>false</c>.
    /// </returns>
    public delegate Task<bool> SwitchCharacterByAvatarDelegate(CharacterAvatar characterAvatar);

    /// <summary>
    /// Asynchronous delegate used to switch character by character id.
    /// </summary>
    /// <param name="characterId">The id of the character to switch to.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that yields <c>true</c> if the switch was successful; otherwise <c>false</c>.
    /// </returns>
    public delegate Task<bool> SwitchCharacterDelegate(long characterId);

    /// <summary>
    /// Gets or sets the asynchronous delegate responsible for filling the character list.
    /// </summary>
    /// <remarks>
    /// This delegate may be <c>null</c> when character switching functionality is not available.
    /// </remarks>
    public static FillCharacterListDelegate? FillCharacterListAsync { get; set; }

    /// <summary>
    /// Gets or sets the asynchronous delegate responsible for switching characters by id.
    /// </summary>
    /// <remarks>
    /// This delegate may be <c>null</c> when character switching functionality is not available.
    /// </remarks>
    public static SwitchCharacterDelegate? SwitchCharacterAsync { get; set; }

    /// <summary>
    /// Gets or sets the asynchronous delegate responsible for switching characters by avatar.
    /// </summary>
    /// <remarks>
    /// This delegate may be <c>null</c> when character switching functionality is not available.
    /// </remarks>
    public static SwitchCharacterByAvatarDelegate? SwitchCharacterByAvatarAsync { get; set; }

    /// <summary>
    /// Gets or sets the delegate used to retrieve the list of available characters.
    /// </summary>
    /// <remarks>
    /// This delegate may be <c>null</c> when character list retrieval is not available.
    /// </remarks>
    public static GetCharacterListDelegate? GetCharacterList { get; set; }

    /// <summary>
    /// Determines whether character switching features are currently available.
    /// </summary>
    /// <returns>
    /// <c>true</c> if both <see cref="FillCharacterListAsync"/> and <see cref="SwitchCharacterAsync"/> are set; otherwise <c>false</c>.
    /// </returns>
    public static bool IsCharacterSwitchingAvailable()
    {
        return FillCharacterListAsync != null && SwitchCharacterAsync != null;
    }

    /// <summary>
    /// Registers a character task provided by an external library.
    /// </summary>
    /// <param name="task">The task to register.</param>
    /// <returns><see langword="true"/> when the task is added; otherwise <see langword="false"/>.</returns>
    public static bool AddCharacterTask(CharacterTask task)
    {
        return CharacterTaskManager.AddTask(task);
    }

    /// <summary>
    /// Removes a previously registered character task.
    /// </summary>
    /// <param name="task">The task to remove.</param>
    /// <returns><see langword="true"/> when the task is removed; otherwise <see langword="false"/>.</returns>
    public static bool RemoveCharacterTask(CharacterTask task)
    {
        return CharacterTaskManager.RemoveTask(task);
    }
}