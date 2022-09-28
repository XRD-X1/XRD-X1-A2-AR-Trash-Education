using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bin : MonoBehaviour
{
    public Spawner.TrashType BinTrashType;
    
    private ARTapToPlaceObject _arTapToPlaceObject;
    void Start()
    {
        _arTapToPlaceObject = FindObjectOfType<ARTapToPlaceObject>();
    }
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger triggered!");
        Trash trash = other.GetComponent<Trash>();
        Spawner.TrashType trashType = trash.TrashType;
        trash.gameObject.SetActive(false);
        Debug.Log(trash.TrashType);
        
        if(trashType == BinTrashType)
            _arTapToPlaceObject.correctBin();
        else
            _arTapToPlaceObject.wrongBin(trash.TrashType);
    }
}
