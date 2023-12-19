using Gayoung;
using Jaeyoung;
using Jinho;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jinho
{
    public class ItemHandgun : WeaponMonoBehaviour, IAttackItemable, Yeseul.IInteractive , IReLoadAble
    {
        public WeaponData weaponData;
        public WeaponData WeaponData { get { return weaponData; } }
        public Player Player 
        { 
            get => player; 
            set 
            { 
                player = value; 
                if(player != null)
                {
                    strategy = new HandgunAttackStrategy(player);
                }
            } 
        }
        public Player player = null;

        public Transform firePos;   //�Ѿ� �߻� ��ġ
        public Collider weaponCol;
        Transform aimPos;           //�Ѿ��� ���ư� ��ġ
        public AudioClip gunFireSound;
        public AudioClip reloadSound;

        IAttackStrategy strategy;
        public ItemType ItemType => weaponData.itemType;
        public IAttackStrategy AttackStrategy
        {
            get => strategy;
            set
            {
                strategy = value;
            }
        }

        public SoundComponent sound;

        void OnEnable()
        {
            strategy = new HandgunAttackStrategy(player);
        }


        public void Use()
        {
            
            if (BulletCount == 0)
                return;

            strategy.Attack();
        }


        public void SetItem(Player player)
        {
            SetWeapon(player, gameObject, 1, this);
        }
        


        public void Interaction(GameObject interactivePlayer)
        {
            if (interactivePlayer.TryGetComponent(out Player player) && this.player == null)
            {
                SetItem(player);
            }
        }

        public void MakeBullet()
        {
            BulletCount--;
            // make bullet -> obj_pull

            aimPos = player.Aim.aimObjPos;
            GameObject bulletObj = PoolingManager.instance.PopObj(PoolingType.BULLET);
            Bullet_Component bulletScript = bulletObj.GetComponent<Bullet_Component>();

            bulletScript.SetBulletData(weaponData, Player);
            bulletScript.SetBulletVec(firePos, aimPos.position);

            this.sound.ActiveSound();
            SoundEffect(gunFireSound, firePos);
        }

        public void UseEffect()
        {
            MakeBullet();
        }

        public void Reloading()    //��������� ������ ����
        {
            if (strategy is IReLoadAble)
                ((IReLoadAble)strategy).ReLoad();
            SoundEffect(reloadSound, transform);
        }
        public void ReloadEffect()
        {
            ReLoad();
        }

        public void ReLoad()
        {
            int needBulletCount = maxBullet - BulletCount;

            if (TotalBullet >= needBulletCount)
                BulletCount = maxBullet;
            else
                BulletCount += TotalBullet;

            TotalBullet -= needBulletCount;
        }


    }
}