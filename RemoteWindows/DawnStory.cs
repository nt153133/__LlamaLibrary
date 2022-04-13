using System.Collections.Generic;
using System.Threading.Tasks;
using Buddy.Coroutines;
using LlamaLibrary.RemoteAgents;

namespace LlamaLibrary.RemoteWindows
{
    public class DawnStory : RemoteWindow<DawnStory>
    {
        private const string WindowName = "DawnStory";

        private static Dictionary<int, (ulong SendAction, byte DutyKey)> _duties = new Dictionary<int, (ulong SendAction, byte DutyKey)>()
        {
            { 676, (0x0, 1) }, //Holminster Switch
            { 649, (0x1, 2) }, //Dohn Mheg
            { 651, (0x2, 3) }, //the Qitana Ravel
            { 656, (0x3, 4) }, //Malikah's Well
            { 659, (0x4, 5) }, //Mt. Gulg
            { 652, (0x5, 6) }, //Amaurot
            { 692, (0x6, 7) }, //The Grand Cosmos
            { 714, (0x7, 8) }, //Anamnesis Anyder
            { 737, (0x8, 9) }, //the Heroes' Gauntlet
            { 746, (0x9, 10) }, //Matoya's Relict
            { 777, (0xA, 11) }, //Paglth'an
            { 783, (0xB, 12) }, //The Tower of Zot
            { 785, (0xC, 13) }, //The Tower of Babil
            { 789, (0xD, 14) }, //Vanaspati
            { 787, (0xE, 15) }, //Ktisis Hyperboreia
            { 786, (0xF, 16) }, //The Aitiascope
            { 790, (0x10, 17) }, //The Mothercrystal
            { 792, (0x11, 18) }, //The Dead Ends
            { 844, (0x12, 19) }, //Alzadaal's Legacy
            { 4, (0x12, 200) }, //Sastasha
            { 2, (0x13, 201) }, //the Tam–Tara Deepcroft
            { 3, (0x14, 202) }, //Copperbell Mines
            { 56, (0x15, 203) }, //the Bowl of Embers
            { 1, (0x16, 204) }, //the Thousand Maws of Toto–Rak
            { 6, (0x17, 205) }, //Haukke Manor
            { 8, (0x18, 206) }, //Brayflox's Longstop
            { 57, (0x19, 207) }, //the Navel
            { 11, (0x1A, 208) }, //the Stone Vigil
            { 58, (0x1B, 209) }, //the Howling Eye
            { 15, (0x1C, 210) }, //Castrum Meridianum
            { 16, (0x1D, 211) }, //the Praetorium
            { 830, (0x1E, 212) }, //The Porta Decumana
        };

        public DawnStory() : base(WindowName)
        {
            _name = WindowName;
        }

        public async Task<bool> SelectDuty(int dutyId)
        {
            if (!_duties.ContainsKey(dutyId))
            {
                return false;
            }

            if (AgentDawnStory.Instance.SelectedDuty == _duties[dutyId].DutyKey)
            {
                return true;
            }

            SendAction(2, 3, 0xC, 4, _duties[dutyId].SendAction);

            return await Coroutine.Wait(5000, () => AgentDawnStory.Instance.SelectedDuty == _duties[dutyId].DutyKey);
        }

        public void Commence()
        {
            SendAction(1, 3, 0xE);
        }
    }
}