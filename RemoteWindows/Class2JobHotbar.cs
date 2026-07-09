using LlamaLibrary.RemoteWindows.Atk;

namespace LlamaLibrary.RemoteWindows
{
    public class Class2JobHotbar : RemoteWindow<Class2JobHotbar>
    {
        public Class2JobHotbar() : base("Class2JobHotbar")
        {
        }

        public void Transfer()
        {
            SendAction(true, (ValueType.Undefined, 0), (ValueType.Int, 0x0));
        }

        public void Wait()
        {
            SendAction(true, (ValueType.Undefined, 0), (ValueType.Int, 0x2));
        }

        public void Cancel()
        {
            SendAction(false, (ValueType.Undefined, 0), (ValueType.Int, 0x1));
        }
    }
}