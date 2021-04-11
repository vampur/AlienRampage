using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject goHorde;
    List<Vector3> lstSpawnVectors;

    List<GameObject> lstHordes;

    public int nEnemiesLeft;
    int nBaseEnemyCount;

    // Start is called before the first frame update
    void Start()
    {
        lstSpawnVectors = new List<Vector3>();
        lstHordes = new List<GameObject>();

        foreach(var gobj in GameObject.FindGameObjectsWithTag("Spawner"))
            lstSpawnVectors.Add(gobj.transform.position);
        
        foreach(var gobj in GameObject.FindGameObjectsWithTag("Horde"))
        {
            lstHordes.Add(gobj);
            nEnemiesLeft += gobj.GetComponentsInChildren<EnemyAi>().Length;
        }
        nBaseEnemyCount = nEnemiesLeft;
    }

    // Update is called once per frame
    void Update()
    {
        if(nEnemiesLeft <= 0)
            SpawnTheHorde();
    }

    void SpawnTheHorde()
    {
        foreach(var vecSpawnPos in lstSpawnVectors)
        {
            lstHordes.Add(Instantiate(goHorde, vecSpawnPos, Quaternion.identity));
        }
        nEnemiesLeft = nBaseEnemyCount;
    }
}
