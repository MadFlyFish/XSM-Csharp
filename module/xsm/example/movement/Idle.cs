using Godot;
using Reversion.Module.Xsm;
using Reversion.Module.Xsm.Example;

namespace Reversion.module.xsm.example.movement;

/// 待机状态
public class Idle : State<Player>
{
    protected override void _OnEnter(dynamic args)
    {
        Play("Idle"); // 播放待机动画
    }

    protected override void _OnUpdate(float delta)
    {   
        // 如果x轴大于临界值，则进入移动状态
        if (Mathf.Abs(Target.Velocity.x) > Target.WalkMargin)
        {
            ChangeState("Walk");
        }
    }
}