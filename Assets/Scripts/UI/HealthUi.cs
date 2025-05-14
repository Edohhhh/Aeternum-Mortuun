using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    public GameObject heartPrefab;
    public int spacing = 4;
    private List<Image> hearts = new List<Image>();

    public void Initialize(float maxHearts)
    {
        ClearHearts();
        for (int i = 0; i < Mathf.CeilToInt(maxHearts); i++)
        {
            var go = Instantiate(heartPrefab, transform);
            hearts.Add(go.GetComponent<Image>());
        }
    }

    public void UpdateHearts(float currentHearts)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            float fill = Mathf.Clamp(currentHearts - i, 0f, 1f);
            hearts[i].fillAmount = fill; // assumes heartPrefab uses Image.type=Filled
        }
    }

    private void ClearHearts()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        hearts.Clear();
    }
}
