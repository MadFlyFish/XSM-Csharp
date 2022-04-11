using Godot;

namespace Reversion.Module.Xsm;

public class RegionColor : State
{
    protected override void _OnEnter(dynamic args)
    {
        GD.Print($"{Name}: _Enter");
    }
    
    protected override void _AfterEnter(dynamic args)
    {
        GD.Print($"{Name}: _AfterEnter");
    }

    protected override void _OnUpdate(dynamic args)
    {
        var prevPressed = Input.IsActionJustPressed("prev_color");
        var nextPressed = Input.IsActionJustPressed("next_color");
        if (prevPressed || nextPressed)
        {
            GetActiveSubState().WhoWasI();
        }
    }

    protected override void _AfterUpdate(dynamic args)
    {
        var prevPressed = Input.IsActionJustPressed("prev_color");
        var nextPressed = Input.IsActionJustPressed("next_color");
        if (prevPressed || nextPressed)
        {
            GD.Print($"Now I am  {GetActiveSubState().Name}");
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
}