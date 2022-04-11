using Godot;
using Godot.Collections;

namespace Reversion.Module.Xsm;

/// 所有的状态节点的父类，包括状态根节点StateRoot
public class State : Node
{
    /// 进入状态时触发
    [Signal] public delegate void StateEntered(State sender);
    /// 退出状态时触发
    [Signal] public delegate void StateExited(State sender);
    /// 更新状态时触发
    [Signal] public delegate void StateUpdated(State sender);
    /// 改变状态时触发
    [Signal] public delegate void StateChanged(State sender, State newState);
    /// 进入子状态时触发
    [Signal] public delegate void SubStateEntered(State sender);
    /// 离开子状态时触发
    [Signal] public delegate void SubStateExited(State sender);
    /// 子状态改变时触发
    [Signal] public delegate void SubStateChanged(State sender, State newState);
    /// 禁用时触发
    [Signal] public delegate void Disabled();
    /// 启用时触发
    [Signal] public delegate void Enabled();

    /// 是否禁用
    [Export] public bool IsDisabled {set; get;} 
    // 是否可同时激活或者未激活子节点（类似行为树的平行节点）
    [Export] public bool HasRegions {set; get;}
    /// 调试打印模式
    [Export] public bool DebugMode {set; get;}
    // 状态机作用的主体的节点路径，每个State可单独指定FsmOwner
    [Export] public NodePath FsmOwner {set; get; }
    /// 动画器的节点路径
    [Export] public NodePath AnimationPlayer {set; get;}

    /// 节点状态类型枚举:未激活，正在进入，已激活，正在退出。
    protected enum StatusType 
    {
        Inactive, 
        Entering, 
        Active,
        Exiting
    }
    /// 初始状态为未激活
    protected StatusType Status{set; get;} = StatusType.Inactive;
    /// 状态根节点
    protected StateRoot StateRoot{set; get;}
    /// 作用主体（注意：可不指定，默认用状态根节点的Target）
    protected Node Target{set; get;}
    /// 动画器
    private AnimationPlayer AnimPlayer{set; get;}
    private State LastState{set; get;}
    private bool DoneForThisFrame { set; get; }
    protected bool StateInUpdate { set; get; }

    // 初始化状态
    public override void _Ready()
    {
        // 如是编辑器Tool模式，则不做任何处理。
        if (Engine.EditorHint)
        {
            return;
        }
        // 取得作用主体
        if (FsmOwner != null)
        {
            Target = GetNode<Node>(FsmOwner);
        }
        // 取得动画器
        if (AnimationPlayer != null)
        {
            AnimPlayer = GetNode<AnimationPlayer>(AnimationPlayer);
        }
    }

    // 提供节点结构检查并警告（但在c#下貌似不工作）
    public override string _GetConfigurationWarning()
    {
        foreach (dynamic c in GetChildren())
        {
            if (c.GetClass() != "State")
            {
                return $"错误：有一个非State类型的子节点：  {c.Name}";
            }
        }
        return "";
    }
    
    
    //
    // 可Override的几个虚方法
    //
    

    protected virtual void _OnEnter(dynamic args)
    {
        
    }
    
    protected virtual void _AfterEnter(dynamic args)
    {
        
    }
    
    protected virtual void _OnUpdate(dynamic args)
    {
        
    }
    
    protected virtual void _AfterUpdate(dynamic args)
    {
        
    }
    
    protected virtual void _BeforeExit(dynamic args)
    {
        
    }
    
    protected virtual void _OnExit(dynamic args)
    {
        
    }
    
