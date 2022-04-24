 using Godot;
using Reversion.Module.Xsm;
using Reversion.Module.Xsm.Example;

namespace Reversion.module.xsm.example.movement;

/// 起跳状态（但尚未升空）
public class Jump : State<Player>
{
    // 是否正在跳跃
    private bool Jumping {set; get;} = false;
    
    protected override void _OnEnter(dynamic args)
    {
       Play("Jump"); // 起跳动画
       AddTimer("JumpTimer", Target.JumpTime);  // 跳跃时间（可以类比为站在地上蓄力跳），时间到了会进入自由落体
       Jumping = true; // 标记为正处于起跳阶段
    }

    protected override void _OnUpdate(float delta)
    {
        // 松开跳跃键，则不再处于“蓄力阶段”，不再累积向上速度
        if (Input.IsActionJustReleased("jump"))
        {
            Jumping = false;
        }
        // 如果一直按着跳跃键，则处于蓄力阶段，会一直累计向上的速度
        if (Jumping)
        {
            Target.Velocity = new Vector2(Target.Velocity.x, -Target.JumpSpeed);
        }
    }

    protected override void _OnExit(dynamic args)
    {
        Jumping = false;
    }

    public void OnJumpFinished()
    {
        Play("FlyUp"); // 起跳动画播完，则继续播放往上飞升的动画
    }

    protected override void _OnTimeout(string name)
    {
        Jumping = false; // 起跳状态结束
        ChangeState("Fall"); // 进入自由落体
    }
}