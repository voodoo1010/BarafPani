using _Features.Player.Scripts;

namespace _Features.Abilities.Core.Scripts
{
    public interface IAbility
    {
        AbilityData Data { get; }
        int ChargesRemaining { get; }
        float CooldownRemaining { get; }
        float CooldownNormalized { get; }
        bool IsReady { get; }

        void OnAcquired(Character owner, AbilityData data, AbilityManagerBase manager);
        void OnReleased();
        bool TryActivate();
    }
}
