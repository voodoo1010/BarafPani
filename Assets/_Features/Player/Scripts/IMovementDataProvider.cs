using UnityEngine;

namespace _Features.Player.Scripts
{
    public interface IMovementDataProvider
    {
        Vector2 MoveInput { get; }
    }
}
