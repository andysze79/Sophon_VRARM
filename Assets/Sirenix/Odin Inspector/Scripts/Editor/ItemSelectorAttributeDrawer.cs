using UnityEngine;
using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector
{
    [DrawerPriority(0.01, 0, 0)]
    public class ItemSelectorAttributeDrawer : OdinAttributeDrawer<ItemSelectorAttribute>
    {
        private static readonly Color SelectedColor = new Color(0.301f, 0.563f, 1f, 0.497f);
        private bool isListElement;
        private InspectorProperty baseMemberProperty;
        private PropertyContext<InspectorProperty> globalSelectedProperty;
        private InspectorProperty selectedProperty;
        private Action<object, int> selectedIndexSetter;

        protected override void Initialize()
        {
            isListElement = Property.Parent?.ChildResolver is IOrderedCollectionResolver;
            bool isList = !isListElement;
            InspectorProperty listProperty = isList ? Property : Property.Parent;
            
            baseMemberProperty = listProperty.FindParent(x => x.Info.PropertyType == PropertyType.Value, true);
            globalSelectedProperty = baseMemberProperty.Context.GetGlobal("selectedIndex" + baseMemberProperty.GetHashCode(), (InspectorProperty)null);
            
            Debug.Log(baseMemberProperty);
            Debug.Log(globalSelectedProperty);
            Debug.Log(globalSelectedProperty.Value);
            
            if (!isList) return;
            Type parentType = baseMemberProperty.ParentValues[0].GetType();
            selectedIndexSetter = EmitUtilities.CreateWeakInstanceMethodCaller<int>(parentType.GetMethod(Attribute.SetSelectedMethod, Flags.AllMembers));
        }

        protected override void DrawPropertyLayout(GUIContent _label)
        {
            EventType t = Event.current.type;
            
            if (isListElement)
            {
                if (t == EventType.Layout)
                {
                    CallNextDrawer(_label);
                }
                else
                {
                    Rect rect = GUIHelper.GetCurrentLayoutRect();
                    bool isSelected = globalSelectedProperty.Value == Property;

                    switch (t)
                    {
                        case EventType.Repaint when isSelected:
                            EditorGUI.DrawRect(rect, SelectedColor);
                            break;
                        case EventType.MouseDown when rect.Contains(Event.current.mousePosition):
                            globalSelectedProperty.Value = Property;
                            break;
                    }

                    CallNextDrawer(_label);
                }
            }
            else
            {
                CallNextDrawer(_label);

                if (Event.current.type == EventType.Layout) return;
                
                InspectorProperty sel = globalSelectedProperty.Value;
                
                // Select
                if (sel != null && sel != selectedProperty)
                {
                    selectedProperty = sel;
                    Select(selectedProperty.Index);
                }
                // Deselect when destroyed
                else if (selectedProperty != null && selectedProperty.Index < Property.Children.Count && selectedProperty != Property.Children[selectedProperty.Index])
                {
                    const int index = -1;
                    Select(index);
                    selectedProperty = null;
                    globalSelectedProperty.Value = null;
                }
            }
        }

        private void Select(int _index)
        {
            GUIHelper.RequestRepaint();
            Property.Tree.DelayAction(() =>
            {
                for (int i = 0; i < baseMemberProperty.ParentValues.Count; i++)
                {
                    selectedIndexSetter(baseMemberProperty.ParentValues[i], _index);
                }
            });
        }
    }
}