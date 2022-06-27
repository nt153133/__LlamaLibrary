using System.Collections.Generic;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot.Navigation;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers.Housing;

namespace LlamaLibrary.Helpers.HousingTravel.Districts
{
    public class TheGoblet : ResidentialDistrict<TheGoblet>
    {
        public override string Name => "The Goblet";
        public override uint TownAetheryteId => 9;
        public override ushort ZoneId => (ushort)HousingZone.Goblet;

        private readonly List<HousingAetheryte> aetherytes = new List<HousingAetheryte>()
        {
            new HousingAetheryte(33, 2003405, "Goblet Exchange", new Vector3(-5.641616f, -8f, -128.4054f), false),
            new HousingAetheryte(34, 2003406, "Goblet Northeast", new Vector3(121.2923f, -32f, -76.59491f), false),
            new HousingAetheryte(35, 2003407, "Goblet West", new Vector3(-105.339f, 8f, -51.23026f), false),
            new HousingAetheryte(36, 2003408, "The Brimming Heart", new Vector3(-4.058663f, -23.22587f, 12.07977f), false),
            new HousingAetheryte(37, 2003409, "Goblet Southeast", new Vector3(143.0668f, -36f, 55.36634f), false),
            new HousingAetheryte(38, 2003999, "Goblet North", new Vector3(21.28819f, -11.07666f, -191.618f), false),
            new HousingAetheryte(39, 2004000, "Goblet East", new Vector3(159.4118f, -30f, -33.32204f), false),
            new HousingAetheryte(40, 2007436, "The Sultana's Breath", new Vector3(-91.85902f, 19.17301f, 76.59766f), false),
            new HousingAetheryte(41, 2004976, "Goblet Exchange Subdivision", new Vector3(-575.9996f, -8f, -710.9706f), true),
            new HousingAetheryte(42, 2004977, "Goblet Southeast Subdivision", new Vector3(-626.9311f, -32f, -581.9917f), true),
            new HousingAetheryte(43, 2004978, "Goblet North Subdivision", new Vector3(-654.0122f, 8f, -809.6718f), true),
            new HousingAetheryte(44, 2004979, "The Brimming Heart Subdivision", new Vector3(-716.6361f, -23.22587f, -708.019f), true),
            new HousingAetheryte(45, 2004980, "Goblet Southwest Subdivision", new Vector3(-758.5844f, -36f, -561.0862f), true),
            new HousingAetheryte(46, 2004981, "Goblet East Subdivision", new Vector3(-512.4191f, -11.07666f, -682.4424f), true),
            new HousingAetheryte(47, 2004982, "Goblet South Subdivision", new Vector3(-669.9449f, -30f, -544.2172f), true),
            new HousingAetheryte(48, 2007439, "The Sultana's Breath Subdivision", new Vector3(-780.1594f, 19.09797f, -796.2185f), true),
        };

        public override List<HousingAetheryte> Aetherytes => aetherytes;

        public override int RequiredQuest => 66749;

        private readonly List<Vector3> _transitionStartLocations = new List<Vector3>() { new Vector3(-6.57211f, -11.07666f, -194.3436f), new Vector3(-509.0797f, -11.07666f, -710.3031f) };
        private readonly List<Vector3> _transitionEndLocations = new List<Vector3>() { new Vector3(-11.01813f, -11.07666f, -197.8161f), new Vector3(-505.6317f, -11.07666f, -715.1525f) };

        public override List<Vector3> TransitionStartLocations => _transitionStartLocations;
        public override List<Vector3> TransitionEndLocations => _transitionEndLocations;

        public override async Task<bool> WalkToResidential()
        {
            await Navigation.GetTo(140, new Vector3(317.0663f, 67.27534f, 232.8395f));

            var zoneChange = new Vector3(316.7798f, 67.13619f, 236.8774f);

            while (!SelectString.IsOpen)
            {
                Navigator.PlayerMover.MoveTowards(zoneChange);
                await Coroutine.Sleep(50);
                Navigator.PlayerMover.MoveStop();
            }

            Navigator.PlayerMover.MoveStop();
            return await Coroutine.Wait(3000, () => SelectString.IsOpen);
        }
    }
}