using System;
using ff14bot;
using LlamaLibrary.Memory.Attributes;

namespace LlamaLibrary.Hooks;

public class InventoryUpdatePatch : AsmFunctionHook
{
    public override string Name => "InventoryUpdatePatch";

    internal static class Offsets
    {
        [Offset("Search E9 ? ? ? ? 48 83 C4 ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 8B 0D ? ? ? ?")]
        internal static IntPtr PatchLocation;

        [Offset("Search E9 ? ? ? ? 48 83 C4 ? C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? 48 8B 0D ? ? ? ? TraceCall")]
        internal static IntPtr OriginalJump;
    }

    public override IntPtr? Hook => Offsets.PatchLocation;

    public static IntPtr TickPtr = Core.Memory.AllocateMemory(8);

    public override bool ShouldEnable => Initialized;

    public override bool Initialize()
    {
        var instructionTarget = Offsets.OriginalJump;

        if (Hook == null || Hook == IntPtr.Zero || instructionTarget == IntPtr.Zero)
        {
            return false;
        }

        JumpTo = Core.Memory.Executor.AllocNear(Offsets.OriginalJump, 60, 64u);

        var asm = Core.Memory.Asm;
        asm.Clear();
        asm.AddLine("[org 0x{0:X16}]", (ulong)Offsets.PatchLocation);
        asm.AddLine("JMP {0}", JumpTo);
        var jzPatch = asm.Assemble();

        var procAddress = Core.Memory.GetProcAddress("kernel32", "GetTickCount");
        asm.Clear();
        asm.AddLine("push rcx");
        asm.AddLine("push rax");
        asm.AddLine("call [GetTickCount]");
        asm.AddLine("mov rcx, [TickPtr]");
        asm.AddLine("mov [rcx], rax");
        asm.AddLine("pop rax");
        asm.AddLine("pop rcx");
        asm.AddLine("JMP [OriginalJmp]");
        asm.AddLine("[align 8]");
        asm.AddLine("TickPtr: dq {0}", TickPtr.ToInt64());
        asm.AddLine("GetTickCount: dq {0}", procAddress);
        asm.AddLine("OriginalJmp: dq {0}", Offsets.OriginalJump.ToInt64());

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