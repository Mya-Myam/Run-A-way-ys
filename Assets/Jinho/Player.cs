using Hojun;
using System;
using System.Collections.Generic;
using UnityEngine;
using Gayoung;

namespace Jinho
{
    #region Player_interface
    public interface IMoveStrategy
    {
        void Moving();
    }
    public interface IAttackStrategy
    {
        void Attack();

    }

    public interface IReLoadAble
    {
        void ReLoad();
    }

    public enum PlayerMoveState
    {
        idle,
        walk,
        run,
        dead,
        jump
    }
    public enum PlayerAttackState
    {
        Rifle,
        Shotgun,
        Handgun,
        melee,
        sub,
        heal,
        granade,
    // 총 타입 세분화 됨 (라이플, 샷건, 권총)
    }
    #endregion
    #region MoveStrategy_Class
    public class Idle : IMoveStrategy
    {
        Player player = null;
        public Idle(object owner)
        { 
            player = (Player)owner;

        }
        public void Moving()
        {
            player.animator.SetFloat("WalkType", 0f);
            if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
                player.moveState = PlayerMoveState.walk;

            if (Input.GetKeyDown(KeyCode.Space) && player.isGrounded)
            {
                player.animator.SetTrigger("Jump");
                player.animator.SetFloat("JumpType", 0f);   //그냥 여기서 짬푸
                player.moveState = PlayerMoveState.jump;
            }

        }
    }
    public class Walk : IMoveStrategy
    {
        Player player = null;
        public Walk(object owner) 
        {
            player = (Player)owner;
        }
        public void Moving()
        {
            Vector3 vec = Vector3.zero;
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
                player.moveState = PlayerMoveState.run;

            if (Input.GetKey(KeyCode.A))
            {
                vec += Vector3.left;
                player.animator.SetFloat("WalkType", 0.2f);

            }
            if (Input.GetKey(KeyCode.W))
            {
                vec += Vector3.forward;
                player.animator.SetFloat("WalkType", 0.8f);
            }
            player.animator.SetBool("Front", true);
            if (Input.GetKey(KeyCode.D))
            {
                vec += Vector3.right;
                player.animator.SetFloat("WalkType", 0.4f);
            }
            if (Input.GetKey(KeyCode.S))
            {
                vec += Vector3.back;
                player.animator.SetFloat("WalkType", 0.6f);
            }

            if (Input.GetKeyDown(KeyCode.Space) && player.isGrounded)
            {
                player.moveState = PlayerMoveState.jump;
                player.animator.SetTrigger("Jump");
                player.animator.SetFloat("JumpType", 1f);   
            }


            if (vec == Vector3.zero)
                player.moveState = PlayerMoveState.idle;
            player.transform.Translate(vec.normalized * player.state.MoveSpeed * Time.deltaTime);
        }
    }
    public class Run : IMoveStrategy
    {
        Player player = null;
        public Run(object owner)
        {
            player = (Player)owner;
        }
        public void Moving()
        {
            if(Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
            {
                player.transform.Translate(Vector3.forward * (player.state.MoveSpeed * 1.2f) * Time.deltaTime);
                player.animator.SetFloat("WalkType", 1f);
                if (Input.GetKey(KeyCode.Space) && player.isGrounded)
                {
                    player.moveState = PlayerMoveState.jump;
                    player.animator.SetTrigger("Jump");
                    player.animator.SetFloat("JumpType", 1f);
                }
            }
            if(Input.GetKeyUp(KeyCode.LeftShift))
                player.moveState= PlayerMoveState.walk;

        }
    }
    public class Jump : IMoveStrategy
    {
        Player player = null;
        
        public Jump(object owner)
        {
            player = (Player)owner;
        }

        public void Moving()
        {
            player.isGrounded = false;
        }


    }
    #endregion
    #region PlayerState_Class
    public class Job
    {
        public string name;
        public float maxHp;
        public float moveSpeed;
    }
    public class PlayerData
    {
        public Job job;
        public float DefaultMoveSpeed => 3;
        public float DefaultMaxHp => 200;
        float moveSpeed;
        public float MoveSpeed
        {
            get { return moveSpeed; } 
            set 
            { 
                moveSpeed = value;
            }
        }
        float maxHp;
        public float MaxHp 
        {
            get { return maxHp; }
            set { maxHp = value; }
        }
        float hp;
        public float Hp
        {
            get { return hp; }
            set 
            { 
                hp = value;
                if (hp <= 0)
                    hp = 0;
                if(hp > MaxHp)
                    hp = MaxHp;
            }
        }
        public PlayerData(Job job = null)
        {
            if (job == null)
            {
                this.job = null;
                moveSpeed = DefaultMoveSpeed;
                MaxHp = DefaultMaxHp;
            }
            else
            {
                this.job = job;
                moveSpeed = job.moveSpeed;
                MaxHp = job.maxHp;
            }
        }
    }
    #endregion

