using UnityEngine;
using Photon.Pun;
public class multiCompass : MonoBehaviourPun
{
    public GameObject missionCompleteUIPrefab;
    private bool collected;
    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        PlayerMission mission = other.GetComponentInParent<PlayerMission>();
        if (mission == null || !mission.photonView.IsMine) return;
        collected = true;
        mission.SetMissionComplete();
        if (missionCompleteUIPrefab != null)
        {
            GameObject ui = Instantiate(missionCompleteUIPrefab, other.transform.position + Vector3.up * 2f, Quaternion.identity);
            MissionCompleteUI message = ui.GetComponent<MissionCompleteUI>();
            if (message != null) message.ShowMessage();
            Destroy(ui, 3f);
        }
        // Each client spawns its own pickup; only its owning player may consume it.
        Destroy(gameObject);
    }
}
