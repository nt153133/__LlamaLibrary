namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Interaction interface for the "Character" window.
    /// Provides methods for updating gearsets and checking UI state.
    /// </summary>
    public class Character : RemoteWindow<Character>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Character"/> class.
        /// </summary>
        public Character() : base("Character")
        {
        }

        /// <summary>
        /// Updates the currently equipped gearset with the items the character is wearing.
        /// Sends the appropriate UI action to trigger the update.
        /// </summary>
        public void UpdateGearSet()
        {
            SendAction(1, 3, 0xF);
        }

        /// <summary>
        /// Determines if the "Update Gearset" button is currently clickable in the character window.
        /// Checks the button state at internal UI index 18.
        /// </summary>
        /// <returns><see langword="true"/> if the update button is clickable; otherwise <see langword="false"/>.</returns>
        public bool CanUpdateGearSet()
        {
            var button = WindowByName?.FindButton(18);

            return button is { Clickable: true };
        }
    }
}