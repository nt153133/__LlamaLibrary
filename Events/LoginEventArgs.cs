using System;

namespace LlamaLibrary.Events;

public class LoginEventArgs : EventArgs
{
    public ulong AccountId { get; }
    public ulong LastKnownCharacterId { get; }
    public bool IsInGame { get; }

    public LoginEventArgs(ulong accountId, ulong lastKnownCharacterId, bool isInGame)
    {
        AccountId = accountId;
        LastKnownCharacterId = lastKnownCharacterId;
        IsInGame = isInGame;
    }
}

public class CharacterSwitchedEventArgs : LoginEventArgs
{
    public ulong NewCharacterId { get; }

    public CharacterSwitchedEventArgs(ulong accountId, ulong lastKnownCharacterId, ulong newCharacterId, bool isInGame) : base(accountId, lastKnownCharacterId, isInGame)
    {
        NewCharacterId = newCharacterId;
    }
}

public class DisconnectedEventArgs : LoginEventArgs
{
    public DisconnectedEventArgs(ulong accountId, ulong lastKnownCharacterId, bool isInGame) : base(accountId, lastKnownCharacterId, isInGame)
    {
    }
}