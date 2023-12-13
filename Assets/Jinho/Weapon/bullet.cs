using Jinho;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Pool;
using Jaeyoung;

public class Bullet : MonoBehaviour, Hojun.IAttackAble
{
    [SerializeField] float moveSpeed;
    public float damage;
    public WeaponData parentWeaponData = null;
    public Jinho.Player player = null;
    Action attackAction;

    Hojun.IHitAble target;

    void OnEnable()
    {
        Invoke("BulletDestroy", 1.2f);  //총알이 불러와지면 1.2초 뒤 스스로 파괴됨
    }
    void Start()
    {
        attackAction += BulletAttack;
    }
    void Update()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }
    void BulletDestroy()    //총알이 ObjectPool로 돌아감
    {
        PoolingManager.instance.ReturnPool(gameObject);
    }
    public void SetBulletData(WeaponData weaponData, Jinho.Player player)    //무기 damage 입력 함수
    {
        this.player = player;
        parentWeaponData = weaponData;
        damage = parentWeaponData.damage;
    }
    public void SetBulletVec(Transform firePos, Vector3 targetPos)  //Bullet의 위치, 회전, 방향값 조정
    {
        transform.position = firePos.position;
        transform.rotation = firePos.rotation;
        transform.forward = (targetPos - transform.position).normalized;
    }
    void BulletAttack()
    {
        target.Hit(damage, this);
    }
    public void Attack()
    {
        //총알이 대미지를 줄 때?
        attackAction();
    }

    public GameObject GetAttacker()
    {
        return player.gameObject;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Hojun.IHitAble hit))
        {
            target = hit;
            Attack();
            BulletDestroy();
        }
    }
}
