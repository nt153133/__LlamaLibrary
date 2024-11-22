using System;

namespace LlamaLibrary.RemoteWindows.Atk;

[Flags]
public enum ValueType
{
    Undefined = 0,
    Null = 0x1,
    Bool = 0x2,
    Int = 0x3,
    Int64 = 0x4,
    UInt = 0x5,
    UInt64 = 0x6,
    Float = 0x7,
    String = 0x8,
    WideString = 0x9,
    String8 = 0xA,
    Vector = 0xB,
    Texture = 0xC,
    AtkValues = 0xD,
    TypeMask = 0xF,
    Managed = 0x20,
    ManagedString = Managed | String,
    ManagedVector = Managed | Vector
}