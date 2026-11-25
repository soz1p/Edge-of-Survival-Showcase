using UnityEngine;
using UnityEngine.UI;
public class cshButton : MonoBehaviour
{
    public Button btnJump;
    public Button btnAttack;
    public cshPlayerController sPlayer;
    void Start()
    {
        if (btnJump == null || btnAttack == null) { enabled = false; return; }
        btnJump.onClick.AddListener(OnClickJumpButton);
        btnAttack.onClick.AddListener(OnClickAttackButton);
    }
    void Update()
    {
        bool ready = sPlayer != null && sPlayer.enabled;
        bool canAttack = ready && sPlayer.CanAttack();
        btnJump.gameObject.SetActive(ready && !canAttack);
        btnAttack.gameObject.SetActive(canAttack);
    }
    void OnClickJumpButton() { if (sPlayer != null) sPlayer.OnVirtualPadJump(); }
    void OnClickAttackButton() { if (sPlayer != null) sPlayer.OnVirtualPadAttack(); }
    void OnDestroy()
    {
        if (btnJump != null) btnJump.onClick.RemoveListener(OnClickJumpButton);
        if (btnAttack != null) btnAttack.onClick.RemoveListener(OnClickAttackButton);
    }
}
