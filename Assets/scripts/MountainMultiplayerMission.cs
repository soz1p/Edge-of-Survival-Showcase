using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>Scene-owned mountain mission setup and local HUD. Network mission state lives in PlayerMission.</summary>
public class MountainMultiplayerMission : MonoBehaviourPunCallbacks
{
    public GameObject compassPrefab;
    public multiEscapeGate escapeGate;
    public HealthBar health;
    public Vector3 compassPosition = new Vector3(61f, 50.5f, 33f);
    public Vector3 gatePosition = new Vector3(69f, 53f, 40f);
    public float compassJitterRadius = 3f;
    public float compassGroundClearance = 0.35f;
    public float gateGroundClearance = 0.05f;

    private TMP_Text statusText;
    private Button lobbyButton;
    private bool initialized;
    private bool roundStarted;
    private bool leaving;
    private bool setupFailed;
    private GameObject localCompass;
    private GameObject hud;

    void Awake()
    {
        // A slow connection must not consume the player's survival time.
        if (health != null) health.enabled = false;
    }

    void Start()
    {
        BuildHud();
        initialized = true;
        if (compassPrefab == null || escapeGate == null || health == null)
        {
            FailSetup("Mission references are missing. Check the scene Inspector.");
            return;
        }
        if (PhotonNetwork.InRoom) StartRound();
        else RefreshStatus();
    }

    public override void OnJoinedRoom() { if (initialized) StartRound(); }
    public override void OnPlayerPropertiesUpdate(Player player, Hashtable changedProps) { RefreshStatus(); }
    public override void OnRoomPropertiesUpdate(Hashtable changedProps) { RefreshStatus(); }
    public override void OnPlayerEnteredRoom(Player player) { RefreshStatus(); }
    public override void OnPlayerLeftRoom(Player player) { RefreshStatus(); }
    public override void OnMasterClientSwitched(Player player) { RefreshStatus(); }

    void StartRound()
    {
        if (roundStarted || setupFailed || !PhotonNetwork.InRoom) return;
        if (!Equals(PhotonNetwork.CurrentRoom.CustomProperties["GameMode"], SceneManager.GetActiveScene().name)) return;
        if (compassPrefab == null || escapeGate == null || health == null) return;
        Physics.SyncTransforms();
        Vector3 gateGround;
        if (!TryGroundPosition(gatePosition, gateGroundClearance, out gateGround))
        {
            FailSetup("No walkable ground at the escape gate anchor. Check gatePosition.");
            return;
        }
        Vector3 pickupGround = Vector3.zero;
        bool found = false;
        // Each client has one personal compass, placed on terrain rather than at island height y=7.
        for (int attempt = 0; attempt < 16 && !found; attempt++)
        {
            Vector2 offset = attempt == 0 ? Vector2.zero : Random.insideUnitCircle * Mathf.Max(0f, compassJitterRadius);
            found = TryGroundPosition(compassPosition + new Vector3(offset.x, 0f, offset.y),
                compassGroundClearance, out pickupGround);
        }
        if (!found)
        {
            FailSetup("No walkable ground at the compass anchor. Check compassPosition.");
            return;
        }
        escapeGate.spawnPosition = gateGround;
        localCompass = Instantiate(compassPrefab, pickupGround, Quaternion.identity);
        roundStarted = true;
        health.enabled = true;
        RefreshStatus();
    }

