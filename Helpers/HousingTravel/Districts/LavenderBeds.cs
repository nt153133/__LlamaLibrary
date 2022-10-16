using System.Collections.Generic;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.NPC;

namespace LlamaLibrary.Helpers.HousingTravel.Districts
{
    public class LavenderBeds : ResidentialDistrict<LavenderBeds>
    {
        public override string Name => "Lavender Beds";
        public override uint TownAetheryteId => 2;
        public override ushort ZoneId => (ushort)HousingZone.LavenderBeds;

        private readonly List<HousingAetheryte> aetherytes = new()
        {
            new HousingAetheryte(17, 2003400, "Dappled Stalls", new Vector3(38.91589f, 65.89468f, -113.3012f), false),
            new HousingAetheryte(18, 2003401, "Lavender Northwest", new Vector3(-87.2435f, 31.29388f, -54.32058f), false),
            new HousingAetheryte(19, 2003402, "Wildflower Stalls", new Vector3(-13.88112f, 39.51287f, -41.16616f), false),
            new HousingAetheryte(20, 2003403, "Lavender Southeast", new Vector3(102.6189f, 6.48383f, 86.53507f), false),
            new HousingAetheryte(21, 2003404, "Lavender Southwest", new Vector3(-63.27133f, 5.822806f, 108.2112f), false),
            new HousingAetheryte(22, 2003997, "Lavender East", new Vector3(94.66289f, 33.99077f, -20.7655f), false),
            new HousingAetheryte(23, 2003998, "Amethyst Shallows", new Vector3(5.749551f, 2.610901f, 188.4618f), false),
            new HousingAetheryte(24, 2007434, "Lily Hills", new Vector3(237.4334f, 53.72666f, -117.038f), false),
            new HousingAetheryte(25, 2004983, "Dappled Stalls Subdivision", new Vector3(-590.9529f, 65.89655f, -664.8567f), true),
            new HousingAetheryte(26, 2004984, "Lavender Northeast Subdivision", new Vector3(-649.3804f, 31.30531f, -791.7295f), true),
            new HousingAetheryte(27, 2004985, "Wildflower Stalls Subdivision", new Vector3(-663.4099f, 39.51358f, -717.3811f), true),
            new HousingAetheryte(28, 2004986, "Lavender Southwest Subdivision", new Vector3(-790.0471f, 6.510614f, -601.8861f), true),
            new HousingAetheryte(29, 2004987, "Lavender Northwest Subdivision", new Vector3(-811.0249f, 5.94287f, -766.8563f), true),
            new HousingAetheryte(30, 2004988, "Lavender South Subdivision", new Vector3(-682.6985f, 34.06768f, -608.7421f), true),
            new HousingAetheryte(31, 2004989, "Amethyst Shallows Subdivision", new Vector3(-892.0838f, 2.608214f, -698.7084f), true),
            new HousingAetheryte(32, 2007437, "Lily Hills Subdivision", new Vector3(-586.8181f, 53.72816f, -466.6527f), true),
        };

        public override List<HousingAetheryte> Aetherytes => aetherytes;
        public override int RequiredQuest => 66748;

        private readonly List<Npc> _transitionNpcIds = new() { new Npc(1005655, 340, new Vector3(10.72699f, 2.610901f, 208.0873f)), new Npc(1005655, 340, new Vector3(-912.108f, 2.607928f, -693.2632f)) };
        public override List<Npc> TransitionNpcs => _transitionNpcIds;

        public override async Task<bool> WalkToResidential()
        {
            await Navigation.GetTo(148, new Vector3(199.5991f, -32.04532f, 324.2699f));

            uint FerryNpc = 1005656;

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

            await Coroutine.Wait(5000, () => SelectIconString.IsOpen);

            if (SelectIconString.IsOpen)
            {
                if (Translator.Language == Language.Chn)
                {
                    SelectIconString.ClickLineContains("薰衣草苗圃");
                }
                else
                {
                    SelectIconString.ClickLineContains("Lavender Beds");
                }

                await Coroutine.Wait(5000, () => Talk.DialogOpen || SelectString.IsOpen);
            }

            if (Talk.DialogOpen)
            {
                Talk.Next();
            }

            return await Coroutine.Wait(3000, () => SelectString.IsOpen);
        }
    }
}