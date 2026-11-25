using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
public class ChangeScene : MonoBehaviourPunCallbacks
{
    private string pendingScene;
    public void LoadIslandScene() { Navigate("IslandLobby"); }
    public void LoadMountainScene() { Navigate("MountainLobby"); }
    public void LoadMainLobby() { Navigate("MainLobby"); }

    void Navigate(string scene)
    {
        if (pendingScene != null) return;
        pendingScene = scene;
        if (!PhotonNetwork.InRoom) LoadPending();
        else if (!PhotonNetwork.LeaveRoom()) pendingScene = null;
    }
    public override void OnLeftRoom() { LoadPending(); }
    public override void OnDisconnected(DisconnectCause cause) { LoadPending(); }
    void LoadPending()
    {
        if (pendingScene == null) return;
        string scene = pendingScene;
        pendingScene = null;
        SceneManager.LoadScene(scene);
    }
}
