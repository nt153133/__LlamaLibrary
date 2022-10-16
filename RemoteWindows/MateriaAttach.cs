namespace LlamaLibrary.RemoteWindows
{
    public class MateriaAttach : RemoteWindow<MateriaAttach>
    {
        public MateriaAttach() : base("MateriaAttach")
        {
        }

        public void ClickItem(int index)
        {
            SendAction(3, 3uL, 1, 3, (ulong)index, 3, 1);
        }

        public void ClickMateria(int index)
        {
            SendAction(3, 3uL, 2, 3, (ulong)index, 3, 1);
        }
    }
}