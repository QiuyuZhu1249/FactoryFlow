using UnityEngine;

public enum Direction
{
    Up,
    Right,
    Down,
    Left
}

public static class DirectionExtensions
{
    public static Vector2Int ToVector2Int(this Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:    return Vector2Int.up;
            case Direction.Right: return Vector2Int.right;
            case Direction.Down:  return Vector2Int.down;
            case Direction.Left:  return Vector2Int.left;
            default:              return Vector2Int.zero;
        }
    }

    public static Direction Opposite(this Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:    return Direction.Down;
            case Direction.Right: return Direction.Left;
            case Direction.Down:  return Direction.Up;
            case Direction.Left:  return Direction.Right;
            default:              return dir;
        }
    }

    public static Direction RotateCW(this Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:    return Direction.Right;
            case Direction.Right: return Direction.Down;
            case Direction.Down:  return Direction.Left;
            case Direction.Left:  return Direction.Up;
            default:              return dir;
        }
    }

    public static Direction RotateCCW(this Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:    return Direction.Left;
            case Direction.Left:  return Direction.Down;
            case Direction.Down:  return Direction.Right;
            case Direction.Right: return Direction.Up;
            default:              return dir;
        }
    }

    /// <summary>
    /// Returns the Z rotation in degrees for a sprite facing this direction.
    /// Default sprite orientation is assumed to be "Up" (0 degrees).
    /// </summary>
    public static float ToRotationZ(this Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:    return 0f;
            case Direction.Right: return -90f;
            case Direction.Down:  return 180f;
            case Direction.Left:  return 90f;
            default:              return 0f;
        }
    }

    /// <summary>
    /// Returns the Direction from one grid position to an adjacent grid position.
    /// </summary>
    public static Direction FromVector(Vector2Int from, Vector2Int to)
    {
        Vector2Int delta = to - from;
        if (delta == Vector2Int.up)    return Direction.Up;
        if (delta == Vector2Int.right) return Direction.Right;
        if (delta == Vector2Int.down)  return Direction.Down;
        if (delta == Vector2Int.left)  return Direction.Left;

        Debug.LogWarning($"[Direction] Non-adjacent vector: {delta}");
        return Direction.Up;
    }
}
