namespace LlamaLibrary.RemoteWindows
{
    public class GuildLeveDifficulty : RemoteWindow<GuildLeveDifficulty>
    {
        public GuildLeveDifficulty() : base("GuildLeveDifficulty")
        {
        }

        public void Confirm()
        {
            SendAction(1, 3, 0);
        }

        public override void Close()
        {
            SendAction(1, 3uL, 0xFFFFFFFFuL);
        }
    }
}