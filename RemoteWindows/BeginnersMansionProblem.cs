namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Class for using the Hall of Novice Window.
    /// </summary>
    public class BeginnersMansionProblem : RemoteWindow<BeginnersMansionProblem>
    {
        public BeginnersMansionProblem() : base("BeginnersMansionProblem")
        {
        }

        public void Begin()
        {
            SendAction(1, 3, 0);
        }

        public override void Close()
        {
            SendAction(1, 3, 1);
        }

        public void DisplayeCompletedExercises()
        {
            SendAction(1, 3, 2);
        }
    }
}