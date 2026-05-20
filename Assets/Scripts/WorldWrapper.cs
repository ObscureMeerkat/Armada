using UnityEngine;

public class WorldWrapper : MonoBehaviour
{
    public float mapWidth = 150f;
    public float mapHeight = 150f;
    public float wrapPadding = 1f;

    private float halfWidth;
    private float halfHeight;

    void Start()
    {
        halfWidth = mapWidth / 2 - wrapPadding;
        halfHeight = mapHeight / 2 - wrapPadding;
    }

    void Update()
    {
        Vector3 pos = transform.position;

        if (pos.x > halfWidth) pos.x = -halfWidth;
        else if (pos.x < -halfWidth) pos.x = halfWidth;

        if (pos.y > halfHeight) pos.y = -halfHeight;
        else if (pos.y < -halfHeight) pos.y = halfHeight;

        transform.position = pos;
    }
}