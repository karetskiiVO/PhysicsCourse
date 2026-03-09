using UnityEngine;

[System.Serializable]
public struct State
{
    public Vector2 position;
    public Vector2 velocity;

    public State(Vector2 position, Vector2 velocity)
    {
        this.position = position;
        this.velocity = velocity;
    }
}
