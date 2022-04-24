using Godot;
using Godot.Collections;

namespace Reversion.Module.Xsm;

/// This is the necessary manager node for any XSM. This is a special State that
/// can handle change requests outside of XSM's logic. See the example to get it!
/// This node will probably expand a bit in the next versions of XSM
public class StateRoot<TTarget> : State<TTarget>
    where TTarget: Node
{
    /// 有任意子状态发生变化时触发
    [Signal] public delegate void SomeStateChanged(State<TTarget> senderNode, State<TTarget> newStateNode);
    /// 待激活状态发生变化时触发
    [Signal] public delegate void PendingStateChanged(State<TTarget> addedStateNode);
    /// 新增了待激活状态时触发
    [Signal] public delegate void PendingStateAdded(string newStateName);
    /// 已激活状态列表发生变化时触发
    [Signal] public delegate void ActiveStateListChanged(Dictionary<string, State<TTarget>> activeStatesList);
    /// 同步方式
    [Export(PropertyHint.Enum, "Idle, Physics")] private int SyncMode{set; get;} 
    /// 历史已激活状态字典的列表保存的记录数
    [Export(PropertyHint.Range, "0, 1024")] public int HistorySize{set; get;} = 12;
    
    /// 状态机作用的主体的节点路径，每个State可单独指定FsmOwner
    [Export] public NodePath FsmOwner {set; get; }
    
    /// 包括状态根节点在内的全部状态
    public Dictionary<string, State<TTarget>> StateMap { set; get; } = new();
    ///  
    public Dictionary<string, int> DuplicateNames { set; get; } = new();
    /// 待激活的状态列表（包含进出的四个参数）
    private Array<Array> PendingStates { set; get; } = new();
    /// 全部已激活状态（不包括状态根节点）
    public Dictionary<string, State<TTarget>> ActiveStates { set; get; } = new();
    /// 历史已激活状态字典的列表
    private Array<Dictionary<string, State<TTarget>> >ActiveStatesHistory { set; get; } = new();
    
    /// 初始化状态根节点
    public override void _Ready()
    {
        base._Ready(); // 和gds不同，c#的override是完全复写父类的虚方法，所以要手动调用base的虚方法。
        StateRoot = this;
        
        if (FsmOwner != null)
        {
            Target = GetNode<TTarget>(FsmOwner);
        }
        else
        {
            if (FsmOwner == null && GetParent() != null)
            {
                Target = GetParent<TTarget>();
            }
        }
        
        SetSyncMode();
        InitStateMap();
        Status = StatusType.Active;
        _OnEnter(null);
        InitChildrenStates(this, true);
        _AfterEnter(null);
    }
    
    public override void _Process(float delta)
    {
        UpdateMain(delta);
    }
    
    public override void _PhysicsProcess(float delta)
    {
        UpdateMain(delta);
    }
    
    // 增加节点结构警告
    public override string _GetConfigurationWarning()
    {
        if (IsDisabled)
        {
            return "Warning : Your root State is disabled. It will not work";
        }

        if (FsmOwner == null)
        {
            return "Warning : Your root State has no target";
        }
        
        if (AnimationPlayer == null)
        {
            return "Warning : Your root State has no AnimationPlayer registered";
        }

        return base._GetConfigurationWarning();
    }

    /// 设置同步方式。
    private void SetSyncMode()
    {
        switch (SyncMode)
        {
            case 0:
                SetProcess(true);
                SetPhysicsProcess(false);
                break;
            case 1:
                SetProcess(false);
                SetPhysicsProcess(true);
                break;
        }
    }

    
    /// 初始化状态列表。
    private void InitStateMap()
    {
        StateMap[Name] = this;
        InitChildrenStateMap(StateMap, this);
    }
    
    /// 更新的主方法，根据同步方式在Process或PhysicsProcess中执行。
    private void UpdateMain(float delta)
    {
        if (Engine.EditorHint)
        {
            return;
        }

        if (!IsDisabled && Status == StatusType.Active)
        {
            ResetDoneThisFrame(false);
            AddToActiveStatesHistory(ActiveStates.Duplicate());
            while (PendingStates.Count > 0)
            {
                StateInUpdate = true;
                var newStateWithArgs = PendingStates[0];
                PendingStates.RemoveAt(0);
                var newState = newStateWithArgs![0] as string;
                var arg1 = newStateWithArgs[1];
                var arg2 = newStateWithArgs[2];
                var arg3 = newStateWithArgs[3];
                var arg4 = newStateWithArgs[4];
                var newStateNode = ChangeState(newState, arg1, arg2, arg3, arg4);
                EmitSignal(nameof(PendingStateChanged), newStateNode);
                StateInUpdate = false;
            }
            UpdateActiveStates(delta);
        }
    }
    
    
    
    /// 将待激活状态的名字与参数，加入到待激活清单中。
    public void NewPendingState(string newStateName, dynamic argsOnEnter = null, dynamic argsAfterEnter = null, dynamic argsBeforeExit = null, dynamic argsOnExit = null)
    {
        var newStateArray = new Array();
        newStateArray.Add(newStateName);
        newStateArray.Add(argsOnEnter);
        newStateArray.Add(argsAfterEnter);
        newStateArray.Add(argsBeforeExit);
        newStateArray.Add(argsOnExit);
        PendingStates.Add(newStateArray);
        EmitSignal(nameof(PendingStateAdded), newStateArray);
    }
    
    
    
    //
    // PUBLIC FUNCTIONS
    //
    
    
    /// 判断此状态是否在已激活状态列表中。
    public bool InActiveStates(string stateName)
    {
        return ActiveStates.ContainsKey(stateName);
    }
    
    /// 根据id取得历史激活状态清单，id为0则为最近的记录
    public Dictionary<string, State<TTarget>> GetPreviousActiveStates(int historyId)
    {
        if (ActiveStatesHistory.Count == 0)
        {
            return new Dictionary<string, State<TTarget>>();
        }
        if (ActiveStatesHistory.Count <= historyId)
        {
            return ActiveStatesHistory[0];
        }
        return ActiveStatesHistory[historyId];
    }
    
    /// CAREFUL IF YOU HAVE TWO STATES WITH THE SAME NAME, THE "state_name"
    /// SHOULD BE OF THE FORM "ParentName/ChildName"
    public bool WasStateActive(string stateName, int historyId = 0)
    {
        var prev = GetPreviousActiveStates(historyId);
        if (prev != null)
        {
            return false;
        }
        return GetPreviousActiveStates(historyId).ContainsKey(stateName);
    }
    
    /// 是否为状态根节点（复写了State的IsRoot方法）
    protected override bool IsRoot()
    {
        return true;
    }
    
    
    
    //
    // PRIVATE FUNCTIONS
    //
    
    
    private void AddToActiveStatesHistory(Dictionary<string, State<TTarget>> newActiveStates)
    {
        ActiveStatesHistory.Insert(0, newActiveStates);
        while (ActiveStatesHistory.Count > HistorySize)
        {
            ActiveStatesHistory.RemoveAt(ActiveStatesHistory.Count - 1);
        }
    }
    
    public void RemoveActiveState(State<TTarget> stateToErase)
    {
        var stateName = stateToErase.Name;
        var nameInStateMap = stateName;
        if (!StateMap.ContainsKey(stateName))
        {
            var parentName = stateToErase.GetParent().Name;
            nameInStateMap = $"{parentName}/{stateName}";
        }
        ActiveStates.Remove(nameInStateMap);
        EmitSignal(nameof(ActiveStateListChanged), ActiveStates);
    }
    
    public void AddActiveState(State<TTarget> stateToAdd)
    {
        
        var stateName = stateToAdd.Name;
        var nameInStateMap = stateName;
        if (!StateMap.ContainsKey(stateName))
        {
            var parentName = stateToAdd.GetParent().Name;
            nameInStateMap = $"{parentName}/{stateName}";
        }
        ActiveStates[nameInStateMap] = stateToAdd;
        EmitSignal(nameof(ActiveStateListChanged), ActiveStates);
    }
}