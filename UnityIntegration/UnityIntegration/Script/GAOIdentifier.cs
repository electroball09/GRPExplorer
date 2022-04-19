using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityIntegration.Script
{
    public class GAOIdentifier : MonoBehaviour
    {
        static Material gaoMat;

        delegate void SetVisible(bool vis);
        static event SetVisible OnVisibleToggled;

        public static bool areIdentifiersVisible { get; private set; } = true;

        MeshRenderer mr;

        public static void ToggleIdentifiersVisible()
        {
            areIdentifiersVisible = !areIdentifiersVisible;
            OnVisibleToggled?.Invoke(areIdentifiersVisible);
        }

        void Start()
        {
            if (!gaoMat)
                gaoMat = (Material)Resources.Load("GAOIdentifierMaterial");

            mr = GetComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.sharedMaterial = gaoMat;

            OnVisibleToggled += GAOIdentifier_OnVisibleToggled;
        }

        private void GAOIdentifier_OnVisibleToggled(bool vis)
        {
            mr.enabled = vis;
        }

        void OnDestroy()
        {
            OnVisibleToggled -= GAOIdentifier_OnVisibleToggled;
        }
    }
}
