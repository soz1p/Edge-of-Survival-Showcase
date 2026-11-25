using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
public class ChangePlayMode : MonoBehaviourPunCallbacks
{
    private string pendingScene;
    public void LoadIslandPlayScene() { Navigate("IslandPlay"); }
    public void LoadIslandMultiPlayScene() { Navigate("IslandMultiPlay"); }
    public void LoadMountainPlayScene() { Navigate("MountainPlay"); }
    public void LoadMountainMultiPlayScene() { Navigate("MountainMultiPlay"); }

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
