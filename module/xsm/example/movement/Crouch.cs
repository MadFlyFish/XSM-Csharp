using Godot;
using Reversion.Module.Xsm;
using Reversion.Module.Xsm.Example;

namespace Reversion.module.xsm.example.movement;

// 下蹲状态（只要是在OnGround状态下，都可以下蹲）
public class Crouch : State<Player>
{
    protected override void _OnEnter(dynamic args)
    {
        Play("Crouch");
    }

    protected override void _OnUpdate(float delta)
    {
        // 如果释放了下蹲键，则恢复到Idle。
        if (Input.IsActionJustReleased("crouch"))
        {
            ChangeState("OnGround/Idle");
        }
    }
}