using UnityEngine;
using UnityEngine.VR;

namespace EVRTH.Scripts.Utility
{
    /// <summary>
    /// Simple script to interact with the IPointerHandlerVR script. 
    /// All uGUI objects that you want to be able to interact with need to be on World Canvases and have colliders
    /// Either the uGUI elements or this object needs to have a collider that is marked as a trigger.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Hand : MonoBehaviour
    {
        public UnityEngine.XR.XRNode vrNode;
        public Transform globeTransform;
        public float globeRotSpeed;
        public float scaleThreshold;
        public float scaleSpeed;
        public bool useLaserPointer;
        public LineRenderer laserPointerRenderer;
    

        private new Transform transform;

        private IPointerHandlerVr isTouchingVrButton;
        private bool isZoom;
        private float oldDist;


        private void Awake()
        {
            transform = GetComponent<Transform>();
            UnityEngine.XR.InputTracking.Recenter();
            if (useLaserPointer && laserPointerRenderer == null)
            {
                laserPointerRenderer = GetComponent<LineRenderer>();
                if (!laserPointerRenderer)
                {
                    print("Requires a line renderer to use laser pointer");
                    useLaserPointer = false;
                }
            }
        }

        // Update is called once per frame
        private void Update()
        {
            transform.localPosition = UnityEngine.XR.InputTracking.GetLocalPosition(vrNode);
            transform.localRotation = UnityEngine.XR.InputTracking.GetLocalRotation(vrNode);
            if (useLaserPointer)
            {
                laserPointerRenderer.SetPosition(0, transform.position);
                RaycastHit hit;
                if (Physics.Raycast(new Ray(transform.position, transform.forward), out hit))
                {
                    laserPointerRenderer.SetPosition(1,hit.point);
                    IPointerHandlerVr ipvr = hit.collider.gameObject.GetComponent<IPointerHandlerVr>();
                    if (ipvr != null)
                    {
                        if (isTouchingVrButton != null && isTouchingVrButton != ipvr)
                        {
                            isTouchingVrButton.PointerExit(transform.position);
                            isTouchingVrButton = null;
                        }
                        isTouchingVrButton = ipvr;
                        isTouchingVrButton.PointerEnter(transform.position);
                    }
                    else
                    {
                        if (isTouchingVrButton != null)
                        {
                            isTouchingVrButton.PointerExit(transform.position);
                            isTouchingVrButton = null;
                        }
                    }
                }
                else
                {
                    if (isTouchingVrButton != null)
                    {
                        isTouchingVrButton.PointerExit(transform.position);
                        isTouchingVrButton = null;
                    }
                    laserPointerRenderer.SetPosition(1, transform.position + transform.forward * 5f);
                }
            }

            if (Input.GetButtonDown("VRSubmit") && isTouchingVrButton != null)
            {
                isTouchingVrButton.PointerDown(transform.position);
            }

            if (Input.GetButtonUp("VRSubmit") && isTouchingVrButton != null)
            {
                isTouchingVrButton.PointerUp(transform.position);
            }

            //rotate the globe with the touchpad
            globeTransform.Rotate(Vector3.up,globeRotSpeed * Input.GetAxis("Horizontal") * Time.deltaTime,Space.World);

            if (!Input.GetButton("VRSubmit") && isZoom)
            {
                isZoom = false;
            }

            if (isZoom)
            {
                float newDist = Vector3.Distance(transform.position, globeTransform.position);
                if (newDist - oldDist > scaleThreshold && globeTransform.localScale.x < 2.75f || oldDist - newDist > scaleThreshold && globeTransform.localScale.x > 0.25f)
                {
                    globeTransform.localScale += Vector3.one * scaleSpeed * (newDist - oldDist) * Time.deltaTime;
                    oldDist = newDist;
                }
            }

            if (Input.GetButton("VRSubmit") && !isZoom)
            {
                isZoom = true;
                oldDist = Vector3.Distance(transform.position, globeTransform.position);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //**********************************************************************************************************************************************************************************************************
            // https://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
            // The c++ object becomes null but part of the c# unity wrapper hangs on so this check is actually important even though it seems insane
            // the link above explains it.
            if (isTouchingVrButton != null && isTouchingVrButton.Equals(null))
            {
                isTouchingVrButton = null;
            }
            // *********************************************************************************************************************************************************************************************************

            if (isTouchingVrButton != null  && !((MonoBehaviour)isTouchingVrButton).gameObject.Equals(other.gameObject))
            {
                isTouchingVrButton.PointerExit(transform.position);
                isTouchingVrButton = null;
            }
            if (other.GetComponent<IPointerHandlerVr>() != null)
            {
                isTouchingVrButton = other.GetComponent<IPointerHandlerVr>();
                isTouchingVrButton.PointerEnter(transform.position);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            //**********************************************************************************************************************************************************************************************************
            // https://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
            // The c++ object becomes null but part of the c# unity wrapper hangs on so this check is actually important even though it seems insane
            // the link above explains it.
            if (isTouchingVrButton != null && isTouchingVrButton.Equals(null))
            {
                isTouchingVrButton = null;
            }
            // *********************************************************************************************************************************************************************************************************

            if (isTouchingVrButton != null && ((MonoBehaviour)isTouchingVrButton).gameObject.Equals(other.gameObject))
            {
                isTouchingVrButton.PointerExit(transform.position);
                isTouchingVrButton = null;
            }
        }
    }
}
