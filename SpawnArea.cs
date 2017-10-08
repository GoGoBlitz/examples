using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Project.Scripts.Weapon;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets._Project.Scripts
{
    class SpawnArea : MonoBehaviour
    {
        public bool IsSpawnedCreatures = false;

        public GameObject[] MonstersPrefabs { get; set; }
        public BoxCollider Collider { get; set; }

        public float AreasMargin { get; set; }

        public bool IsActive { get; set; }

        private System.Random random = new System.Random();

        private List<Transform> creatures = new List<Transform>();
        private List<BehaviorDesigner.Runtime.BehaviorTree> creaturesBehaviours = new List<BehaviorDesigner.Runtime.BehaviorTree>();
        private List<Animator> creaturesAnimators = new List<Animator>();
        private List<Enemy> creaturesEnemyComponents = new List<Enemy>();
        private int getPositionRecursiveCounter;
        private ShootSetting shootSettings;

        void Awake()
        {
            shootSettings = (ShootSetting)PlayerPrefs.GetInt(Constants.ShootSettings, 0);

            EventsChannel.General.Subscribe(Constants.FinalBattleStarted, (o, o1) =>
            {
                foreach (var creature in creatures)
                {
                    if(creature != null)
                        GameObject.Destroy(creature.gameObject);
                }
            });
        }


        void OnTriggerExit(Collider collider)
        {
            if (collider.tag == "Player")
            {
                IsActive = false;

                for (var index = 0; index < creatures.Count; index++)
                {
                    var creature = creatures[index];

                    if (creaturesEnemyComponents[index].IsAlive())
                    {
                        if (Vector3.Distance(creature.transform.position, collider.gameObject.transform.position) > AreasMargin)
                        {

                            var creaturesBehaviour = creaturesBehaviours[index];

                            if (creaturesBehaviour != null)
                                creaturesBehaviour.DisableBehavior();

                            var creaturesAnimator = creaturesAnimators[index];

                            if (creaturesAnimator != null)
                                creaturesAnimator.speed = 0;

                        }
                    }
                }
            }
        }

        void OnTriggerEnter(Collider collider)
        {
            if (collider.tag == "Player")
            {
                IsActive = true;

                if (!IsSpawnedCreatures)
                    StartCoroutine(SpawnCreaturesCoroutine(0.2f));
                else
                {
                    for (var index = 0; index < creatures.Count; index++)
                    {
                        if (creaturesEnemyComponents[index].IsAlive())
                        {
                            var behaviour = creaturesBehaviours[index];

                            if (behaviour != null)
                                behaviour.EnableBehavior();

                            var creaturesAnimator = creaturesAnimators[index];

                            if (creaturesAnimator != null)
                                creaturesAnimator.speed = 1;
                        }
                    }
                }
            }
        }

        public void SpawnCreatures()
        {
            if (IsSpawnedCreatures)
                return;

            StartCoroutine(SpawnCreaturesCoroutine(0.1f));
        }

        private IEnumerator SpawnCreaturesCoroutine(float delay)
        {
            var sizeX = (int)Collider.bounds.size.x;
            var sizeZ = (int)Collider.bounds.size.z;


            for (int i = 0; i < 22; i++)
            {
                var randomCreature = MonstersPrefabs.OrderBy(x => Guid.NewGuid()).First();

                var position = GetPosition(sizeX, sizeZ);

                getPositionRecursiveCounter = 0;

                var creature = GameObject.Instantiate(randomCreature, position, new Quaternion(0, 180, 0, 0));
                creature.name = creature.name + Guid.NewGuid();
                creatures.Add(creature.transform);
                creaturesBehaviours.Add(creature.GetComponent<BehaviorDesigner.Runtime.BehaviorTree>());
                creaturesAnimators.Add(creature.GetComponent<Animator>());
                creaturesEnemyComponents.Add(creature.GetComponent<Enemy>());

                creature.transform.SetParent(this.gameObject.transform);

                yield return new WaitForSeconds(delay);
            }

            IsSpawnedCreatures = true;
        }

        private Vector3 GetPosition(int sizeX, int sizeZ)
        {
            Vector3 position = new Vector3(
                random.Next((int)this.transform.position.x - sizeX / 2, (int)this.transform.position.x + sizeX / 2), 0,
                random.Next((int)this.transform.position.z - sizeZ / 2, (int)this.transform.position.z + sizeZ / 2));

            if (getPositionRecursiveCounter == 20)
            {
                Debug.LogWarning("Recursive positionin reached max value");
                return position;
            }

            Collider[] colliders = Physics.OverlapSphere(position, 3);

            //if we have any object near constant value - we need to random a new place to position
            if (colliders.Any(x => x.tag == "Wall"))
            {
                getPositionRecursiveCounter++;
                return GetPosition(sizeX, sizeZ);
            }

            colliders = Physics.OverlapSphere(position, 30);

            //if we have any object near constant value - we need to random a new place to position
            if (colliders.Any(x => x.tag == "Enemy"))
            {
                getPositionRecursiveCounter++;
                return GetPosition(sizeX, sizeZ);
            }

            colliders = Physics.OverlapSphere(position, 100);

            if (colliders.Any(x => x.tag == "Player"))
            {
                getPositionRecursiveCounter++;
                return GetPosition(sizeX, sizeZ);
            }

            return position;
        }
    }
}
