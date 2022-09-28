using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTapToPlaceObject : MonoBehaviour
{   // global
    private ARRaycastManager _arRaycastManager;
    private Camera _camera;
    
    // placing
    public int placemntIndex = 0;
    public List<GameObject> objectToPlace;
    public List<GameObject> placementIndicator;

    private Pose placementPose;
    private bool placementPoseIsValid = false;
    
    // carrying
    bool carrying;
    public GameObject carriedObject;
    public float distance;
    public float smooth;
    
    // text
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI scoreText;
    
    // score
    public int score = 0;
    
    // audio
    public AudioClip throwSound;
    public AudioClip goodJobSound;
    
    public AudioClip wrongBlue;
    public AudioClip wrongYellow;
    public AudioClip wrongGreen;
    public AudioClip wrongBlack;

    void Start()
    {
        // init
        _arRaycastManager = FindObjectOfType<ARRaycastManager>();
        _camera = GameObject.Find("AR Camera").GetComponent<Camera>();
        
        // text
        messageText.text = "";
        scoreText.text = "";
    }

    public void correctBin()
    {
        score += 1;
        scoreText.text = "Score: " + score;
        messageText.text = "Good Job!";
        
        // audio
        _camera.GetComponent<AudioSource>().clip = goodJobSound;
        _camera.GetComponent<AudioSource>().Play();
    }

    public void wrongBin(Spawner.TrashType trashType)
    {
        switch (trashType)
        {
            case(Spawner.TrashType.FOOD):
                _camera.GetComponent<AudioSource>().clip = wrongGreen;
                break; 
            case(Spawner.TrashType.PAPER):
                _camera.GetComponent<AudioSource>().clip = wrongBlue;
                break;
            case(Spawner.TrashType.PLASTIC):
                _camera.GetComponent<AudioSource>().clip = wrongYellow;
                break;
            case(Spawner.TrashType.OTHER):
                _camera.GetComponent<AudioSource>().clip = wrongBlack;
                break;
            
        }

        messageText.text = "That's not right!";
        _camera.GetComponent<AudioSource>().Play();
    }

    void Update()
    {
        if (placemntIndex < placementIndicator.Count)
        {
            UpdatePlacementPose();
            UpdatePlacementIndicator();

            if (placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                PlaceObject();
            }

            else if (Input.GetMouseButtonDown(0))
            {
                PlaceObject();
            }
        }
        else
        {
            if(carrying) {
                rotateObject();
                carry(carriedObject);
                checkDrop();
            } else {
                // Debug.Log("Pickup");
                pickup();
            }
        }
    }
    
    void rotateObject()
    {
        float rotateSpeed = 10.0f;
        carriedObject.transform.Rotate(1*Time.deltaTime * rotateSpeed,2*Time.deltaTime * rotateSpeed,3*Time.deltaTime * rotateSpeed);
    }
	
    void carry(GameObject o) {
        o.transform.position = Vector3.Lerp (o.transform.position, _camera.transform.position + _camera.transform.forward * distance, Time.deltaTime * smooth);
    }
	
    void pickup() {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            var touchPosition = touch.position;
        
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = _camera.ScreenPointToRay(touchPosition);
                RaycastHit hitObject;
                if (Physics.Raycast(ray, out hitObject))
                {
                    PlacementObject p = hitObject.transform.GetComponent<PlacementObject>();
                    if (p != null)
                    {
                        p.isHit = !p.isHit;
                        carrying = true;
                        carriedObject = p.gameObject;
                        p.transform.GetComponent<Rigidbody>().isKinematic = true;
                    }
                    else
                    {
                        // Debug.Log("po null 3");
                    }
                }
            }
        }
        
        else if(Input.GetMouseButtonDown(0)) {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitObject;
            if (Physics.Raycast(ray, out hitObject))
            {
                PlacementObject p = hitObject.transform.GetComponent<PlacementObject>();
                if (p != null)
                {
                    p.isHit = !p.isHit;
                    carrying = true;
                    carriedObject = p.gameObject;
                    p.transform.GetComponent<Rigidbody>().isKinematic = true;
                }
                else
                {
                    // Debug.Log("po null 3");
                }
            }
        }
    }
	
    void checkDrop() {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            dropObject();
        }
        
        else if(Input.GetMouseButtonDown(0)) {
            dropObject();
        }
    }
	
    void dropObject()
    {
        _camera.GetComponent<AudioSource>().clip = throwSound;
        _camera.GetComponent<AudioSource>().Play();
        carrying = false;
        // carriedObject.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        carriedObject.transform.GetComponent<Rigidbody>().isKinematic = false;
        carriedObject.transform.GetComponent<Rigidbody>().AddForce(_camera.transform.forward.normalized * 25);
        carriedObject = null;
    }

    private void PlaceObject()
    {
        // set all indicators to false
        foreach (GameObject gm in placementIndicator)
        {
            gm.SetActive(false);
        }
        Instantiate(objectToPlace[placemntIndex], placementPose.position, placementPose.rotation);
        placemntIndex += 1;
    }

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid)
        {
            placementIndicator[placemntIndex].SetActive(true);
            placementIndicator[placemntIndex].transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            // set all indicators to false
            foreach (GameObject gm in placementIndicator)
            {
                gm.SetActive(false);
            }
        }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = _camera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        _arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon);

        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;
            // Debug.Log(hits.ToString());

            var cameraForward = _camera.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }
}