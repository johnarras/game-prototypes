using Genrpg.Editor.UI.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Editor.UI
{
    public class ButtonBase : Button { }

    public class CheckBoxBase : CheckBox { }

    public class TextBoxBase : TextBox { }

    public class ComboBoxBase : ComboBox { }

    public class CanvasBase : Canvas, IUICanvas
    {
        public void Add(object obj, double x, double y)
        {
            if (obj is UIElement elem)
            {
                GetChildren().Add(elem);
                SetLeft(elem, x);
                SetTop(elem, y);
            }
        }

        public void ClearChildren()
        {
            GetChildren().Clear();
        }

        public object GetChildNamed(string name)
        {
            foreach (UIElement element in GetChildren())
            {
                if (element is Control cont && cont.Name == name)
                {
                    return cont;
                }
            }

            return null;
        }

        public int GetChildCount()
        {
            return GetChildren().Count;
        }

        public UIElement GetChild(int index)
        {
            return GetChildren()[index];
        }

        public void Add(object obj)
        {
            if (obj is UIElement elem)
            {
                GetChildren().Add(elem);
            }
        }

        public List<UIElement> GetChildList()
        {
            return GetChildren().ToList();
        }

        public bool Contains(object obj)
        {
            if (obj is UIElement elem)
            {
                return GetChildren().Contains(elem);
            }
            return false;
        }

        public void Remove(object obj)
        {
            if (obj is UIElement elem)
            {
                GetChildren().Remove(elem);
            }
        }

        protected UIElementCollection GetChildren()
        {
            return Children;
        }
    }


    public enum ContentDialogResultBase
    {
        None=0,
        Primary=1,
        Secondary=2,
    }
}