    // 定时器时间到了触发
    protected virtual void _OnTimeout(string name)
    {
        
    }
    
    
    //
    // 可在状态中调用的几个公共方法
    //
    
    
    /// 最重要的公共方法：切换状态，并且携带四个进出相关的参数，传递给对应的可复写方法。
    public State ChangeState(string newState, dynamic argsOnEnter = null, dynamic argsAfterEnter = null, dynamic argsBeforeExit = null, dynamic argsOnExit = null)
    {
        if (!StateRoot.StateInUpdate)
        {
            if (DebugMode)
            {
                GD.Print($"{Target.Name} pending state : {Name} -> {newState}");
            }
            StateRoot.NewPendingState(newState, argsOnEnter, argsAfterEnter, argsBeforeExit, argsOnExit);
            return null;
        }
        
        if (DoneForThisFrame)
        {
            return null;
        }

        // 如果切换至空状态或者自身，则不做任何处理。
        if (newState == "" || newState == Name)
        {
            return null;
        }

        // finds the path to next state, return if null, disabled or active
        var newStateNode = FindStateNode(newState);
        if (newStateNode == null)
        {
            return null;
        }
        if (newStateNode.IsDisabled)
        {
            return null;
        }
        if (newStateNode.Status != StatusType.Inactive)
        {
            return null;
        }

        if (DebugMode)
        {
            GD.Print($"{Target.Name} changing state : {Name} -> {newState}");
        }

        // compare the current path and the new one -> get the common_root
        var commonRoot = GetCommonRoot(newStateNode);
        
        // change the children status to EXITING
        commonRoot.ChangeChildrenStatusToExiting();
        
        // exits all active children of the old branch,
        // from farthest to common_root (excluded)
        // If EXITED, change the status to INACTIVE
        commonRoot.ExitChildren(argsBeforeExit, argsOnExit);
        
        // change the children status to ENTERING
        commonRoot.ChangeChildrenStatusToEntering(newStateNode.GetPath());
        
        
        // enters the nodes of the new branch from the parent to the next_state
        // enters the first leaf of each following branch
        // If ENTERED, change the status to ACTIVE
        commonRoot.EnterChildren(argsOnEnter, argsAfterEnter);
        
        // sets this State as last_state for the new one
        newStateNode.LastState = this;
        
        // set "done this frame" to avoid another round of state change in this branch
        commonRoot.ResetDoneThisFrame(true);
        
        // 触发状态改变的信号，并携带旧/新两个状态作为参数
        EmitSignal(nameof(StateChanged), this, newState);

        // 如果不是根节点，则让自身的父节点发出“子状态发生变化”的信号，并携带旧/新两个状态作为参数
        if (!IsRoot())
        {
            newStateNode.GetParent().EmitSignal(nameof(SubStateChanged), this, newStateNode);
        }
        // 状态根节点发出“子状态发生变化”的信号，并携带旧/新两个状态作为参数
        StateRoot.EmitSignal(nameof(StateRoot.SomeStateChanged), this, newStateNode);

        if (DebugMode)
        {
            GD.Print($"{Target.Name} changed state : {Name} -> {newState}");
        }
        // 返回新状态的节点
        return newStateNode;
    }
    
    /// ChangeState方法的不带其他参数的精简版本
    public void GotoState(string newState)
    {
        ChangeState(newState);
    }

    
    public State ChangeStateIf(string newState, string ifState)
    {
        var s = FindStateNode(ifState);
        if (s == null || s.Status == StatusType.Active)
        {
            return ChangeState(newState);
        }
        return null;
    }

    
    public bool HasParent(State stateNode)
    {
        var parent = GetParent<State>();
        if (parent == stateNode)
        {
            return true;
        }

        if (parent.GetClass() != "State" || parent == StateRoot)
        {
            return false;
        }
        return parent.HasParent(stateNode);
    }

    /// 获取一个节点在当前帧的激活状态。
    public bool IsActive(string stateName)
    {
        var s = FindStateNode(stateName);
        if (s == null)
        {
            return false;
        }
        return s.Status == StatusType.Active;
    }
    
    // 获取一个节点在上一帧的激活状态。
    public bool WasActive(string stateName, int historyId = 0)
    {
        return StateRoot.WasStateActive(stateName, historyId);
    }

    /// 获取已激活的子状态（如果HasRegions为true，则返回全部的已激活的子状态。）
    public dynamic GetActiveSubState()
    {
        if (HasRegions && Status == StatusType.Active)
        {
            return GetChildren();
        }
        else
        {
            foreach (dynamic c in GetChildren())
            {
                if (c.Status == StatusType.Active)
                {
                    return c;
                }
            }
        }
        return null;
    }

