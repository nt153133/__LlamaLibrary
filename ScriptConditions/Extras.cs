using System.Collections.Generic;
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
using LlamaLibrary.Memory;
using LlamaLibrary.RemoteAgents;
using LlamaLibrary.RemoteWindows;

namespace LlamaLibrary.ScriptConditions
{
    /// <summary>
    /// Provides additional condition-check methods for use in bot profiles and scripts.
    /// Covers battle character counts, fate status, job categories, client settings, and relic progress.
    /// </summary>
    public static class Extras
    {
        private static bool? isLisbethPresentCache;

        /// <summary>
        /// Returns the number of enemies that can be attacked and are currently targetable.
        /// </summary>
        /// <param name="dist">Optional maximum distance to search. If 0, searches all loaded objects.</param>
        /// <param name="ids">Optional list of specific NPC IDs to filter by.</param>
        /// <returns>The count of matching attackable enemies.</returns>
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

        /// <summary>
        /// Returns the spirit bond progress (0-100) of the specified item, typically used for relic sphere scrolls.
        /// </summary>
        /// <param name="itemId">The raw item ID to check.</param>
        /// <returns>The spirit bond value as an integer percentage.</returns>
        public static int SphereCompletion(int itemId)
        {
            return (int)(InventoryManager.FilledInventoryAndArmory.FirstOrDefault(i => i.RawItemId == (uint)itemId)?.SpiritBond ?? 0);
        }

        /// <summary>
        /// Returns the highest average item level among gear sets associated with the specified job.
        /// </summary>
        /// <param name="job">The <see cref="ClassJobType"/> to check.</param>
        /// <returns>The maximum item level found, or 0 if no sets exist for that job.</returns>
        public static int HighestILvl(ClassJobType job)
        {
            var sets = GearsetManager.GearSets.Where(g => g.InUse && g.Class == job && g.Gear.Length != 0).ToList();
            return sets.Count != 0 ? sets.Max(GeneralFunctions.GetGearSetiLvl) : 0;
        }

        /// <summary>
        /// Checks if a FATE with the specified ID is currently active in the current zone.
        /// </summary>
        /// <param name="fateID">The numeric ID of the FATE.</param>
        /// <returns><see langword="true"/> if the FATE is active; otherwise <see langword="false"/>.</returns>
        public static bool IsFateActive(int fateID)
        {
            return FateManager.ActiveFates.Any(i => i.Id == (uint)fateID);
        }

        /// <summary>
        /// Checks if any FATEs are currently active in the current zone.
        /// </summary>
        /// <returns><see langword="true"/> if at least one FATE is active; otherwise <see langword="false"/>.</returns>
        public static bool IsAnyFateActive()
        {
            return FateManager.ActiveFates.Any();
        }

        /// <summary>
        /// Determines if the specified levequest is the currently active quest director.
        /// </summary>
        /// <param name="leveId">The numeric ID of the levequest.</param>
        /// <returns><see langword="true"/> if the levequest is active; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Determines if the specified levequest is fully complete (Step 255).
        /// </summary>
        /// <param name="leveId">The numeric ID of the levequest.</param>
        /// <returns><see langword="true"/> if completed; otherwise <see langword="false"/>.</returns>
        public static bool IsLeveComplete(int leveId)
        {
            var leve = LeveManager.Leves.FirstOrDefault(i => i.GlobalId == leveId);
            var step = leve?.Step;
            return step == 255;
        }

        /// <summary>
        /// Checks if the player currently has the specified levequest in their journal.
        /// </summary>
        /// <param name="leveId">The numeric ID of the levequest.</param>
        /// <returns><see langword="true"/> if possessed; otherwise <see langword="false"/>.</returns>
        public static bool HasLeve(int leveId)
        {
            return GuildLeve.HasLeve((uint)leveId);
        }

