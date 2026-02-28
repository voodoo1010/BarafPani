using UnityEngine;

/// <summary>
/// IceWallSegment - Attach this to your Wall Segment Prefab (the individual cube).
///
/// SETUP:
/// 1. Create a small cube prefab (e.g. X=1, Y=2, Z=0.2).
/// 2. Give it an ice-blue material.
/// 3. Attach this script.
/// 4. Assign it as the "wallSegmentPrefab" in IceWall.cs.
///
/// HOW DAMAGE WORKS:
/// - Segments slowly deteriorate on their own (natural decay).
/// - Call TakeDamage(amount) from your attack/melee script to break it faster.
/// - The segment visually shows 4 health states: Pristine → Cracked → Damaged → Critical.
/// - At 0 HP it shatters and destroys itself.
/// </summary>
public class IceWallSegment : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Inspector Settings
    // -----------------------------------------------------------------------

    [Header("Health")]
    [Tooltip("Maximum health of this segment.")]
    public float maxHealth = 100f;

    [Header("Deterioration (Self-Decay)")]
    [Tooltip("Health lost per second naturally over time.")]
    public float deteriorationPerSecond = 2f;

    [Tooltip("Delay in seconds before natural decay starts after placement.")]
    public float deteriorationDelay = 3f;

    [Header("Visual Damage States")]
    [Tooltip("Colour when the segment is pristine (full health).")]
    public Color colourPristine = new Color(0.4f, 0.85f, 1f, 0.85f);

    [Tooltip("Colour at the Cracked stage (~66% HP).")]
    public Color colourCracked = new Color(0.3f, 0.65f, 0.9f, 0.80f);

    [Tooltip("Colour at the Damaged stage (~33% HP).")]
    public Color colourDamaged = new Color(0.2f, 0.4f, 0.75f, 0.75f);

    [Tooltip("Colour at Critical stage (<15% HP).")]
    public Color colourCritical = new Color(0.6f, 0.3f, 0.3f, 0.70f);

    [Header("Shatter FX (Optional)")]
    [Tooltip("Optional particle effect spawned when the segment is destroyed.")]
    public GameObject shatterVFXPrefab;

    // -----------------------------------------------------------------------
    // Private State
    // -----------------------------------------------------------------------

    private float _currentHealth;
    private float _deteriorationTimer;
    private bool _isDead;
    private Renderer[] _renderers;

    // Cached health thresholds
    private float _crackedThreshold;   // 66%
    private float _damagedThreshold;   // 33%
    private float _criticalThreshold;  // 15%

    // -----------------------------------------------------------------------
    // Unity Lifecycle
    // -----------------------------------------------------------------------

    void Awake()
    {
        _currentHealth = maxHealth;
        _deteriorationTimer = deteriorationDelay;
        _renderers = GetComponentsInChildren<Renderer>();

        _crackedThreshold = maxHealth * 0.66f;
        _damagedThreshold = maxHealth * 0.33f;
        _criticalThreshold = maxHealth * 0.15f;

        ApplyVisualState();
    }

    void Update()
    {
        if (_isDead) return;

        // -- Natural Decay --
        if (_deteriorationTimer > 0f)
        {
            _deteriorationTimer -= Time.deltaTime;
        }
        else
        {
            ApplyDamage(deteriorationPerSecond * Time.deltaTime);
        }
    }

    // -----------------------------------------------------------------------
    // Public API (call this from your attack / projectile scripts)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Deal damage to this segment. Call this from your player's attack script.
    /// Example:
    /// hit.collider.GetComponent<IceWallSegment>()?.TakeDamage(25f);
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (_isDead) return;
        ApplyDamage(amount);
    }

    /// <summary>Returns current health (0–maxHealth).</summary>
    public float CurrentHealth => _currentHealth;

    /// <summary>Returns health as a 0–1 percentage.</summary>
    public float HealthPercent => _currentHealth / maxHealth;

    // -----------------------------------------------------------------------
    // Internal Damage & Death
    // -----------------------------------------------------------------------

    void ApplyDamage(float amount)
    {
        _currentHealth -= amount;
        _currentHealth = Mathf.Max(_currentHealth, 0f);

        ApplyVisualState();

        if (_currentHealth <= 0f)
            Shatter();
    }

    void Shatter()
    {
        if (_isDead) return;
        _isDead = true;

        if (shatterVFXPrefab != null)
            Instantiate(shatterVFXPrefab, transform.position, transform.rotation);

        Destroy(gameObject);
    }

    // -----------------------------------------------------------------------
    // Visual Damage States
    // -----------------------------------------------------------------------

    void ApplyVisualState()
    {
        Color target;

        if (_currentHealth > _crackedThreshold)
            target = colourPristine;
        else if (_currentHealth > _damagedThreshold)
            target = colourCracked;
        else if (_currentHealth > _criticalThreshold)
            target = colourDamaged;
        else
            target = colourCritical;

        // Slight shrink effect as it deteriorates
        float scaleY = Mathf.Lerp(0.6f, 1f, _currentHealth / maxHealth);
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(s.x, scaleY * GetInitialScaleY(), s.z);

        SetColour(target);
    }

    // Store the original Y scale on first call
    private float _initialScaleY = -1f;

    float GetInitialScaleY()
    {
        if (_initialScaleY < 0f)
            _initialScaleY = transform.localScale.y;

        return _initialScaleY;
    }

    void SetColour(Color c)
    {
        foreach (Renderer r in _renderers)
        {
            foreach (Material mat in r.materials)
            {
                if (mat.HasProperty("_Color"))
                    mat.color = c;

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", c);
            }
        }
    }

    // -----------------------------------------------------------------------
    // Debug Gizmos
    // -----------------------------------------------------------------------

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.Lerp(Color.red, Color.cyan, _currentHealth / maxHealth);

        Gizmos.DrawWireCube(
            transform.position + Vector3.up * 1.5f,
            new Vector3(HealthPercent, 0.1f, 0.1f)
        );
    }
}