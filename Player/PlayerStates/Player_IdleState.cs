using UnityEngine;

public class Player_IdleState : PlayerState
{
    private float idleTimer;
    private const float LookAroundDuration = 6f;
    private const float LookAroundInterval = 10f;
    private const float BlendSpeed = 0.5f;
    private const float IdleBlendValue = -1f;
    private const float LookAroundBlendValue = 1f;

    private bool isLookingAround = false;
    private float targetBlendValue;
    public Player_IdleState(Player player,StateMachine stateMachine, string stateName) : base(player,stateMachine, stateName)
    {
        targetBlendValue = -1f;
    }

    public override void Update()
    {
        base.Update();

        idleTimer += Time.deltaTime;

        if(player.playerMovement.moveInput.sqrMagnitude > 0f)
        {
            player.RequestStateChange(player.moveState);
        }

        //condition to start looking around 
        if(!isLookingAround && idleTimer > LookAroundInterval)
        {
            isLookingAround = true;
            idleTimer = 0f;
            targetBlendValue = LookAroundBlendValue;
        } 

        //condition to idle
        if(isLookingAround && idleTimer > LookAroundDuration)
        {
            isLookingAround = false;
            idleTimer = 0f;
            targetBlendValue = IdleBlendValue;
        }

        float currentBlendValue = player.anim.GetFloat("poseOrLook");
        currentBlendValue = Mathf.Lerp(currentBlendValue, targetBlendValue, Time.deltaTime * BlendSpeed);
        player.anim.SetFloat("poseOrLook", currentBlendValue);

        


    }
}
