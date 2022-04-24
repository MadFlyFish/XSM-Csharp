using Godot;
using Reversion.Module.Xsm;
using Reversion.Module.Xsm.Example;

namespace Reversion.module.xsm.example.colors;

public class RegionColor : State<Player>
{
    protected override void _OnUpdate(float delta)
    {
        var prevPressed = Input.IsActionJustPressed("prev_color");
        var nextPressed = Input.IsActionJustPressed("next_color");
        if (prevPressed || nextPressed)
        {
            GetActiveSubState().WhoWasI();
        }
    }

    protected override void _AfterUpdate(float delta)
    {
        var prevPressed = Input.IsActionJustPressed("prev_color");
        var nextPressed = Input.IsActionJustPressed("next_color");
        if (prevPressed || nextPressed)
        {
            GD.Print($"Now I am  {GetActiveSubState().Name}");
        }
    }
}