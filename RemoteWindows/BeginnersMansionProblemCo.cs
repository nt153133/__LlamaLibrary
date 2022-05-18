namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    ///     Class for using the Hall of Noivce Window
    /// </summary>
    public class BeginnersMansionProblemCo : RemoteWindow<BeginnersMansionProblemCo>
    {
        private const string WindowName = "BeginnersMansionProblemCo";

        public BeginnersMansionProblemCo() : base(WindowName)
        {
            _name = WindowName;
        }

        /// <summary>
        ///     Sets the squadron command mission.
        /// </summary>
        /// <param name="index">The duty index from the list starting at 0.</param>
        public void SelectExercise(int index)
        {
            SendAction(2, 3, 0, 4, (ulong)index);
        }

        public void Close()
        {
            SendAction(1, 3, 1);
        }


    }
}