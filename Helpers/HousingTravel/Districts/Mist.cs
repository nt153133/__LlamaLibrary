using System.Collections.Generic;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot.Navigation;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers.Housing;

namespace LlamaLibrary.Helpers.HousingTravel.Districts
{
    public class Mist : ResidentialDistrict<Mist>
    {
        public override string Name => "Mist";
        public override uint TownAetheryteId => 8;
        public override ushort ZoneId => (ushort)HousingZone.Mist;

        private readonly List<HousingAetheryte> aetherytes = new List<HousingAetheryte>()
        {
            new HousingAetheryte(4, 2003398, "Seagaze Markets", new Vector3(11.41876f, 6.002154f, 13.62034f), false),
            new HousingAetheryte(1, 2003395, "Mistgate Square", new Vector3(-6.041023f, 48.99702f, -113.7678f), false),
            new HousingAetheryte(2, 2003396, "Mist West", new Vector3(-103.0097f, 32.64326f, -86.21572f), false),
            new HousingAetheryte(3, 2003397, "Mist Northeast", new Vector3(90.80508f, 42.05377f, -98.41236f), false),
            new HousingAetheryte(5, 2003399, "Mist Southeast", new Vector3(147.2631f, 23.99999f, 39.81422f), false),
            new HousingAetheryte(6, 2003995, "Mist South (Docks)", new Vector3(-56.31385f, 10.00168f, 38.29704f), false),
            new HousingAetheryte(7, 2003996, "Mist East", new Vector3(84.20611f, 18.00072f, -15.60657f), false),
            new HousingAetheryte(8, 2007435, "The Topmast", new Vector3(-147.2394f, 29.81588f, -50.84625f), false),
            new HousingAetheryte(12, 2004971, "Seagaze Markets Subdivision", new Vector3(-716.7053f, 6.002154f, -692.8323f), true),
            new HousingAetheryte(9, 2004968, "Mistgate Square Subdivision", new Vector3(-589.9782f, 48.99702f, -709.728f), true),
            new HousingAetheryte(10, 2004969, "Mist Northeast Subdivision", new Vector3(-616.4923f, 32.71832f, -807.1437f), true),
            new HousingAetheryte(11, 2004970, "Mist Southeast Subdivision", new Vector3(-605.3936f, 41.85083f, -614.0589f), true),
            new HousingAetheryte(13, 2004972, "Mist Southwest Subdivision", new Vector3(-743.9232f, 24f, -556.3939f), true),
            new HousingAetheryte(14, 2004973, "Mist Northwest Subdivision", new Vector3(-742.963f, 10.00166f, -760.6319f), true),
            new HousingAetheryte(15, 2004974, "Central Mist Subdivision", new Vector3(-688.5404f, 18.00072f, -618.5428f), true),
            new HousingAetheryte(16, 2007438, "The Topmast Subdivision", new Vector3(-653.4601f, 29.81396f, -851.3586f), true),
        };

        public override List<HousingAetheryte> Aetherytes => aetherytes;
        public override int RequiredQuest => 66750;
        public override Vector3 TownAetheryteLocation => new Vector3(-89.30112f, 18.80033f, -2.019181f);

        private readonly List<Vector3> _transitionStartLocations = new List<Vector3>() { new Vector3(-539.8311f, 48.57784f, -714.2647f), new Vector3(-10.03277f, 48.73076f, -162.865f) };
        private readonly List<Vector3> _transitionEndLocations = new List<Vector3>() { new Vector3(-534.1337f, 48.32912f, -714.3889f), new Vector3(-9.982697f, 48.34089f, -169.6015f) };

        public override List<Vector3> TransitionStartLocations => _transitionStartLocations;
        public override List<Vector3> TransitionEndLocations => _transitionEndLocations;

        public override async Task<bool> WalkToResidential()
        {
            await Navigation.GetTo(135, new Vector3(597.4801f, 61.59979f, -110.7737f));

            var zoneChange = new Vector3(598.1823f, 61.52054f, -108.3216f);

            while (!SelectString.IsOpen)
            {
                Navigator.PlayerMover.MoveTowards(zoneChange);
                await Coroutine.Sleep(50);
                Navigator.PlayerMover.MoveStop();
            }

            Navigator.PlayerMover.MoveStop();
            return (await Coroutine.Wait(5000, () => SelectString.IsOpen));
        }
    }
}