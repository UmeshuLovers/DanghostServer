using System;
using Unity.Netcode;

public struct OnlinePlayerInfo : INetworkSerializable, IEquatable<OnlinePlayerInfo>
{
    private OnlinePlayerInfo(ulong playerID, string playerName)
    {
        this.playerID = playerID;
        this.playerName = playerName;
    }

    private static ulong LocalClientID => NetworkManager.Singleton.LocalClientId;
    public ulong PlayerID => playerID;
    public string PlayerName => playerName;
    private ulong playerID;
    private string playerName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerID);
        playerName ??= "";
        serializer.SerializeValue(ref playerName);
    }

    private static string GetOrGenerateName()
    {
        return "SERVER";
    }

    public static OnlinePlayerInfo GetLocalPlayerData() => new(LocalClientID, GetOrGenerateName());
    public static implicit operator ulong(OnlinePlayerInfo playerData) => playerData.playerID;
    public static implicit operator OnlinePlayerInfo(ulong playerID) => new(playerID, null);

    public bool Equals(OnlinePlayerInfo other) => playerID == other.playerID;
}