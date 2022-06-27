using System.Collections.Generic;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.NPC;

namespace LlamaLibrary.Helpers.HousingTravel.Districts
{
    public class Shirogan : ResidentialDistrict<Shirogan>
    {
        public override string Name => "Shirogan";
        public override uint TownAetheryteId => 111;
        public override ushort ZoneId => (ushort)HousingZone.Shirogane;

        private readonly List<HousingAetheryte> _aetherytes = new List<HousingAetheryte>()
        {
            new HousingAetheryte(49, 2007855, "Akanegumo Bridge", new Vector3(-95.99971f, 2.02f, 125.951f), false),
            new HousingAetheryte(50, 2007856, "Northwestern Shirogane", new Vector3(-105.4554f, 29.99999f, -117.8433f), false),
            new HousingAetheryte(51, 2007857, "Western Shirogane", new Vector3(-97.47319f, 16.02f, -34.98248f), false),
            new HousingAetheryte(52, 2007858, "Kobai Goten", new Vector3(-20.02024f, 25.05f, -12.79487f), false),
            new HousingAetheryte(53, 2007859, "Northeastern Shirogane", new Vector3(100.0319f, 30.02f, 25.49472f), false),
            new HousingAetheryte(54, 2007860, "Southwestern Shirogane", new Vector3(-144.4256f, 10f, 2.870443f), false),
            new HousingAetheryte(55, 2007861, "Southern Shirogane", new Vector3(-6.227466f, 10.02f, 99.66935f), false),
            new HousingAetheryte(56, 2007862, "Southeastern Shirogane", new Vector3(63.60813f, 10.02f, 114.1939f), false),
            new HousingAetheryte(59, 2007865, "Akanegumo Bridge Subdivision", new Vector3(-830.3062f, 2.019996f, -800f), true),
            new HousingAetheryte(58, 2007864, "Eastern Shirogane Subdivision", new Vector3(-586.4534f, 29.99999f, -809.2762f), true),
            new HousingAetheryte(60, 2007866, "Northeastern Shirogane Subdivision", new Vector3(-667.9701f, 16.02f, -801.836f), true),
            new HousingAetheryte(62, 2007868, "Kobai Goten Subdivision", new Vector3(-690.7285f, 25.05f, -723.9414f), true),
            new HousingAetheryte(64, 2007870, "Southern Shirogane Subdivision", new Vector3(-728.722f, 30.02f, -604.1376f), true),
            new HousingAetheryte(57, 2007863, "Northern Shirogane Subdivision", new Vector3(-706.4394f, 10.06999f, -849.1778f), true),
            new HousingAetheryte(61, 2007867, "Western Shirogane Subdivision", new Vector3(-802.6294f, 10.03267f, -707.055f), true),
            new HousingAetheryte(63, 2007869, "Southwestern Shirogane Subdivision", new Vector3(-817.4475f, 10.02f, -638.4796f), true),
        };

        public override List<HousingAetheryte> Aetherytes => _aetherytes;

        public override int RequiredQuest => 68167;

        public override Vector3 TownAetheryteLocation => new Vector3(48.03579f, 4.549999f, -31.83851f);

        private readonly List<Npc> _transitionNpcIds = new List<Npc>() { new Npc(1019108, 641, new Vector3(-121.172f, 2.029419f, 154.8943f)), new Npc(1019108, 641, new Vector3(-858.9456f, 2.029419f, -825.1926f)) };

        public override List<Npc> TransitionNpcs => _transitionNpcIds;

        public override async Task<bool> WalkToResidential()
        {
            await Navigation.GetTo(628, new Vector3(-116.2294f, -7.010099f, -40.55866f));

            uint FerryNpc = 1019006;

            var unit = GameObjectManager.GetObjectByNPCId(FerryNpc);

            if (!unit.IsWithinInteractRange)
            {
                var target = unit.Location;
                Navigator.PlayerMover.MoveTowards(target);
                while (!unit.IsWithinInteractRange)
                {
                    Navigator.PlayerMover.MoveTowards(target);
                    await Coroutine.Sleep(100);
                }

                Navigator.PlayerMover.MoveStop();
            }

            unit.Target();
            unit.Interact();

            await Coroutine.Wait(5000, () => Talk.DialogOpen);

            if (Talk.DialogOpen)
            {
                Talk.Next();
            }

            return await Coroutine.Wait(3000, () => SelectString.IsOpen);
        }
    }
}