        /// <summary>
        /// Checks if the player has learned the specified mount.
        /// </summary>
        /// <param name="mountId">The numeric ID of the mount.</param>
        /// <returns><see langword="true"/> if learned; otherwise <see langword="false"/>.</returns>
        public static bool HasLearnedMount(int mountId)
        {
            return ActionManager.AvailableMounts.Any(i => i.Id == (uint)mountId);
        }

        /// <summary>
        /// Gets the current reputation rank for the specified beast tribe.
        /// </summary>
        /// <param name="tribeId">The ID of the beast tribe.</param>
        /// <returns>The current rank as an integer.</returns>
        public static int BeastTribeRank(int tribeId)
        {
            return BeastTribeHelper.GetBeastTribeRank(tribeId);
        }

        /// <summary>
        /// Returns the number of remaining daily beast tribe quest allowances.
        /// </summary>
        /// <returns>The number of allowances left for the current day.</returns>
        public static int DailyQuestAllowance()
        {
            return BeastTribeHelper.DailyQuestAllowance();
        }

        /// <summary>
        /// Checks if the Lisbeth plugin is currently loaded in RebornBuddy.
        /// </summary>
        /// <returns><see langword="true"/> if Lisbeth is found; otherwise <see langword="false"/>.</returns>
        public static bool LisbethPresent()
        {
            isLisbethPresentCache ??= BotManager.Bots
                .FirstOrDefault(c => c.Name == "Lisbeth") != null;

            return isLisbethPresentCache.Value;
        }

        /// <summary>
        /// Checks if an NPC with the specified ID is currently visible and targetable in the game world.
        /// </summary>
        /// <param name="npcId">The numeric NPC ID to search for.</param>
        /// <returns><see langword="true"/> if the NPC is targetable; otherwise <see langword="false"/>.</returns>
        public static bool IsTargetableNPC(int npcId)
        {
            return GameObjectManager.GameObjects.Any(i => i.NpcId == (uint)npcId && i.IsVisible && i.IsTargetable);
        }

        /// <summary>
        /// Determines if the specified achievement has been completed by the player.
        /// </summary>
        /// <param name="achId">The numeric ID of the achievement.</param>
        /// <returns><see langword="true"/> if completed; otherwise <see langword="false"/>.</returns>
        public static bool AchievementComplete(int achId)
        {
            return Achievements.HasAchievement(achId);
        }

        /// <summary>
        /// Determines if the current duty instance has officially ended.
        /// </summary>
        /// <returns><see langword="true"/> if in an instance that has ended or if not in an instance; otherwise <see langword="false"/>.</returns>
        public static bool IsDutyEnded()
        {
            if (DirectorManager.ActiveDirector == null)
            {
                return true;
            }

            var instanceDirector = (InstanceContentDirector)DirectorManager.ActiveDirector;
            return instanceDirector.InstanceEnded;
        }

        /// <summary>
        /// Gets the player's shared fate rank in the specified zone.
        /// </summary>
        /// <param name="zoneId">The zone ID to check.</param>
        /// <returns>The shared fate rank (e.g. 1, 2, 3).</returns>
        public static int SharedFateRank(int zoneId)
        {
            return SharedFateHelper.CachedProgress.FirstOrDefault(i => i.Zone == (uint)zoneId).Rank;
        }

        /// <summary>
        /// Asynchronously triggers a refresh of the shared fate progress data from game memory.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task UpdateSharedFates()
        {
            await SharedFateHelper.CachedRead();
        }

        /// <summary>
        /// Asynchronously triggers a refresh of the fish guide completion data from game memory.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task UpdateFishGuide()
        {
            await FishGuideHelper.CachedRead();
        }

        /// <summary>
        /// Checks if the player has ever caught the specified fish.
        /// </summary>
        /// <param name="fishId">The numeric ID of the fish.</param>
        /// <returns><see langword="true"/> if caught; otherwise <see langword="false"/>.</returns>
        public static bool HasCaughtFish(int fishId)
        {
            return FishGuideHelper.GetFishSync(fishId).HasCaught;
        }

