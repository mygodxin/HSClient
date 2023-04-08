using System.Collections.Generic;

/// <summary>
/// ����״̬��
/// </summary>
public class FSM
{
    Dictionary<EState, BaseState> _states;
    private BaseState _curState;    //��ǰ״̬
    private BaseState _preState;    //��һ��״̬
    public FSM(EState initial, Dictionary<EState, BaseState> states)
    {
        _states = states;
    }

    public void Update()
    {
        _curState.Run();
    }
    /// <summary>
    /// �л�״̬
    /// </summary>
    /// <param name="state"></param>
    public void SwitchState(BaseState state)
    {
        this._preState = _curState;
        _curState.Exit();
        _curState = state;
        _curState.Enter();
    }
    public void RevertToPreState()
    {
        this.SwitchState(this._preState);
    }
    /// <summary>
    /// ��ǰ״̬
    /// </summary>
    /// <returns></returns>
    public BaseState CurState()
    {
        return this._curState;
    }
    /// <summary>
    /// ��һ��״̬
    /// </summary>
    /// <returns></returns>
    public BaseState PreState()
    {
        return this._preState;
    }
}