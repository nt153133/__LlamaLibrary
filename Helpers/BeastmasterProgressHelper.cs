using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ff14bot;
using LlamaLibrary.Structs;

namespace LlamaLibrary.Helpers
{
    /// <summary>Reads permanent Beastmaster ranks and EXP independently of Bestiary selection or board-synced ranks.</summary>
    public static class BeastmasterProgressHelper
    {
        // Notebook detail-population calls, with Ghidra-derived operand masks for
        // calls, immediates and displacements. Each matches once on Global 2026.09.15;
        // two earlier Global builds and TC do not match. CN is unverified.
        private const string HandlerSite = "Search E8 ? ? ? ? BA ? ? ? ? 48 8B C8 E8 ? ? ? ? 48 8B F8 48 85 C0 0F 84 ? ? ? ? 8B 8E ? ? ? ?";
        private const string RankSite = "Search 0F B7 96 ? ? ? ? 48 8B CF E8 ? ? ? ? 44 0F B6 F8 45 85 FF 0F 84 ? ? ? ?";
        private const string ExperienceSite = "Search 0F B7 96 ? ? ? ? 48 8B CF E8 ? ? ? ? EB ? 8B D3 48 8B CF E8 ? ? ? ?";
        // Verify the notebook's event ID before calling the resolved handler lookup.
        private const uint BeastmasterEventId = 0xB03D0;
        private static object? resolvedMemory;
        private static int resolvedProcessId;
        private static IntPtr resolvedModule, frameworkSlot, findHandler;
        private static int rankOffset;

        /// <summary>Reads a detached snapshot for a freshly verified owned-species collection.</summary>
        /// <param name="ownedSpecies">Owned species IDs from the host's unlock API.</param>
        /// <param name="progress">Complete validated data, or null if data is absent or invalid.</param>
        /// <param name="reason">Initialization or validation failure description.</param>
        /// <returns>True only when every owned species has valid permanent progression.</returns>
        /// <remarks>Call on the bot thread or under an appropriate host frame context. Unsupported layouts throw before native calls.</remarks>
        public static bool TryRead(IEnumerable<uint> ownedSpecies, [NotNullWhen(true)] out IReadOnlyDictionary<uint, BeastmasterProgress>? progress, out string reason)
        {
            progress = null;
            reason = "The Beastmaster progression handler is not loaded.";
            Resolve();
            using (Core.Memory.AcquireFrame())
            using (Core.Memory.TemporaryCacheState(false))
            {
                var framework = Core.Memory.Read<IntPtr>(frameworkSlot);
                if (framework == IntPtr.Zero)
                {
                    return false;
                }

                // Reacquire the handler each read; its lifetime does not match the resolver's.
                var owner = Core.Memory.CallInjected64<IntPtr>(findHandler, framework, BeastmasterEventId);
                if (owner == IntPtr.Zero)
                {
                    return false;
                }
                byte[] bytes = Core.Memory.ReadBytes(owner + rankOffset, BeastmasterProgressDecoder.SpeciesCount * 2);
                return BeastmasterProgressDecoder.TryDecode(bytes, ownedSpecies, out progress, out reason);
            }
        }

