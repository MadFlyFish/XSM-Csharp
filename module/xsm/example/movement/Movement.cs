using Godot;
using Reversion.Module.Xsm;
using Reversion.Module.Xsm.Example;

namespace Reversion.module.xsm.example.movement;

/// 总的移动状态，在这个状态，共同操作是左右移动的操作，以及最基础的KinematicBody2D移动
public class Movement : State<Player>
{
    protected override void _OnUpdate(float delta)
    {
        // 控制左右朝向
        Target.Dir = 0;

        if (Input.IsActionPressed("left"))
        {
            Target.Dir = -1;
            if (! Target.GetNode<Sprite>("Sprite").FlipH)
            {
                Target.GetNode<Sprite>("Sprite").FlipH = true;
            }
        }
        else if (Input.IsActionPressed("right"))
        {
            Target.Dir = 1;
            if (Target.GetNode<Sprite>("Sprite").FlipH)
            {
                Target.GetNode<Sprite>("Sprite").FlipH = false;
            }
        }

        // 重力加速
        Target.Velocity += new Vector2(0, delta * Target.Gravity);
    }

    // 注意：这里放在_AfterUpdate中，目的是？？？（猜测是等各状态_OnUpdate全部执行完，再对Velocity的修改再执行最终的MoveAndSlide）
    protected override void _AfterUpdate(float delta)
    {
        Target.Velocity = Target.MoveAndSlide(Target.Velocity, Vector2.Up);
    }
}