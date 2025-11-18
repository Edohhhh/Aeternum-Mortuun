using UnityEngine;

public class ShowUIOnDestroy : MonoBehaviour
{
    [Header("UI a mostrar")]
    public GameObject uiToShow;

    private bool shown = false;

    private void Start()
    {
        if (uiToShow != null)
            uiToShow.SetActive(false);
    }

    public void ActivateUI()
    {
        if (shown) return;

        shown = true;
        if (uiToShow != null)
            uiToShow.SetActive(true);
    }
}
