using System;
using System.Collections.Generic;
//using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

//using System.Threading.Tasks.Dataflow;
using UnityEngine;

namespace UnityVolumeRendering
{
    //GUI de la orientación
    public class OrientationLoadGUI : MonoBehaviour
    {
        private Rect windowRect = new Rect(150, 0, WINDOW_WIDTH, WINDOW_HEIGHT);

        private const int WINDOW_WIDTH = 400;
        private const int WINDOW_HEIGHT = 400;
        
        private int windowID;
        private static OrientationLoadGUI instance;
        public VolumeDataset dataset;

        private void Awake()
        {
            // Fetch a unique ID for our window (see GUI.Window)
            windowID = WindowGUID.GetUniqueWindowID();
        }
        private void OnGUI()
        {
            windowRect = GUI.Window(windowID, windowRect, UpdateWindow, "Orientation of the volume");
        }

        public static void ShowWindow(VolumeDataset dataset)
        {
        
        if(instance != null)
            GameObject.Destroy(instance);

        GameObject obj = new GameObject($"OrientationLoadGUI{dataset}");
        instance = obj.AddComponent<OrientationLoadGUI>();
        instance.dataset = dataset;
        
        }
        private void UpdateWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            string[] ButtonsText = new string[] { "Frontal", "Back", "Lateral Left", "Lateral Right", "Superior", "Inferior" };

            for (int fila = 0; fila < 2; fila++)
            {
                GUILayout.BeginHorizontal();

                for (int col = 0; col < 3; col++)
                {
                    int id = fila * 3 + col;
                    if (GUILayout.Button(ButtonsText[id], GUILayout.Width(100), GUILayout.Height(40)))
                    {
                        LoadVolume(id, dataset);    
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            // Botones Open y Close
            if (GUILayout.Button("Open"))
            {
                GameObject.Destroy(this.gameObject);
            }
            if (GUILayout.Button("Close"))
            {
                GameObject.Destroy(this.gameObject);
                VolumeRenderedObject volObj = FindObjectOfType<VolumeRenderedObject>();

                if(volObj!=null)
                    GameObject.Destroy(FindObjectOfType<VolumeRenderedObject>().gameObject);
            }
            GUILayout.EndHorizontal();
        }
        
        ///función para cargar el volumen. El id viene de los botones pulsados en OrientationLoadGUI y el dataset es el nifti loadeado
        private  async void  LoadVolume(int id,VolumeDataset dataset){

            ///miramos si hay objetos ya cargados en la pantalla de Game y si los hay los quitamos para que no haya uno por cada botón que pulsamos
            VolumeRenderedObject volObj = FindObjectOfType<VolumeRenderedObject>();

            if (id == 0)
            {
                if (volObj != null)
                {
                    Destroy(volObj.gameObject);
                }
                dataset.rotation = UnityEngine.Quaternion.Euler(90f, 0f, 0f);
            }
            //aquí cambiar rotación en función de botón pulsado
            ///esto lo tengo que mirar para ver si es consistente, porque depende las orientaciones de los volúmenes habrá que cambiar cosas, 
            ///aunque igual no aquí, sino a la hora de cargarlo
            else if (id == 1)
            {

                //tengo que eliminar el volobj anterior si hay para que solo haya uno y giro el volumen para que la cámara vea la orientación que quiero
                if (volObj != null)
                {
                    Destroy(volObj.gameObject);
                }
                dataset.rotation = UnityEngine.Quaternion.Euler(90f, 180f, 0f);
            }
            else if (id == 2)
            {
                if (volObj != null)
                {
                    Destroy(volObj.gameObject);
                }
                dataset.rotation = new UnityEngine.Quaternion(0f, 0f, 0f, 0f);
                dataset.rotation = UnityEngine.Quaternion.Euler(90f, 90f, 0f);
            }
            else if (id == 3)
            {
                if (volObj != null)
                {
                    Destroy(volObj.gameObject);
                }
                dataset.rotation = new UnityEngine.Quaternion(0f, 0f, 0f, 0f);
                dataset.rotation = UnityEngine.Quaternion.Euler(90f, -90f, 0f);
            }
            else if (id == 4)
            {
                if (volObj != null)
                {
                    Destroy(volObj.gameObject);
                }
                dataset.rotation = new UnityEngine.Quaternion(0f, 0f, 0f, 0f);
                dataset.rotation = UnityEngine.Quaternion.Euler(0f, 0f, 0f);
            }
            else if (id == 5)
            {
                if (volObj != null)
                {
                    Destroy(volObj.gameObject);
                }
                dataset.rotation = new UnityEngine.Quaternion(0f, 0f, 0f, 0f);
                dataset.rotation = UnityEngine.Quaternion.Euler(180f, 0f, 0f);
            }
        
        // esto es para del dataset, hacer el objeto que se muestra renderizado en la pantalla de Game
        if (dataset != null)
        {
             await VolumeObjectFactory.CreateObjectAsync(dataset);
        }
        else
        {
            Debug.LogError("Failed to import datset");
            } 
            
        }
        
        
    }

    
}