    // Prefer the landscape over decorations; reject steep slopes and player/enemy colliders.
    public static bool TryGroundPosition(Vector3 anchor, float clearance, out Vector3 position)
    {
        RaycastHit[] hits = Physics.RaycastAll(anchor + Vector3.up * 200f, Vector3.down, 400f,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
        for (int pass = 0; pass < 2; pass++)
        {
            foreach (RaycastHit hit in hits)
            {
                if (hit.normal.y < 0.71f) continue;
                if (pass == 0 && !(hit.collider is TerrainCollider)) continue;
                if (hit.collider.GetComponentInParent<PhotonView>() != null ||
                    hit.collider.GetComponentInParent<UnityEngine.AI.NavMeshAgent>() != null) continue;
                position = hit.point + Vector3.up * Mathf.Max(0f, clearance);
                return true;
            }
        }
        position = anchor;
        return false;
    }

    void RefreshStatus()
    {
        if (statusText == null || setupFailed || leaving) return;
        if (!PhotonNetwork.InRoom)
        {
            statusText.text = "MOUNTAIN SURVIVAL\nConnecting to the multiplayer room...";
            return;
        }
        int complete = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
            if (player.CustomProperties["MissionComplete"] is bool && (bool)player.CustomProperties["MissionComplete"]) complete++;
        bool collected = PhotonNetwork.LocalPlayer.CustomProperties["MissionComplete"] is bool &&
            (bool)PhotonNetwork.LocalPlayer.CustomProperties["MissionComplete"];
        string instruction = multiEscapeGate.IsReady
            ? "Escape gate OPEN. Reach the door at X: " + gatePosition.x.ToString("0") + ", Z: " + gatePosition.z.ToString("0")
            : collected ? "Compass collected. Waiting for the other survivors..."
            : "Find your compass near X: " + compassPosition.x.ToString("0") + ", Z: " + compassPosition.z.ToString("0");
        statusText.text = "MOUNTAIN SURVIVAL\n" + instruction + "\nTeam compasses: " + complete + " / " + PhotonNetwork.PlayerList.Length;
    }

    void FailSetup(string message)
    {
        setupFailed = true;
        Debug.LogError(message, this);
        if (statusText != null) statusText.text = "Mission setup failed\n" + message;
        if (lobbyButton != null) lobbyButton.gameObject.SetActive(true);
    }

    void BuildHud()
    {
        hud = new GameObject("Mountain Mission HUD", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = hud.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = hud.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        GameObject panel = new GameObject("Mission panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(hud.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -145f);
        panelRect.sizeDelta = new Vector2(1160f, 190f);
        panel.GetComponent<Image>().color = new Color(0.04f, 0.07f, 0.1f, 0.9f);
        panel.GetComponent<Image>().raycastTarget = false;
        statusText = CreateText("Mission status", hud.transform, new Vector2(0.5f, 1f),
            new Vector2(0f, -145f), new Vector2(1100f, 160f), 30f);
        TMP_Text success = CreateText("Survival success", hud.transform, new Vector2(0.5f, 0.5f),
            new Vector2(0f, 80f), new Vector2(1000f, 100f), 58f);
        success.text = "YOU SURVIVED";
        success.color = new Color(0.6f, 1f, 0.7f);
        success.gameObject.SetActive(false);

        GameObject buttonObject = new GameObject("Return to lobby", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(hud.transform, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(360f, 75f);
        rect.anchoredPosition = new Vector2(0f, -30f);
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.12f, 0.2f, 0.27f, 0.95f);
        lobbyButton = buttonObject.GetComponent<Button>();
        lobbyButton.targetGraphic = image;
        lobbyButton.onClick.AddListener(ReturnToLobby);
        TMP_Text label = CreateText("Label", buttonObject.transform, new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(340f, 70f), 28f);
        label.text = "RETURN TO LOBBY";
        buttonObject.SetActive(false);
        if (escapeGate != null)
        {
            escapeGate.survivalText = success;
            escapeGate.lobbyButton = buttonObject;
            if (health != null) escapeGate.healthBar = health.healthSlider;
        }
    }

    static TMP_Text CreateText(string name, Transform parent, Vector2 anchor, Vector2 offset, Vector2 size, float fontSize)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.sizeDelta = size;
        rect.anchoredPosition = offset;
        TMP_Text text = obj.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    public void ReturnToLobby()
    {
        if (leaving) return;
        leaving = true;
        if (lobbyButton != null) lobbyButton.interactable = false;
        if (!PhotonNetwork.InRoom) SceneManager.LoadScene("MainLobby");
        else if (!PhotonNetwork.LeaveRoom())
        {
            leaving = false;
            if (lobbyButton != null) lobbyButton.interactable = true;
        }
    }
    public override void OnLeftRoom()
    {
        if (leaving) SceneManager.LoadScene("MainLobby");
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (leaving) { SceneManager.LoadScene("MainLobby"); return; }
        if (health != null) health.StopDrain();
        FailSetup("Disconnected: " + cause + ". Return to the lobby to retry.");
    }
    void OnDestroy()
    {
        if (localCompass != null) Destroy(localCompass);
        if (hud != null) Destroy(hud);
    }
}
