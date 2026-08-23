using System;
using ff14bot;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Hooks;

public class InventoryUpdatePatch : AsmFunctionHook
{
    private const int RelativeJumpSize = 5;
    private const int TrampolineSize = 60;

    public override string Name => "InventoryUpdatePatch";
    public override IntPtr? Hook => InventoryUpdatePatchOffsets.PatchLocation;

    public static IntPtr TickPtr;

    public override bool ShouldEnable => Initialized;

    public override bool Initialize()
    {
        var hook = Hook;
        var instructionTarget = InventoryUpdatePatchOffsets.OriginalJump;

        if (hook is null || hook.Value == IntPtr.Zero || instructionTarget == IntPtr.Zero)
        {
            return false;
        }

        TickPtr = Core.Memory.AllocateMemory(sizeof(ulong));
        if (TickPtr == IntPtr.Zero)
        {
            return false;
        }

        var jumpOrigin = hook.Value + RelativeJumpSize;
        JumpTo = Core.Memory.Executor.AllocNear(jumpOrigin, TrampolineSize, 64u);
        if (JumpTo is null || JumpTo.Value == IntPtr.Zero)
        {
            return false;
        }

        var displacement = JumpTo.Value.ToInt64() - jumpOrigin.ToInt64();
        if (displacement is < int.MinValue or > int.MaxValue)
        {
            return false;
        }

        var asm = Core.Memory.Asm;
        byte[] jumpPatch;
        lock (Core.Memory.Executor.AssemblyLock)
        {
            try
            {
                asm.Clear();
                asm.AddLine("[org 0x{0:X16}]", (ulong)hook.Value);
                asm.AddLine("JMP 0x{0:X16}", (ulong)JumpTo.Value);
                jumpPatch = asm.Assemble();
                if (jumpPatch.Length != RelativeJumpSize)
                {
                    throw new InvalidOperationException($"Inventory jump patch assembled to {jumpPatch.Length} bytes instead of {RelativeJumpSize}.");
                }

                asm.Clear();
                asm.AddLine("pushfq");
                asm.AddLine("push rax");
                asm.AddLine("mov rax, [TickPtr]");
                asm.AddLine("lock inc qword [rax]");
                asm.AddLine("pop rax");
                asm.AddLine("popfq");
                asm.AddLine("JMP [OriginalJmp]");
                asm.AddLine("[align 8]");
                asm.AddLine("OriginalJmp: dq 0x{0:X16}", (ulong)instructionTarget);
                asm.AddLine("TickPtr: dq 0x{0:X16}", (ulong)TickPtr);
                var trampoline = asm.Assemble(JumpTo.Value);
                if (trampoline.Length > TrampolineSize)
                {
                    throw new InvalidOperationException($"Inventory trampoline assembled to {trampoline.Length} bytes, exceeding its {TrampolineSize}-byte allocation.");
                }

                Core.Memory.WriteBytes(JumpTo.Value, trampoline);
            }
            finally
            {
                asm.Clear();
            }
        }

        JumpPatch = Core.Memory.Patches.Create(hook.Value, jumpPatch, Name);

        Initialized = true;

        return true;
    }

    public override void Cleanup()
    {
        base.Cleanup();

        if (TickPtr != IntPtr.Zero)
        {
            Core.Memory.FreeMemory(TickPtr);
            TickPtr = IntPtr.Zero;
        }
    }
}
