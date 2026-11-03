using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance = null;
    public static T Instance => _instance;

    protected virtual void Awake()
    {
        // 씬을 다시 불러오면 이전 인스턴스는 파괴되어 있으므로, Unity 오브젝트의 null 판정을 쓴다.
        if ((Object)_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
    }
}
