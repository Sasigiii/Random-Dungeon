using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private GameObject healthBar;

    /// <summary>
    /// Enable the health bar
    /// </summary>
    public void EnableHealthBar()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Disable the health bar
    /// </summary>
    public void DisableHealthBar()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Set health bar value with health percent between 0 and 1
    /// </summary>
    public void SetHealthBarValue(float healthPercent)
    {
        healthBar.transform.localScale = new Vector3(healthPercent, 1f, 1f);
    }
}