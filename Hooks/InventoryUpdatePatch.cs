using System;
using ff14bot;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Hooks;

public class InventoryUpdatePatch : AsmFunctionHook
{
    public override string Name => "InventoryUpdatePatch";

    

    public override IntPtr? Hook => InventoryUpdatePatchOffsets.PatchLocation;

    public static IntPtr TickPtr = Core.Memory.AllocateMemory(8);

    public override bool ShouldEnable => Initialized;

    public override bool Initialize()
    {
        var instructionTarget = InventoryUpdatePatchOffsets.OriginalJump;

        if (Hook == null || Hook == IntPtr.Zero || instructionTarget == IntPtr.Zero)
        {
            return false;
        }

        JumpTo = Core.Memory.Executor.AllocNear(InventoryUpdatePatchOffsets.OriginalJump, 60, 64u);

        var asm = Core.Memory.Asm;
        asm.Clear();
        asm.AddLine("[org 0x{0:X16}]", (ulong)InventoryUpdatePatchOffsets.PatchLocation);
        asm.AddLine("JMP {0}", JumpTo);
        var jzPatch = asm.Assemble();

        var procAddress = Core.Memory.GetProcAddress("kernel32", "GetTickCount64");
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
        asm.AddLine("OriginalJmp: dq {0}", InventoryUpdatePatchOffsets.OriginalJump.ToInt64());
        asm.AddLine("TickPtr: dq {0}", TickPtr.ToInt64());
        asm.AddLine("GetTickCount: dq {0}", procAddress);

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