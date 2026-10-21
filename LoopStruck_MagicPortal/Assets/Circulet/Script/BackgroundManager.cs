using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Game/Background Manager")]
public class BackgroundManager : MonoBehaviour
{
    [Header("The UI Image that displays the background")]
    [SerializeField] Image backgroundImage;

    [Header("Drag every possible background sprite in here")]
    [SerializeField] Sprite[] backgrounds;

    [Header("Optional: avoid repeating the same background twice in a row")]
    [SerializeField] bool avoidImmediateRepeat = true;

    int lastIndex = -1;

    private void Awake()
    {
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
    }

    /// <summary>
    /// Assigns a random sprite from the array to the background Image.
    /// Call this whenever you want a new random background (e.g. on Play).
    /// </summary>
    public void SetRandomBackground()
    {
        if (backgroundImage == null)
        {
            Debug.LogWarning("BackgroundManager: no Image assigned/found.");
            return;
        }

        if (backgrounds == null || backgrounds.Length == 0)
        {
            Debug.LogWarning("BackgroundManager: no background sprites assigned.");
            return;
        }

        int index = Random.Range(0, backgrounds.Length);

        if (avoidImmediateRepeat && backgrounds.Length > 1)
        {
            while (index == lastIndex)
            {
                index = Random.Range(0, backgrounds.Length);
            }
        }

        backgroundImage.sprite = backgrounds[index];
        lastIndex = index;
    }
}