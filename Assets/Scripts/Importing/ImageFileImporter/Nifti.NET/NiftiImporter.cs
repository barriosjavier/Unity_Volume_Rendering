using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using Nifti.NET;
using System.Threading.Tasks;

namespace UnityVolumeRendering
{
    /// <summary>
    /// SimpleITK-based DICOM importer.
    /// </summary>
    public class NiftiImporter : IImageFileImporter
    {
        private static double[,] GetAffineMatrix(string filePath)
        {
            var nifti = NiftiFile.Read(filePath);
            NiftiHeader header = nifti.Header;

            if (header.sform_code > 0)
            {
                // Usar SForm (más precisa normalmente)

                return new double[4, 4]
                {
                { header.srow_x[0], header.srow_x[1], header.srow_x[2], header.srow_x[3] },
                { header.srow_y[0], header.srow_y[1], header.srow_y[2], header.srow_y[3] },
                { header.srow_z[0], header.srow_z[1], header.srow_z[2], header.srow_z[3] },
                { 0, 0, 0, 1 }
                };
            }
            else if (header.qform_code > 0)
            {
                // Usar QForm si no hay SForm
                var affine = new double[4, 4];
                affine = ComputeAffineFromQuaternion(header.quatern_b, header.quatern_c, header.quatern_d, header.qoffset_x, header.qoffset_y, header.qoffset_z, header.pixdim);

                return affine;
            }
            else
            {
                // a rezar porque esté bien orientado
                throw new Exception("El archivo NIfTI no tiene información de orientación (sform/qform).");
            }
    }
        private static double[,] ComputeAffineFromQuaternion(
        float qb, float qc, float qd,
        float qx, float qy, float qz,
        float[] pixdim)
        {
            float qfac = (pixdim[0] == 0) ? 1 : Math.Sign(pixdim[0]);
            float sx = pixdim[1];
            float sy = pixdim[2];
            float sz = pixdim[3];

            // Normalizar y calcular qa
            double a2 = 1.0 - (qb * qb + qc * qc + qd * qd);
            double qa = a2 > 0.0 ? Math.Sqrt(a2) : 0.0;

            // Rotación (matriz 3x3)
            double[,] R = new double[3, 3];
            R[0, 0] = qa * qa + qb * qb - qc * qc - qd * qd;
            R[0, 1] = 2.0 * (qb * qc - qa * qd);
            R[0, 2] = 2.0 * (qb * qd + qa * qc);
            R[1, 0] = 2.0 * (qb * qc + qa * qd);
            R[1, 1] = qa * qa + qc * qc - qb * qb - qd * qd;
            R[1, 2] = 2.0 * (qc * qd - qa * qb);
            R[2, 0] = 2.0 * (qb * qd - qa * qc);
            R[2, 1] = 2.0 * (qc * qd + qa * qb);
            R[2, 2] = qa * qa + qd * qd - qb * qb - qc * qc;

            // Aplicar escalas y qfac
            for (int i = 0; i < 3; i++)
            {
                R[i, 0] *= sx;
                R[i, 1] *= sy;
                R[i, 2] *= sz * qfac;
            }

            var affine = new double[4, 4]
                {
                { R[0, 0], R[0, 1], R[0, 2], qx },
                { R[1, 0], R[1, 1], R[1, 2], qy },
                { R[2, 0], R[2, 1], R[2, 2], qz },
                { 0, 0, 0, 1 }
                };
            return affine;
        }
    
