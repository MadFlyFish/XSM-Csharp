using Godot;

namespace Reversion.Module.Xsm.Example;

public class Player : KinematicBody2D
{
    public Vector2 Velocity { set; get;}
    public int Dir { set; get;} = 0;

    [Export] public int Gravity { set; get;} = 2500;
    [Export] public int Acceleration { set; get;} = 15;
    
    [Export] public int GroundSpeed { set; get;} = 680;
    [Export] public int GroundFriction { set; get;} = 6;
    [Export] public int WalkMargin { set; get;} = 80;
    [Export] public int RunMargin { set; get;} = 150;
    
    [Export] public int JumpSpeed { set; get;} = 450;
    [Export] public int AirSpeed { set; get;} = 610;
    [Export] public int AirFriction { set; get;} = 8;
    
    [Export] public float CoyoteTime { set; get;} = 0.1f;
    [Export] public float PreJumpTime { set; get;} = 0.3f;
    [Export] public float JumpTime { set; get;} = 0.3f;
}