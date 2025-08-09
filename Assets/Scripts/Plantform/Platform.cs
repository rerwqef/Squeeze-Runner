using UnityEngine;

public class Platform : MonoBehaviour
{
    public float speed;
    public Type myType;

    public void Init(float moveSpeed)
    {
        speed = moveSpeed;
    }

    void Update()
    {
      //  Debug.Log("calling in here");
      //  transform.Translate(Vector3.forward * -speed * Time.deltaTime, Space.World);
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
    Starting,
    small,
    medium,
    large,
    transition
}