        /// <summary>
        /// Counts the number of items with the specified ID in the player's inventory that are marked as collectables.
        /// </summary>
        /// <param name="itemId">The numeric ID of the item.</param>
        /// <returns>The total count of matching collectable items.</returns>
        public static int LLItemCollectableCount(int itemId)
        {
            return InventoryManager.FilledSlots.Count(i => i.RawItemId == itemId && i.IsCollectable);
        }

        /// <summary>
        /// Returns the player's current Grand Company rank identifier.
        /// </summary>
        /// <returns>The rank index as an integer.</returns>
        public static int CurrentGCRank()
        {
            return (int)Core.Me.GCRank();
        }

        /// <summary>Returns whether the specified Challenge Log row is complete this week.</summary>
        public static bool ChallengeLogComplete(int rowId)
        {
            return RemoteWindows.ContentsNote.Instance.IsComplete(rowId);
        }

        /// <summary>Returns whether Timers reports Squadron enlistment papers awaiting review.</summary>
        public static bool SquadronNewRecruitAvailable()
        {
            SquadronStatus.Update();
            return SquadronStatus.Status.NewRecruits;
        }

        /// <summary>
        /// Checks if the player's current job is a tanking (Fending) class.
        /// </summary>
        /// <returns><see langword="true"/> if the player is a tank; otherwise <see langword="false"/>.</returns>
        public static bool IsFendingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Marauder or ClassJobType.Warrior or ClassJobType.Gladiator or ClassJobType.Paladin or ClassJobType.Gunbreaker or ClassJobType.DarkKnight => true,
                _                                                                                                                                                     => false
            };
        }

        /// <summary>
        /// Checks if the player's current job is a magical ranged DPS (Casting) class.
        /// </summary>
        /// <returns><see langword="true"/> if the player is a caster; otherwise <see langword="false"/>.</returns>
        public static bool IsCastingClass()
        {
            // (ClassJobType)0x2A Pictomancer
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Arcanist or ClassJobType.Summoner or ClassJobType.Thaumaturge or ClassJobType.BlackMage or ClassJobType.RedMage or ClassJobType.BlueMage or (ClassJobType)0x2A => true,
                _                                                                                                                                                                           => false
            };
        }

        /// <summary>
        /// Checks if the player's current job is a healer class.
        /// </summary>
        /// <returns><see langword="true"/> if the player is a healer; otherwise <see langword="false"/>.</returns>
        public static bool IsHealingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Scholar or ClassJobType.Conjurer or ClassJobType.WhiteMage or ClassJobType.Astrologian or ClassJobType.Sage => true,
                _                                                                                                                        => false
            };
        }

        /// <summary>
        /// Checks if the player's current job is a physical ranged DPS (Aiming) class.
        /// </summary>
        /// <returns><see langword="true"/> if the player is an aiming class; otherwise <see langword="false"/>.</returns>
        public static bool IsAimingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Archer or ClassJobType.Bard or ClassJobType.Dancer or ClassJobType.Machinist => true,
                _                                                                                         => false
            };
        }

        /// <summary>
        /// Checks if the player's current job is a Maiming melee DPS class.
        /// </summary>
        /// <returns><see langword="true"/> if the player is a maiming class; otherwise <see langword="false"/>.</returns>
        public static bool IsMaimingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Lancer or ClassJobType.Dragoon or ClassJobType.Reaper => true,
                _                                                                  => false
            };
        }

        /// <summary>
        /// Checks if the player's current job is a Striking melee DPS class.
        /// </summary>
        /// <returns><see langword="true"/> if the player is a striking class; otherwise <see langword="false"/>.</returns>
        public static bool IsStrikingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Pugilist or ClassJobType.Monk or ClassJobType.Samurai => true,
                _                                                                  => false
            };
        }

        /// <summary>
        /// Checks if the player's current job is a Scouting melee DPS class.
        /// </summary>
        /// <returns><see langword="true"/> if the player is a scouting class; otherwise <see langword="false"/>.</returns>
        public static bool IsScoutingClass()
        {
            // (ClassJobType)0x29 Viper
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Rogue or ClassJobType.Ninja or (ClassJobType)0x29 => true,
                _                                                              => false
            };
        }

        /// <summary>
        /// Checks if the player's current job is any melee DPS (Slaying) class.
        /// </summary>
        /// <returns><see langword="true"/> if the player is a melee DPS; otherwise <see langword="false"/>.</returns>
        public static bool IsSlayingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Pugilist or ClassJobType.Monk or ClassJobType.Lancer or ClassJobType.Dragoon or ClassJobType.Samurai or ClassJobType.Reaper => true,
                _                                                                                                                                        => false
            };
        }

        /// <summary>
        /// Checks if the player's current job is any Disciple of War class.
        /// </summary>
        /// <returns><see langword="true"/> if the player is a Disciple of War; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Checks if the player's current job is any Disciple of Magic class.
        /// </summary>
        /// <returns><see langword="true"/> if the player is a Disciple of Magic; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Checks if the player's current job is any Disciple of the Hand (crafter) class.
        /// </summary>
        /// <returns><see langword="true"/> if the player is a crafter; otherwise <see langword="false"/>.</returns>
        public static bool IsCraftingClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Carpenter or ClassJobType.Blacksmith or ClassJobType.Armorer or ClassJobType.Goldsmith or ClassJobType.Leatherworker or ClassJobType.Weaver or ClassJobType.Alchemist or ClassJobType.Culinarian => true,
                _                                                                                                                                                                                                             => false
            };
        }

        /// <summary>
        /// Checks if the player's current job is any Disciple of the Land (gatherer) class.
        /// </summary>
        /// <returns><see langword="true"/> if the player is a gatherer; otherwise <see langword="false"/>.</returns>
        public static bool IsGatheringClass()
        {
            return Core.Me.CurrentJob switch
            {
                ClassJobType.Miner or ClassJobType.Botanist or ClassJobType.Fisher => true,
                _                                                                  => false
            };
        }

        /// <summary>
        /// Determines if the specified NPC is nearby (within 40 yalms) and currently targetable.
        /// Useful for shortcut or interactive object checks in profiles.
        /// </summary>
        /// <param name="npcId">The numeric ID of the NPC or object.</param>
        /// <returns><see langword="true"/> if the NPC is nearby and targetable; otherwise <see langword="false"/>.</returns>
        public static bool IsNearShortcut(int npcId)
        {
            var npc = GameObjectManager.GetObjectByNPCId((uint)npcId);
            if (npc != null)
            {
                return npc.Distance(Core.Me.Location) <= 40 && npc.IsTargetable;
            }

            return false;
        }

        /// <summary>
        /// Checks if the player is running on the Global game client.
        /// </summary>
        /// <returns><see langword="true"/> if Global; otherwise <see langword="false"/>.</returns>
        public static bool IsGlobalClient()
        {
            return OffsetManager.ActiveRegion == ClientRegion.Global;
        }

        /// <summary>
        /// Checks if the player is running on the Traditional Chinese game client.
        /// </summary>
        /// <returns><see langword="true"/> if Traditional Chinese; otherwise <see langword="false"/>.</returns>
        public static bool IsTCClient()
        {
            return OffsetManager.ActiveRegion == ClientRegion.TraditionalChinese;
        }

        /// <summary>
        /// Checks if the player's client language is set to English.
        /// </summary>
        /// <returns><see langword="true"/> if English; otherwise <see langword="false"/>.</returns>
        public static bool IsEnglishClient()
        {
            return Translator.Language == Language.Eng;
        }

        /// <summary>
        /// Checks if the player's client language is set to Chinese.
        /// </summary>
        /// <returns><see langword="true"/> if Chinese; otherwise <see langword="false"/>.</returns>
        public static bool IsChineseClient()
        {
            return Translator.Language == Language.Chn;
        }

        /// <summary>
        /// Checks if the player's client language is set to Japanese.
        /// </summary>
        /// <returns><see langword="true"/> if Japanese; otherwise <see langword="false"/>.</returns>
        public static bool IsJapaneseClient()
        {
            return Translator.Language == Language.Jap;
        }

        /// <summary>
        /// Checks if the player's client language is set to French.
        /// </summary>
        /// <returns><see langword="true"/> if French; otherwise <see langword="false"/>.</returns>
        public static bool IsFrenchClient()
        {
            return Translator.Language == Language.Fre;
        }

        /// <summary>
        /// Checks if the player's client language is set to German.
        /// </summary>
        /// <returns><see langword="true"/> if German; otherwise <see langword="false"/>.</returns>
        public static bool IsGermanClient()
        {
            return Translator.Language == Language.Ger;
        }

        /// <summary>
        /// Checks if the player possesses at least one item from the provided list of item IDs in their inventory.
        /// </summary>
        /// <param name="list">A variable-length list of numeric item IDs.</param>
        /// <returns><see langword="true"/> if any matching item is found; otherwise <see langword="false"/>.</returns>
        public static bool HasAtLeastOneItem(params int[] list)
        {
            return InventoryManager.FilledSlots.Any(i => list.Contains((int)i.RawItemId));
        }

        /// <summary>
        /// Gets the ID of the mount the local player is currently riding.
        /// </summary>
        /// <returns>The mount identifier, or 0 if not mounted.</returns>
        public static int CurrentMount()
        {
            return Core.Me.CurrentMount();
        }

        /// <summary>
        /// Checks if there are any unopened treasure chests currently loaded in the game world.
        /// </summary>
        /// <returns><see langword="true"/> if any unopened chests exist; otherwise <see langword="false"/>.</returns>
        public static bool AnyChestsToOpen()
        {
            return GameObjectManager.GetObjectsOfType<Treasure>().Any(i => i.State == 0);
        }

        /// <summary>
        /// Determines if the specified NPC has a specific status effect (aura).
        /// </summary>
        /// <param name="npcId">The numeric ID of the NPC to check.</param>
        /// <param name="auraId">The numeric ID of the aura (status effect).</param>
        /// <returns><see langword="true"/> if the NPC has the aura; otherwise <see langword="false"/>.</returns>
        public static bool BossHasAura(int npcId, int auraId)
        {
            var npc = GameObjectManager.GetObjectByNPCId<BattleCharacter>((uint)npcId);
            return npc != null && npc.HasAura((uint)auraId);
        }

        /// <summary>
        /// Checks if the player has unlocked the specified emote.
        /// </summary>
        /// <param name="Id">The numeric ID of the emote.</param>
        /// <returns><see langword="true"/> if unlocked; otherwise <see langword="false"/>.</returns>
        public static bool EmoteUnlocked(int Id)
        {
            return UIState.EmoteUnlocked(Id);
        }

        /// <summary>
        /// Checks if a Triple Triad card with the specified name is unlocked for the current player.
        /// </summary>
        /// <param name="name">The English or localized name of the card.</param>
        /// <returns><see langword="true"/> if unlocked; otherwise <see langword="false"/>.</returns>
        public static bool IsCardUnlocked(string name)
        {
            return TripleTriadCards.GetCardByName(name) is { IsUnlocked: true };
        }

        /// <summary>
        /// Checks a TripleTriadCardResident row directly. Generated profiles use this overload so
        /// their conditions remain independent of the client's language.
        /// </summary>
        public static bool IsTripleTriadCardUnlocked(int cardId)
        {
            return cardId > 0 && UIState.CardUnlocked(cardId);
        }

        /// <summary>
        /// Checks whether the current character has not yet defeated a Triple Triad NPC.
        /// </summary>
        /// <param name="tripleTriadResidentId">The TripleTriadResident sheet row ID, not the ENpc ID.</param>
        public static bool HasNotBeatenNPC(uint tripleTriadResidentId)
        {
            return !UIState.TripleTriadNpcBeaten(tripleTriadResidentId);
        }

        /// <summary>
        /// Checks if the player has the item in their inventory required to unlock a specific Triple Triad card.
        /// </summary>
        /// <param name="name">The name of the card.</param>
        /// <returns><see langword="true"/> if the unlock item is possessed; otherwise <see langword="false"/>.</returns>
        public static bool HasItemThatUnlocksCard(string name)
        {
            return TripleTriadCards.GetCardByName(name) is { HaveItem: true };
        }

        /// <summary>
        /// Checks if the player has unlocked the specified minion.
        /// </summary>
        /// <param name="Id">The numeric ID of the minion.</param>
        /// <returns><see langword="true"/> if unlocked; otherwise <see langword="false"/>.</returns>
        public static bool MinionUnlocked(int Id)
        {
            return UIState.MinionUnlocked(Id);
        }

        /// <summary>
        /// Checks if the MVP voting window is currently available or open (in instances).
        /// </summary>
        /// <returns><see langword="true"/> if the vote window can be interacted with; otherwise <see langword="false"/>.</returns>
        public static bool IsMVPVoteReady()
        {
            return DutyManager.InInstance && (AgentVoteMVP.Instance.CanToggle || VoteMvp.Instance.IsOpen);
        }

        /// <summary>
        /// Checks if the current party includes NPC members (e.g. Trust NPCs).
        /// </summary>
        /// <returns><see langword="true"/> if NPCs are present in the party; otherwise <see langword="false"/>.</returns>
        public static bool IsInNPCParty()
        {
            return PartyManager.VisibleMembers.Any(i => i.GetType() == typeof(TrustPartyMember));
        }

        private static readonly Dictionary<ClassJobType, uint> PenumbraeWeaponIds =
            new Dictionary<ClassJobType, uint>
            {
                { ClassJobType.Paladin, 47869 },
                { ClassJobType.Monk, 47870 },
                { ClassJobType.Warrior, 47871 },
                { ClassJobType.Dragoon, 47872 },
                { ClassJobType.Bard, 47873 },
                { ClassJobType.WhiteMage, 47874 },
                { ClassJobType.BlackMage, 47875 },
                { ClassJobType.Summoner, 47876 },
                { ClassJobType.Scholar, 47877 },
                { ClassJobType.Ninja, 47878 },
                { ClassJobType.DarkKnight, 47879 },
                { ClassJobType.Machinist, 47880 },
                { ClassJobType.Astrologian, 47881 },
                { ClassJobType.Samurai, 47882 },
                { ClassJobType.RedMage, 47883 },
                { ClassJobType.Gunbreaker, 47884 },
                { ClassJobType.Dancer, 47885 },
                { ClassJobType.Reaper, 47886 },
                { ClassJobType.Sage, 47887 },
                { ClassJobType.Viper, 47888 },
                { ClassJobType.Pictomancer, 47889 },
            };

        private static readonly Dictionary<ClassJobType, uint> UmbreaWeaponIds =
            new Dictionary<ClassJobType, uint>
            {
                { ClassJobType.Paladin, 47006 },
                { ClassJobType.Monk, 47007 },
                { ClassJobType.Warrior, 47008 },
                { ClassJobType.Dragoon, 47009 },
                { ClassJobType.Bard, 47010 },
                { ClassJobType.WhiteMage, 47011 },
                { ClassJobType.BlackMage, 47012 },
                { ClassJobType.Summoner, 47013 },
                { ClassJobType.Scholar, 47014 },
                { ClassJobType.Ninja, 47015 },
                { ClassJobType.DarkKnight, 47016 },
                { ClassJobType.Machinist, 47017 },
                { ClassJobType.Astrologian, 47018 },
                { ClassJobType.Samurai, 47019 },
                { ClassJobType.RedMage, 47020 },
                { ClassJobType.Gunbreaker, 47021 },
                { ClassJobType.Dancer, 47022 },
                { ClassJobType.Reaper, 47023 },
                { ClassJobType.Sage, 47024 },
                { ClassJobType.Viper, 47025 },
                { ClassJobType.Pictomancer, 47026 },
            };

        private static readonly Dictionary<ClassJobType, uint> ObscurumWeaponIds =
            new Dictionary<ClassJobType, uint>
            {
                { ClassJobType.Paladin, 50032 },
                { ClassJobType.Monk, 50033 },
                { ClassJobType.Warrior, 50034 },
                { ClassJobType.Dragoon, 50035 },
                { ClassJobType.Bard, 50036 },
                { ClassJobType.WhiteMage, 50037 },
                { ClassJobType.BlackMage, 50038 },
                { ClassJobType.Summoner, 50039 },
                { ClassJobType.Scholar, 50040 },
                { ClassJobType.Ninja, 50041 },
                { ClassJobType.DarkKnight, 50042 },
                { ClassJobType.Machinist, 50043 },
                { ClassJobType.Astrologian, 50044 },
                { ClassJobType.Samurai, 50045 },
                { ClassJobType.RedMage, 50046 },
                { ClassJobType.Gunbreaker, 50047 },
                { ClassJobType.Dancer, 50048 },
                { ClassJobType.Reaper, 50049 },
                { ClassJobType.Sage, 50050 },
                { ClassJobType.Viper, 50051 },
                { ClassJobType.Pictomancer, 50052 },
            };

        /// <summary>
        /// Checks if the player has a Dawntrail relic weapon (Penumbrae tier) equipped for their current job.
        /// </summary>
        /// <returns><see langword="true"/> if the relic weapon is equipped; otherwise <see langword="false"/>.</returns>
        public static bool IsPenumbraeWeaponEquipped()
        {
            ClassJobType currentJob = Core.Me.CurrentJob;

            if (!PenumbraeWeaponIds.TryGetValue(currentJob, out uint expectedItemId))
                return false;

            var mainHand =
                InventoryManager
                    .GetBagByInventoryBagId(InventoryBagId.EquippedItems)
                    [EquipmentSlot.MainHand];

            if (mainHand == null)
                return false;

            return mainHand.RawItemId == expectedItemId;
        }

        /// <summary>
        /// Checks if the player has a Dawntrail relic weapon (Umbrae tier) equipped for their current job.
        /// </summary>
        /// <returns><see langword="true"/> if the relic weapon is equipped; otherwise <see langword="false"/>.</returns>
        public static bool IsUmbraeWeaponEquipped()
        {
            ClassJobType currentJob = Core.Me.CurrentJob;

            if (!UmbreaWeaponIds.TryGetValue(currentJob, out uint expectedItemId))
                return false;

            var mainHand =
                InventoryManager
                    .GetBagByInventoryBagId(InventoryBagId.EquippedItems)
                    [EquipmentSlot.MainHand];

            if (mainHand == null)
                return false;

            return mainHand.RawItemId == expectedItemId;
        }

        /// <summary>
        /// Checks if the player has a Dawntrail relic weapon (Obscurum tier) equipped for their current job.
        /// </summary>
        /// <returns><see langword="true"/> if the relic weapon is equipped; otherwise <see langword="false"/>.</returns>
        public static bool IsObscurumWeaponEquipped()
        {
            ClassJobType currentJob = Core.Me.CurrentJob;

            if (!ObscurumWeaponIds.TryGetValue(currentJob, out uint expectedItemId))
                return false;

            var mainHand =
                InventoryManager
                    .GetBagByInventoryBagId(InventoryBagId.EquippedItems)
                    [EquipmentSlot.MainHand];

            if (mainHand == null)
                return false;

            return mainHand.RawItemId == expectedItemId;
        }
    }
}
