using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;


public partial class LobbiesManager
{
    public static LobbiesCollection Lobbies => instance == null ? null : instance.lobbies;
    private readonly LobbiesCollection lobbies = new();
#if UNITY_EDITOR
    public static LobbiesCollection ServerLobbies => instance == null ? null : instance.serverLobbies;
    private LobbiesCollection serverLobbies;
#endif

    public static void RequestLobbyValidation(int lobbyID)
    {
        if (Client_CanInteractWithNetwork)
        {
            instance.RequestLobbyValidation_ServerRPC(LocalClientID, lobbyID);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestLobbyValidation_ServerRPC(ulong playerID, int lobbyID)
    {
        LobbyID.ValidationStep validationStep;
        if (lobbies.TryGetLobbyByID(lobbyID, out MultiplayerLobby lobby, LobbySearchParameters.noParameters))
        {
            validationStep = lobby.IsFull ? LobbyID.ValidationStep.Full : LobbyID.ValidationStep.Valid;
        }
        else
        {
            validationStep = LobbyID.ValidationStep.Invalid;
        }

        ValidateLobbyID_ClientRPC(lobbyID, validationStep, playerID.ToClientRpcParams_Receiver());
    }


    [ServerRpc(RequireOwnership = false)]
    private void CreatePrivateLobby_ServerRPC(OnlinePlayerInfo playerID)
    {
        var lobby = lobbies.CreateLobby(privateLobby: true);
        Server_MoveClientToLobby(playerID, lobby);
    }

    [ServerRpc(RequireOwnership = false)]
    private void JoinPrivateLobby_ServerRPC(OnlinePlayerInfo playerInfo, int joinCode)
    {
        if (lobbies.TryGetLobbyByID(joinCode, out MultiplayerLobby lobby, LobbySearchParameters.excludePrivate | LobbySearchParameters.excludeFull))
        {
            Server_MoveClientToLobby(playerInfo, lobby);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreateOrJoinLobbyMatchmaking_ServerRPC(OnlinePlayerInfo playerID)
    {
        MultiplayerLobby lobby;
        MultiplayerLobby currentPlayerLobby = lobbies[playerID];
        if (currentPlayerLobby != null)
        {
            lobby = null;
            List<MultiplayerLobby> candidates = lobbies.FindLobbies(LobbySearchParameters.matchmaking);
            foreach (MultiplayerLobby candidate in candidates)
            {
                if (candidate != currentPlayerLobby)
                {
                    lobby = candidate;
                    break;
                }
            }
        }
        else
        {
            lobby = lobbies.FindLobby(LobbySearchParameters.matchmaking);
        }

        lobby ??= lobbies.CreateLobby(privateLobby: false);
        Server_MoveClientToLobby(playerID, lobby);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ExitLobby_ServerRPC(ulong playerID)
    {
        Server_ClientExitLobby(playerID, true);
    }

    /// <summary>
    /// Make a player leave a lobby from the server side. Replicates the change to all clients, including the player that left.
    /// </summary>
    private void Server_ClientExitLobby(ulong playerID, bool replicateToExitingPlayer)
    {
        var playerLobby = lobbies.LeaveLobby(playerID);
        if (playerLobby != null)
        {
            if (replicateToExitingPlayer) ExitLobby_ClientRPC(playerID.ToClientRpcParams_Receiver());
            Server_UpdateLobby(playerLobby);
        }
    }

    /// <summary>
    /// Update the lobby for all clients from the server.
    /// </summary>
    private void Server_UpdateLobby(MultiplayerLobby lobby)
    {
        if (!IsServer) return;
        if (lobby == null) return;
        SetLocalLobby_ClientRPC(lobby, lobby.PlayersToClientRpcParams_Receivers());
    }

    /// <summary>
    /// Move a client to a new lobby. If the lobby is full, the client will not be moved. If the client is already in a lobby, they will be removed from it.
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="lobby"></param>
    private void Server_MoveClientToLobby(ulong clientID, MultiplayerLobby lobby)
    {
        if (lobby.IsFull) return;
        Server_ClientExitLobby(clientID, false);
        if (lobby.AddPlayer(clientID))
        {
            Server_UpdateLobby(lobby);
        }
    }

#if UNITY_EDITOR
    public static void RequestLobbiesFetch()
    {
        if (!instance) return;
        if (instance.IsServer) return;
        instance.RequestSendLobbies_ServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSendLobbies_ServerRPC() => ReceiveLobbies_ClientRPC(lobbies);

    [ClientRpc]
    private void ReceiveLobbies_ClientRPC(LobbiesCollection lobbies) => this.serverLobbies = lobbies;
#endif
}