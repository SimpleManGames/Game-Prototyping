using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CombatManager : Singleton<CombatManager>
{
    GameManager _gameManager;
    Player _player;

    [SerializeField, ReadOnly]
    private List<Agent> _combatants = new List<Agent>();
    [SerializeField, ReadOnly]
    private int _currentTurnIndex = -1;
    private float _combatRange;
    private Vector3 _combatWorldPosition;
    private Vector3 _currentAgentCombatPosition;

    public Agent CurrentAgentTurn
    {
        get
        {
            if (_currentTurnIndex == -1)
                return new Agent();

            return _combatants?[_currentTurnIndex];
        }
    }

    public delegate void OnTurnChange();
    public OnTurnChange OnNextTurn;

    public bool inBattle
    {
        get { return _gameManager?.CurrentState == GameStateEnum.Combat; }
    }

    public override void Awake()
    {
        base.Awake();

        _gameManager = GameManager.Instance;
        _player = _gameManager.Player;
    }

    public void Init(List<Agent> enemies, float combatRange, Vector3 worldPosition)
    {
        _combatants = new List<Agent>();
        _combatants.Add(_player);
        _combatants.AddRange(enemies);

        _combatRange = combatRange;

        _combatWorldPosition = worldPosition;

        _currentTurnIndex = 0;

        _currentAgentCombatPosition = _combatants[0].transform.position;
    }

    public void NextTurn()
    {
        _currentTurnIndex++;
        if (_currentTurnIndex > _combatants.Count - 1)
            _currentTurnIndex = 0;

        _currentAgentCombatPosition = CurrentAgentTurn.transform.position;

        // Note: Might change this to only player turns
        OnNextTurn?.Invoke();
    }

    public void Update()
    {
        if (inBattle)
        {
            if (_currentTurnIndex == -1)
            {
                Debug.Log("Init Func failed to run or finish");
                return;
            }

            Vector3 agentPositionRef = CurrentAgentTurn.transform.position;

            if (CurrentAgentTurn.GetType() != typeof(Player))
            { }

            // Movement range
            //ClampPositionToCircle(_currentAgentCombatPosition, CurrentAgentTurn.stats.MovementRange, ref agentPositionRef);

            // Combat circle
            ClampPositionToCircle(_combatWorldPosition, _combatRange, ref agentPositionRef);

            CurrentAgentTurn.transform.position = agentPositionRef;


        }
    }

    public void ClampPositionToCircle(Vector3 center, float radius, ref Vector3 position)
    {
        float distance = Vector3.Distance(center, position);

        if (distance > radius)
        {
            Vector3 originToObject = position - center;
            originToObject *= radius / distance;
            position = center + originToObject;
        }
    }

    private void OnDrawGizmos()
    {
        if (inBattle)
        {
            Handles.color = Color.green;
            //Handles.DrawWireDisc(_currentAgentCombatPosition + (Vector3.up * .5f), Vector3.up, _player.stats.MovementRange);

            Handles.color = Color.red;
            Handles.DrawWireDisc(_combatWorldPosition + (Vector3.up * .2f), Vector3.up, _combatRange);
        }
    }
}