using Godot;
using Reversion.Module.Xsm;
using Reversion.Module.Xsm.Example;

namespace Reversion.module.xsm.example.movement;

// 自由落体状态（飞升或者下落）
public class Fall : State<Player>
{
    protected override void _OnEnter(dynamic args)
    {
        // 根据速度方向播放飞升或者下落的动画
        Play(Target.Velocity.y >= 0 ? "Fall" : "FlyUp");
        // 摔下来
        if (StateRoot.WasStateActive("Walk"))
        {
            GD.Print("Fall from Walk");
        }
        // 跳上去
        if (StateRoot.WasStateActive("Jump"))
        {
            GD.Print("Fall from Jump");
        }
    }

    protected override void _OnUpdate(float delta)
    {
        if (Target.IsOnFloor() && GetNodeOrNull("PreJump") != null)
        {
            ChangeState("Jump");
            return;
        }

        if (Target.IsOnFloor())
        {
            ChangeState("Land");
            return;
        }
        
        // 到达顶部时，播放“浮空”动画
        if (Target.Velocity.y < 0 && Target.Velocity.y > -200 && !IsPlaying("TopCurve"))
        {
            Play("TopCurve");
        }

        if (Input.IsActionJustPressed("jump") && GetNodeOrNull("CoyoteTime") != null)
        {
            ChangeState("Jump");
        }
        
        else if (Input.IsActionJustPressed("jump"))
        {
            AddTimer("PreJump", Target.PreJumpTime);
        }
    }

    public void OnTopCurveFinished()
    {
        Play("Fall");
    }
}