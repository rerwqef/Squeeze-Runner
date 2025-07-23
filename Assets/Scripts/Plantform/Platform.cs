using UnityEngine;

public class Platform : MonoBehaviour
{
    private float speed;
    public Type myType;

    public void Init(float moveSpeed)
    {
        speed = moveSpeed;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * -speed * Time.deltaTime, Space.World);
    }

    void OnDrawGizmosSelected()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(rend.bounds.center, rend.bounds.size);
        }
    }
}

public enum Type
{
    small,
    medium,
    large,
    transition // ✅ Added for mediator platforms
}
