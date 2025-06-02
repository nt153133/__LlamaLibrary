using System.Linq;
using System.Threading.Tasks;
using ff14bot;
using ff14bot.Directors;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Data;
using LlamaLibrary.Extensions;
using LlamaLibrary.Helpers;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;

namespace LlamaLibrary.ScriptConditions
{
    public static class Extras
    {
        private static bool? isLisbethPresentCache;

        public static int NumAttackableEnemies(float dist = 0, params uint[] ids)
        {
            if (ids.Length == 0)
            {
                if (dist > 0)
                {
                    return GameObjectManager.GetObjectsOfType<BattleCharacter>().Count(i => i.CanAttack && i.IsTargetable && i.Distance() < dist);
                }

                return GameObjectManager.GetObjectsOfType<BattleCharacter>().Count(i => i.CanAttack && i.IsTargetable);
            }

            if (dist > 0)
            {
                return GameObjectManager.GetObjectsByNPCIds<BattleCharacter>(ids).Count(i => i.CanAttack && i.IsTargetable && i.Distance() < dist);
            }

            return GameObjectManager.GetObjectsByNPCIds<BattleCharacter>(ids).Count(i => i.CanAttack && i.IsTargetable);
        }

        public static int SphereCompletion(int itemId)
        {
            return (int)(InventoryManager.FilledInventoryAndArmory.FirstOrDefault(i => i.RawItemId == (uint)itemId)?.SpiritBond ?? 0);
        }

        public static int HighestILvl(ClassJobType job)
        {
            var sets = GearsetManager.GearSets.Where(g => g.InUse && g.Class == job && g.Gear.Length != 0).ToList();
            return sets.Count != 0 ? sets.Max(GeneralFunctions.GetGearSetiLvl) : 0;
        }

        public static bool IsFateActive(int fateID)
        {
            return FateManager.ActiveFates.Any(i => i.Id == (uint)fateID);
        }

        public static bool IsAnyFateActive()
        {
            return FateManager.ActiveFates.Any();
        }

        public static bool IsLeveActive(int leveId)
        {
            if (DirectorManager.ActiveDirector == null)
            {
                return false;
            }

            if (DirectorManager.ActiveDirector is LeveDirector activeAsLeve)
            {
                if (activeAsLeve.LeveId == leveId)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsLeveComplete(int leveId)
        {
            var leve = LeveManager.Leves.FirstOrDefault(i => i.GlobalId == leveId);
            var step = leve?.Step;
            return step == 255;
        }

        public static bool HasLeve(int leveId)
        {
            return GuildLeve.HasLeve((uint)leveId);
        }

        public static bool HasLearnedMount(int mountId)
        {
            return ActionManager.AvailableMounts.Any(i => i.Id == (uint)mountId);
        }

        public static int BeastTribeRank(int tribeId)
        {
            return BeastTribeHelper.GetBeastTribeRank(tribeId);
        }

        public static int DailyQuestAllowance()
        {
            return BeastTribeHelper.DailyQuestAllowance();
        }

        public static bool LisbethPresent()
        {
            isLisbethPresentCache ??= BotManager.Bots
                .FirstOrDefault(c => c.Name == "Lisbeth") != null;

            return isLisbethPresentCache.Value;
        }

        public static bool IsTargetableNPC(int npcId)
        {
            return GameObjectManager.GameObjects.Any(i => i.NpcId == (uint)npcId && i.IsVisible && i.IsTargetable);
        }

        public static bool AchievementComplete(int achId)
        {
            return Achievements.HasAchievement(achId);
        }

        public static bool IsDutyEnded()
        {
            if (DirectorManager.ActiveDirector == null)
            {
                return true;
            }

            var instanceDirector = (InstanceContentDirector)DirectorManager.ActiveDirector;
            return instanceDirector.InstanceEnded;
        }

        public static int SharedFateRank(int zoneId)
        {
            return SharedFateHelper.CachedProgress.FirstOrDefault(i => i.Zone == (uint)zoneId).Rank;
        }

        public static async Task UpdateSharedFates()
        {
            await SharedFateHelper.CachedRead();
        }

        public static async Task UpdateFishGuide()
        {
            await FishGuideHelper.CachedRead();
        }

        public static bool HasCaughtFish(int fishId)
        {
            return FishGuideHelper.GetFishSync(fishId).HasCaught;
        }

        public static int LLItemCollectableCount(int itemId)
        {
            return InventoryManager.FilledSlots.Count(i => i.RawItemId == itemId && i.IsCollectable);
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
                _                                                                                                                                                     => false
            };
        }

        public static bool IsCastingClass()
        {
            // (ClassJobType)0x2A Pictomancer
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Arcanist or ClassJobType.Summoner or ClassJobType.Thaumaturge or ClassJobType.BlackMage or ClassJobType.RedMage or ClassJobType.BlueMage or (ClassJobType)0x2A => true,
                _                                                                                                                                                                           => false
            };
        }

