using System.Collections.Generic;
using Godot;

namespace Reversion.Module.Xsm;

public class XSM : Sprite
{
    private Button _button;
    private Button _button2;
    private Button _button3;
    private StateRoot _stateRoot;
    private State _state2;
    private State _state3;
        
    public override void _Ready()
    {
        _stateRoot = GetNode<StateRoot>("StateRoot");
        _state2 = GetNode<State>("StateRoot/MyState1/MyState2");
        _state3 = GetNode<State>("StateRoot/MyState1/MyState3");
        
        _button = GetNode<Button>("Button");
        _button2 = GetNode<Button>("Button2");
        _button3 = GetNode<Button>("Button3");
        
        _button.Connect("pressed", this, nameof(OnButtonPressed));
        _button2.Connect("pressed", this, nameof(OnButton2Pressed));
        _button3.Connect("pressed", this, nameof(OnButton3Pressed));
        
    }

    private void OnButtonPressed()
    {
        GD.Print(GetChildren());
        foreach (var state in _stateRoot.GetActiveSubState())
        {
            GD.Print(state.Name);
        }
    }
    
    private void OnButton2Pressed()
    {
        _state2.ChangeState("MyState2");
    }
    
    private void OnButton3Pressed()
    {
        _state3.ChangeState("MyState3");
    }
}