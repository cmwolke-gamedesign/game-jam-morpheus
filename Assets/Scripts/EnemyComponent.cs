using Assets.Scripts.entity;
using Assets.Scripts.entity.modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityStandardAssets.Utility;

namespace Assets.Scripts
{
    public class EnemyComponent : MonoBehaviour
    {
        public Animator SpriteAnimator;
        public Light Spotlight;
        public SpriteRenderer HeadSprite;
        public SphereCollider Trigger;
        public float move = 10;

        public GameObject _target;

        private GameEntity _gameEntity;
        public GameEntity GameEntity
        {
            get { return _gameEntity; }
            set { _gameEntity = value; Refresh(); }
        }

        private void Refresh()
        {
            HeadSprite.sprite = Resources.Load<Sprite>("Characters/enemies/enemy_" + _gameEntity.GetModule<EnemyModule>().BaseData.CurrentMusicType.Value);
            SpriteAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animation/EnemyController/"+ _gameEntity.GetModule<EnemyModule>().BaseData.CurrentMusicType.Value);
            SetEnemyType(_gameEntity.GetModule<EnemyModule>().BaseData.CurrentMusicType.Value);
        }

        public void SetEnemyType(String musikType)
        {
            if (musikType == MusicTypes.metal.ToString())
            {
                Spotlight.color = Color.blue;
                move =  7.5f;
            }
            if (musikType == MusicTypes.classic.ToString())
            {
                Spotlight.color = Color.red;
                move = 5;
            }
            if (musikType == MusicTypes.techno.ToString())
            {
                Spotlight.color = Color.green;
                move = 10;
            }
        }
        public void Update()
        {
            AttackPlayer();
            if (transform.position.y <= -20)
                Destroy(transform.parent.gameObject);
            transform.localEulerAngles = new Vector3(90.0f, transform.localEulerAngles.y, transform.localEulerAngles.z);

        }
        public void AttackPlayer()
        {
            if (_target != null)
            {
                transform.LookAt(_target.transform.position, Vector3.down);
                transform.localEulerAngles = new Vector3(90, transform.localEulerAngles.y, 0);
                transform.Translate(new Vector3(0, 1, 0) * move * Time.fixedDeltaTime);
                SpriteAnimator.SetBool("OnWalk",true);
                HeadSprite.gameObject.SetActive(false);
            }
            else
            {
                transform.position = transform.position;
            }
        }

        public void OnParticleCollision(GameObject other)
        {
            Debug.Log(CheckToKill(other.name, _gameEntity.GetModule<EnemyModule>().BaseData.CurrentMusicType.Value));
            if (CheckToKill(other.name, _gameEntity.GetModule<EnemyModule>().BaseData.CurrentMusicType.Value))
            {
                Destroy(transform.parent.gameObject);
            }
        }

        public bool CheckToKill(String AttckerMusicType , String VictimMusicType )
        {
            if (AttckerMusicType.Contains(MusicTypes.metal.ToString()) && VictimMusicType.Contains(MusicTypes.techno.ToString()))
            {
                return true;
            }
            else if (AttckerMusicType.Contains(MusicTypes.classic.ToString()) && VictimMusicType.Contains(MusicTypes.metal.ToString()))
            {
                return true;
            }
            else if (AttckerMusicType.Contains(MusicTypes.techno.ToString()) && VictimMusicType.Contains(MusicTypes.classic.ToString()))
            {
                return true;
            }
            else
                return false;

        }

        public void OnTriggerStay(Collider other)
        {
            PlayerComponent comp = other.gameObject.GetComponentInChildren<PlayerComponent>();
            if (comp != null && CheckToKill(_gameEntity.GetModule<EnemyModule>().BaseData.CurrentMusicType.Value, comp.GameEntity.GetModule<PlayerModule>().BaseData.CurrentMusicType.Value))
            {
                _target = comp.Spotlight.gameObject;
            }
            else if (comp != null)
            {
                _target = null;
                if (_gameEntity.GetModule<EnemyModule>().BaseData.CurrentMusicType.Value != comp.GameEntity.GetModule<PlayerModule>().BaseData.CurrentMusicType.Value)
                {
                    SpriteAnimator.SetBool("OnWalk", true);
                    HeadSprite.gameObject.SetActive(false);
                }
                else
                {
                    SpriteAnimator.SetBool("OnWalk", false);
                    HeadSprite.gameObject.SetActive(true);
                }
            }
        }
    }
}
