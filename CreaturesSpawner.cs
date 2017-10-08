using UnityEngine;

namespace Assets._Project.Scripts
{
    class CreaturesSpawner : MonoBehaviour
    {
        [SerializeField]
        private int terrainWidth = 1600;

        [SerializeField]
        private Vector3 bottomLeftPoint;

        [SerializeField]
        private GameObject[] monstersPrefabs;


        public void Awake()
        {
            EventsChannel.General.Subscribe(EventConstants.PlayerModelInitialized, (o, o1) =>
            {
                var player = GameObject.Find("Player");
                playerShootController = player.GetComponentInChildren<ShootController>();
                CreateSpawnAreas();
            });
        }

        private void CreateSpawnAreas()
        {
            int areaPerRow = 5;
            float areaWidth = 1600 * 1.157f / areaPerRow;
            float offset = areaWidth / 1.25f;

            float basexOffset = bottomLeftPoint.x;
            float basezOffset = bottomLeftPoint.z;


            for (int i = 0; i < areaPerRow; i++)
            {
                basexOffset = bottomLeftPoint.x;

                for (int j = 0; j < areaPerRow; j++)
                {
                    GameObject area = new GameObject { name = string.Format("SpawnArea_{0}_{1}", i, j) };
                    area.transform.position = new Vector3(basexOffset, 0, basezOffset);
                    area.transform.parent = this.gameObject.transform;
                    area.tag = "SpawnArea";


                    var colliderComponent = area.AddComponent<BoxCollider>();
                    colliderComponent.size = new Vector3(areaWidth, 16, areaWidth);
                    colliderComponent.isTrigger = true;

                    var spawnAreaBehaviour = area.AddComponent<SpawnArea>();
                    spawnAreaBehaviour.Collider = colliderComponent;
                    spawnAreaBehaviour.MonstersPrefabs = monstersPrefabs;
                    spawnAreaBehaviour.AreasMargin = areaWidth * 0.125f;

                    //preload near to player area
                    if (i == 0 && j == 2)
                    {
                        spawnAreaBehaviour.SpawnCreatures();
                    }

                    basexOffset += offset;
                }

                basezOffset += offset;
            }
        }

    }
}
