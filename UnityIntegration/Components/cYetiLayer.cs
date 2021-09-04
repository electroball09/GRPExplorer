using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.YetiObjects;

namespace UnityIntegration.Components
{
    public class cYetiLayer : cYetiObjectReference
    {
        public delegate void LayerToggledDelegate(bool isToggledOn);
        public event LayerToggledDelegate OnLayerToggled;

        public bool isToggledOn { get; private set; } = true;
        public YetiLayer YetiLayer { get; private set; }

        public void SetLayer(YetiLayer layer)
        {
            YetiLayer = layer;
        }

        public void ToggleLayer()
        {
            isToggledOn = !isToggledOn;
            OnLayerToggled?.Invoke(isToggledOn);
        }
    }
}
