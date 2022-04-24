using Godot;
using Reversion.Module.Xsm;
using Reversion.Module.Xsm.Example;

namespace Reversion.module.xsm.example.colors;

public class Green : State<Player>
{
    protected override void _OnEnter(dynamic args)
    {
        Target.Modulate = Colors.Green;
    }
    
    protected override void _OnUpdate(float delta)
    {
        if (Input.IsActionJustPressed("prev_color"))
        {
            ChangeState("Purple", "我要变Purple了");
        }
        else if (Input.IsActionJustPressed("next_color"))
        {
            ChangeState("Orange", "我要变Orange了");
        }
    }
    
    public void WhoWasI()
    {
        GD.Print("I was Green");
    }
}
