using UnityEngine;
using TMPro;

public partial class PhotonGameManager
{
    /// <summary>
    /// Update status UI text
    /// </summary>
    private void UpdateStatusUI()
    {
        if (statusText != null)
        {
            if (isInLobby)
            {
                statusText.text = $"Lobby: {lobbyRoomName}";
            }
            else if (isConnecting)
            {
                statusText.text = "Connecting...";
            }
            else if (Photon.Pun.PhotonNetwork.IsConnected)
            {
                statusText.text = "Connected";
            }
            else
            {
                statusText.text = "Disconnected";
            }
        }

        if (playerCountText != null && Photon.Pun.PhotonNetwork.CurrentRoom != null)
        {
            playerCountText.text = $"Players Online: {Photon.Pun.PhotonNetwork.CurrentRoom.PlayerCount}";
        }
    }
}
