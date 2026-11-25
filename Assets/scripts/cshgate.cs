using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class cshgate : MonoBehaviour
{
    public TMP_Text survivalText;
    public GameObject lobbyButton;
    public Slider healthBar;
    public TMP_Text mission;
    void Start()
    {
        if (mission != null && mission.gameObject.scene.IsValid()) StartCoroutine(ShowMissionMessage());
    }
    void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.InRoom) return;
        multiEscapeGate gate = GetComponent<multiEscapeGate>();
        if (gate != null) gate.TrySurvive(other);
    }
    IEnumerator ShowMissionMessage()
    {
        mission.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        if (mission != null) mission.gameObject.SetActive(false);
    }
}
