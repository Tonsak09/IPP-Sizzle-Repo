using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkZone : MonoBehaviour
{
    [Header("Charging")]
    [SerializeField] float chargeRadius;
    [SerializeField] float lifeTime;
    [SerializeField] AnimationCurve chargeRadiusCurve;
    [SerializeField] GameObject chargeZoneObj;
    [SerializeField] LayerMask chargeable;

    [Header("Mushroom Spawning")]
    [SerializeField] GameObject mushroom;
    [SerializeField] float timeTillMushroomsSpawn;
    [SerializeField] float timeBetweenSpawnMin;
    [SerializeField] float timeBetweenSpawnMax;
    [SerializeField] float spawnRadius;
    [SerializeField] float spawnDistance;
    [SerializeField] int minSpawn;
    [SerializeField] int maxSpawn;
    [SerializeField] LayerMask spawnableSurface;
    private bool mushSpawned = false;

    private ParticleSystem sparksEmitter;
    private ParticleSystem.Particle[] particles;

    //private Vector3 chargeZone;
    private Transform chargeZone; 

    private float timer;

    private void Awake()
    {
        chargeZone = Instantiate(chargeZoneObj, this.transform.position, Quaternion.identity).transform;
        chargeZone.parent = this.transform;
    }

    private void Start()
    {
        sparksEmitter = this.GetComponent<ParticleSystem>();

        if (particles == null || particles.Length < sparksEmitter.main.maxParticles)
            particles = new ParticleSystem.Particle[sparksEmitter.main.maxParticles];

    }

    private void Update()
    {
        timer += Time.deltaTime;
        chargeRadius = Mathf.Lerp(chargeRadius, 0.0f, chargeRadiusCurve.Evaluate(timer / lifeTime));

        Collider[] chargeColliders = Physics.OverlapSphere(chargeZone.position, chargeRadius, chargeable);
        for (int i = 0; i < chargeColliders.Length; i++)
            chargeColliders[i].GetComponent<Chargeable>().Charge(chargeZone);

        MushroomSpawnLogic();
    }

    private void LateUpdate()
    {
        SparkZonePositioning();
    }

    private void MushroomSpawnLogic()
    {
        if(!mushSpawned && timer >= timeTillMushroomsSpawn)
        {
            StartCoroutine(SpawningOverTime());

            mushSpawned = true;
        }
    }

    private void SparkZonePositioning()
    {
        // Get the average position of all the sparks 

        // NOTE: This script should be attached to the emitter for sparks
        //       so that it will track the correct direction that it is 
        //       facing. It also means localizing the positions of the
        //       particles will be easier 
        //
        //       We do not want the charge zone to deviate away from the 
        //       primary direction that the sparks were shot from

        Vector3 avgPos = Vector3.zero;

        int pAlive = sparksEmitter.GetParticles(particles);
        for (int i = 0; i < pAlive; i++)
        {
            avgPos += particles[i].position;
        }
        avgPos /= pAlive;

        if(pAlive > 0)
            chargeZone.position = avgPos;
    }

    private IEnumerator SpawningOverTime()
    {
        int spawnCount = Random.Range(minSpawn, maxSpawn);
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 startPos = chargeZone.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                Random.Range(-spawnRadius, spawnRadius),
                Random.Range(-spawnRadius, spawnRadius));

            RaycastHit hit;
            // transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask
            if (Physics.Raycast(startPos, Vector3.down, out hit, spawnDistance, spawnableSurface))
            {
                Instantiate(mushroom, hit.point, Quaternion.identity);
            }

            yield return new WaitForSeconds(Random.Range(timeBetweenSpawnMin, timeBetweenSpawnMax));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(chargeZone.position, chargeRadius);
    }
}
