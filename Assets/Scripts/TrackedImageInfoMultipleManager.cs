using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class TrackedImageInfoMultipleManager : MonoBehaviour
{
    [SerializeField]
    private bool isViewOneImageAtATime;

    [SerializeField]
    private bool isViewBothImagesTogether;

    [SerializeField]
    private Text imageTrackedText;

    [SerializeField]
    private GameObject[] arObjectsToPlace;

    [SerializeField]
    private Vector3 scaleFactor = new Vector3(0.1f,0.1f,0.1f);

    private ARTrackedImageManager m_TrackedImageManager;

    private Dictionary<string, GameObject> arObjects = new Dictionary<string, GameObject>();

    public XRReferenceImageLibrary m_ImageLibrary;

    static Guid s_FirstImageGUID;
    static Guid s_SecondImageGUID;
    void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
        
        // setup all game objects in dictionary
        foreach(GameObject arObject in arObjectsToPlace)
        {
            GameObject newARObject = Instantiate(arObject, Vector3.zero, Quaternion.identity);
            newARObject.name = arObject.name;
            arObjects.Add(arObject.name, newARObject);
            newARObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        s_FirstImageGUID = m_ImageLibrary[0].guid;
        s_SecondImageGUID = m_ImageLibrary[1].guid;

        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    private GameObject m_SpawnedOnePrefab;
    private GameObject m_SpawnedTwoPrefab;

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs obj)
    {
        if(isViewOneImageAtATime && !isViewBothImagesTogether)
        {
            foreach (ARTrackedImage trackedImage in obj.added)
            {
                imageTrackedText.text = trackedImage.referenceImage.name + " added";
                UpdateARImage(trackedImage);
            }

            foreach (ARTrackedImage trackedImage in obj.updated)
            {
                imageTrackedText.text = trackedImage.referenceImage.name + " updated";
                UpdateARImage(trackedImage);
            }

            foreach (ARTrackedImage trackedImage in obj.removed)
            {
                imageTrackedText.text = trackedImage.referenceImage.name + " removed";
                arObjects[trackedImage.name].SetActive(false);
            }
        }
        else if(!isViewOneImageAtATime && isViewBothImagesTogether)
        {
            // added, spawn prefab
            foreach(ARTrackedImage image in obj.added)
            {
                if (image.referenceImage.guid == s_FirstImageGUID)
                {
                    m_SpawnedOnePrefab = Instantiate(arObjectsToPlace[0], image.transform.position, image.transform.rotation);
                }
                else if (image.referenceImage.guid == s_SecondImageGUID)
                {
                    m_SpawnedTwoPrefab = Instantiate(arObjectsToPlace[1], image.transform.position, image.transform.rotation);
                }

                imageTrackedText.text = image.referenceImage.name + " added";
            }
        
            // updated, set prefab position and rotation
            foreach(ARTrackedImage image in obj.updated)
            {
                // image is tracking or tracking with limited state, show visuals and update it's position and rotation
                if (image.trackingState == TrackingState.Tracking)
                {
                    if (image.referenceImage.guid == s_FirstImageGUID)
                    {
                        m_SpawnedOnePrefab.SetActive(true);
                        m_SpawnedOnePrefab.transform.SetPositionAndRotation(image.transform.position, image.transform.rotation);
                    }
                    else if (image.referenceImage.guid == s_SecondImageGUID)
                    {
                        m_SpawnedTwoPrefab.SetActive(true);
                        m_SpawnedTwoPrefab.transform.SetPositionAndRotation(image.transform.position, image.transform.rotation);
                    }
                }
                // image is no longer tracking, disable visuals TrackingState.Limited TrackingState.None
                else
                {
                    if (image.referenceImage.guid == s_FirstImageGUID)
                    {
                        m_SpawnedOnePrefab.SetActive(false);
                    }
                    else if (image.referenceImage.guid == s_SecondImageGUID)
                    {
                        m_SpawnedTwoPrefab.SetActive(false);
                    }
                }
                
            }
        
            // removed, destroy spawned instance
            foreach(ARTrackedImage image in obj.removed)
            {
                if (image.referenceImage.guid == s_FirstImageGUID)
                {
                    Destroy(m_SpawnedOnePrefab);
                }
                else if (image.referenceImage.guid == s_SecondImageGUID)
                {
                    Destroy(m_SpawnedTwoPrefab);
                }
                imageTrackedText.text = image.referenceImage.name + " removed";
            }
        }
        
    }

    private void UpdateARImage(ARTrackedImage trackedImage)
    {
        // Display the name of the tracked image in the canvas
        //imageTrackedText.text = trackedImage.referenceImage.name;

        // Assign and Place Game Object
        AssignGameObject(trackedImage.referenceImage.name, trackedImage.transform.position);

        Debug.Log($"trackedImage.referenceImage.name: {trackedImage.referenceImage.name}");
    }

    void AssignGameObject(string name, Vector3 newPosition)
    {
        if(arObjectsToPlace != null)
        {
            GameObject goARObject = arObjects[name];
            goARObject.SetActive(true);
            goARObject.transform.position = newPosition;
            goARObject.transform.localScale = scaleFactor;
            foreach(GameObject go in arObjects.Values)
            {
                Debug.Log($"Go in arObjects.Values: {go.name}");
                if(go.name != name)
                {
                    go.SetActive(false);
                }
            } 
        }
    }
}