    private static string GetOrientationFromAffine(double[,] affine)
        {
            // Se asume que affine es 4x4 y que las columnas 0,1,2 son los vectores de eje
            string[] axisLabelsPos = { "R", "A", "S" }; // X+, Y+, Z+
            string[] axisLabelsNeg = { "L", "P", "I" }; // X-, Y-, Z-

            string orientation = "";

            for (int col = 0; col < 3; col++)
            {
                // Extraer el eje como vector
                double x = affine[0, col];
                double y = affine[1, col];
                double z = affine[2, col];

                // Encontrar el componente dominante
                double absX = Math.Abs(x);
                double absY = Math.Abs(y);
                double absZ = Math.Abs(z);

                if (absX > absY && absX > absZ)
                {
                    orientation += (x > 0 ? axisLabelsPos[0] : axisLabelsNeg[0]);
                }
                else if (absY > absX && absY > absZ)
                {
                    orientation += (y > 0 ? axisLabelsPos[1] : axisLabelsNeg[1]);
                }
                else
                {
                    orientation += (z > 0 ? axisLabelsPos[2] : axisLabelsNeg[2]);
                }
            }

            return orientation;
        }
        public VolumeDataset Import(string filePath)
        {
            Nifti.NET.Nifti niftiFile = NiftiFile.Read(filePath);
            if (niftiFile == null)
            {
                Debug.LogError("Failed to read NIFTI dataset");
                return null;
            }
            int numDimensions = niftiFile.Header.dim[0];
            if (numDimensions > 3)
            {
                Debug.LogError($"Unsupported dimension. Expected 3-dimensional dataset, but got {numDimensions}.");
                return null;
            }
           
            // Create dataset
            VolumeDataset volumeDataset = ScriptableObject.CreateInstance<VolumeDataset>();
            bool succeeded = ImportInternal(volumeDataset, niftiFile, filePath);

            if (!succeeded)
                volumeDataset = null;

            return volumeDataset;
        }

        public async Task<VolumeDataset> ImportAsync(string filePath)
        {
            Nifti.NET.Nifti niftiFile = null;
            VolumeDataset volumeDataset = ScriptableObject.CreateInstance<VolumeDataset>();

            await Task.Run(() =>niftiFile = NiftiFile.Read(filePath));

            if (niftiFile == null)
            {
                Debug.LogError("Failed to read NIFTI dataset");
                return null;
            }

            int numDimensions = niftiFile.Header.dim[0];
            if (numDimensions > 3)
            {
                Debug.LogError($"Unsupported dimension. Expected 3-dimensional dataset, but got {numDimensions}.");
                return null;
            }

            bool succeeded = await Task.Run(() => ImportInternal(volumeDataset,niftiFile,filePath));

            if (!succeeded)
                volumeDataset = null;

            return volumeDataset;
        }
        private bool ImportInternal(VolumeDataset volumeDataset,Nifti.NET.Nifti niftiFile,string filePath)
        {
            int dimX = niftiFile.Header.dim[1];
            int dimY = niftiFile.Header.dim[2];
            int dimZ = niftiFile.Header.dim[3];
            float[] pixelData = niftiFile.ToSingleArray();

            if (pixelData == null)
            {
                Debug.LogError($"Failed to read data, of type: {niftiFile.Data?.GetType()}");
                return false;
            }

            Vector3 pixdim = new Vector3(niftiFile.Header.pixdim[1], niftiFile.Header.pixdim[2], niftiFile.Header.pixdim[3]);
            Vector3 size = new Vector3(dimX * pixdim.x, dimY * pixdim.y, dimZ * pixdim.z);

            // Create dataset
            volumeDataset.data = pixelData;
            volumeDataset.dimX = dimX;
            volumeDataset.dimY = dimY;
            volumeDataset.dimZ = dimZ;
            volumeDataset.datasetName = Path.GetFileName(filePath);
            volumeDataset.filePath = filePath;
            volumeDataset.scale = size;

            double[,] affine= new double[4,4];
            affine = GetAffineMatrix(filePath);
            string orientation = GetOrientationFromAffine(affine);

            orientation=GetOrientationFromAffine(affine);
            int len_data = dimX * dimY * dimZ;
            if(orientation[2]=='S'){

                float[] data_inv = new float[len_data];
                for (int i = 0; i < len_data-1; i++)
                {
                    data_inv[i] = pixelData[(len_data-1) - i];
                }
                volumeDataset.data = data_inv;
            }

            volumeDataset.FixDimensions();
            volumeDataset.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

            return true;
        }
    }
}
