using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class multiEscapeGate : MonoBehaviourPun
{
    public TMP_Text survivalText;
    public GameObject lobbyButton;
    public Slider healthBar;
    public string escapeGatePrefabName = "EscapeGate";
    public Vector3 spawnPosition = new Vector3(0.93f, 6.2f, 5.1f);
    private bool spawning;
    private bool survived;
    public static bool IsReady
    {
        get { return PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties["EscapeGateReady"] is bool &&
            (bool)PhotonNetwork.CurrentRoom.CustomProperties["EscapeGateReady"]; }
    }
    void Awake()
    {
        if (survivalText != null && survivalText.gameObject.scene.IsValid()) survivalText.gameObject.SetActive(false);
        if (lobbyButton != null && lobbyButton.scene.IsValid()) lobbyButton.SetActive(false);
    }
    void OnTriggerEnter(Collider other) { TrySurvive(other); }
    public void TrySurvive(Collider other)
    {
        if (survived || !IsReady || !other.CompareTag("Player")) return;
        PhotonView player = other.GetComponentInParent<PhotonView>();
        if (player == null || !player.IsMine) return;
        // Route runtime gate triggers to the manager with real scene UI bindings.
        if (healthBar == null || !healthBar.gameObject.scene.IsValid())
        {
            foreach (multiEscapeGate gate in FindObjectsOfType<multiEscapeGate>())
                if (gate != this && gate.healthBar != null && gate.healthBar.gameObject.scene.IsValid())
                { gate.TrySurvive(other); return; }
            return;
        }
        survived = true;
        if (survivalText != null) survivalText.gameObject.SetActive(true);
        if (lobbyButton != null) lobbyButton.SetActive(true);
        HealthBar.FinishSurvival(other);
    }
    public void EnableEscapeGate()
    {
        if (!PhotonNetwork.IsMasterClient || !PhotonNetwork.InRoom || spawning || IsReady) return;
        // A previous master may have sent the room-owned spawn before its ready-property update.
        foreach (multiEscapeGate existing in FindObjectsOfType<multiEscapeGate>())
        {
            PhotonView view = existing.GetComponent<PhotonView>();
            if (view != null && view.IsRoomView && view.InstantiationId > 0)
            {
                MarkGateReady();
                return;
            }
        }
        GameObject prefab = Resources.Load<GameObject>(escapeGatePrefabName);
        if (prefab == null || prefab.GetComponent<PhotonView>() == null)
        { Debug.LogError("Escape gate must be a Resources prefab with PhotonView.", this); return; }
        spawning = true;
        GameObject gate = PhotonNetwork.InstantiateRoomObject(escapeGatePrefabName,
            spawnPosition, Quaternion.identity);
        if (gate == null) { spawning = false; return; }
        // Room ownership survives a master leaving; no buffered RPC replays spawns on each client.
        MarkGateReady();
    }
    void MarkGateReady()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { "EscapeGateReady", true } });
    }
}
