using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform target;
    [Min(float.Epsilon)] public float damp = 1f;

    private void Start()
    {
        if (!target)
        {
            target = GameObject.Find("Player").transform;
        }
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, target.position, damp * Time.deltaTime);
    }
}