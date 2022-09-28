using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    // spawner
    public GameObject prefab;
    public List<GameObject> prefabs;
    
    public float period = 0.1f;
    public float radius = 0.5f;
    
    private float nextActionTime = 0.0f;
    private Vector3 center;
    
    // spawn types
    public List<GameObject> foodModels;
    public List<GameObject> paperModels;
    public List<GameObject> plasticModels;
    public List<GameObject> otherModels;
    

    void Start(){
        center = transform.position; 
    }
    
    void Update()
    {
        if (Time.time > nextActionTime ) {
            nextActionTime += period;
            
            // Pick random trash type
            GameObject trashModel;
            TrashType trashType = (TrashType)Random.Range(0, 4);
            // Debug.Log(trashType);

            if (trashType == TrashType.FOOD)
                trashModel = foodModels[Random.Range(0, foodModels.Count)];
            else if (trashType == TrashType.PAPER)
                trashModel = paperModels[Random.Range(0, paperModels.Count)];
            else if (trashType == TrashType.PLASTIC)
                trashModel = plasticModels[Random.Range(0, plasticModels.Count)];
            else if (trashType == TrashType.OTHER)
                trashModel = otherModels[Random.Range(0, otherModels.Count)];
            else
                trashModel = prefab;
            
            // Instantiate
            Vector3 pos = RandomCircle();
            // Quaternion rot = Quaternion.FromToRotation(Vector3.forward, center-pos);
            Quaternion rot = Quaternion.Euler(Random.Range(0, 361), 0, 0);
            
            Instantiate(trashModel, pos, rot);
        }
    }
    
    Vector3 RandomCircle (){
        float ang = Random.value * 360;
        float centerDistance = Random.value * radius;
        Vector3 pos;
        pos.x = center.x + centerDistance * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.z = center.z + centerDistance * Mathf.Cos(ang * Mathf.Deg2Rad);
        pos.y = center.y;
        return pos;
    }
    
    public enum TrashType
    {
        FOOD, PAPER, PLASTIC, OTHER
    }
}