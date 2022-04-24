using Godot;
using Reversion.Module.Xsm;
using Reversion.Module.Xsm.Example;

namespace Reversion.module.xsm.example.movement;

// 空中状态
public class InAir : State<Player>
{
    protected override void _OnUpdate(float delta)
    {
        // 控制空中左右速度:如果有按左右键，则逼近预设速度
        if (Target.Dir != 0)
        {
            Target.Velocity = new Vector2(Mathf.Lerp(Target.Velocity.x, Target.AirSpeed * Target.Dir, Target.Acceleration * delta), Target.Velocity.y);
        }
        // 控制空中左右速度:如果有按左右键，则逼近0
        if (Target.Dir == 0)
        {
            Target.Velocity = new Vector2(Mathf.Lerp(Target.Velocity.x, 0, Target.AirFriction * delta), Target.Velocity.y);
            // 减速过程需要设一个临界值，小于临界值直接设为0。
            if (Mathf.Abs(Target.Velocity.x) < Target.WalkMargin)
            {
                Target.Velocity = new Vector2(0, Target.Velocity.y);
            }
        }
    }
}