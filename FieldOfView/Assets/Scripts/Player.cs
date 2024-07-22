using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 1f;
    [Range(float.Epsilon, 1f)] public float dampSpeed = 0.5f;

    private float horizontal = 0f;
    private float vertical = 0f;

    private void Update()
    {
        // Movement
        transform.position += new Vector3(Time.deltaTime * speed * horizontal, Time.deltaTime * speed * vertical);

        // Rotatement
        Vector3 mousePos2WorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 diff = mousePos2WorldPos - transform.position;
        transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg - 90f);
    }

    private void OnMove(InputValue value)
    {
        Vector2 v = value.Get<Vector2>();

        horizontal = v.x;
        vertical = v.y;
    }
}