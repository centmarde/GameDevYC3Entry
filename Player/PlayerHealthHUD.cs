using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHealthHUD : MonoBehaviour
{
    [SerializeField] private Image hpFillImage;
    [SerializeField] private TMP_Text hpText; 

    private Entity_Health playerHealth;

    private void Start()
    {
        StartCoroutine(WaitForPlayer());
    }

    private IEnumerator WaitForPlayer()
    {
        while (playerHealth == null)
        {
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                playerHealth = player.GetComponent<Entity_Health>();
                if (playerHealth != null)
                {
                    playerHealth.OnDamaged += OnHealthChanged;
                    playerHealth.OnDeath += OnPlayerDeath;
                    playerHealth.OnHealed += OnHealed;
                    UpdateHealthBar();
                }
            }
            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamaged -= OnHealthChanged;
            playerHealth.OnDeath -= OnPlayerDeath;
            playerHealth.OnHealed -= OnHealed;
        }
    }

    private void OnHealthChanged(float damage, Vector3 hitPoint, Vector3 hitNormal, object source)
    {
        UpdateHealthBar();
    }

    private void OnPlayerDeath() => UpdateHealthBar();
    private void OnHealed(float amount) => UpdateHealthBar();

    private void UpdateHealthBar()
    {
        if (hpFillImage == null || playerHealth == null) return;

        hpFillImage.fillAmount = playerHealth.HealthPercent;

        if (hpText != null)
        {
            hpText.text = $"{Mathf.RoundToInt(playerHealth.CurrentHealth)} / {Mathf.RoundToInt(playerHealth.MaxHealth)}";
        }
    }

    private void LateUpdate()
    {
        if (playerHealth != null && hpFillImage != null)
        {
            float target = playerHealth.HealthPercent;
            hpFillImage.fillAmount = Mathf.Lerp(hpFillImage.fillAmount, target, Time.deltaTime * 8f);

            // Update number dynamically even during lerp
            if (hpText != null)
                hpText.text = $"{Mathf.RoundToInt(playerHealth.CurrentHealth)} / {Mathf.RoundToInt(playerHealth.MaxHealth)}";
        }
    }
}
