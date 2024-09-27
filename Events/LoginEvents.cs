using System;
using System.Windows.Media;
using ff14bot;
using LlamaLibrary.Extensions;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;

namespace LlamaLibrary.Events;

public static class LoginEvents
{
    public static ulong PreviousCharacterId { get; internal set; }
    public static ulong LastKnownCharacterId { get; internal set; }
    public static ulong AccountId { get; internal set; }

    private static LLogger Log = new LLogger("LoginEvents", Colors.DimGray, LogLevel.Verbose);

    public static event EventHandler<LoginEventArgs>? OnLogin;
    public static event EventHandler<DisconnectedEventArgs>? OnDisconnected;
    public static event EventHandler<CharacterSwitchedEventArgs>? OnCharacterSwitched;

    private static readonly DebounceDispatcher LoginDebounce = new(invokeOnLogin);
    private static readonly DebounceDispatcher DisconnectedDebounce = new(invokeOnDisconnected);
    private static readonly DebounceDispatcher CharacterSwitchedDebounce = new(invokeOnCharacterSwitched);

    private static void invokeOnLogin(object? _)
    {
        var previousCharacterId = LastKnownCharacterId;
        LastKnownCharacterId = Core.Me != null ? Core.Me.PlayerId() : 0;

        Log.Verbose($"OnLogin invoked: AccountId: {AccountId}, LastKnownCharacterId: {LastKnownCharacterId}, IsInGame: {Core.IsInGame}");
        OnLogin?.Invoke(null, new LoginEventArgs(AccountId, LastKnownCharacterId, Core.IsInGame));

        if (Core.IsInGame && LastKnownCharacterId != previousCharacterId)
        {
            invokeOnCharacterSwitched(null);
        }
    }

    private static void invokeOnCharacterSwitched(object? _)
    {
        Log.Verbose($"OnCharacterSwitched invoked: AccountId: {AccountId}, PreviousCharacterId: {PreviousCharacterId}, NewCharacterId: {LastKnownCharacterId}");
        OnCharacterSwitched?.Invoke(null, new CharacterSwitchedEventArgs(AccountId, PreviousCharacterId, LastKnownCharacterId, Core.IsInGame));
    }

    private static void invokeOnDisconnected(object? _)
    {
        Log.Verbose($"OnDisconnected invoked: AccountId: {AccountId}, LastKnownCharacterId: {LastKnownCharacterId}, IsInGame: {Core.IsInGame}");
        OnDisconnected?.Invoke(null, new DisconnectedEventArgs(AccountId, LastKnownCharacterId, Core.IsInGame));
    }

    public static void InvokeOnLogin(bool force = false)
    {
        if (force)
        {
            invokeOnLogin(null);
            return;
        }

        LoginDebounce.Debounce(1000);
    }

    public static void InvokeOnDisconnected(bool force = false)
    {
        if (force)
        {
            invokeOnDisconnected(null);
            return;
        }

        DisconnectedDebounce.Debounce(1000);
    }

    public static void InvokeOnCharacterSwitched(bool force = false)
    {
        UpdateInfo();

        if (force)
        {
            invokeOnCharacterSwitched(null);
            return;
        }

        CharacterSwitchedDebounce.Debounce(1000);
    }

    internal static void SetAccountId()
    {
        AccountId = Core.Me?.AccountId() ?? 0;
    }

    public static void UpdateInfo()
    {
        if (Core.Me == null)
        {
            return;
        }

        PreviousCharacterId = LastKnownCharacterId;
        LastKnownCharacterId = Core.Me.PlayerId();
    }
}