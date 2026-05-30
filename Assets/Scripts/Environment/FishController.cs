using UnityEngine;

/// <summary>
/// Điều khiển một con cá bơi lội trong hồ:
/// bơi theo waypoints ngẫu nhiên trong vùng bán kính pondRadius,
/// thân cá lắc nhẹ để trông tự nhiên.
/// </summary>
[AddComponentMenu("Thôn An Lúa/Environment/Fish Controller")]
public class FishController : MonoBehaviour
{
    [HideInInspector] public float pondRadius = 3f;
    [HideInInspector] public Vector3 pondCenter;

    [Range(0.3f, 3f)] public float swimSpeed     = 0.6f;
    [Range(0.5f, 5f)] public float waypointDelay = 2f;   // dừng tại điểm đích rồi đổi hướng
    [Range(5f, 40f)]  public float turnSpeed     = 18f;

    private Vector3 _target;
    private float   _waitTimer;
    private bool    _waiting;

    // Lắc đuôi
    private float _wiggleTime;
    [HideInInspector] public float wiggleAmp   = 12f;
    [HideInInspector] public float wiggleSpeed = 4f;

    // ─────────────────────────────────────────────────────────────────────────
    private void Start()
    {
        _target    = PickTarget();
        _wiggleTime = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        if (_waiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f) { _waiting = false; _target = PickTarget(); }
            return;
        }

        // Di chuyển về phía mục tiêu
        Vector3 dir = (_target - transform.position);
        dir.y = 0;
        float dist = dir.magnitude;

        if (dist < 0.1f)
        {
            _waiting   = true;
            _waitTimer = waypointDelay;
            return;
        }

        // Xoay mượt về hướng di chuyển
        Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);

        // Tiến tới
        transform.position += transform.forward * swimSpeed * Time.deltaTime;

        // Lắc đuôi
        _wiggleTime += Time.deltaTime * wiggleSpeed;
        transform.localRotation = Quaternion.Euler(0,
            transform.localRotation.eulerAngles.y + Mathf.Sin(_wiggleTime) * wiggleAmp * Time.deltaTime,
            0);
    }

    private Vector3 PickTarget()
    {
        // Điểm ngẫu nhiên trong hồ
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float r     = Random.Range(0f, pondRadius * 0.8f);
        return pondCenter + new Vector3(Mathf.Cos(angle) * r, 0, Mathf.Sin(angle) * r);
    }
}
