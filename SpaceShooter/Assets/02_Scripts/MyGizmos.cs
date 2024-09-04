using UnityEngine;

public class MyGizmos : MonoBehaviour
{
    public Color _color = Color.green; // 처음 색
    [Range(0.1f, 2.0f)]
    public float _radius = 0.3f;

    void OnDrawGizmos()
    {
        Gizmos.color = _color;
        Gizmos.DrawSphere(transform.position, _radius);
    }
}
