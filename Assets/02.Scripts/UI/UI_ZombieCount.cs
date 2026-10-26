using TMPro;
using UnityEngine;

public class UI_ZombieCount : MonoBehaviour
{
    private TextMeshProUGUI _zombieCountText;
    [SerializeField] private MonsterSpawner _monsterSpawner;

    private void Awake()
    {
        _zombieCountText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        _monsterSpawner.OnAliveCountChanged += RefreshZombieCountText;
        RefreshZombieCountText();
    }

    private void RefreshZombieCountText()
    {
        _zombieCountText.text = $"남은 좀비:{_monsterSpawner.AliveCount}/{_monsterSpawner.MaxAliveCount}";
    }
}
