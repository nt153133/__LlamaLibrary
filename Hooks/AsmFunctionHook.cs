using System;
using GreyMagic;

namespace LlamaLibrary.Hooks;

public abstract class AsmFunctionHook
{
    public virtual string Name { get; }

    public virtual string DisplayName => Name;
    public IntPtr? JumpTo { get; set; }
    public virtual int InitialInstructionSize { get; }
    public virtual IntPtr? Hook { get; }
    public bool Initialized { get; set; }
    public Patch? JumpPatch { get; set; }
    public virtual bool ShouldEnable { get; }

    public bool Enable
    {
        get
        {
            if (Initialized == false || JumpPatch == null)
            {
                return false;
            }

            return JumpPatch.IsApplied;
        }
        set
        {
            if (Initialized == false || JumpPatch == null)
            {
                return;
            }

            switch (value)
            {
                case true when JumpPatch.IsApplied == false:
                    JumpPatch.Apply();
                    OnHookStateChanged(this);
                    break;
                case false when JumpPatch.IsApplied:
                    JumpPatch.Remove();
                    OnHookStateChanged(this);
                    break;
            }
        }
    }

    public virtual bool Initialize()
    {
        return false;
    }

    public event Action<OnHookStateChangeArgs>? OnHookStateChange;

    private void OnHookStateChanged(AsmFunctionHook hook)
    {
        OnHookStateChange?.Invoke(new OnHookStateChangeArgs(hook));
    }
}