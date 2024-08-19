using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.NeoProfiles;
using ff14bot.RemoteWindows;
using LlamaLibrary;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.Helpers.WorldTravel;
using LlamaLibrary.JsonObjects.Lisbeth;
using LlamaLibrary.Logging;
using LlamaLibrary.Retainers;
using static ff14bot.RemoteWindows.Talk;

namespace LlamaLibrary.Utilities
{
    public static class RetainerJobs
    {
        private static readonly LLogger Log = new("They Took Our Jobs", Colors.Khaki);

        // List of the very first weapon available for each class
        public static List<KeyValuePair<ClassJobType, int>> StarterWeapons = new List<KeyValuePair<ClassJobType, int>>()
        {
            new KeyValuePair<ClassJobType, int>(ClassJobType.Alchemist, 2467),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Arcanist, 2142),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Archer, 1889),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Armorer, 2366),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Blacksmith, 2340),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Botanist, 2545),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Carpenter, 2314),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Conjurer, 1995),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Culinarian, 2493),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Fisher, 2571),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Gladiator, 1601),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Goldsmith, 2391),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Lancer, 1819),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Leatherworker, 2416),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Marauder, 1749),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Miner, 2519),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Pugilist, 1680),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Thaumaturge, 2055),
            new KeyValuePair<ClassJobType, int>(ClassJobType.Rogue, 7952),

            new KeyValuePair<ClassJobType, int>(ClassJobType.Astrologian, 10524), // Star Globe
            new KeyValuePair<ClassJobType, int>(ClassJobType.Bard, 1889), // Weathered Shortbow
            new KeyValuePair<ClassJobType, int>(ClassJobType.BlackMage, 2055), // Weathered Scepter
            new KeyValuePair<ClassJobType, int>(ClassJobType.Dancer, 25644), // Deepgold War Quoits
            new KeyValuePair<ClassJobType, int>(ClassJobType.DarkKnight, 10400), // Steel Claymore
            new KeyValuePair<ClassJobType, int>(ClassJobType.Gunbreaker, 25643), // Deepgold Gunblade
            new KeyValuePair<ClassJobType, int>(ClassJobType.Paladin, 1601), // Weathered Shortsword
            new KeyValuePair<ClassJobType, int>(ClassJobType.Dragoon, 1819), // Weathered Spear
            new KeyValuePair<ClassJobType, int>(ClassJobType.Warrior, 1749), // Weathered War Axe
            new KeyValuePair<ClassJobType, int>(ClassJobType.Monk, 1680), // Weathered Hora
            new KeyValuePair<ClassJobType, int>(ClassJobType.Machinist, 10462), // Steel-Barreled Carbine
            new KeyValuePair<ClassJobType, int>(ClassJobType.Ninja, 7952), // Weathered Daggers
            new KeyValuePair<ClassJobType, int>(ClassJobType.Reaper, 35760), // Deepgold War Scythe
            new KeyValuePair<ClassJobType, int>(ClassJobType.RedMage, 18203), // Koppranickel Rapier
            new KeyValuePair<ClassJobType, int>(ClassJobType.Sage, 35778), // Stonegold Milpreves
            new KeyValuePair<ClassJobType, int>(ClassJobType.Samurai, 18046), // High Steel Tachi
            new KeyValuePair<ClassJobType, int>(ClassJobType.Scholar, 34091), // Gaja Codex
            new KeyValuePair<ClassJobType, int>(ClassJobType.Summoner, 2142), // Weathered Grimoire
            new KeyValuePair<ClassJobType, int>(ClassJobType.WhiteMage, 1995), // Weathered Cane
        };

        public static async Task<bool> GiveThemJobs(string job)
        {
            var foundJob = Enum.TryParse(job.Trim(), true, out ClassJobType newjob);
            var weapon = StarterWeapons.FirstOrDefault(x => x.Key == newjob).Value;

            // Check to see if you've completed the quest 'An Ill-conceived Venture' for your starting city
            // Only one of these need to be completed
            if (!QuestLogManager.IsQuestCompleted(66968) && !QuestLogManager.IsQuestCompleted(66969) && !QuestLogManager.IsQuestCompleted(66970))
            {
                Log.Error("Retainer ventures are not unlocked");
                TreeRoot.Stop("Retainer ventures are not unlocked");
                return true;
            }

            if (Core.Me.Levels[newjob] < 1)
            {
                Log.Error($"You need to unlock {job} first");
                TreeRoot.Stop($"You need to unlock {job} first");
                return true;
            }

            if (!await WorldTravel.MakeSureHome())
            {
                Log.Error("Could not get to home point");
                TreeRoot.Stop("Could not get to home point");
                return true;
            }

            var retainers = await HelperFunctions.GetOrderedRetainerArray(true);

            var unemployed = retainers.Where(i => i.Job == ClassJobType.Adventurer);
            var count = unemployed.Count();

            if (!ConditionParser.HasAtLeast((uint)weapon, count))
            {
                if (!await Lisbeth.IsProductKeyValid())
                {
                    Log.Error("Lisbeth key is not valid");
                    TreeRoot.Stop("Lisbeth key is not valid");
                    return true;
                }

                var order = new Order()
                {
                    Amount = (uint)count,
                    AmountMode = AmountMode.Restock,
                    Item = (uint)weapon,
                    Type = SourceType.Purchase
                };

                if (!await Lisbeth.ExecuteOrders(new List<Order>() { order }.GetOrderJson()))
                {
                    Log.Error("Could not purchase pickaxes");
                    TreeRoot.Stop("Could not purchase pickaxes");
                    return true;
                }
            }

            foreach (var retainer in unemployed)
            {
                Log.Information($"Assigning retainer {retainer.Name} to {job}");
                if (!await RetainerRoutine.SelectRetainer(retainer.Unique))
                {
                    Log.Error($"Could not select retainer {retainer.Name}");
                    TreeRoot.Stop($"Could not select retainer {retainer.Name}");
                    return true;
                }

                if (!SelectString.IsOpen)
                {
                    Log.Error($"Could not open retainer menu for {retainer.Name}");
                    TreeRoot.Stop($"Could not open retainer menu for {retainer.Name}");
                    return true;
                }

                if (!SelectString.ClickLineContains("Assign retainer class"))
                {
                    Log.Error($"Could not find Assign retainer class for {retainer.Name}");
                    TreeRoot.Stop($"Could not find Assign retainer class for {retainer.Name}");
                    return true;
                }

                if (!await Coroutine.Wait(5000, () => DialogOpen || !SelectString.IsOpen))
                {
                    Log.Error($"Dialog did not open for {retainer.Name}");
                    TreeRoot.Stop($"Dialog did not open for {retainer.Name}");
                    return true;
                }

                if (DialogOpen)
                {
                    while (!SelectString.IsOpen)
                    {
                        if (DialogOpen)
                        {
                            Next();
                            await Coroutine.Sleep(500);
                        }

                        await Coroutine.Wait(1500, () => DialogOpen || SelectString.IsOpen);
                    }
                }
                else
                {
                    await Coroutine.Sleep(500);
                }

                if (!SelectString.IsOpen)
                {
                    while (!SelectString.IsOpen)
                    {
                        if (DialogOpen)
                        {
                            Next();
                            await Coroutine.Sleep(500);
                        }

                        await Coroutine.Wait(1500, () => DialogOpen || SelectString.IsOpen);
                    }
                }

                if (!SelectString.ClickLineContains($"{job}"))
                {
                    Log.Error($"Could not find {job} Line for {retainer.Name}");
                    TreeRoot.Stop($"Could not find {job} for {retainer.Name}");
                    return true;
                }

                if (!await Coroutine.Wait(5000, () => DialogOpen || SelectYesno.IsOpen))
                {
                    Log.Error($"Post job dialog did not open for {retainer.Name}");
                    TreeRoot.Stop($"Post job dialog did not open for {retainer.Name}");
                    return true;
                }

                if (DialogOpen)
                {
                    while (!SelectYesno.IsOpen)
                    {
                        if (DialogOpen)
                        {
                            Next();
                            await Coroutine.Sleep(100);
                        }

                        await Coroutine.Wait(1500, () => DialogOpen || SelectYesno.IsOpen);
                    }
                }
                else
                {
                    await Coroutine.Sleep(500);
                }

                if (!await Coroutine.Wait(5000, () => SelectYesno.IsOpen))
                {
                    Log.Error($"YesNo dialog did not open for {retainer.Name}");
                    TreeRoot.Stop($"YesNo dialog did not open for {retainer.Name}");
                    return true;
                }

                SelectYesno.ClickYes();

                if (!await Coroutine.Wait(5000, () => DialogOpen || SelectString.IsOpen))
                {
                    Log.Error($"Post YesNo dialog did not open for {retainer.Name}");
                    TreeRoot.Stop($"Post YesNo dialog did not open for {retainer.Name}");
                    return true;
                }

                if (DialogOpen)
                {
                    while (!SelectString.IsOpen)
                    {
                        if (DialogOpen)
                        {
                            Next();
                            await Coroutine.Sleep(100);
                        }

                        await Coroutine.Wait(1500, () => DialogOpen || SelectString.IsOpen);
                    }
                }
                else
                {
                    await Coroutine.Sleep(500);
                }

                if (!SelectString.IsOpen)
                {
                    Log.Error($"Retainer Menu did not open for {retainer.Name}");
                    TreeRoot.Stop($"Retainer Menu did not open for {retainer.Name}");
                    return true;
                }

                var item = InventoryManager.FilledSlots.FirstOrDefault(i => i.RawItemId == weapon);
                if (item == null)
                {
                    Log.Error($"Could not find pickaxe for {retainer.Name}");
                    TreeRoot.Stop($"Could not find pickaxe for {retainer.Name}");
                    return true;
                }

                Log.Information($"Equipping pickaxe for {retainer.Name}");
                item.Move(InventoryManager.GetBagByInventoryBagId(InventoryBagId.Retainer_EquippedItems)[EquipmentSlot.MainHand]);
                await LlamaLibrary.Extensions.BagSlotExtensions.BagSlotNotFilledWait(item, 600);

                Log.Information("DeSelecting Retainer");

                await RetainerRoutine.DeSelectRetainer();
            }

            await RetainerRoutine.CloseRetainers();

            return true;
        }
    }
}