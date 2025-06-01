using System;
using ff14bot;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Hooks;

public class InstanceQuestDungeonHook : AsmFunctionHook
{

    public override string Name => "InstanceQuestDungeonHook";

    internal static class Offsets
    {
        //4? 55 53 57 4? 8d ?? ?4 c9 4? 81 ec ?? ?? ?? ?? 4? 8b 05 ?? ?? ?? ??
        // +9 = patch location // + 16 = hook location
        [Offset("Search 40 55 53 57 48 8d ? ? c9 48 81 ec ? ? ? ? 48 8b 05 ? ? ? ? 48 33 c4 48 89 45 ? 8b c2 41 8b f9 48 8b d9 2d ? ? ? ?")]
        internal static IntPtr PatchLocation;

        [Offset("Search 40 55 53 57 48 8d ? ? c9 48 81 ec ? ? ? ? 48 8b 05 ? ? ? ? 48 33 c4 48 89 45 ? 8b c2 41 8b f9 48 8b d9 2d ? ? ? ? Add C Read32")]
        public static IntPtr SubAmt;
    }

    public override IntPtr? Hook => Offsets.PatchLocation + 9;

    public static IntPtr StatePtr = Core.Memory.AllocateMemory(8);

    public static uint State => Core.Memory.Read<uint>(StatePtr);

    public static IntPtr SubAmt => Offsets.SubAmt;

    public override bool ShouldEnable => Initialized;

    public override bool Initialize()
    {
        var instructionTarget = Offsets.PatchLocation;

        if (Hook == null || Hook == IntPtr.Zero || Hook.Value.ToInt64() == 9 || instructionTarget == IntPtr.Zero)
        {
            return false;
        }

        JumpTo = Core.Memory.Executor.AllocNear(Offsets.PatchLocation + 9, 60, 64u);

        var asm = Core.Memory.Asm;
        asm.Clear();
        asm.AddLine("[org 0x{0:X16}]", (ulong)Offsets.PatchLocation + 9);
        asm.AddLine("JMP {0}", JumpTo);
        var jzPatch = asm.Assemble();



        asm.Clear();
        asm.AddLine("push rax");
        asm.AddLine("mov rax, [offsetPtr]");
        asm.AddLine("mov [rax], r8d");
        asm.AddLine("pop rax");
        asm.AddLine($"sub rsp, 0x{Offsets.SubAmt.ToInt32():X}");
        asm.AddLine("JMP [OriginalJmp]");
        asm.AddLine("[align 8]");
        asm.AddLine("OriginalJmp: dq {0}", Offsets.PatchLocation.ToInt64() + 16); //    MOV        RAX ,qword ptr [g_StackCookie ]

        asm.AddLine("offsetPtr: dq {0}", StatePtr.ToInt64());

        if (JumpTo == null)
        {
            return false;
        }

        asm.Inject(JumpTo.Value);

        JumpPatch = Core.Memory.Patches.Create(Hook.Value, jzPatch, Name);

        Initialized = true;

        return true;
    }
}