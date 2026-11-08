using UnityEngine;

namespace FireBarrelCrafting.Spawning;

/// <summary>Vertical placement helpers based on what the player actually sees (mesh bounds), not on pivots.</summary>
internal static class GroundAligner
{
    private const float RayStartAboveBottom = 0.05f;
    private const float MaxRayDistance = 2f;
    private const float MinGap = 0.01f;
    private const float MaxDrop = 1.5f;

    /// <summary>Finds the lowest visible point of an object.</summary>
    /// <param name="gameObject">Object to measure, including its children.</param>
    /// <param name="bottom">Lowest world Y of its mesh renderers, falling back to its solid colliders.</param>
    /// <returns><see langword="false"/> if the object has neither renderers nor solid colliders.</returns>
    public static bool TryGetBottom(GameObject gameObject, out float bottom)
    {
        bottom = float.MaxValue;

        foreach (MeshRenderer meshRenderer in gameObject.GetComponentsInChildren<MeshRenderer>(true))
        {
            Bounds bounds = meshRenderer.bounds;
            if (bounds.size.sqrMagnitude > 0f)
            {
                bottom = Mathf.Min(bottom, bounds.min.y);
            }
        }

        if (bottom == float.MaxValue)
        {
            foreach (Collider collider in gameObject.GetComponentsInChildren<Collider>(true))
            {
                if (!collider.isTrigger)
                {
                    bottom = Mathf.Min(bottom, collider.bounds.min.y);
                }
            }
        }

        return bottom != float.MaxValue;
    }

    /// <summary>Moves an object vertically so its lowest visible point is at <paramref name="worldY"/>.</summary>
    /// <param name="gameObject">Object to move.</param>
    /// <param name="worldY">Target world height of its bottom.</param>
    public static void AlignBottomTo(GameObject gameObject, float worldY)
    {
        if (TryGetBottom(gameObject, out float bottom))
        {
            gameObject.transform.position += Vector3.up * (worldY - bottom);
        }
    }

    /// <summary>Lowers an object onto the surface below it. Never moves up, never drops more than 1.5 m.</summary>
    /// <param name="gameObject">Object to lower.</param>
    public static void SnapDown(GameObject gameObject)
    {
        if (!TryGetBottom(gameObject, out float bottom))
        {
            return;
        }

        Vector3 origin = gameObject.transform.position;
        origin.y = bottom + RayStartAboveBottom;

        if (DistanceToSurfaceBelow(gameObject.transform, origin) is not float hitDistance)
        {
            return;
        }

        float gap = hitDistance - RayStartAboveBottom;
        if (gap is > MinGap and < MaxDrop)
        {
            gameObject.transform.position += Vector3.down * gap;
        }
    }

    private static float? DistanceToSurfaceBelow(Transform self, Vector3 origin)
    {
        float? closest = null;
        foreach (RaycastHit hit in Physics.RaycastAll(origin, Vector3.down, MaxRayDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            Transform hitTransform = hit.collider.transform;
            bool isSelf = hitTransform == self || hitTransform.IsChildOf(self);
            if (!isSelf && (closest is null || hit.distance < closest))
            {
                closest = hit.distance;
            }
        }

        return closest;
    }
}
