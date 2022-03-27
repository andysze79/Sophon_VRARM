using System;

namespace Sirenix.OdinInspector
{
    public class ListItemSelectorAttribute : Attribute
    {
        public string SetSelectedMethod;

        public ListItemSelectorAttribute(string _setSelectedMethod)
        {
            this.SetSelectedMethod = _setSelectedMethod;
        }
    }
}