using System;
using System.Text;
using ff14bot;
using GreyMagic;

namespace LlamaLibrary.RemoteWindows.Atk;


public class AtkValue : IDisposable
{
    private readonly bool _bool;
    private readonly float _float;
    private readonly int _int;
    private readonly AllocatedMemory? _string;
    private readonly uint _uInt;
    private readonly IntPtr _vector;

    public AtkValue(int value)
    {
        Type = ValueType.Int;
        _int = value;
    }

    public AtkValue(uint value)
    {
        Type = ValueType.UInt;
        _uInt = value;
    }

    public AtkValue(string value)
    {
        Type = ValueType.String;
        var array = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(value));
        _string = Core.Memory.CreateAllocatedMemory(array.Length + 30);
        _string.AllocateOfChunk("atkString", array.Length);
        _string.WriteBytes("atkString", array);
    }

    public AtkValue(float value)
    {
        Type = ValueType.Float;
        _float = value;
    }

    public AtkValue(IntPtr value)
    {
        Type = ValueType.Vector;
        _vector = value;
    }

    public AtkValue(bool value)
    {
        Type = ValueType.Bool;
        _bool = value;
    }

    public ValueType Type { get; }

    public static implicit operator AtkValue(int value)
    {
        return new AtkValue(value);
    }

    public static implicit operator AtkValue(uint value)
    {
        return new AtkValue(value);
    }

    public static implicit operator AtkValue(string value)
    {
        return new AtkValue(value);
    }

    public static implicit operator AtkValue(float value)
    {
        return new AtkValue(value);
    }

    public static implicit operator AtkValue(IntPtr value)
    {
        return new AtkValue(value);
    }

    public static implicit operator AtkValue(bool value)
    {
        return new AtkValue(value);
    }

    public static implicit operator int(AtkValue value)
    {
        return value._int;
    }

    public static implicit operator uint(AtkValue value)
    {
        return value._uInt;
    }

    public static implicit operator float(AtkValue value)
    {
        return value._float;
    }

    public static implicit operator IntPtr(AtkValue value)
    {
        return value._vector;
    }

    public static implicit operator bool(AtkValue value)
    {
        return value._bool;
    }

    public static implicit operator AtkValue((ValueType type, int value) value)
    {
        return value.type switch
        {
            ValueType.Int    => new AtkValue((int)value.value),
            ValueType.UInt   => new AtkValue((uint)value.value),
            ValueType.Float  => new AtkValue((float) value.value),
            _                => new AtkValue(0)
        };
    }


    public static implicit operator AtkValue((ValueType type, object value) value)
    {
        return value.type switch
        {
            ValueType.Int    => new AtkValue((int)value.value),
            ValueType.UInt   => new AtkValue((uint)value.value),
            ValueType.Float  => new AtkValue((float) value.value),
            ValueType.String => new AtkValue((string) value.value),
            ValueType.Bool   => new AtkValue((bool) value.value),
            _                => new AtkValue(0)
        };
    }

    //function to convert AtkValue to a pair of ulongs
    public static implicit operator ulong[](AtkValue value)
    {
        var temp = new ulong[] { (ulong)value.Type, 0 };

        temp[1] = value.Type switch
        {
            ValueType.Int    => (ulong)value._int,
            ValueType.UInt   => value._uInt,
            ValueType.Float  => (ulong)BitConverter.DoubleToInt64Bits(value._float),
            ValueType.String => (ulong)value._string?.Address!,
            ValueType.Vector => (ulong)value._vector,
            ValueType.Bool   => (ulong)(value._bool ? 1 : 0),
            _                => temp[1]
        };

        return temp;
    }

    public void Dispose()
    {
        if (Type == ValueType.String)
        {
            _string?.Dispose();
        }
    }
}