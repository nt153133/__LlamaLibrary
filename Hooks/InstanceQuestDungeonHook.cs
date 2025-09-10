using System;
using ff14bot;
using LlamaLibrary.Memory.Attributes;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Hooks;

public class InstanceQuestDungeonHook : AsmFunctionHook
{

    public override string Name => "InstanceQuestDungeonHook";

    

    public override IntPtr? Hook => InstanceQuestDungeonHookOffsets.PatchLocation + 9;

    public static IntPtr StatePtr = Core.Memory.AllocateMemory(8);

    public static uint State => Core.Memory.Read<uint>(StatePtr);

    public static IntPtr SubAmt => InstanceQuestDungeonHookOffsets.SubAmt;

    public override bool ShouldEnable => Initialized;

    public override bool Initialize()
    {
        var instructionTarget = InstanceQuestDungeonHookOffsets.PatchLocation;

        if (Hook == null || Hook == IntPtr.Zero || Hook.Value.ToInt64() == 9 || instructionTarget == IntPtr.Zero)
        {
            return false;
        }

        JumpTo = Core.Memory.Executor.AllocNear(InstanceQuestDungeonHookOffsets.PatchLocation + 9, 60, 64u);

        var asm = Core.Memory.Asm;
        asm.Clear();
        asm.AddLine("[org 0x{0:X16}]", (ulong)InstanceQuestDungeonHookOffsets.PatchLocation + 9);
        asm.AddLine("JMP {0}", JumpTo);
        var jzPatch = asm.Assemble();



        asm.Clear();
        asm.AddLine("push rax");
        asm.AddLine("mov rax, [offsetPtr]");
        asm.AddLine("mov [rax], r8d");
        asm.AddLine("pop rax");
        asm.AddLine($"sub rsp, 0x{InstanceQuestDungeonHookOffsets.SubAmt.ToInt32():X}");
        asm.AddLine("JMP [OriginalJmp]");
        asm.AddLine("[align 8]");
        asm.AddLine("OriginalJmp: dq {0}", InstanceQuestDungeonHookOffsets.PatchLocation.ToInt64() + 16); //    MOV        RAX ,qword ptr [g_StackCookie ]

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