    public class Player : MonoBehaviour, IHitAble , IDieable, IAttackAble
    {
        public PlayerData state = null;                                   //player의 기본state
        public GameObject[] weaponObjSlot = new GameObject[4];        //현재 들고있는 weaponSlot
        
        public IUseable currentWeapon = null;                         //현재 들고있는 weapon

        public PlayerMoveState moveState;                           //현재 move전략
        public ItemType attackState;                                //현재 attack전력

        Dictionary<PlayerMoveState, IMoveStrategy> moveDic;         //move 전략 dictionary
        Dictionary<ItemType, AttackStrategy> attackDic;            //attack 전략 dictionary

        public Camera mainCamera;
        public AimComponent Aim;

        public Animator animator;
        public bool isGrounded = true;

        public Transform weaponHand;
        public GameObject weapon;
        public int weaponIndex;
        
        public event Action onWeaponChange;
        public event Action attackAction;
        
        public int WeaponIndex
        {
            get { return weaponIndex; }
            set { 
                
                weaponIndex = value;
                WeaponChange();
                //onWeaponChange();

            }
        }

        public CharacterData Data => throw new NotImplementedException();

        CharacterData IHitAble.Data => throw new NotImplementedException();

        void Start()
        {
            state = new PlayerData();

            moveDic = new Dictionary<PlayerMoveState, IMoveStrategy>();
            moveDic.Add(PlayerMoveState.idle, new Idle(this));
            moveDic.Add(PlayerMoveState.walk, new Walk(this));
            moveDic.Add(PlayerMoveState.run, new Run(this));
            moveDic.Add(PlayerMoveState.jump, new Jump(this));

            attackDic = new Dictionary<ItemType, AttackStrategy>();
            attackDic.Add(ItemType.rifle, new RifleAttackStrategy(this));
            attackDic.Add(ItemType.shotgun, new ShotGunStregy(this));
            attackDic.Add(ItemType.Handgun, new HandgunAttackStrategy(this));
            attackDic.Add(ItemType.Melee, new MeleeAttackStrategy(this));
            attackDic.Add(ItemType.Grenade, new GranadeAttackStrategy(this));

            moveState = PlayerMoveState.idle;

            //weaponSlot[0] = new Rifle(new WeaponData("", null, 1, 1, 1, 1, 1, null, PlayerAttackState.Rifle, null));
            //weapon = GameObject.Find("AssaultRilfe_Prototype 1");
            currentWeapon = weapon.GetComponent<IUseable>();
            attackState = currentWeapon.ItemType;
            Aim = mainCamera.GetComponent<AimComponent>();
            //WeaponChange(); // 아무것도 안들고 있는 것
        }
        void Update()
        {
            //weapon.SetActive(true);
            weapon.transform.position = weaponHand.position;
            weapon.transform.rotation = weaponHand.rotation;

            moveDic[moveState]?.Moving();
            if (Input.GetKey(KeyCode.Mouse0))
            {
                this.animator.SetBool("Shot", true);
                attackDic[attackState]?.Attack();
            }
            else
            {
                this.animator.SetBool("Shot", false);
            }

            if ( Input.GetKey(KeyCode.R) )
            {
                if (currentWeapon is IReLoadAble)
                {
                    ((IReLoadAble)attackDic[attackState]).ReLoad();
                }
                    
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                WeaponIndex = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                WeaponIndex = 1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                WeaponIndex = 2;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                WeaponIndex = 3;
            }

        }


        public void ItemUseEffect() //Animation Event 함수(아이템 사용)
        {
            currentWeapon.Use();
        }
        public void ItemUseReload()//Animation Event 함수(재장전)
        {
            currentWeapon.Reload();
        }
        public void WeaponChange() // 무기 교환 메서드!
        {
            if (weaponObjSlot[WeaponIndex] == null)
            { 
                Debug.Log("무기가 없다");
                return;
            }

            Debug.Log(weapon.name + "이게 꺼졌음");
       
            weapon.SetActive(false);
           
            weapon = weaponObjSlot[WeaponIndex];
            Debug.Log(weapon.name + "이게 켜졌음");
            weapon.SetActive(true);
            currentWeapon = weapon.GetComponent<IUseable>();
            attackState = currentWeapon.ItemType;
     
            Debug.Log("무기교체완");
           
        }

        public virtual void Hit(float damage, IAttackAble attacker)
        {
        }
        public void Attack()
        {
            currentWeapon.Use();
        }
        public GameObject GetAttacker()
        {
            return this.gameObject;
        }
        public void Die()
        {
            throw new NotImplementedException();
        }

    }   
}
