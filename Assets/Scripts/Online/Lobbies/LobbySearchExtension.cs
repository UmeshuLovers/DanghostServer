using System.Collections;
using System.Collections.Generic;

[System.Flags]
public enum LobbySearchParameters
{
    noParameters = 0,
    excludePrivate = 1,
    excludeFull = 2,
    matchmaking = excludePrivate | excludeFull
}

public static class LobbySearchExtension
{
    public static bool MatchesSearchParameter(this MultiplayerLobby lobby, LobbySearchParameters parameters)
    {
        if (parameters.HasFlag(LobbySearchParameters.excludePrivate) && lobby.IsPrivate) return false;
        if (parameters.HasFlag(LobbySearchParameters.excludeFull) && lobby.IsFull) return false;
        return true;
    }

    public static MultiplayerLobby FindLobby(this IEnumerable<MultiplayerLobby> lobbies, LobbySearchParameters searchParams)
    {
        foreach (MultiplayerLobby lobby in lobbies)
        {
            if (lobby.MatchesSearchParameter(searchParams))
            {
                return lobby;
            }
        }

        return null;
    }

    public static List<MultiplayerLobby> FindLobbies(this IEnumerable<MultiplayerLobby> lobbies, LobbySearchParameters searchParams)
    {
        List<MultiplayerLobby> foundLobbies = new();
        foreach (MultiplayerLobby lobby in lobbies)
        {
            if (lobby.MatchesSearchParameter(searchParams))
            {
                foundLobbies.Add(lobby);
            }
        }

        return foundLobbies;
    }

    public static MultiplayerLobby GetLobbyByID(this IEnumerable<MultiplayerLobby> lobbies, int lobbyID, LobbySearchParameters searchParams)
    {
        foreach (MultiplayerLobby lobby in lobbies)
        {
            if (lobby.LobbyID == lobbyID && lobby.MatchesSearchParameter(searchParams))
            {
                return lobby;
            }
        }

        return null;
    }

    public static bool LobbyExists(this IEnumerable<MultiplayerLobby> lobbies, int lobbyID, LobbySearchParameters searchParams) => lobbies.TryGetLobbyByID(lobbyID, out _, searchParams);
    public static bool TryGetLobbyByID(this IEnumerable<MultiplayerLobby> lobbies, int lobbyID, out MultiplayerLobby lobby, LobbySearchParameters searchParams)
    {
        lobby = lobbies.GetLobbyByID(lobbyID, searchParams);
        return lobby != null;
    }
}