using UnityEngine;
using System.Collections;

public class Spider : Enemy
{

    public GameObject ShotPoint;

    private bool canShoot;

    public void FixedUpdate()
    {
        if (canShoot)
        {
            shoot();

            canShoot = false;
        }
    }

    public override void Initialize(EnemyPoint enemyPoint, Vector3 islePosition, Vector3 navMeshPosition, NavMeshTarget target)
    {
        base.Initialize(enemyPoint, islePosition, navMeshPosition, target);

        canShoot = false;

        StartCoroutine(ShotRythmHandler());
    }

    protected override GameObject getGhost()
    {
        return mr.getObject(ObjectPool.categorie.enemy, (int)ObjectPool.enemy.spiderGhost);
    }

    public override void die()
    {
        StopAllCoroutines();

        base.die();
    }

    private void shoot()
    {
        Stats stats = GetComponent<Stats>();

        Vector3 v = transform.forward;
        v = v.normalized;
        Vector3[] shotVecs = new Vector3[3];
        shotVecs[0] = transform.forward + ((transform.right / 3) * -1);
        shotVecs[1] = transform.forward;
        shotVecs[2] = transform.forward + (transform.right / 3);


        for (int i = 0; i < shotVecs.Length; i++)
        {
            Shot shot = mr.getObject(ObjectPool.categorie.shot, (int)ObjectPool.shot.roundEnemy).GetComponent<Shot>();
            shot.gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            Vector3 startPoint = ShotPoint.transform.position;
            Vector3 startDirection = shotVecs[i];
            startDirection = (startDirection.normalized + new Vector3(0, 0.2f, 0)) * stats.shotSpeed;

            shot.reset(gameObject, startPoint, startDirection, new Quaternion(), stats.possibleShotEffects);
        }
    }


    public IEnumerator ShotRythmHandler()
    {
        while (true)
        {
            // calculate difference-wait-time
            Stats stats = GetComponent<Stats>();

            float max = stats.fireRateDifference * 100;
            float offset = mr.random.Next(0, (int)max);
            offset = offset - (max / 2);
            offset = offset / 100;

            float actualWaitTimeSeconds = stats.fireRate + offset;

            yield return new WaitForSeconds(actualWaitTimeSeconds);

            canShoot = true;
        }
    }

}
