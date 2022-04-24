using Godot;
using Reversion.Module.Xsm;
using Reversion.Module.Xsm.Example;

namespace Reversion.module.xsm.example.movement;

// 在地板上：控制左右速度
public class OnGround : State<Player>
{

    protected override void _OnUpdate(float delta)
    {
        // 如果按了左右键，左右速度插值逼近预设移动速度
        if (Target.Dir != 0)
        {
            Target.Velocity = new Vector2(Mathf.Lerp(Target.Velocity.x, Target.GroundSpeed * Target.Dir, Target.Acceleration * delta), Target.Velocity.y);
        }
        // 如果没按左右键，左右速度插值逼近0
        else
        {
            Target.Velocity = new Vector2(Mathf.Lerp(Target.Velocity.x, 0, Target.GroundFriction * delta), Target.Velocity.y);
            if (Mathf.Abs(Target.Velocity.x) < Target.WalkMargin)
            {
                Target.Velocity = new Vector2(0, Target.Velocity.y);
            }
        }

        // 如果按了空格键，则进入跳跃状态。
        if (Input.IsActionJustPressed("jump"))
        {
            ChangeState("Jump");
        }
        // 如果不在地板上，则进入下落状态。
        else if (! Target.IsOnFloor())
        {
            GetNode<State<Player>>("../InAir/Fall").AddTimer("CoyoteTime", Target.CoyoteTime);
            ChangeState("Fall");
        }
        // 如果左右速度接近0，而且按下了下蹲键，则进入下蹲状态。
        else if (Mathf.Abs(Target.Velocity.x) < Target.WalkMargin && Input.IsActionJustPressed("crouch"))
        {
            ChangeState("Crouch");
        }
    }
}