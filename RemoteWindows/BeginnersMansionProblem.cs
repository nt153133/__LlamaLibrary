namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    ///     Class for using the Hall of Noivce Window
    /// </summary>
    public class BeginnersMansionProblem : RemoteWindow<BeginnersMansionProblem>
    {
        private const string WindowName = "BeginnersMansionProblem";

        public BeginnersMansionProblem() : base(WindowName)
        {
            _name = WindowName;
        }

        public void Begin()
        {
            SendAction(1, 3, 0);
        }

        public void Close()
        {
            SendAction(1, 3, 1);
        }

        public void DisplayeCompletedExercises()
        {
            SendAction(1, 3, 2);
        }
    }
}