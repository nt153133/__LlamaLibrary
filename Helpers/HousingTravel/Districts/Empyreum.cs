using System.Collections.Generic;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot.Behavior;
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers.Housing;
using LlamaLibrary.Helpers.NPC;
using LlamaLibrary.RemoteWindows;

namespace LlamaLibrary.Helpers.HousingTravel.Districts
{
    public class Empyreum : ResidentialDistrict<Empyreum>
    {
        public override string Name => "Empyreum";
        public override uint TownAetheryteId => 70;
        public override ushort ZoneId => (ushort)HousingZone.Empyreum;

        private readonly List<HousingAetheryte> aetherytes = new List<HousingAetheryte>()
        {
            new HousingAetheryte(65, 2011677, "Highmorn's Horizon", new Vector3(49.22337f, -16f, 173.9398f), false),
            new HousingAetheryte(66, 2011678, "Empyreum Southeast", new Vector3(149.2002f, -50f, 94.79701f), false),
            new HousingAetheryte(67, 2011679, "Central Empyreum", new Vector3(69.14905f, -18.4997f, 7.661618f), false),
            new HousingAetheryte(68, 2011680, "Empyreum Southwest", new Vector3(-71.70401f, 6.068961E-14f, 77.35139f), false),
            new HousingAetheryte(69, 2011681, "Ingleside", new Vector3(-136.1852f, 10f, -18.23948f), false),
            new HousingAetheryte(70, 2011682, "Empyreum Northwest", new Vector3(-95.23731f, -2.004401E-16f, -113.391f), false),
            new HousingAetheryte(71, 2011683, "Empyreum East", new Vector3(198.0256f, -40f, -12.60111f), false),
            new HousingAetheryte(72, 2011684, "Empyreum Northeast", new Vector3(111.9527f, -20f, -107.5535f), false),
            new HousingAetheryte(81, 2012252, "The Halberd's Head", new Vector3(-49.284f, -20.00002f, -0.381f), false),
            new HousingAetheryte(73, 2011685, "Highmorn's Horizon Subdivision", new Vector3(-879.1049f, -16f, -655.8184f), true),
            new HousingAetheryte(74, 2011686, "Empyreum Southwest Subdivision", new Vector3(-796.9323f, -50f, -555.6836f), true),
            new HousingAetheryte(75, 2011687, "Central Empyreum Subdivision", new Vector3(-710.9193f, -18.49974f, -634.8365f), true),
            new HousingAetheryte(76, 2011688, "Empyreum Northwest Subdivision", new Vector3(-780.9319f, 7.757503E-12f, -774.3503f), true),
            new HousingAetheryte(77, 2011689, "Ingleside Subdivision", new Vector3(-686.4949f, 10f, -840.9035f), true),
            new HousingAetheryte(78, 2011690, "Empyreum Northeast Subdivision", new Vector3(-591.9549f, -1.120468E-11f, -798.5291f), true),
            new HousingAetheryte(79, 2011691, "Empyreum South Subdivision", new Vector3(-690.0454f, -40f, -504.3877f), true),
            new HousingAetheryte(80, 2011692, "Empyreum Southeast Subdivision", new Vector3(-595.2066f, -20f, -592.1836f), true),
            new HousingAetheryte(82, 2012253, "The Halberd's Head Subdivision", new Vector3(-703.2429f, -20.00002f, -754.7298f), true),
        };

        public override List<HousingAetheryte> Aetherytes => aetherytes;
        public override int RequiredQuest => 69708;

        public override bool OffMesh => true;

        private readonly List<Npc> _transitionNpcIds = new List<Npc>() { new Npc(1035622, 979, new Vector3(13.71777f, -15.2f, 182.9403f)), new Npc(1035622, 979, new Vector3(-887.1443f, -15.2f, -690.3029f)) };
        public override List<Npc> TransitionNpcs => _transitionNpcIds;

        private readonly Npc _gateKeeperNpc = new Npc(1031682, 418, new Vector3(152.9716f, -20f, 63.76746f)); //Thomelin (Gatekeep)

        public override async Task<bool> WalkToResidential()
        {
            if (!await Navigation.GetToNpc(_gateKeeperNpc))
            {
                return false;
            }

            var unit = _gateKeeperNpc.GameObject;

            if (unit == default(GameObject))
            {
                return false;
            }

            if (!unit.IsWithinInteractRange)
            {
                await Navigation.OffMeshMoveInteract(unit);
            }

            unit.Target();
            unit.Interact();

            await Coroutine.Wait(5000, () => Conversation.IsOpen || Talk.DialogOpen);

            if (Talk.DialogOpen)
            {
                while (Talk.DialogOpen)
                {
                    Talk.Next();
                    await Coroutine.Wait(100, () => !Talk.DialogOpen);
                    await Coroutine.Wait(100, () => Talk.DialogOpen);
                    await Coroutine.Yield();
                }

                await Coroutine.Wait(5000, () => Conversation.IsOpen);
            }

            int test = 0;
            foreach (var line in Conversation.GetConversationList)
            {
                if (line.Contains(Translator.EnterEmpyreum))
                {
                    break;
                }

                test++;
            }

            if (test == Conversation.GetConversationList.Count)
            {
                return false;
            }

            Conversation.SelectLine((uint)test);

            if (!await Coroutine.Wait(30000, () => CommonBehaviors.IsLoading))
            {
                return false;
            }

            await Coroutine.Wait(-1, () => (!CommonBehaviors.IsLoading));

            return true;
        }
    }
}