    /// FindStateNode的别名：通过状态名获取状态节点
    public State GetState(string stateName)
    {
        return FindStateNode(stateName);
    }
    
    /// 获取整个XSM状态机的已激活状态
    public Dictionary GetActiveStates()
    {
        return StateRoot.ActiveStates;
    }

    /// 简单播放动画器里的某个动画。
    private void Play(string anim, float customSpeed = 1.0f, bool fromEnd = false)
    {
        if (Status == StatusType.Active && AnimPlayer != null && AnimPlayer.HasAnimation(anim))
        {
            if (AnimPlayer.CurrentAnimation != anim)
            {
                AnimPlayer.Stop();
                AnimPlayer.Play(anim);
            }
        }
    }

    /// 倒置播放某个动画
    public void PlayBackwards(string anim)
    {
        Play(anim, -1.0f, true);
    }

    /// 与当前动画一起混合播放某个动画。
    public void PlayBlend(string anim, float customBlend, float customSpeed = 1.0f, bool fromEnd = false)
    {
        if (Status == StatusType.Active && AnimPlayer != null && AnimPlayer.HasAnimation(anim))
        {
            if (AnimPlayer.CurrentAnimation != anim)
            {
                AnimPlayer.Play(anim, customBlend, customSpeed, fromEnd);
            }
        }
    }
    
    /// 与当前动画一起同步播放某个动画。
    public void PlaySync(string anim, float customBlend, float customSpeed = 1.0f, bool fromEnd = false)
    {
        if (Status == StatusType.Active && AnimPlayer != null && AnimPlayer.HasAnimation(anim))
        {
            var currAnim = AnimPlayer.CurrentAnimation;
            if (currAnim != anim && currAnim != "")
            {
                var currAnimPos = AnimPlayer.CurrentAnimationPosition;
                var currAnimLength = AnimPlayer.CurrentAnimationLength;
                var ratio = currAnimPos / currAnimLength;
                Play(anim, customSpeed, fromEnd);
                AnimPlayer.Seek(ratio * AnimPlayer.CurrentAnimationLength);
            }
            else
            {
                Play(anim, customSpeed, fromEnd);
            }
        }
    }

    /// 暂停动画（动画停留在当前状态）
    public void Pause()
    {
        Stop(false);
    }
    
    /// 将某个动画排队在播放序列的最后（如果当前动画为循环，则不会播放排队中的动画）
    public void Queue(string anim)
    {
        if (Status == StatusType.Active && AnimPlayer != null && AnimPlayer.HasAnimation(anim))
        {
            AnimPlayer.Queue(anim);
        }
    }

    /// 停止播放当前动画（可选是否重置动画播放参数）
    private void Stop(bool reset = true)
    {
        if (Status == StatusType.Active && AnimPlayer != null)
        {
            AnimPlayer.Stop(reset);
        }
    }
    
    /// 判断某个动画是否正在播放。
    public bool IsPlaying(string anim)
    {
        if (AnimPlayer != null)
        {
            return AnimPlayer.CurrentAnimation == anim;
        }
        return false;
    }
    
    /// 为此状态增加一个定时器，并且返回此定时器。
    public Timer AddTimer(string name, float time)
    {
        DelTimer(name);
        var timer = new Timer();
        AddChild(timer);
        timer.Name = name;
        timer.OneShot = true;
        timer.Start(time);
        timer.Connect("timeout", this, nameof(OnTimerTimeout));
        return timer;
    }

    /// 删除此状态下的某个定时器
    private void DelTimer(string name)
    {
        if (HasNode(name))
        {
            GetNode<Timer>(name).Stop();
            GetNode<Timer>(name).QueueFree();
            GetNode<Timer>(name).Name = "to_delete";
        }
    }

    /// 删除此状态下的全部定时器
    private void DelTimers()
    {
        foreach (var c in GetChildren())
        {
            if (c is Timer timer)
            {
                timer.Stop();
                timer.QueueFree();
                timer.Name = "to_delete";
            }
        }
    }
    
