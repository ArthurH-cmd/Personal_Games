using UnityEngine;

public class PlayerAware : MonoBehaviour
{
    public bool AwareOfPlayer { get; private set; }
    public Vector2 DirectionToPayer { get; private set; }

    [SerializeField]
    private float playerAwarenessDistance;

    [SerializeField]
    private Transform player;


    private Transform _Player;

    private void Awake()
    {
        _Player = player.transform;
    }

    private void Update()
    {
        Vector2 enemyToPlayerVector = _Player.position - transform.position;
        DirectionToPayer = enemyToPlayerVector.normalized;

        if (enemyToPlayerVector.magnitude <= playerAwarenessDistance)
        {
            AwareOfPlayer = true;
        }
        else
        {
            AwareOfPlayer = false;
        }
    }
}
