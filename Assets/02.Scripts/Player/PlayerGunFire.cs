using System;
using System.Collections;
using Lean.Pool;
using UnityEngine;

// 마우스 왼쪽 버튼을 누르면 바라보는 방향으로 총알을 발사하고 싶다.
public class PlayerGunFire : MonoBehaviour
{
    [SerializeField] private ParticleSystem _hitEffectPrefab;
    [SerializeField] private float _hitEffectReturnDelay = 1f;

    [Header("스텟")]
    [SerializeField] private int _maxBulletCount = 30;
    [SerializeField] private float _reloadTime = 3f;
    [SerializeField] private float _range = 100f; // 사거리
    [SerializeField] private float _damage = 10f; // 공격력
    [SerializeField] private float _fireInterval = 0.1f; // 발사 속도

    private float _fireTimer = 0;
    private int _bulletCount = 0;
    private Awaitable _reloadAwaitable = null;

    private void Start()
    {
        _bulletCount = _maxBulletCount;
    }

    private void Update()
    {
        // 0. 발사 타이머를 지난 시간만큼 줄인다.
        _fireTimer -= Time.deltaTime;
        // 1. 마우스 왼쪽 버튼 입력 받기
        if (Input.GetButton("Fire1") && _fireTimer <= 0 && _bulletCount > 0)
        {
            _bulletCount--;
            _fireTimer = _fireInterval;
            Shoot();
            if (_reloadAwaitable != null)
            {
                _reloadAwaitable.Cancel();
                // GameLogger.Log("재장전 취소");
            }
            // 이 방식은 이 MonoBehaviour가 등록한 모든 Coroutine을 등록 취소한다
            // StopAllCoroutines();
        }

        // 2. R키 누르면 총알 재장전
        if (Input.GetButtonDown("Reload") && _reloadAwaitable == null && _bulletCount < _maxBulletCount)
        {
            ReloadingAsync();
            GameLogger.Log("재장전");
        }
    }

    private void Shoot()
    {
        // 2. 레이(광선)을 생상하고 발사될 위치와 진행 방향 설정하기
        Transform cameraTransform = Camera.main.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        // 3. 레이가 부딛힌 대상의 정보를 저장할 변수 생성하기
        RaycastHit hitInfo = new RaycastHit();

        // 4. 레이를 발사하고, 만일 부딛힌 물체가 있다면 이펙트 표시하기
        bool isHit = Physics.Raycast(ray, out hitInfo, _range);
        if (isHit)
        {
            // GameLogger.Log($"{hitInfo.collider.name}이 총알을 맞았다. 데미지: {_damage}");
            ParticleSystem effect =
                LeanPool.Spawn(_hitEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));

            if (effect != null)
            {
                effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                effect.Play();

                LeanPool.Despawn(effect.gameObject, _hitEffectReturnDelay);
            }
        }
    }

    private async Awaitable ReloadingAsync()
    {
        try
        {
            _reloadAwaitable = Awaitable.WaitForSecondsAsync(_reloadTime);
            await _reloadAwaitable;

            _bulletCount = _maxBulletCount;
        }
        catch (OperationCanceledException)
        {
            GameLogger.Log("재장전 취소");
        }

        finally
        {
            _reloadAwaitable = null;
        }
    }
}
