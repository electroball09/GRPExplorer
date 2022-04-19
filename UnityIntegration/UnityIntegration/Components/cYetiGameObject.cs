using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.YetiObjects;
using UnityEngine;

namespace UnityIntegration.Components
{
    public class cYetiGameObject : cYetiObjectReference
    {
        public bool TransformFromMatrix = true;
        public bool UseYetiMatrix = false;
        public bool ConvertPosition = true;
        public bool ConvertRotation = true;

        public Matrix4x4 YetiMatrix;
        public Matrix4x4 ConvertedMatrix;

        public cYetiLayer Layer;

        public void SetYetiGameObject(YetiGameObject obj)
        {
            YetiMatrix = obj.Matrix.ToUnityNoTranspose();
            ConvertedMatrix = obj.Matrix.ToUnity();
        }

        public void SetYetiLayer(cYetiLayer layer)
        {
            Layer = layer;
            if (Layer)
                layer.OnLayerToggled += OnLayerToggled;
        }

        void OnDestroy()
        {
            if (Layer)
                Layer.OnLayerToggled -= OnLayerToggled;
        }

        void OnLayerToggled(bool isToggledOn)
        {
            gameObject.SetActive(isToggledOn);
        }

        public void UpdateTransformFromMatrix()
        {
            if (!TransformFromMatrix)
                return;

            Vector3 pos;
            Quaternion rot;
            Vector3 scale;
            if (UseYetiMatrix)
                IntegrationUtil.DecomposeMatrix(ref YetiMatrix, out pos, out rot, out scale);
            else
                IntegrationUtil.DecomposeMatrix(ref ConvertedMatrix, out pos, out rot, out scale);

            if (ConvertPosition)
                pos = pos.ConvertYetiToUnityCoords();
            if (ConvertRotation)
            {
                rot = rot.ConvertYetiToUnityRotation();
            }

            transform.SetPositionAndRotation(pos, rot);
            transform.localScale = scale;

            //Debug.Log(ConvertedMatrix.ValidTRS());
        }
    }
}
