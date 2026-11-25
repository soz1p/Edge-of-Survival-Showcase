using UnityEngine;
using TMPro;
using Photon.Pun;
public class MissionCompleteUI : MonoBehaviourPun
{
    public TMP_Text missionCompleteText;
    void Awake()
    {
        if (missionCompleteText == null) missionCompleteText = GetComponentInChildren<TMP_Text>(true);
        if (missionCompleteText != null) missionCompleteText.gameObject.SetActive(false);
    }
    [PunRPC]
    public void ShowMessage()
    {
        if (missionCompleteText != null) missionCompleteText.gameObject.SetActive(true);
    }
}
