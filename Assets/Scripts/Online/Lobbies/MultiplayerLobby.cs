using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;


[System.Serializable]
public class MultiplayerLobby : IReadOnlyList<OnlinePlayerInfo>, INetworkSerializable
{
    /// <summary>
    /// Default constructor for serialization.
    /// </summary>
    public MultiplayerLobby()
    {
        playersInfo = new List<OnlinePlayerInfo>();
    }

    /// <summary>
    /// Constructor for creating a valid new lobby.
    /// </summary>
    public MultiplayerLobby(int lobbyID, bool isPrivateLobby) : this()
    {
        this.lobbyID = lobbyID;
        this.isPrivateLobby = isPrivateLobby;
        gameplaySeed = GenerateRandomSeed();
    }

    public const int MaxLobbySeed = 100000;
    public const int MaxPlayers = 4;
    private int lobbyID;
    private int gameplaySeed;
    private bool isPrivateLobby;
    private List<OnlinePlayerInfo> playersInfo;
    public int LobbyID => lobbyID;

    /// <summary>
    /// The active player count. For the max player count, use <see cref="MaxPlayers"/>.
    /// </summary>
    public int Count => playersInfo.Count;

    public int GameplaySeed => gameplaySeed;
    public OnlinePlayerInfo this[int index] => playersInfo[index];
    public bool IsEmpty => Count == 0;
    public bool IsFull => Count == MaxPlayers;
    public bool IsPrivate => isPrivateLobby;

    public bool Contains(ulong playerId) => playersInfo.Contains(playerId);


    public bool AddPlayer(OnlinePlayerInfo playerId)
    {
        if (Contains(playerId) || IsFull)
        {
            return false;
        }
        else
        {
            playersInfo.Add(playerId);
            return true;
        }
    }

    public void RemovePlayer(OnlinePlayerInfo playerId)
    {
        if (Contains(playerId))
        {
            int index = playersInfo.IndexOf(playerId);
            playersInfo.RemoveAt(index);
        }
    }

    public ulong? GetPlayerIDSafe(int index) => index < Count && index >= 0 ? playersInfo[index].PlayerID : null;
    public IEnumerator<OnlinePlayerInfo> GetEnumerator() => playersInfo.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => playersInfo.GetEnumerator();


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref gameplaySeed);
        serializer.SerializeValue(ref lobbyID);
        serializer.SerializeValue(ref isPrivateLobby);
        serializer.SerializeList_NetworkSerializable(ref playersInfo);
    }

    private static int GenerateRandomSeed() => Mathf.FloorToInt(Random.Range(0, MaxLobbySeed));

    public static int GetFirstAvailableUniqueID(List<MultiplayerLobby> lobbies)
    {
        int id;
        if (lobbies == null)
        {
            id = 0;
        }
        else
        {
            IOrderedEnumerable<MultiplayerLobby> orderedLobbies = lobbies.OrderBy(lobby => lobby.LobbyID);
            id = 0;
            foreach (MultiplayerLobby lobby in orderedLobbies)
            {
                if (lobby.LobbyID == id)
                {
                    id++;
                }
                else
                {
                    break;
                }
            }
        }
        return id;
    }
}