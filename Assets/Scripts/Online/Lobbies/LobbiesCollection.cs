using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

/// <summary>
/// Facilitate lobby management, but does not handle the network replication logic.
/// </summary>
public class LobbiesCollection : IReadOnlyList<MultiplayerLobby>, INetworkSerializable
{
    private List<MultiplayerLobby> lobbies = new();
    public int Count => lobbies.Count;
    public MultiplayerLobby this[int index] => lobbies[index];
    private MultiplayerLobby localLobby;

    public int GetAvailableUniqueID() => MultiplayerLobby.GetFirstAvailableUniqueID(lobbies);

    /// <summary>
    /// Returns the lobby the client is in.
    /// </summary>
    public MultiplayerLobby this[ulong clientID]
    {
        get
        {
            foreach (MultiplayerLobby lobby in lobbies)
            {
                if (lobby.Contains(clientID))
                {
                    return lobby;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Returns true if the client is in a lobby.
    /// </summary>
    public bool IsClientInLobby(ulong clientID)
    {
        return this[clientID] != null;
    }

    /// <summary>
    /// Create a lobby with a unique ID and add it to the collection.
    /// </summary>
    public MultiplayerLobby CreateLobby(bool privateLobby)
    {
        MultiplayerLobby lobby = new MultiplayerLobby(GetAvailableUniqueID(), privateLobby);
        lobbies.Add(lobby);
        return lobby;
    }

    /// <summary>
    /// Find the lobby of a client and remove it from that lobby. If the lobby is empty, it is also removed from the collection.
    /// </summary>
    public MultiplayerLobby LeaveLobby(OnlinePlayerInfo playerID)
    {
        MultiplayerLobby currentLobby = this[playerID];
        if (currentLobby == null) return null;
        currentLobby.RemovePlayer(playerID);
        if (currentLobby.IsEmpty)
        {
            lobbies.Remove(currentLobby);
        }

        return currentLobby;
    }


#if UNITY_EDITOR
    public void Clear() => lobbies.Clear();
#endif
    public IEnumerator<MultiplayerLobby> GetEnumerator() => lobbies.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => lobbies.GetEnumerator();

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeList_NetworkSerializable(ref lobbies);
    }
}