using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttackIndicatorUIController : MonoBehaviour
{
    [Header("References")]
    public Player_RangeAttackController rangeController;

    [Header("Attack Icons")]
    public GameObject normalAttackUI;
    public GameObject chargedAttackUI;
    public GameObject scatterAttackUI;
    public Image weaponUseUI;

    [Header("Shared Shine Image (behind icons)")]
    public Image shineImage;   // ← your single shine sprite

    [Header("Label")]
    public TextMeshProUGUI attackLabelText;

    [Header("Animation Settings")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float scaleLerpSpeed = 6f;
    [SerializeField] private float shineFadeSpeed = 6f;
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float pulseAmount = 0.15f;

    private GameObject[] attackIcons;
    private int currentIndex = 0;
    private int previousIndex = 0;

    void Start()
    {
        GameObject autoCanvas = GameObject.Find("AttackTypeCanvas");
        if (autoCanvas != null)
            Destroy(autoCanvas);

        attackIcons = new GameObject[]
        {
            normalAttackUI,
            chargedAttackUI,
            scatterAttackUI
        };

        foreach (var icon in attackIcons)
        {
            icon.SetActive(true);
            icon.transform.localScale = Vector3.one * normalScale;
        }

        currentIndex = 0;
        if (attackLabelText != null)
            attackLabelText.text = GetDisplayName(currentIndex);

        attackIcons[currentIndex].transform.localScale = Vector3.one * selectedScale;

        // hide shine initially
        if (shineImage != null)
        {
            Color c = shineImage.color;
            c.a = 0f;
            shineImage.color = c;
        }
    }

    void Update()
    {
        UpdateAttackIndicator(rangeController.CurrentAttack);
        AnimateScaleTransition();
        AnimateShine();
    }

    private void AnimateScaleTransition()
    {
        for (int i = 0; i < attackIcons.Length; i++)
        {
            float targetScale = (i == currentIndex) ? selectedScale : normalScale;
            attackIcons[i].transform.localScale = Vector3.Lerp(
                attackIcons[i].transform.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * scaleLerpSpeed
            );
        }
    }

    private void AnimateShine()
    {
        if (shineImage == null) return;

        // fade the shine in
        Color c = shineImage.color;
        c.a = Mathf.Lerp(c.a, 1f, Time.deltaTime * shineFadeSpeed);
        shineImage.color = c;

        // pulse softly
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        shineImage.transform.localScale = Vector3.one * pulse;

        // position the shine behind the active icon
        shineImage.transform.position = attackIcons[currentIndex].transform.position;
    }

    public void UpdateAttackIndicator(Player_RangeAttack attack)
    {
        if (attack == null) return;

        int index = 0;
        if (attack is Player_NormalShotAttack) index = 0;
        else if (attack is Player_ChargedRangeAttack) index = 1;
        else if (attack is Player_ScatterRangeAttack) index = 2;

        if (index != currentIndex)
        {
            previousIndex = currentIndex;
            currentIndex = index;

            if (attackLabelText)
                attackLabelText.text = GetDisplayName(index);

            // brief brightness pop when switching
            if (shineImage != null)
            {
                shineImage.color = new Color(shineImage.color.r, shineImage.color.g, shineImage.color.b, 1.2f);
            }
        }
    }

    private string GetDisplayName(int i)
    {
        switch (i)
        {
            case 0: return "Normal Shot";
            case 1: return "Charged Shot";
            case 2: return "Scatter Shot";
            default: return "Normal Shot";
        }
    }
}
