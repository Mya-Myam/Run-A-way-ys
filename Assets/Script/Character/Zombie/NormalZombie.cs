using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Hojun;
using UnityEditor;
using UnityEngine.AI;
using Jaeyoung;
using System;
using Unity.VisualScripting;
using JetBrains.Annotations;

namespace Hojun 
{


    public class NormalZombie : Zombie , IAttackAble , IHitAble
    {

        public event Action dieAction;
        public event Action attackAction;

        public IAttackStrategy attackStrategy;
        public IHitStrategy hitStrategy;



        public override float Hp 
        {
            get => zombieData.hp;
            set
            {
                if (value <= 0)
                    stateMachine.SetState( (int)Zombie.ZombieState.DEAD );
                
                zombieData.hp = value;
            }
        }



        public new void Awake()
        {
        
            base.Awake();

            moveDict.Add(ZombieMove.SEARCH, new SearchStrategy(this));
            moveDict.Add(ZombieMove.IDLE, new IdleStrategy(this));
            moveDict.Add(ZombieMove.FIND, new FindStrategy(this));

            stateMachine.AddState((int)Zombie.ZombieState.IDLE, new IdleState(stateMachine));
            stateMachine.AddState((int)Zombie.ZombieState.SEARCH, new SearchState(stateMachine));
            stateMachine.AddState((int)Zombie.ZombieState.FIND , new FindState(stateMachine));
            stateMachine.AddState((int)Zombie.ZombieState.DEAD, new DeadState(stateMachine));
            stateMachine.AddState((int)Zombie.ZombieState.ATTACK, new AttackState(stateMachine));

            stateMachine.SetState((int)Zombie.ZombieState.IDLE);


            //MoveStrategy = moveDict[ZombieMove.IDLE];
        }


        public void Start()
        {
            hearComponent = gameObject.GetComponent<HearComponent>();
            dieAction = Die;

            attackStrategy = new ZombieAttack();
            attackAction += Attack;

        }

        // Update is called once per frame
        void Update()
        {
            stateMachine.Update();
        }



        IEnumerator DieCo()
        {
            float deathTime = 3.0f;

            yield return new WaitForSeconds(deathTime);

            Debug.Log("좀비 꿱");
            // objejct pool 쓰는지 destroy 쓰는지 팀원과 상의 할 것

        }

        public override void Die()
        {
            dieAction();
        }

        public void Hit(float damage, IAttackAble attacker)
        {
            if (Target != null)
                Target = attacker.GetAttacker();

            Debug.Log("hit");
        }


        public void Attack()
        {

            if(Target.TryGetComponent<IHitAble>(out IHitAble hitObj))
            {
                float damage = attackStrategy.GetDamage();

                hitObj.Hit( damage ,this);
            
            }

        }


        public GameObject GetAttacker()
        {
            return this.gameObject;
        }

    }



}
