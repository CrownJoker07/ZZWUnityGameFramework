//------------------------------------------------------------
// File : IUniversalISelectTabPanel.cs
// Email: mailto:zewei.zhuang@kingboat.io
// Desc : 
//------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KBGame
{
    public interface IUniversalISelectTabPanel
    {
        public void Select(int tabType);

        public void UnSelect(int tabType); 
    } 
}