    /// 判断此状态是否有某个定时器正在运行。
    public bool HasTimer(string name)
    {
        return HasNode(name);
    }

    
    //
    // PROTECTED FUNCTIONS
    //
    
    
    protected void InitChildrenStateMap(Dictionary<string, State> dict, State newStateRoot)
    {
        StateRoot = newStateRoot as StateRoot;
        foreach (dynamic c in GetChildren())
        {
            if (dict.ContainsKey(c.Name))
            {
                var currState = dict[c.Name];
                var currParent = currState!.GetParent() as State;
                dict.Remove(c.Name);
                dict[$"{currParent!.Name}/{c.Name}"] = currState;
                dict[$"{Name}/{c.Name}"] = c;
                StateRoot.DuplicateNames[c.Name] = 1;
            }
            else if(StateRoot.DuplicateNames.ContainsKey(c.Name))
            {
                dict[$"{Name}/{c.Name}"] = c;
                StateRoot.DuplicateNames[c.Name] += 1;
            }
            else
            {
                dict[c.Name] = c;
            }
            c.InitChildrenStateMap(dict, StateRoot);
        }
    }

    protected void InitChildrenStates(State rootState, bool firstBranch)
    {
        foreach (dynamic c in GetChildren())
        {
            if (c.GetClass() == "State")
            {
                c.Status = StatusType.Inactive;
                c.StateRoot = rootState as StateRoot;
                c.Target ??= rootState.Target;
                c.AnimPlayer ??= rootState.AnimPlayer;
                if (firstBranch && (HasRegions || c == GetChild(0)))
                {
                    c.Enter();
                    c.LastState = rootState;
                    c.InitChildrenStates(rootState, true);
                    c._AfterEnter(null);
                }
                else
                {
                    c.InitChildrenStates(rootState, false);
                }
            }
        }
    }

    protected void UpdateActiveStates(float delta)
    {
        if (IsDisabled)
        {
            return;
        }

        StateInUpdate = true;
        Update(delta);
        foreach (dynamic c in GetChildren())
        {
            if (c.GetClass() == "State" && c.Status == StatusType.Active && !c.DoneForThisFrame)
            {
                c.UpdateActiveStates(delta);
            }
        }
        _AfterUpdate(delta);
        StateInUpdate = false;
    }
    
    protected void ResetDoneThisFrame(bool newDone)
    {
        DoneForThisFrame = newDone;
        if (!IsAtomic())
        {
            foreach (dynamic c in GetChildren())
            {
                if (c.GetClass() == "State")
                {
                    c.ResetDoneThisFrame(newDone);
                }
            }
        }
    }

    //
    // PRIVATE FUNCTIONS
    //
    
    private void SetDisabled(bool newDisabled)
    {
        IsDisabled = newDisabled;
        EmitSignal(IsDisabled ? nameof(Disabled) : nameof(Enabled));
        SetDisabledChildren(newDisabled);
    }

    private void SetDisabledChildren(bool newDisabled)
    {
        foreach (dynamic c in GetChildren())
        {
            c.SetDisabled(newDisabled);
        }
    }

    // 取得旧/新节点的的公共父节点
    private State GetCommonRoot(State newStateNode)
    {
        var result = newStateNode;
        while (result.Status != StatusType.Active && !result.IsRoot())
        {
            result = result.GetParent<State>();
        }

        return result;
    }

    private void Update(float delta)
    {
        if (Status == StatusType.Active)
        {
            _OnUpdate(delta);
            EmitSignal(nameof(StateUpdated), this);
        }
    }

    private void Enter(dynamic args = null)
    {
        if (IsDisabled)
        {
            return;
        }
        Status = StatusType.Active;
        StateRoot.AddActiveState(this);
        _OnEnter(args);
        EmitSignal(nameof(StateEntered), this);
        if (!IsRoot())
        {
            GetParent().EmitSignal(nameof(SubStateEntered), this);
        }
    }

