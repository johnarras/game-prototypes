using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI.UI.Controls;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using System;
using Microsoft.UI.Xaml.Media;
using Genrpg.Editor.Constants;
using Genrpg.Editor.UI.Interfaces;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Interfaces;
using Microsoft.UI.Xaml.Data;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Genrpg.Editor.UI
{
    public static class UIHelper
    {
        public static SmallPopup ShowBlockingDialog(WindowBase parent, string text, double width = 0, double height = 0)
        {         
            SmallPopup win = new SmallPopup(text, (int)width, (int)height);
            win.Activate();
            return win;
        }

        public static async Task<ContentDialogResultBase> ShowMessageBox(WindowBase window, string content, string title = null, bool showCancelButton = false)
        {
            MessageBoxWaiter waiter = new MessageBoxWaiter();

            window.DispatcherQueue.TryEnqueue(() =>
            {
                ContentDialog noWifiDialog = new ContentDialog
                {
                    Title = title,
                    Content = content,
                    PrimaryButtonText = "Ok",
                    SecondaryButtonText = (showCancelButton ? "Cancel" : null),
                };

                noWifiDialog.XamlRoot = window.Content.XamlRoot;

                waiter.Operation = noWifiDialog.ShowAsync();
                waiter.DidSetOperation = true;
            });

            while (!waiter.DidSetOperation ||
               waiter.Operation.Status == AsyncStatus.Started)
            {
                await Task.Delay(1);
            }

            int val = (int)(waiter.Operation.GetResults());
            waiter.Result = (ContentDialogResultBase)val;
            return waiter.Result;
        }

        public static ButtonBase CreateButton(IUICanvas canvas, EButtonTypes buttonType, 
            string name, string text, double width, double height, double xpos, double ypos, Action<object,object> clickAction)
        {
            ButtonBase button = new ButtonBase()
            {
                Height = height,
                Width = width,
                Content = text,
                Name = name,
            };

            canvas.Add(button, xpos, ypos);
            button.Click += (object o, RoutedEventArgs e) => { clickAction(o, e); };

            return button;
        }

        public static void CreateLabel(IUICanvas canvas, ELabelTypes labelType,
            string name, string text, double width, double height, double xpos, double ypos, float fontSize = FormatterConstants.DefaultLabelFontSize, TextAlignment alignment = TextAlignment.Center)
        {
            TextBlock label = new TextBlock()
            {
                RequestedTheme = ElementTheme.Default,
                Height = height,
                Width = width,
                Text = text,
                Name = name,
                TextAlignment = alignment,
                FontSize = fontSize,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
            };

            canvas.Add(label, xpos, ypos);
        }

        public static double GetWidth(object obj)
        {
            if (obj is Control cont)
            {
                return cont.Width;
            }
            return 0;
        }

        public static double GetHeight(object obj)
        {
            if (obj is Control cont)
            {
                return cont.Height;
            }
            return 0;
        }

        public static double GetTop(object obj)
        {
            if (obj is UIElement elem)
            {
                return CanvasBase.GetTop(elem);
            }
            return 0;
        }

        public static double GetLeft(object obj)
        {
            if (obj is UIElement elem)
            {
                return CanvasBase.GetLeft(elem);
            }
            return 0;
        }

        public static void SetSize(object obj, int width, int height)
        {
            if (obj is Control cont)
            {
                cont.Width = width;
                cont.Height = height;   
            }
        }

        public static void SetPosition(object obj, int x, int y)
        {
            if (obj is UIElement elem)
            {
                CanvasBase.SetLeft(elem, x);
                CanvasBase.SetTop(elem, y);
            }
        }

        public static CommunityToolkit.WinUI.UI.Controls.DataGrid CreateDataGridView(IUICanvas canvas,
            string name, double width, double height, double xpos, double ypos)
        {
            CommunityToolkit.WinUI.UI.Controls.DataGrid dataGridView = new CommunityToolkit.WinUI.UI.Controls.DataGrid()
            {
                Name = name,
                Width = width,
                Height = height,
                SelectionMode = DataGridSelectionMode.Extended,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            };

            canvas.Add(dataGridView, xpos, ypos);

            return dataGridView;
        }

        public static void SetVisible(object obj, bool visible)
        {
            if (obj  is UIElement elem)
            {
                elem.Visibility = (visible?  Visibility.Visible : Visibility.Collapsed);
            }
        }

        public static ComboBoxBase CreateComboBoxBase(IUICanvas canvas, string name,
            double width, double height, double xpos, double ypos)
        {
            ComboBoxBase comboBox = new ComboBoxBase()
            {
                Height = height,
                Width = width,
                Name = name,
                AllowDrop = true,
            };

            canvas.Add(comboBox, xpos, ypos);   
            return comboBox;
        }


        public static TextBoxBase CreateTextBoxBase(IUICanvas canvas,  string name, string initialText,
            double width, double height, double xpos, double ypos, TextChangedEventHandler eventHandler)
        {
            TextBoxBase textBox = new TextBoxBase()
            {
                Name = name,
                Text = initialText,
                Height = height,
                Width = width,
            };

            if (eventHandler != null)
            {
                textBox.TextChanged += eventHandler;
            }

            canvas.Add(textBox, xpos, ypos);

            return textBox;
        }

        public static CheckBoxBase CreateCheckBox(IUICanvas canvas,  string name,
            double width, double height, double xpos, double ypos)
        {
            CheckBoxBase checkBox = new CheckBoxBase()
            {
                Name = name,
                Height = height,
                Width = width,
            };

            canvas.Add(checkBox, xpos, ypos);
            return checkBox;
        }


        public static void SetWindowRect(WindowBase window, double xpos, double ypos, double width, double height)
        {
            window.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32((int)xpos,(int)ypos, (int)(width*ScalingConstants.DisplayScaling),
                (int)(height*ScalingConstants.DisplayScaling)));    
        }

        public static bool IsKeyDown(VirtualKey key)
        {
            CoreVirtualKeyStates state = CoreWindow.GetForCurrentThread().GetKeyState(key);
            return (state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }

        public static void RemoveDataColumnNamed(DataGrid _multiGrid, string name)
        {
            for (int j = 0; j < _multiGrid.Columns.Count; j++)
            {
                DataGridColumn col = _multiGrid.Columns[j];
                if (col.Header != null && col.Header.ToString() == name)
                {
                    _multiGrid.Columns.Remove(col);
                    break;
                }
            }
        }


        public static void AddComboBoxColumn(DataGrid dataGrid, string columnName, MemberInfo mem, Type underlyingType, MemberInfo nameMember, List<IIdName> dropdownList)
        {
            long firstKey = dropdownList.FirstOrDefault()?.IdKey ?? 1;


            DataGridColumn col = dataGrid.Columns.FirstOrDefault(x => x.Header != null &&
            x.Header.ToString() == columnName);

            if (col == null)
            {
                return;
            }

            // This is the dirtiest thing I've done I think.
            // Because the dropdown has to have a property with
            // the same name as the property of the underlying 
            // object we want to modify, convert the 
            // Idkey,Name pairs in the original dropdownList
            // to new objects of the current type that have
            // the correct property name and almost always a 
            // Name property.

            // And I am not doing "Corporate Enterprise Web Dev Best Practices"
            // This is "Indie Game Dev META (Most Effective Techniques Available)
            // since IMO "Best Practices" is a thought terminating cliche where
            // Anyway, the editor is not something the players interact with,
            // and in order to make it simpler to add new features and still
            // have the "foreign key dropdowns" generated without making a bunch
            // of small auxilitary classes or dynamic objects...idk seems
            // simplest way to make this small, but very useful and powerful
            // corner of the tooling work without having to remember to update
            // it with every new feature.
            List<object> newDropdown = new List<object>();

            foreach (IIdName nv in dropdownList)
            {
                object newObj = Activator.CreateInstance(underlyingType);

                newDropdown.Add(newObj);

                EntityUtils.SetObjectValue(newObj, nameMember, nv.Name);
                EntityUtils.SetObjectValue(newObj, mem, nv.IdKey);
            }

            Binding binding = new Binding()
            {
                Mode = BindingMode.TwoWay,
                Path = new Microsoft.UI.Xaml.PropertyPath(mem.Name),
                ElementName = mem.Name,
            };

            DataGridComboBoxColumn col2 = new DataGridComboBoxColumn()
            {
                ItemsSource = newDropdown,
                DisplayMemberPath = "Name",
                Header = col.Header,
                Binding = binding,
            };

            dataGrid.Columns.Remove(col);
            dataGrid.Columns.Add(col2);

        }
    }
}
