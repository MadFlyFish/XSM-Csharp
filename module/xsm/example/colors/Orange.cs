using Godot;
using Reversion.Module.Xsm;
using Reversion.Module.Xsm.Example;

namespace Reversion.module.xsm.example.colors;

public class Orange : State<Player>
{
    protected override void _OnEnter(dynamic args)
    {
        Target.Modulate = Colors.Orange;
    }

    protected override void _OnUpdate(float delta)
    {
        if (Input.IsActionJustPressed("prev_color"))
        {
            ChangeState("Green", "我要变Green了");
        }
        else if (Input.IsActionJustPressed("next_color"))
        {
            ChangeState("Purple", "我要变Purple了");
        }
    }
    
    public void WhoWasI()
    {
        GD.Print("I was Orange");
    }
}
