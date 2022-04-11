using Godot;
using System;

namespace Reversion.Module.Xsm;

public class Orange : State
{
    protected override void _OnEnter(dynamic args)
    {
        ((Sprite)Target).Modulate = Colors.Orange;
        GD.Print($"{Name}: _Enter");
    }
    
    protected override void _AfterEnter(dynamic args)
    {
        GD.Print($"{Name}: _AfterEnter");
    }


    protected override void _OnUpdate(dynamic args)
    {
        if (Input.IsActionJustPressed("prev_color"))
        {
            ChangeState("Green", "我要变绿了");
        }
        else if (Input.IsActionJustPressed("next_color"))
        {
            ChangeState("Purple","我要变紫了");
        }
    }
    
    protected override void _BeforeExit(dynamic args)
    {
        GD.Print($"{Name}: _BeforeExit");
    }
    
    protected override void _OnExit(dynamic args)
    {
        GD.Print($"{Name}: _OnExit");
    }
    
    
    public void WhoWasI()
    {
        GD.Print("I was Orange");
    }
}
