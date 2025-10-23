
using UnityEngine;

public class Player_DeathState : PlayerState
{
    public Player_DeathState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.input.Player.Disable();

        foreach (var col in player.GetComponentsInChildren<Collider>())
            col.enabled = false;


    }

    public override void Update()
    {
        var info = anim.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("playerDeath") && info.normalizedTime >= 1f)
        {
            Debug.Log("Death animation finished — calling EntityDeath()");

            player.EntityDeath();
        }
    }

    public override void Exit()
    {
        player.input.Player.Enable();

    }
}
