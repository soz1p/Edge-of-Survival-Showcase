using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public Gradient gradient;
    public Image fillImage;
    public float decreaseRate = 1f;
    public bool EscapeUnlocked { get; private set; }
    private float currentHealth;
    private bool finished;

    void Start()
    {
        if (healthSlider == null) { Debug.LogError("Assign the scene health slider.", this); enabled = false; return; }
        currentHealth = healthSlider.maxValue;
        UpdateHealthBar();
    }
    void Update()
    {
        if (finished || healthSlider == null) return;
        currentHealth = Mathf.Max(healthSlider.minValue, currentHealth - Mathf.Max(0f, decreaseRate) * Time.deltaTime);
        UpdateHealthBar();
        if (currentHealth <= healthSlider.minValue)
        {
            finished = true;
            // A local loss must not make the master load a failure scene for every client.
            // Remove the failed participant immediately so the remaining team can complete the round.
            if (PhotonNetwork.InRoom) PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("tryagain");
        }
    }
    public void UnlockEscape() { EscapeUnlocked = true; }
    public void StopDrain()
    {
        finished = true;
        if (healthSlider != null) healthSlider.gameObject.SetActive(false);
    }
    public static void FinishSurvival(Collider player)
    {
        foreach (HealthBar bar in FindObjectsOfType<HealthBar>()) bar.StopDrain();
        cshPlayerController networkPlayer = player.GetComponentInParent<cshPlayerController>();
        if (networkPlayer != null) networkPlayer.enabled = false;
        cshSinglePlayer singlePlayer = player.GetComponentInParent<cshSinglePlayer>();
        if (singlePlayer != null) singlePlayer.enabled = false;
    }
    void UpdateHealthBar()
    {
        healthSlider.value = currentHealth;
        if (fillImage != null && gradient != null) fillImage.color = gradient.Evaluate(healthSlider.normalizedValue);
    }
}