        private static void Resolve()
        {
            var module = Core.Memory.Process.MainModule ?? throw new InvalidOperationException("No client module is attached.");
            if (ReferenceEquals(resolvedMemory, Core.Memory) && resolvedProcessId == Core.Memory.Process.Id &&
                resolvedModule == module.BaseAddress)
            {
                return;
            }
            using (var finder = new GreyMagic.PatternFinder(Core.Memory))
            {
                // The call boundaries below are instruction lengths, not game RVAs:
                // handler site CALL(5), MOV EDX,imm32(5), MOV RCX,RAX(3), CALL.
                IntPtr handlerSite = FindUnique(finder, HandlerSite);
                IntPtr rankSite = FindUnique(finder, RankSite);
                IntPtr expSite = FindUnique(finder, ExperienceSite);
                if (Core.Memory.Read<uint>(handlerSite + 6) != BeastmasterEventId)
                {
                    throw new NotSupportedException("The Bestiary progression event identifier changed.");
                }
                IntPtr instanceGetter = TraceCall(handlerSite);
                byte[] getter = Core.Memory.ReadBytes(instanceGetter, 8);
                // MOV RAX,[RIP+rel32]; RET: TraceRelative at byte3 identifies the
                // global pointer slot. Read that slot afresh for every snapshot.
                if (getter[0] != 0x48 || getter[1] != 0x8B || getter[2] != 0x05 || getter[7] != 0xC3)
                {
                    throw new NotSupportedException("The EventFramework accessor changed.");
                }
                IntPtr slot = instanceGetter + 7 + BitConverter.ToInt32(getter, 3);
                RequireModuleAddress(slot);
                IntPtr lookup = TraceCall(handlerSite + 13);
                // MOVZX EDX,[RSI+disp32](7), MOV RCX,RDI(3), CALL. The
                // two call targets must be the bounded byte-array getters.
                int rank = ReadArrayOffset(TraceCall(rankSite + 10));
                int exp = ReadArrayOffset(TraceCall(expSite + 10));
                // Reject implausible member offsets as well as a changed array layout.
                if (rank < 0 || rank > 0x10000 || exp != rank + BeastmasterProgressDecoder.SpeciesCount)
                {
                    throw new NotSupportedException("The permanent rank/EXP arrays are no longer contiguous.");
                }
                frameworkSlot = slot;
                findHandler = lookup;
                rankOffset = rank;
            }
            // Cache only a fully validated resolver for this attachment.
            resolvedModule = module.BaseAddress;
            resolvedProcessId = Core.Memory.Process.Id;
            resolvedMemory = Core.Memory;
        }

        private static IntPtr FindUnique(GreyMagic.PatternFinder finder, string pattern)
        {
            // FindMany exposes ambiguity that FindSingle would hide. With no
            // extraction suffix RB returns module RVAs and dontRebase=false;
            // verified against FindSingle(..., true) on the live EXP call site.
            bool dontRebase = false;
            IntPtr[] matches = finder.FindMany(pattern, ref dontRebase);
            if (dontRebase || matches == null || matches.Length != 1)
            {
                throw new NotSupportedException("This client does not uniquely match the permanent pet progression reader.");
            }
            var module = Core.Memory.Process.MainModule ?? throw new InvalidOperationException("No client module is attached.");
            IntPtr address = module.BaseAddress + checked((int)matches[0].ToInt64());
            RequireModuleAddress(address);
            return address;
        }

        private static IntPtr TraceCall(IntPtr instruction)
        {
            RequireModuleAddress(instruction);
            byte[] bytes = Core.Memory.ReadBytes(instruction, 5);
            if (bytes[0] != 0xE8)
            {
                throw new NotSupportedException("A Bestiary call-site instruction changed.");
            }
            IntPtr target = instruction + 5 + BitConverter.ToInt32(bytes, 1);
            RequireModuleAddress(target);
            return target;
        }

        private static int ReadArrayOffset(IntPtr getter)
        {
            byte[] bytes = Core.Memory.ReadBytes(getter, 27);
            // Nonzero ushort ID, subtract one, unsigned bound 50, byte[owner+index+disp].
            // Read32 at byte 19 extracts the array offset; both branches must return zero.
            byte?[] shape = { 0x66, 0x85, 0xD2, 0x74, null, 0x0F, 0xB7, 0xC2, 0xFF, 0xC8,
                0x83, 0xF8, null, 0x73, null, 0x0F, 0xB6, 0x84, 0x08, null, null, null, null, 0xC3, 0x32, 0xC0, 0xC3 };
            if (bytes.Length != shape.Length || shape.Where((value, i) => value.HasValue && value.Value != bytes[i]).Any() ||
                bytes[12] != BeastmasterProgressDecoder.SpeciesCount || 5 + unchecked((sbyte)bytes[4]) != 24 || 15 + unchecked((sbyte)bytes[14]) != 24)
            {
                throw new NotSupportedException("A permanent pet progression getter changed.");
            }
            return BitConverter.ToInt32(bytes, 19);
        }

        private static void RequireModuleAddress(IntPtr address)
        {
            var module = Core.Memory.Process.MainModule ?? throw new InvalidOperationException("No client module is attached.");
            long offset = address.ToInt64() - module.BaseAddress.ToInt64();
            if (offset < 0 || offset >= module.ModuleMemorySize)
            {
                throw new NotSupportedException("A Bestiary signature resolved outside the client module.");
            }
        }
    }
}
