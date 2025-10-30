using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterEntryDisplayUI : MonoBehaviour
{
    [Header("Display Elements")]
    public Image entryImage;
    public TMP_Text entryName;
    public TMP_Text baybayinNameText;
    public TMP_Text classification;
    public TMP_Text appearance;
    public TMP_Text behavior;
    public TMP_Text origin;
    public TMP_Text lore;

    [Header("Fallbacks")]
    public Sprite unknownSprite;

    [Header("Behavior")]
    public bool ignoreDiscovery = false; // ← set TRUE on the Monster Database panel only

    public void ShowEntry(MonsterEntry entry)
    {
        if (entry == null)
        {
            Debug.LogWarning("[MonsterEntryDisplayUI] Tried to show a null entry.");
            return;
        }

        bool reveal = ignoreDiscovery || entry.discovered; // ← key line

        if (!reveal)
        {
            entryImage.sprite = unknownSprite;
            entryName.text = "???";
            baybayinNameText.text = "Hindi Kilala";
            classification.text = "<b>Klasipikasyon:</b> ???";
            appearance.text = "<b>Anyo:</b> ???";
            behavior.text = "<b>Ugali:</b> ???";
            origin.text = "<b>Pinagmulan:</b> ???";
            lore.text = "<b>Kasaysayan:</b> Isang misteryosong nilalang na hindi pa naitatala.";
            return;
        }

        // reveal full data
        entryImage.sprite = entry.image;
        entryName.text = entry.entryName;
        baybayinNameText.text = entry.entryName;
        classification.text = $"<b>Klasipikasyon:</b> {entry.classification}";
        appearance.text = $"<b>Anyo:</b> {entry.appearance}";
        behavior.text = $"<b>Ugali:</b> {entry.behavior}";
        origin.text = $"<b>Pinagmulan:</b> {entry.origin}";
        lore.text = $"<b>Kasaysayan:</b> {entry.lore}";
    }
}
