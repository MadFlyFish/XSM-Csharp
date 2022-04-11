using Godot;

namespace Reversion.Module.Xsm;

public class MyState : State
{
    protected override void _OnEnter(dynamic args)
    {
        ((Sprite)Target).Modulate = Colors.Green;
        GD.Print($"{Name}: _Enter");
    }

    protected override void _AfterEnter(dynamic args)
    {
        GD.Print($"{Name}: _AfterEnter");
    }
    
    protected override void _BeforeExit(dynamic args)
    {
        GD.Print($"{Name}: _BeforeExit");
    }
    
    protected override void _OnExit(dynamic args)
    {
        GD.Print($"{Name}: _OnExit");
    }
}