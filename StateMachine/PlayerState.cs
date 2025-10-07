using UnityEngine;

public class PlayerState : EntityState
{
    protected Player player;
    protected PlayerInputSet input;
    protected PlayerSkill_Manager skillManager;

    protected PlayerAttack CurrentAttack => player.playerCombat?.currentAttack;


    public PlayerState(Player player, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {

        this.player = player;
        input = player.input;
        anim = player.anim;
        rb = player.rb;
        skillManager = player.skillManager;



    }

    public override void Enter()
    {
        base.Enter();
    
    }

    public override void Update()
    {
        base.Update();


        if (input.Player.Interact.WasPressedThisFrame())
        {
            if (player.TryFindInteractable(player.interactRadius, out var interactable, out var profile))
            {
                player.pendingInteractable = interactable;
                player.pendingProfile = profile;

                if(interactable is Object_Chest)
                    stateMachine.ChangeState(player.openChestState);
                else
                    interactable.Interact(player);
            }
        }

    }
}
