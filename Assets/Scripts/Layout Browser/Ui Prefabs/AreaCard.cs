using System;
using UnityEngine;

namespace fireMCG.PathOfLayouts.LayoutBrowser.Ui
{
    public sealed class AreaCard : MonoBehaviour
    {
        private Action<string> _selectedCallback;
        private string _areaId;

        public void Initialize(Action<string> selectedCallback, string areaId)
        {
            _selectedCallback = selectedCallback;
            _areaId = areaId;
        }

        public void Select()
        {
            _selectedCallback?.Invoke(_areaId);
        }
    }
}