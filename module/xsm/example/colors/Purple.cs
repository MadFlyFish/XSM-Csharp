using Godot;
using Reversion.Module.Xsm;
using Reversion.Module.Xsm.Example;

namespace Reversion.module.xsm.example.colors;

public class Purple : State<Player>
{
    protected override void _OnEnter(dynamic args)
    {
        Target.Modulate = Colors.Purple;
    }
    
    protected override void _OnUpdate(float delta)
    {
        if (Input.IsActionJustPressed("prev_color"))
        {
            ChangeState("Orange", "我要变Orange了");
        }
        else if (Input.IsActionJustPressed("next_color"))
        {
            ChangeState("Green", "我要变Green了");
        }
    }
    
    public void WhoWasI()
    {
        GD.Print("I was Purple");
    }
}