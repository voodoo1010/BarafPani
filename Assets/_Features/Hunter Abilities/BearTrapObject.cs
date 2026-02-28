using UnityEngine;

public class BearTrapObject : MonoBehaviour
{
    // -----------------------------------------------------------------------
    //  Inspector
    // -----------------------------------------------------------------------

    [Header("Trap Behaviour")]
    [Tooltip("How long the trap holds the victim before releasing and destroying itself.")]
    public float TrapDuration = 3f;

    [Tooltip("Damage dealt to the victim the instant the trap fires.")]
    public float SnapDamage = 25f;

    [Tooltip("Damage per second while the victim is held. Hooks into ITrappable if available.")]
    public float DotDamagePerSecond = 5f;

    [Header("Visual States")]
    [Tooltip("Colour while armed and waiting.")]
    public Color ColourArmed = new(0.55f, 0.35f, 0.10f, 1f);

    [Tooltip("Colour flash the instant the trap fires.")]
    public Color ColourSnapped = new(0.9f, 0.2f, 0.1f, 1f);

    [Tooltip("Colour while actively holding a victim.")]
    public Color ColourActive = new(0.7f, 0.15f, 0.05f, 1f);

    [Header("Optional FX")]
    [Tooltip("Particle prefab spawned on snap.")]
    public GameObject SnapVfxPrefab;

    [Tooltip("Sound played when the trap fires.")]
    public AudioClip SnapSfx;

    // -----------------------------------------------------------------------
    //  Private State
    // -----------------------------------------------------------------------

    private enum TrapState { Armed, Holding, Released }

    private TrapState _state = TrapState.Armed;

    private float _trapTimer;
    private GameObject _victim;

    private Rigidbody _victimRb;
    private RigidbodyConstraints _originalConstraints;
    private CharacterController _victimCc;
    private Vector3 _frozenPosition;

    private Renderer[] _renderers;
    private AudioSource _audioSource;
    private Vector3 _baseScale;

    // -----------------------------------------------------------------------
    //  Unity Lifecycle
    // -----------------------------------------------------------------------

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _audioSource = GetComponent<AudioSource>();
        _baseScale = transform.localScale;

        ApplyColour(ColourArmed);
    }

    private void Update()
    {
        if (_state != TrapState.Holding)
            return;

        EnforceFreeze();
        TickDot();
        TickTimer();
    }

    // -----------------------------------------------------------------------
    //  Trigger
    // -----------------------------------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (_state != TrapState.Armed)
            return;

        if (other.transform.IsChildOf(transform))
            return;

        Snap(other.gameObject);
    }

    // -----------------------------------------------------------------------
    //  Snap
    // -----------------------------------------------------------------------

    private void Snap(GameObject victim)
    {
        _state = TrapState.Holding;
        _victim = victim;
        _trapTimer = TrapDuration;
        _frozenPosition = victim.transform.position;

        FreezeVictim(victim);

        transform.localScale = new Vector3(
            _baseScale.x * 1.2f,
            _baseScale.y * 0.3f,
            _baseScale.z * 1.2f);

        ApplyColour(ColourSnapped);

        if (SnapVfxPrefab != null)
            Instantiate(SnapVfxPrefab, transform.position, Quaternion.identity);

        if (_audioSource != null && SnapSfx != null)
            _audioSource.PlayOneShot(SnapSfx);

        if (SnapDamage > 0f)
            TryDealDamage(victim, SnapDamage);

        Invoke(nameof(SetActiveColour), 0.1f);

        Debug.Log($"[BearTrap] Snapped on '{victim.name}' — holding for {TrapDuration}s.");
    }

    private void SetActiveColour() => ApplyColour(ColourActive);

    // -----------------------------------------------------------------------
    //  While Holding
    // -----------------------------------------------------------------------

    private void EnforceFreeze()
    {
        if (_victim == null)
        {
            ReleaseTrap();
            return;
        }

        _victim.transform.position = _frozenPosition;

        if (_victimRb != null)
        {
            _victimRb.linearVelocity = Vector3.zero;
            _victimRb.angularVelocity = Vector3.zero;
        }
    }

    private void TickDot()
    {
        if (DotDamagePerSecond <= 0f || _victim == null)
            return;

        TryDealDamage(_victim, DotDamagePerSecond * Time.deltaTime);
    }

    private void TickTimer()
    {
        _trapTimer -= Time.deltaTime;

        if (_trapTimer <= 0f)
            ReleaseTrap();
    }

    // -----------------------------------------------------------------------
    //  Release
    // -----------------------------------------------------------------------

    private void ReleaseTrap()
    {
        if (_state == TrapState.Released)
            return;

        _state = TrapState.Released;

        if (_victim != null)
            UnfreezeVictim(_victim);

        Debug.Log("[BearTrap] Released victim — trap destroyed.");
        Destroy(gameObject);
    }

    // -----------------------------------------------------------------------
    //  Freeze Helpers
    // -----------------------------------------------------------------------

    private void FreezeVictim(GameObject victim)
    {
        _victimRb = victim.GetComponent<Rigidbody>();
        if (_victimRb != null)
        {
            _originalConstraints = _victimRb.constraints;
            _victimRb.linearVelocity = Vector3.zero;
            _victimRb.angularVelocity = Vector3.zero;
            _victimRb.constraints = RigidbodyConstraints.FreezeAll;
        }

        _victimCc = victim.GetComponent<CharacterController>();
        if (_victimCc != null)
            _victimCc.enabled = false;

        victim.GetComponent<ITrappable>()?.SetTrapped(true);
    }

    private void UnfreezeVictim(GameObject victim)
    {
        if (_victimRb != null)
        {
            _victimRb.constraints = _originalConstraints;
            _victimRb = null;
        }

        if (_victimCc != null)
        {
            _victimCc.enabled = true;
            _victimCc = null;
        }

        victim.GetComponent<ITrappable>()?.SetTrapped(false);
    }

    // -----------------------------------------------------------------------
    //  Damage
    // -----------------------------------------------------------------------

    private void TryDealDamage(GameObject target, float amount)
    {
        ITrappable trappable = target.GetComponent<ITrappable>();

        if (trappable != null)
        {
            trappable.TrapDamage(amount);
            return;
        }

        Debug.Log($"[BearTrap] {amount:F1} dmg → {target.name}  " +
                  "(add ITrappable to your character or hook up your health script here).");
    }

    // -----------------------------------------------------------------------
    //  Colour
    // -----------------------------------------------------------------------

    private void ApplyColour(Color colour)
    {
        foreach (Renderer r in _renderers)
        {
            foreach (Material mat in r.materials)
            {
                if (mat.HasProperty("_Color"))
                    mat.color = colour;

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", colour);
            }
        }
    }

    // -----------------------------------------------------------------------
    //  Gizmos
    // -----------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
            return;

        Gizmos.color = _state == TrapState.Holding
            ? new Color(1f, 0.1f, 0.1f, 0.4f)
            : new Color(1f, 0.5f, 0f, 0.3f);

        Gizmos.DrawCube(col.bounds.center, col.bounds.size * 1.05f);
    }
}

// ---------------------------------------------------------------------------
//  ITrappable Interface
// ---------------------------------------------------------------------------

public interface ITrappable
{
    void TrapDamage(float amount);
    void SetTrapped(bool trapped);
}