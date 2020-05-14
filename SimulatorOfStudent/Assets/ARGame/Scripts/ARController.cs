using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GoogleARCore;

#if UNITY_EDITOR
    using input = GoogleARCore.InstantPreviewInput;
#endif

public class ARController : MonoBehaviour
{
    //We will fill this list with the planes that ARCore detected in the current frame
    private List<DetectedPlane> m_NewTrackedPlanes = new List<DetectedPlane>();

    public GameObject GridPrefab;

    public GameObject Dormitory;

    public GameObject HSE;

    public GameObject ARCamera;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Check ARCore session status
        if(Session.Status != SessionStatus.Tracking)
        {
            return;
        }

        //The following function will fill m_NewTrackedPlanes with the planes that ARCore detected in the current frame
        Session.GetTrackables<DetectedPlane>(m_NewTrackedPlanes, TrackableQueryFilter.New);

        //Instantiate a Grid for each DetectedPlane in m_NewTrackedPlanes
        for (int i = 0; i < m_NewTrackedPlanes.Count; ++i)
        {
            GameObject grid = Instantiate(GridPrefab, Vector3.zero, Quaternion.identity, transform);

            //This function will set the position of grid and modify the vertices of the attached mesh
            grid.GetComponent<GridVisualiser>().Initialize(m_NewTrackedPlanes[i]);

            //Check if the user touches the screen 
            Touch touch;
            if(Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            //Check if the user touched any of the tracked planes
            TrackableHit hit;
            if(Frame.Raycast(touch.position.x, touch.position.y, TrackableHitFlags.PlaneWithinPolygon, out hit))
            {
                //Place the model on top of the tracked plane that we touched

                //Enable the model
                Dormitory.SetActive(true);

                //Create a new Anchor
                Anchor anchor = hit.Trackable.CreateAnchor(hit.Pose);

                //Set the position of the portal to be the same as the hit position
                Dormitory.transform.position = hit.Pose.position;
                Dormitory.transform.rotation = hit.Pose.rotation;

                //Dormitory to face the camera
                Vector3 cameraPosition = ARCamera.transform.position;

                //Dormitory should only rotate around the Y axis
                cameraPosition.y = hit.Pose.position.y;

                //Rotate the dormitory to face the camera
                Dormitory.transform.LookAt(cameraPosition, Dormitory.transform.up);

                //ARCore will keep understanding the world and update the anchors accordingly hence we need to attach our model to the anchor
                Dormitory.transform.parent = anchor.transform;
            }
        }
    }
}
