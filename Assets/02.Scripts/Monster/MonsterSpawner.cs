using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    // [SerializeField] private GameObject[] _monsterPrefabs;
    [SerializeField] private Zombie _zombiePrefabs;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _maxAliveCount = 10;
    public float MaxAliveCount => _maxAliveCount;
    [SerializeField] private float _spawnInterval = 3f;
    [SerializeField] private float _spawnRadius = 2f;

    private int _aliveCount;
    public int AliveCount => _aliveCount;
    private float _spawnTimer;

    public event Action OnAliveCountChanged;

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;
        if (_aliveCount >= _maxAliveCount) return;

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer < _spawnInterval) return;

        _spawnTimer = 0f;
        SpawnMonster();
    }

    private void SpawnMonster()
    {
        Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

        Vector2 offset = Random.insideUnitCircle * _spawnRadius;
        Vector3 position = spawnPoint.position + new Vector3(offset.x, 0f, offset.y);

        Zombie newMonster = Instantiate(_zombiePrefabs, position, Quaternion.identity);
        newMonster.OnDeath += OnMonsterDeath;
        _aliveCount++;
        OnAliveCountChanged?.Invoke();
    }

    private void OnMonsterDeath()
    {
        _aliveCount--;
        OnAliveCountChanged?.Invoke();
    }
}
