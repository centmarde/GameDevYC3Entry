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

    [Header("Label")]
    public TextMeshProUGUI attackLabelText;

    [Header("Animation Settings")]
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float selectedScale = 1.5f;
    [SerializeField] private float scaleLerpSpeed = 6f;

    private GameObject[] attackIcons;
    private int currentIndex = 0;
    private int previousIndex = 0;

    void Start()
    {
        // Destroy any old runtime "AttackTypeCanvas" if it exists
        GameObject autoCanvas = GameObject.Find("AttackTypeCanvas");
        if (autoCanvas != null)
            Destroy(autoCanvas);

        attackIcons = new GameObject[]
        {
        normalAttackUI,
        chargedAttackUI,
        scatterAttackUI
        };

        // Make sure all icons are visible and scaled normally
        foreach (var icon in attackIcons)
        {
            icon.SetActive(true);
            icon.transform.localScale = Vector3.one * normalScale;
        }

        // ✅ Force initial display to Normal Shot
        currentIndex = 0;
        if (attackLabelText != null)
            attackLabelText.text = GetDisplayName(currentIndex);

        // ✅ Also make sure the normal shot icon starts scaled up
        attackIcons[currentIndex].transform.localScale = Vector3.one * selectedScale;
    }


    void Update()
    {
        UpdateAttackIndicator(rangeController.CurrentAttack);
        AnimateScaleTransition();
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
