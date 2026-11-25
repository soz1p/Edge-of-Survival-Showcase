using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class EscapeGate : MonoBehaviour
{
    public TMP_Text survivalText;
    public GameObject lobbyButton;
    public Slider healthBar;
    private bool survived;
    void Start()
    {
        if (survivalText != null && survivalText.gameObject.scene.IsValid()) survivalText.gameObject.SetActive(false);
        if (lobbyButton != null && lobbyButton.scene.IsValid()) lobbyButton.SetActive(false);
    }
    void OnTriggerEnter(Collider other) { TrySurvive(other); }
    public void TrySurvive(Collider other)
    {
        if (survived || !other.CompareTag("Player")) return;
        HealthBar bar = FindObjectOfType<HealthBar>();
        if (bar == null || !bar.EscapeUnlocked) return;
        if (healthBar == null || !healthBar.gameObject.scene.IsValid())
        {
            foreach (EscapeGate gate in FindObjectsOfType<EscapeGate>())
                if (gate != this && gate.healthBar != null && gate.healthBar.gameObject.scene.IsValid())
                { gate.TrySurvive(other); return; }
            return;
        }
        survived = true;
        if (survivalText != null) survivalText.gameObject.SetActive(true);
        if (lobbyButton != null) lobbyButton.SetActive(true);
        HealthBar.FinishSurvival(other);
    }
}
