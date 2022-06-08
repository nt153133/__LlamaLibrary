using System.Linq;
using System.Threading.Tasks;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Helpers;
using ff14bot;
using LlamaLibrary.Extensions;

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
            return (int) InventoryManager.FilledInventoryAndArmory.FirstOrDefault(i => i.RawItemId == (uint) itemID).SpiritBond;
        }

        public static int HighestILvl(ClassJobType job)
        {
            var sets = GearsetManager.GearSets.Where(g => g.InUse && g.Class == job && g.Gear.Any());
            return sets.Any() ? sets.Max(GeneralFunctions.GetGearSetiLvl) : 0;
        }

        public static bool IsFateActive(int fateID)
        {
            return FateManager.ActiveFates.Any(i => i.Id == (uint) fateID);
        }

        public static bool HasLearnedMount(int mountID)
        {
            return ActionManager.AvailableMounts.Any(i => i.Id == ((uint) mountID));
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
            if (isLisbethPresentCache is null)
            {
                isLisbethPresentCache = BotManager.Bots
                    .FirstOrDefault(c => c.Name == "Lisbeth") != null;
            }

            return isLisbethPresentCache.Value;
        }

        public static bool IsTargetableNPC(int npcID)
        {
            return GameObjectManager.GameObjects.Any(i => i.NpcId == (uint) npcID && i.IsVisible && i.IsTargetable);
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

            var instanceDirector = (ff14bot.Directors.InstanceContentDirector) DirectorManager.ActiveDirector;
            return instanceDirector.InstanceEnded;
        }

        public static int SharedFateRank(int zoneID)
        {
            return SharedFateHelper.CachedProgress.FirstOrDefault(i => i.Zone == (uint) zoneID).Rank;
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
            return (int) Core.Me.GCRank();
        }

        public static bool IsFendingClass()
        {
            switch (Core.Me.CurrentJob)
            {
                case ClassJobType.Marauder:
                case ClassJobType.Warrior:
                case ClassJobType.Gladiator:
                case ClassJobType.Paladin:
                case ClassJobType.Gunbreaker:
                case ClassJobType.DarkKnight:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsCastingClass()
        {
            switch (Core.Me.CurrentJob)
            {
                case ClassJobType.Arcanist:
                case ClassJobType.Summoner:
                case ClassJobType.Thaumaturge:
                case ClassJobType.BlackMage:
                case ClassJobType.RedMage:
                case ClassJobType.BlueMage:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsHealingClass()
        {
            switch (Core.Me.CurrentJob)
            {
                case ClassJobType.Scholar:
                case ClassJobType.Conjurer:
                case ClassJobType.WhiteMage:
                case ClassJobType.Astrologian:
                case ClassJobType.Sage:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsAimingClass()
        {
            switch (Core.Me.CurrentJob)
            {
                case ClassJobType.Archer:
                case ClassJobType.Bard:
                case ClassJobType.Dancer:
                case ClassJobType.Machinist:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsMaimingClass()
        {
            switch (Core.Me.CurrentJob)
            {
                case ClassJobType.Lancer:
                case ClassJobType.Dragoon:
                case ClassJobType.Reaper:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsStrikingClass()
        {
            switch (Core.Me.CurrentJob)
            {
                case ClassJobType.Pugilist:
                case ClassJobType.Monk:
                case ClassJobType.Samurai:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsScoutingClass()
        {
            switch (Core.Me.CurrentJob)
            {
                case ClassJobType.Rogue:
                case ClassJobType.Ninja:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSlayingClass()
        {
            switch (Core.Me.CurrentJob)
            {
                case ClassJobType.Pugilist:
                case ClassJobType.Monk:
                case ClassJobType.Lancer:
                case ClassJobType.Dragoon:
                case ClassJobType.Samurai:
                case ClassJobType.Reaper:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsDiscipleofWarClass()
        {
            switch (Core.Me.CurrentJob)
            {
                case ClassJobType.Pugilist:
                case ClassJobType.Monk:
                case ClassJobType.Lancer:
                case ClassJobType.Dragoon:
                case ClassJobType.Samurai:
                case ClassJobType.Rogue:
                case ClassJobType.Ninja:
                case ClassJobType.Archer:
                case ClassJobType.Bard:
                case ClassJobType.Dancer:
                case ClassJobType.Machinist:
                case ClassJobType.Marauder:
                case ClassJobType.Warrior:
                case ClassJobType.Gladiator:
                case ClassJobType.Paladin:
                case ClassJobType.Gunbreaker:
                case ClassJobType.DarkKnight:
                case ClassJobType.Reaper:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsDiscipleofMagicClass()
        {
            switch (Core.Me.CurrentJob)
            {
                case ClassJobType.Arcanist:
                case ClassJobType.Summoner:
                case ClassJobType.Thaumaturge:
                case ClassJobType.BlackMage:
                case ClassJobType.RedMage:
                case ClassJobType.BlueMage:
                case ClassJobType.Scholar:
                case ClassJobType.Conjurer:
                case ClassJobType.WhiteMage:
                case ClassJobType.Astrologian:
                case ClassJobType.Sage:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNearShortcut(int npcID)
        {
            var npc = GameObjectManager.GetObjectByNPCId((uint) npcID);
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
            return InventoryManager.FilledSlots.Any(i => list.Contains((int) i.RawItemId));
        }
    }
}