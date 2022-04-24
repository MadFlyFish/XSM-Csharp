using Godot;
using Reversion.Module.Xsm;
using Reversion.Module.Xsm.Example;

namespace Reversion.module.xsm.example.movement;

/// 落地瞬间的状态（摔扁了的那下）
public class Land : State<Player>
{
    protected override void _OnEnter(dynamic args)
    {
        Play("Land");
    }

    // 落地动画播放完后触发，转入到待机
    public void OnLandFinished()
    {
        ChangeState("Idle");
    }
}