using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollViewStartAtTop : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;

    void OnEnable()
    {
        // Wait one frame so layout can finish updating
        StartCoroutine(ResetScrollPosition());
    }

    IEnumerator ResetScrollPosition()
    {
        yield return null; // wait one frame
        scrollRect.verticalNormalizedPosition = 1f; // scroll to top
    }
}
