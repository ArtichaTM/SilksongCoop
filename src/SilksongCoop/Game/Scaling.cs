using System.Runtime.CompilerServices;
using SilksongCoop.Rules;

namespace SilksongCoop.Game;

/// <summary>
/// D22, D41, D50: an enemy is scaled once at spawn and a boss again at its title, only while a friend is
/// connected; joins and leaves never rescale what is already alive.
/// </summary>
internal static class Scaling
{
    private static readonly ConditionalWeakTable<HealthManager, Applied> Scaled = new();

    public static void OnSpawn(HealthManager health, CoopConfig config)
    {
        if (Scaled.TryGetValue(health, out _))
        {
            return;
        }
        bool boss = HpScaling.LooksLikeBoss(health.hp, config.BossHpThreshold.Value);
        float factor = boss ? config.BossHp.Value : config.EnemyHp.Value;
        Apply(health, 1f, factor);
        Scaled.Add(health, new Applied(factor, boss));
    }

    public static void OnBossTitle(HealthManager health, CoopConfig config)
    {
        float applied = 1f;
        if (Scaled.TryGetValue(health, out Applied? before))
        {
            if (before.Boss)
            {
                return;
            }
            applied = before.Factor;
            Scaled.Remove(health);
        }
        float factor = config.BossHp.Value;
        Apply(health, applied, factor);
        Scaled.Add(health, new Applied(factor, true));
    }

    private static void Apply(HealthManager health, float applied, float wanted)
    {
        if (applied == wanted || health.isDead)
        {
            return;
        }
        health.hp = HpScaling.Rescale(health.hp, applied, wanted);
        health.initHp = HpScaling.Rescale(health.initHp, applied, wanted);
    }

    private sealed class Applied(float factor, bool boss)
    {
        public float Factor { get; } = factor;

        public bool Boss { get; } = boss;
    }
}
