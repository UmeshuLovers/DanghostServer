using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public static class NetcodeRpcExtensions
{
    public static ClientRpcParams ToClientRpcParams_Receiver(this ulong clientID) => new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientID } } };
    public static ClientRpcParams ToClientRpcParams_Receivers(this IEnumerable<ulong> clientIDs) => new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = clientIDs.ToArray() } };

    public static ClientRpcParams PlayersToClientRpcParams_Receivers(this MultiplayerLobby lobby)
    {
        return lobby.Select(player => player.PlayerID).ToClientRpcParams_Receivers();
    }
}