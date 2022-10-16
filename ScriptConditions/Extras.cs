using System.Linq;
using System.Threading.Tasks;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;

namespace LlamaLibrary.ScriptConditions
{
    public static class Extras
    {
        public static int NumAttackableEnemies(float dist = 0, params uint[] ids)
        {
            if (ids.Length == 0)
            {
                if (dist > 0)
                {
                    return GameObjectManager.GetObjectsOfType<BattleCharacter>().Count(i => i.CanAttack && i.IsTargetable && i.Distance() < dist);
                }
                else
                {
                    return GameObjectManager.GetObjectsOfType<BattleCharacter>().Count(i => i.CanAttack && i.IsTargetable);
                }
            }
            else
            {
                if (dist > 0)
                {
                    return GameObjectManager.GetObjectsByNPCIds<BattleCharacter>(ids).Count(i => i.CanAttack && i.IsTargetable && i.Distance() < dist);
                }
                else
                {
                    return GameObjectManager.GetObjectsByNPCIds<BattleCharacter>(ids).Count(i => i.CanAttack && i.IsTargetable);
                }
            }
        }

        public static int SphereCompletion(int itemID)
        {
            return (int)InventoryManager.FilledInventoryAndArmory.FirstOrDefault(i => i.RawItemId == (uint)itemID).SpiritBond;
        }

        public static int HighestILvl(ClassJobType job)
        {
            var sets = GearsetManager.GearSets.Where(g => g.InUse && g.Class == job && g.Gear.Any());
            return sets.Any() ? sets.Max(GeneralFunctions.GetGearSetiLvl) : 0;
        }

        public static bool IsFateActive(int fateID)
        {
            return FateManager.ActiveFates.Any(i => i.Id == (uint)fateID);
        }

        public static bool HasLearnedMount(int mountID)
        {
            return ActionManager.AvailableMounts.Any(i => i.Id == ((uint)mountID));
        }

        public static int BeastTribeRank(int tribeID)
        {
            return BeastTribeHelper.GetBeastTribeRank(tribeID);
        }

        public static int DailyQuestAllowance()
        {
            return BeastTribeHelper.DailyQuestAllowance();
        }

        private static bool? isLisbethPresentCache;

        public static bool LisbethPresent()
        {
            isLisbethPresentCache ??= BotManager.Bots
                    .FirstOrDefault(c => c.Name == "Lisbeth") != null;

            return isLisbethPresentCache.Value;
        }

        public static bool IsTargetableNPC(int npcID)
        {
            return GameObjectManager.GameObjects.Any(i => i.NpcId == (uint)npcID && i.IsVisible && i.IsTargetable);
        }

        public static bool AchievementComplete(int achID)
        {
            return Achievements.HasAchievement(achID);
        }

        public static bool IsDutyEnded()
        {
            if (DirectorManager.ActiveDirector == null)
            {
                return true;
            }

            var instanceDirector = (ff14bot.Directors.InstanceContentDirector)DirectorManager.ActiveDirector;
            return instanceDirector.InstanceEnded;
        }

        public static int SharedFateRank(int zoneID)
        {
            return SharedFateHelper.CachedProgress.FirstOrDefault(i => i.Zone == (uint)zoneID).Rank;
        }

        public static async Task UpdateSharedFates()
        {
            await SharedFateHelper.CachedRead();
        }

        public static int LLItemCollectableCount(int itemID)
        {
            return InventoryManager.FilledSlots.Count(i => i.RawItemId == itemID && i.IsCollectable);
        }

        public static int CurrentGCRank()
        {
            return (int)Core.Me.GCRank();
        }

        public static bool IsFendingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Marauder or ClassJobType.Warrior or ClassJobType.Gladiator or ClassJobType.Paladin or ClassJobType.Gunbreaker or ClassJobType.DarkKnight => true,
                _ => false,
            };
        }

        public static bool IsCastingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Arcanist or ClassJobType.Summoner or ClassJobType.Thaumaturge or ClassJobType.BlackMage or ClassJobType.RedMage or ClassJobType.BlueMage => true,
                _ => false,
            };
        }

        public static bool IsHealingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Scholar or ClassJobType.Conjurer or ClassJobType.WhiteMage or ClassJobType.Astrologian or ClassJobType.Sage => true,
                _ => false,
            };
        }

        public static bool IsAimingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Archer or ClassJobType.Bard or ClassJobType.Dancer or ClassJobType.Machinist => true,
                _ => false,
            };
        }

        public static bool IsMaimingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Lancer or ClassJobType.Dragoon or ClassJobType.Reaper => true,
                _ => false,
            };
        }

        public static bool IsStrikingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Pugilist or ClassJobType.Monk or ClassJobType.Samurai => true,
                _ => false,
            };
        }

        public static bool IsScoutingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Rogue or ClassJobType.Ninja => true,
                _ => false,
            };
        }

        public static bool IsSlayingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Pugilist or ClassJobType.Monk or ClassJobType.Lancer or ClassJobType.Dragoon or ClassJobType.Samurai or ClassJobType.Reaper => true,
                _ => false,
            };
        }

        public static bool IsDiscipleofWarClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Pugilist or ClassJobType.Monk or ClassJobType.Lancer or ClassJobType.Dragoon or ClassJobType.Samurai or ClassJobType.Rogue or ClassJobType.Ninja or ClassJobType.Archer or ClassJobType.Bard or ClassJobType.Dancer or ClassJobType.Machinist or ClassJobType.Marauder or ClassJobType.Warrior or ClassJobType.Gladiator or ClassJobType.Paladin or ClassJobType.Gunbreaker or ClassJobType.DarkKnight or ClassJobType.Reaper => true,
                _ => false,
            };
        }

        public static bool IsDiscipleofMagicClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Arcanist or ClassJobType.Summoner or ClassJobType.Thaumaturge or ClassJobType.BlackMage or ClassJobType.RedMage or ClassJobType.BlueMage or ClassJobType.Scholar or ClassJobType.Conjurer or ClassJobType.WhiteMage or ClassJobType.Astrologian or ClassJobType.Sage => true,
                _ => false,
            };
        }

        public static bool IsNearShortcut(int npcID)
        {
            var npc = GameObjectManager.GetObjectByNPCId((uint)npcID);
            if (npc != null)
            {
                return npc.Distance2D(Core.Me.Location) <= 30 && npc.IsTargetable;
            }

            return false;
        }

        public static bool IsChineseClient()
        {
            return Translator.Language == Language.Chn;
        }

        public static bool HasAtLeastOneItem(params int[] list)
        {
            return InventoryManager.FilledSlots.Any(i => list.Contains((int)i.RawItemId));
        }

        public static int CurrentMount()
        {
            var patternFinder = new GreyMagic.PatternFinder(Core.Memory);
            var offset = patternFinder.Find("66 83 B9 ? ? ? ? ? 48 8B DA Add 3 Read32").ToInt32();

            return (int)Core.Memory.Read<uint>(Core.Me.Pointer + offset);
        }
    }
}