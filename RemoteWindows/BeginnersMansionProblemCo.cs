namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Class for using the Hall of Novice Window.
    /// </summary>
    public class BeginnersMansionProblemCo : RemoteWindow<BeginnersMansionProblemCo>
    {
        public BeginnersMansionProblemCo() : base("BeginnersMansionProblemCo")
        {
        }

        /// <summary>
        /// Sets the squadron command mission.
        /// </summary>
        /// <param name="index">The duty index from the list starting at 0.</param>
        public void SelectExercise(int index)
        {
            SendAction(2, 3, 0, 4, (ulong)index);
        }

        public override void Close()
        {
            SendAction(1, 3, 1);
        }
    }
}