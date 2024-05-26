using System;
using System.Collections.Generic;
using ff14bot;
using LlamaLibrary.Memory;

namespace LlamaLibrary.ClientDataHelpers;

public static class UiManagerProxy
{
    //private static readonly PropertyInfo[] Properties = typeof(DataManager).Assembly.GetType("ff14bot.Managers.UiManager").GetProperties(BindingFlags.Static | BindingFlags.Public);

    private static readonly Dictionary<string, int> VFunctionIds = new()
    {
        { "GetRaptureTextModule", 6 },
        { "GetRaptureAtkModule", 7 },
        { "GetRaptureShellModule", 9 },
        { "AcquaintanceModule", 15 },
        { "GetFieldMarkerModule", 49 },
        { "GetRecommendEquipModule", 32 },
        { "GetInfoModule", 34 },
        { "GetUIInputData", 65 },
        { "GetUIInputModule", 66 },
        { "GetUIInputModule_Topic", 67 }
    };
    private static IntPtr _uiModule;

    static UiManagerProxy()
    {
       // _uiModule = Core.Memory.CallInjected64<IntPtr>(Offsets.GetUiModule, Core.Memory.Read<IntPtr>(Offsets.Framework));
    }

    public static IntPtr AcquaintanceModule { get; } = Core.Memory.CallInjected64<IntPtr>(VFunctionAddress(VFunctionIds["AcquaintanceModule"]), UIModule);

    public static IntPtr FieldMarkerModule { get; } = Core.Memory.CallInjected64<IntPtr>(VFunctionAddress(VFunctionIds["GetFieldMarkerModule"]), UIModule);

    public static IntPtr RecommendEquipModule { get; } = Core.Memory.CallInjected64<IntPtr>(VFunctionAddress(VFunctionIds["GetRecommendEquipModule"]), UIModule);

    //public static IntPtr UIModule => (IntPtr)Properties.First(i => i.Name.Equals("UIModule")).GetValue(null);

    public static IntPtr UIModule => _uiModule == IntPtr.Zero ? _uiModule = Core.Memory.CallInjected64<IntPtr>(Offsets.GetUiModule, Core.Memory.Read<IntPtr>(Offsets.Framework)) : _uiModule;

    public static IntPtr InfoModule { get; } = Core.Memory.CallInjected64<IntPtr>(VFunctionAddress(VFunctionIds["GetInfoModule"]), UIModule);

    public static IntPtr RaptureTextModule => Core.Memory.CallInjected64<IntPtr>(VFunctionAddress(VFunctionIds["GetRaptureTextModule"]), UIModule);

    public static IntPtr UIInputData => Core.Memory.CallInjected64<IntPtr>(VFunctionAddress(VFunctionIds["GetUIInputData"]), UIModule);

    public static IntPtr UIInputModule => Core.Memory.CallInjected64<IntPtr>(VFunctionAddress(VFunctionIds["GetUIInputModule"]), UIModule);

    public static IntPtr UIInputModule_Topic2 => Core.Memory.CallInjected64<IntPtr>(VFunctionAddress(VFunctionIds["GetUIInputModule_Topic"]), UIModule);

    //public static IntPtr RaptureAtkModule => (IntPtr)Properties.First(i => i.Name.Equals("RaptureAtkModule")).GetValue(null);

    public static IntPtr RaptureAtkModule => Core.Memory.CallInjected64<IntPtr>(VFunctionAddress(VFunctionIds["GetRaptureAtkModule"]), UIModule);

    //public static IntPtr RaptureShellModule => (IntPtr)Properties.First(i => i.Name.Equals("RaptureShellModule")).GetValue(null);

    public static IntPtr RaptureShellModule => Core.Memory.CallInjected64<IntPtr>(VFunctionAddress(VFunctionIds["GetRaptureShellModule"]), UIModule);

    public static IntPtr VFunctionCall(int index)
    {
        return Core.Memory.CallInjected64<IntPtr>(VFunctionAddress(index), UIModule);
    }

    public static IntPtr VFunctionAddress(int index)
    {
        return Core.Memory.Read<IntPtr>(Core.Memory.Read<IntPtr>(UIModule) + 0x8 * index);
    }

    /*
    public static IntPtr RaptureTeleportHistory => (IntPtr)Properties.First(i => i.Name.Equals("RaptureTeleportHistory")).GetValue(null);

    public static IntPtr RaptureLogModule => (IntPtr)Properties.First(i => i.Name.Equals("RaptureLogModule")).GetValue(null);

    public static IntPtr RaptureGearsetModule => (IntPtr)Properties.First(i => i.Name.Equals("RaptureGearsetModule")).GetValue(null);

    public static IntPtr RaptureHotbarModule => (IntPtr)Properties.First(i => i.Name.Equals("RaptureHotbarModule")).GetValue(null);

    public static IntPtr PronounModule => (IntPtr)Properties.First(i => i.Name.Equals("PronounModule")).GetValue(null);
    */

    //public static IntPtr UIInputModule => (IntPtr)Properties.First(i => i.Name.Equals("UIInputModule")).GetValue(null);

    //public static IntPtr UIInputModule_Topic => (IntPtr)Properties.First(i => i.Name.Equals("UIInputModule_Topic")).GetValue(null);
}