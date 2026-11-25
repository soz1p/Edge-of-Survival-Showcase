using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerMission : MonoBehaviourPunCallbacks
{
    public bool missionComplete;
    public TMP_Text missionCompleteUI;
    public TMP_Text survivalTextUI;
    public GameObject lobbyButtonUI;
    public GameObject healthBarUI;
    void Start()
    {
        if (!photonView.IsMine) return;
        missionComplete = false;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "MissionComplete", false } });
        // Prefab asset UI references are not scene instances. Use local spawned UI at pickup instead.
    }
    void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine && other.CompareTag("Compass")) SetMissionComplete();
    }
    public void SetMissionComplete()
    {
        if (!photonView.IsMine || !PhotonNetwork.InRoom || missionComplete) return;
        missionComplete = true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "MissionComplete", true } });
        if (missionCompleteUI != null && missionCompleteUI.gameObject.scene.IsValid())
            StartCoroutine(ShowMissionCompleteUICoroutine());
    }
    [PunRPC]
    public void ShowMissionCompleteUI(int viewID)
    {
        if (photonView.IsMine && photonView.ViewID == viewID && missionCompleteUI != null &&
            missionCompleteUI.gameObject.scene.IsValid()) StartCoroutine(ShowMissionCompleteUICoroutine());
    }
    IEnumerator ShowMissionCompleteUICoroutine()
    {
        missionCompleteUI.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        if (missionCompleteUI != null) missionCompleteUI.gameObject.SetActive(false);
    }
    public override void OnPlayerPropertiesUpdate(Player player, Hashtable changedProps) { CheckAllMissions(); }
    public override void OnPlayerEnteredRoom(Player player) { CheckAllMissions(); }
    public override void OnPlayerLeftRoom(Player player) { CheckAllMissions(); }
    public override void OnMasterClientSwitched(Player player) { CheckAllMissions(); }
    void CheckAllMissions()
    {
        // Only the local master's component may create the single room-owned gate.
        if (!photonView.IsMine || !PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom) return;
        foreach (Player player in PhotonNetwork.PlayerList)
            if (!(player.CustomProperties["MissionComplete"] is bool) || !(bool)player.CustomProperties["MissionComplete"]) return;
        multiEscapeGate gate = FindObjectOfType<multiEscapeGate>();
        if (gate != null) gate.EnableEscapeGate();
    }
}
