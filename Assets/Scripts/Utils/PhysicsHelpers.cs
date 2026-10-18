using Unity.Collections;
using UnityEngine;

public static class PhysicsHelpers
{
    public static ColliderArray2D OverlapBox(this BoxCollider2D boxCollider2D)
    {
        Vector2 point = boxCollider2D.transform.position;
        Vector2 size = boxCollider2D.size;
        float angle = boxCollider2D.transform.rotation.z;

        return Physics2D.OverlapBox(point, size, angle, ContactFilter2D.noFilter, Allocator.Temp);
    }
}