        public static bool IsHealingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Scholar or ClassJobType.Conjurer or ClassJobType.WhiteMage or ClassJobType.Astrologian or ClassJobType.Sage => true,
                _                                                                                                                        => false
            };
        }

        public static bool IsAimingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Archer or ClassJobType.Bard or ClassJobType.Dancer or ClassJobType.Machinist => true,
                _                                                                                                               => false
            };
        }

        public static bool IsMaimingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Lancer or ClassJobType.Dragoon or ClassJobType.Reaper => true,
                _                                                                  => false
            };
        }

        public static bool IsStrikingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Pugilist or ClassJobType.Monk or ClassJobType.Samurai => true,
                _                                                                  => false
            };
        }

        public static bool IsScoutingClass()
        {
            // (ClassJobType)0x29 Viper
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Rogue or ClassJobType.Ninja or (ClassJobType)0x29 => true,
                _                                                              => false
            };
        }

        public static bool IsSlayingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Pugilist or ClassJobType.Monk or ClassJobType.Lancer or ClassJobType.Dragoon or ClassJobType.Samurai or ClassJobType.Reaper => true,
                _                                                                                                                                        => false
            };
        }

        public static bool IsDiscipleofWarClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Pugilist or ClassJobType.Monk or ClassJobType.Lancer or ClassJobType.Dragoon or ClassJobType.Samurai or ClassJobType.Rogue or ClassJobType.Ninja or ClassJobType.Archer or ClassJobType.Bard or ClassJobType.Dancer or ClassJobType.Machinist or ClassJobType.Marauder or ClassJobType.Warrior or ClassJobType.Gladiator or ClassJobType.Paladin or ClassJobType.Gunbreaker or ClassJobType.DarkKnight or ClassJobType.Reaper
#if RB_DT
    or ClassJobType.Viper
#endif
                    => true,
                _ => false
            };
        }

        public static bool IsDiscipleofMagicClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Arcanist or ClassJobType.Summoner or ClassJobType.Thaumaturge or ClassJobType.BlackMage or ClassJobType.RedMage or ClassJobType.BlueMage or ClassJobType.Scholar or ClassJobType.Conjurer or ClassJobType.WhiteMage or ClassJobType.Astrologian or ClassJobType.Sage
#if RB_DT
    or ClassJobType.Pictomancer
#endif
                    => true,
                _ => false
            };
        }

        public static bool IsCraftingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Carpenter or ClassJobType.Blacksmith or ClassJobType.Armorer or ClassJobType.Goldsmith or ClassJobType.Leatherworker or ClassJobType.Weaver or ClassJobType.Alchemist or ClassJobType.Culinarian => true,
                _                                                                                                                                                                                                             => false
            };
        }

        public static bool IsGatheringClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Miner or ClassJobType.Botanist or ClassJobType.Fisher => true,
                _                                                                  => false
            };
        }

        public static bool IsNearShortcut(int npcId)
        {
            var npc = GameObjectManager.GetObjectByNPCId((uint)npcId);
            if (npc != null)
            {
                return npc.Distance(Core.Me.Location) <= 40 && npc.IsTargetable;
            }

            return false;
        }

        public static bool IsEnglishClient()
        {
            return Translator.Language == Language.Eng;
        }

        public static bool IsChineseClient()
        {
            return Translator.Language == Language.Chn;
        }

        public static bool IsJapaneseClient()
        {
            return Translator.Language == Language.Jap;
        }

        public static bool IsFrenchClient()
        {
            return Translator.Language == Language.Fre;
        }

        public static bool IsGermanClient()
        {
            return Translator.Language == Language.Ger;
        }

        public static bool HasAtLeastOneItem(params int[] list)
        {
            return InventoryManager.FilledSlots.Any(i => list.Contains((int)i.RawItemId));
        }

        public static int CurrentMount()
        {
            return Core.Me.CurrentMount();
        }

        public static bool AnyChestsToOpen()
        {
            return GameObjectManager.GetObjectsOfType<Treasure>().Any(i => i.State == 0);
        }

        public static bool BossHasAura(int npcId, int auraId)
        {
            var npc = GameObjectManager.GetObjectByNPCId<BattleCharacter>((uint)npcId);
            return npc != null && npc.HasAura((uint)auraId);
        }

        public static bool EmoteUnlocked(int Id)
        {
            return UIState.EmoteUnlocked(Id);
        }

        public static bool IsCardUnlocked(string name)
        {
            return TripleTriadCards.GetCardByName(name) is { IsUnlocked: true };
        }

        public static bool HasItemThatUnlocksCard(string name)
        {
            return TripleTriadCards.GetCardByName(name) is { HaveItem: true };
        }

        public static bool MinionUnlocked(int Id)
        {
            return UIState.MinionUnlocked(Id);
        }

        public static bool IsMVPVoteReady()
        {
            return DutyManager.InInstance && (AgentVoteMVP.Instance.CanToggle || VoteMvp.Instance.IsOpen);
        }

        public static bool IsInNPCParty()
        {
            return PartyManager.VisibleMembers.Any(i => i.GetType() == typeof(TrustPartyMember));
        }
    }
}