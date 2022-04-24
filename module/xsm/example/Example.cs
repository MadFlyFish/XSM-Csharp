using Godot;
using System;
using Reversion.Module.Xsm;
using Reversion.Module.Xsm.Example;

public class Example : Node2D
{
    private StateRoot<Player> _stateRoot;
    private Button _showBtn;
    
    public override void _Ready()
    {
        _stateRoot = GetNode<StateRoot<Player>>("Player/StateRoot");
        _showBtn = GetNode<Button>("Show");

        _showBtn.Connect("pressed", this, nameof(OnShowBtnPressed));
    }

    private void OnShowBtnPressed()
    {
        GD.Print(_stateRoot.ActiveStates);
    }
}
