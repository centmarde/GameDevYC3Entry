using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RollIconUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Player_Roll component from your player object.")]
    public Player_Roll playerRoll;

    [Tooltip("The main roll icon image (the silhouette).")]
    public Image iconImage;

    [Tooltip("The child image used as a radial fill mask.")]
    public Image cooldownMask;

    [Header("Visual Settings")]
    [Range(0f, 1f)] public float cooldownAlpha = 0.35f; // opacity when on cooldown
    public bool pulseWhenReady = true;
    public float readyPulseTime = 0.15f;
    public float readyPulseScale = 1.1f;

    private Color readyColor;
    private Color cooldownColor;
    private Vector3 baseScale;

    void Awake()
    {
        if (!iconImage) iconImage = GetComponent<Image>();

        readyColor = Color.white;
        cooldownColor = new Color(1f, 1f, 1f, cooldownAlpha);
        baseScale = transform.localScale;
    }

    void OnEnable()
    {
        if (playerRoll != null)
        {
            playerRoll.OnRollStarted += HandleRollStarted;
            playerRoll.OnRollReady += HandleRollReady;
        }

        RefreshInstant();
    }

    void OnDisable()
    {
        if (playerRoll != null)
        {
            playerRoll.OnRollStarted -= HandleRollStarted;
            playerRoll.OnRollReady -= HandleRollReady;
        }
    }

    void Update()
    {
        if (playerRoll == null || cooldownMask == null || iconImage == null) return;

        if (playerRoll.IsOnCooldown)
        {
            float t = Mathf.Clamp01(1f - (playerRoll.CooldownRemaining / playerRoll.CooldownDuration));
            // Mask shrinks as cooldown refills
            cooldownMask.fillAmount = 1f - t;
            iconImage.color = cooldownColor;
        }
        else
        {
            cooldownMask.fillAmount = 0f;
            iconImage.color = readyColor;
        }
    }

    private void HandleRollStarted()
    {
        if (cooldownMask) cooldownMask.fillAmount = 1f;
        if (iconImage) iconImage.color = cooldownColor;
    }

    private void HandleRollReady()
    {
        if (cooldownMask) cooldownMask.fillAmount = 0f;
        if (iconImage) iconImage.color = readyColor;
        if (pulseWhenReady) StartCoroutine(ReadyPulse());
    }

    private IEnumerator ReadyPulse()
    {
        float t = 0f;
        while (t < readyPulseTime)
        {
            t += Time.unscaledDeltaTime;
            float k = 1f + (readyPulseScale - 1f) * Mathf.Sin((t / readyPulseTime) * Mathf.PI);
            transform.localScale = baseScale * k;
            yield return null;
        }
        transform.localScale = baseScale;
    }

    private void RefreshInstant()
    {
        if (playerRoll != null && playerRoll.IsOnCooldown)
        {
            float t = Mathf.Clamp01(1f - (playerRoll.CooldownRemaining / playerRoll.CooldownDuration));
            cooldownMask.fillAmount = 1f - t;
            iconImage.color = cooldownColor;
        }
        else
        {
            cooldownMask.fillAmount = 0f;
            iconImage.color = readyColor;
        }
    }
}
