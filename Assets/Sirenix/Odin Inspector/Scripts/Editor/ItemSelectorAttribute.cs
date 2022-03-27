using System;

namespace Sirenix.OdinInspector
{
    public class ItemSelectorAttribute : Attribute
    {
        public string SetSelectedMethod;

        public ItemSelectorAttribute(string _setSelectedMethod)
        {
            this.SetSelectedMethod = _setSelectedMethod;
        }
    }
}