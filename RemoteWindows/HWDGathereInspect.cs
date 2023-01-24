namespace LlamaLibrary.RemoteWindows
{
    public class HWDGathereInspect : RemoteWindow<HWDGathereInspect>
    {
        public HWDGathereInspect() : base("HWDGathereInspect")
        {
        }

        public void ClickAutoSubmit()
        {
            if (CanAutoSubmit())
            {
                SendAction(1, 3, 0xC);
            }
        }

        public void ClickRequestInspection()
        {
            if (CanRequestInspection())
            {
                SendAction(1, 3, 0xB);
            }
        }

        public void ClickClass(int index)
        {
            SendAction(2, 3, 0xE, 4, (ulong)index);
        }

        public bool CanAutoSubmit()
        {
            var button = WindowByName?.FindButton(8);

            return button is { Clickable: true };
        }

        public bool CanRequestInspection()
        {
            var button = WindowByName?.FindButton(10);

            return button is { Clickable: true };
        }

        public override void Close()
        {
            SendAction(1, 3, ulong.MaxValue);
        }
    }
}