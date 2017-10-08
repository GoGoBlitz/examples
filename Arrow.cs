using System.Collections;
using Assets._Project.Scripts.Elements;
using UnityEngine;

namespace Assets._Project.Scripts.Weapon
{
    class Arrow : Projectile
    {
        public ElementType? ElementModificator { get; set; }

        private bool isFallen;

        protected override void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.tag == "Player")
                return;

            if (other.collider.tag == "Enemy")
            {
                DamageEnemy(other.gameObject, other.contacts[0].point);
            }
            else if (other.gameObject.tag == "Terrain")
            {
                isFallen = true;
                StartCoroutine(WaitAndDestroy());
            }
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
                return;

            if (other.tag == "Enemy")
            {
                DamageEnemy(other.gameObject, other.transform.position);
            }
            else if(other.tag == "Terrain")
            {
                isFallen = true;
                StartCoroutine(WaitAndDestroy());
            }
        }

        protected override void DamageEnemy(GameObject enemy, Vector3 collide)
        {
            if (isFallen)
                return;

            isFallen = true;

            var enemyComponent = enemy.GetComponent<Enemy>();

            if (enemyComponent == null)
                enemyComponent = enemy.GetComponentInParent<Enemy>();

            if (enemyComponent != null)
            {
                enemyComponent.ApplyImpactEffect(collide);
                enemyComponent.TakeDamage(damage);

                if (ElementModificator.HasValue)
                {
                    ApplyModificatorAdditionalDamage(enemyComponent, ElementModificator.Value);

                    enemyComponent.ApplyVisualModificator(ElementModificator.Value, collide);
                }

                GameObject.Destroy(this.gameObject);
            }
        }


        private IEnumerator WaitAndDestroy()
        {
            yield return new WaitForSeconds(25);

            GameObject.Destroy(this.gameObject);
        }
    }
}
