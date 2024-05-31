using System;
using Unity.Netcode;
using UnityEngine;

public partial class LobbiesManager
{
    public static event OnLobbyChange onLocalLobbyChange;
    public static MultiplayerLobby LocalLobby => instance == null ? null : instance.localLobby;
    private MultiplayerLobby localLobby;
    private static bool Client_CanInteractWithNetwork => instance != null && NetworkManager.Singleton != null && !instance.IsServer;

    private static void TryConnect()
    {
        if (!instance) return;
        if (instance.IsServer) return;
        if (instance.IsClient) return;
        NetworkManager.Singleton.StartClient();
    }

    public static void SelectOnlineMode(OnlineMode selectedMode)
    {
        if (Client_CanInteractWithNetwork)
        {
            switch (selectedMode)
            {
                case OnlineMode.HostPrivate:
                    instance.CreatePrivateLobby_ServerRPC(OnlinePlayerInfo.GetLocalPlayerData());
                    break;
                case OnlineMode.Matchmaking:
                    instance.CreateOrJoinLobbyMatchmaking_ServerRPC(OnlinePlayerInfo.GetLocalPlayerData());
                    break;
                case OnlineMode.JoinPrivate:
                    instance.JoinPrivateLobby_ServerRPC(OnlinePlayerInfo.GetLocalPlayerData(), LobbyID.Instance.ID);
                    break;
                case OnlineMode.Offline:
                    instance.ExitLobby_ServerRPC(LocalClientID);
                    break;
            }
        }
    }

    /// <summary>
    /// Exit a lobby from the client side. Is not replicated to the server ! See <see cref="ExitLobby_ServerRPC"/>.
    /// </summary>
    private void Client_ExitLobby()
    {
        if (Client_CanInteractWithNetwork)
        {
            localLobby = null;
            onLocalLobbyChange?.Invoke(null);
        }
    }

    [ClientRpc]
    private void ExitLobby_ClientRPC(ClientRpcParams clientRpcParams)
    {
        Client_ExitLobby();
    }

    /// <summary>
    /// Set the local lobby for the client from the server.
    /// </summary>
    /// <param name="lobby"></param>
    /// <param name="clientRpcParams"></param>
    [ClientRpc]
    private void SetLocalLobby_ClientRPC(MultiplayerLobby lobby, ClientRpcParams clientRpcParams)
    {
        if (Client_CanInteractWithNetwork)
        {
            localLobby = lobby;
            onLocalLobbyChange?.Invoke(lobby);
        }
    }

    [ClientRpc] //client RPC param ensures that this is sent to a single client
    private void ValidateLobbyID_ClientRPC(int requestLobbyID, LobbyID.ValidationStep validationStatus, ClientRpcParams clientRpcParams) => LobbyID.Instance.ValidateID(requestLobbyID, validationStatus);
}