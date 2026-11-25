using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class SimpleLauncher : MonoBehaviourPunCallbacks
{
    public PhotonView playerPrefab;
    private bool spawned;
    private bool joining;
    private string mode;
    void Start()
    {
        mode = SceneManager.GetActiveScene().name;
        if (PhotonNetwork.InRoom)
        {
            if ((string)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"] == mode) OnJoinedRoom();
            else PhotonNetwork.LeaveRoom();
        }
        else if (PhotonNetwork.IsConnectedAndReady) JoinRoom();
        else if (!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster() { JoinRoom(); }
    public override void OnLeftRoom() { spawned = false; joining = false; }
    void JoinRoom()
    {
        if (joining || PhotonNetwork.InRoom) return;
        joining = PhotonNetwork.JoinRandomRoom(new Hashtable { { "GameMode", mode } }, 4);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions options = new RoomOptions { MaxPlayers = 4,
            CustomRoomProperties = new Hashtable { { "GameMode", mode } },
            CustomRoomPropertiesForLobby = new[] { "GameMode" } };
        PhotonNetwork.CreateRoom(null, options);
    }
    public override void OnJoinedRoom()
    {
        joining = false;
        if (spawned) return;
        if (playerPrefab == null) { Debug.LogError("Assign a Resources player prefab.", this); return; }
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "MissionComplete", false } });
        spawned = true;
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0f, 6.7f, 0f), Quaternion.identity);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        joining = false;
        Debug.LogError("Room creation failed: " + message, this);
    }
}
