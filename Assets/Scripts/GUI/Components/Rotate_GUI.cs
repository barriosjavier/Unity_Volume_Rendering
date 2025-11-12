using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
//using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography.X509Certificates;


//using System.Threading.Tasks.Dataflow;
using UnityEngine;

namespace UnityVolumeRendering
{
    public class RotageGUI : MonoBehaviour{

    public VolumeRenderedObject targetVolume;


    public bool showGUI = true;
    public float rotationSpeed = 50f;

    private bool initialized = false;

        private void Start()
        {

            if (targetVolume == null)
                showGUI = false;
            else
                initialized = true;
        }

        public void SetTargetVolume(VolumeRenderedObject volObj)
    {
        if(volObj != null)
        {
            targetVolume = volObj;
            showGUI = true;
            initialized = true;
        }
    }
        private void OnGUI()
        {
            int panelwidth = 150;
            int panelheight = 620;
            VolumeRenderedObject volObj = FindObjectOfType<VolumeRenderedObject>();
            VolumeDataset volume = volObj.dataset;

            if (volObj==null)
            return;


            GUILayout.BeginArea(new Rect(Screen.width - panelwidth, 0, panelheight, panelwidth));
            GUILayout.BeginVertical();
            if (GUILayout.Button("Frontal"))
            {
                //Moverse a parte frontal
                //And que tiene que ser un quaternion, vamos de locos
                volObj.transform.rotation = new UnityEngine.Quaternion(0f, 0f, 0f, 0f);
                            }
            if (GUILayout.Button("Back"))
            {
                 volObj.transform.rotation = new UnityEngine.Quaternion(0f, 90f, 0f, 0f);

            }
            if (GUILayout.Button("Lateral_Left"))
            {
                volObj.transform.rotation = new UnityEngine.Quaternion(0f, 0f, 0f, 0f);
                volObj.transform.rotation = UnityEngine.Quaternion.Euler(0f, 90f, 0f);

            }
            if (GUILayout.Button("Lateral_Right"))
            {
                volObj.transform.rotation = new UnityEngine.Quaternion(0f, 0f, 0f, 0f);
                volObj.transform.rotation = UnityEngine.Quaternion.Euler(0f, -90f, 0f);

            }
            if (GUILayout.Button("Superior"))
            {
                volObj.transform.rotation = new UnityEngine.Quaternion(0f, 0f, 0f, 0f);
                volObj.transform.rotation = UnityEngine.Quaternion.Euler(-90f, 0f, 0f);
            }
            if (GUILayout.Button("Inferior"))
            {
                volObj.transform.rotation = new UnityEngine.Quaternion(0f, 0f, 0f, 0f);
                volObj.transform.rotation = UnityEngine.Quaternion.Euler(90f, 0f, 0f);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();

        
        }
        
               
            private void MoveCameratoFrontal(UnityEngine.Vector3 center)
        {
                //esto lo hace mal porque cada vez se va a un sitio cuando tenía que estar siempre en el mismo
            UnityEngine.Vector3 frontal_pos = new UnityEngine.Vector3(center.x, center.y, center.z);
            Camera.main.transform.position = frontal_pos;
            //Camera.main.transform.rotation = Quaternion.Euler(90f, 0, 0);
            }
            private void MoveCameratoBack() { }
            private void MoveCameratoLateralLeft() { }
            private void MoveCameratoLateralRight() { }
            private void MoveCameratoSuperior() { }
            private void MoveCameratoInferior() { }
        
    }

   

}



