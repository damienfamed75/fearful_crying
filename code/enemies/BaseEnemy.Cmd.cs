using Sandbox;
using System.Linq;

namespace FearfulCry.Enemies;

public partial class BaseEnemy
{
    [ConCmd.Server("ffc_enemy_clear")]
    public static void EnemyClear()
    {
        foreach (var enemy in All.OfType<BaseEnemy>().ToArray()) {
			enemy.Delete();
		}
    }

    [ConCmd.Server("ffc_enemy_agro")]
    public static void EnemyAgro()
    {
        foreach (var enemy in All.OfType<BaseEnemy>().ToArray()) {
			enemy.StartChase();
		}
    }

    [ConCmd.Server("ffc_enemy_wander")]
    public static void EnemyWander()
    {
        foreach (var enemy in All.OfType<BaseEnemy>().ToArray()) {
			enemy.StartWander();
		}
    }

    [ConCmd.Server("ffc_enemy_idle")]
    public static void EnemyIdle()
    {
        foreach (var enemy in All.OfType<BaseEnemy>().ToArray()) {
			enemy.EnemyState = EnemyState.None;
			enemy.Steer = null;
		}
    }

    [ConCmd.Server("ffc_nav_drawpath")]
    public static void NavDrawPath(bool status)
    {
		DrawNavPath = status;
	}
}