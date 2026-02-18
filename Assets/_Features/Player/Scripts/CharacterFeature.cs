using UnityEngine;

namespace _Features.Player.Scripts
{
    [RequireComponent(typeof(Character))]
    public abstract class CharacterFeature : MonoBehaviour
    {
        protected Character Character { get; private set; }

        protected virtual void Awake()
        {
            Character = GetComponent<Character>();
        }
    }
}