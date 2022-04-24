using Godot;
using Reversion.Module.Xsm;
using Reversion.Module.Xsm.Example;

namespace Reversion.module.xsm.example.movement;

/// 在地面移动
public class Walk : State<Player>
{
    protected override void _OnEnter(dynamic args)
    {
        Play("Walk");
    }

    protected override void _OnUpdate(float delta)
    {
        // 如果速度接近零，则切换到Idle
        if (Mathf.Abs(Target.Velocity.x) < Target.WalkMargin)
        {
            ChangeState("Idle");
        }
        // 如果速度大于临界值，则播放Run动画，但要注意不能反复播，因为会打断动画
        else if (Mathf.Abs(Target.Velocity.x) > Target.WalkMargin)
        {
            if (! IsPlaying("Run"))
            {
                Play("Run");
            }
        }
        // 如果没按移动键，则播放刹车动画
        else if (Target.Dir == 0)
        {
            if (! IsPlaying("Brake"))
            {
                Play("Brake");
            }
        }
        else
        {
            if (! IsPlaying("Walk"))
            {
                Play("Walk");
            }
        }
    }
}