    private void ChangeChildrenStatusToEntering(NodePath newStatePath)
    {
        if (HasRegions)
        {
            foreach (dynamic c in GetChildren())
            {
                c.Status = StatusType.Entering;
                c.ChangeChildrenStatusToEntering(newStatePath);
                return;
            }
        }

        var newStateLvl = newStatePath.GetNameCount();
        var currentLvl = GetPath().GetNameCount();
        if (newStateLvl > currentLvl)
        {
            foreach (dynamic c in GetChildren())
            {
                var currentName = newStatePath.GetName(currentLvl);
                if (c.GetClass() == "State" && c.Name == currentName)
                {
                    c.Status = StatusType.Entering;
                    c.ChangeChildrenStatusToEntering(newStatePath);
                }
            }
        }
        else
        {
            if (GetChildCount() > 0)
            {
                var c = GetChild<State>(0);
                if (GetChild<State>(0).GetClass() == "State")
                {
                    c.Status = StatusType.Entering;
                    c.ChangeChildrenStatusToEntering(newStatePath);
                }
            }
        }
    }

    private void EnterChildren(dynamic argsOnEnter = null, dynamic argsAfterEnter = null)
    {
        if (IsDisabled)
        {
            return;
        }

        // enter all ENTERING substates
        foreach (dynamic c in GetChildren())
        {
            if (c.GetClass() == "State" && c.Status == StatusType.Entering)
            {
                c.Enter(argsOnEnter);
                c.EnterChildren(argsOnEnter, argsAfterEnter);
                c._AfterEnter(argsAfterEnter);
            }
        }
    }

    private void Exit(dynamic args = null)
    {
        DelTimers();
        _OnExit(args);
        Status = StatusType.Inactive;
        StateRoot.RemoveActiveState(this);
        EmitSignal(nameof(StateExited), this);
        if (!IsRoot())
        {
            GetParent().EmitSignal(nameof(SubStateExited), this);
        }
    }

    private void ChangeChildrenStatusToExiting()
    {
        if (HasRegions)
        {
            foreach (dynamic c in GetChildren())
            {
                c.Status = StatusType.Exiting;
                c.ChangeChildrenStatusToExiting();
            }
        }
        else
        {
            foreach (dynamic c in GetChildren())
            {
                if (c.GetClass() == "State" && c.Status != StatusType.Inactive)
                {
                    c.Status = StatusType.Exiting;
                    c.ChangeChildrenStatusToExiting();
                }
            }
        }
    }


    private void ExitChildren(dynamic argsBeforeExit = null, dynamic argsOnExit = null)
    {
        foreach (dynamic c in GetChildren())
        {
            if (c.GetClass() == "State" && c.Status == StatusType.Exiting)
            {
                c._BeforeExit(argsBeforeExit);
                c.ExitChildren();
                c.Exit(argsOnExit);
            }
        }
    }

    /// 将所有子状态重置为未激活。
    public void ResetChildrenStatus()
    {
        foreach (dynamic c in GetChildren())
        {
            if (c.GetClass() == "State")
            {
                c.Status = StatusType.Inactive;
                c.ResetChildrenStatus();
            }
        }
    }

    // 根据节点名称获取状态节点（需要处理重名的情况）
    private State FindStateNode(string newState)
    {
        if (Name == newState)
        {
            return this;
        }

        var stateMap = StateRoot.StateMap;
        if (stateMap.ContainsKey(newState))
        {
            return stateMap[newState];
        }

        if (StateRoot.DuplicateNames.ContainsKey(newState))
        {
            if (stateMap.ContainsKey($"{Name}/{newState}"))
            {
                return stateMap[$"{Name}/{newState}"];
            }
            else if(stateMap.ContainsKey($"{GetParent().Name}/{newState}"))
            {
                return stateMap[$"{GetParent().Name}/{newState}"];
            }
        }

        return null;
    }

    /// 定时器时间到了触发，实际使用时复写_OnTimeout(name)
    private void OnTimerTimeout(string name)
    {
        DelTimer(name);
        _OnTimeout(name);
    }

    /// 返回类名(屏蔽了原GetClass方法)
    private new string GetClass()
    {
        return "State";
    }

    /// 是否没有子节点
    private bool IsAtomic()
    {
        return GetChildCount() == 0;
    }

    /// 是否为状态根节点
    protected virtual bool IsRoot()
    {
        return false;
    }
}