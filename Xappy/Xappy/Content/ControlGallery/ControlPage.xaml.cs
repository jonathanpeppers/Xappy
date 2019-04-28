﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xappy.Content.ControlGallery;
using Xappy.Content.ControlGallery.ControlTemplates;
using Xappy.Content.ControlGallery.ProppyControls;

namespace Xappy.ControlGallery
{
    [XamlCompilation(XamlCompilationOptions.Skip)]
    public partial class ControlPage : ContentPage
    {
        PropertyInfo ActiveProperty;

        View _element; // this is the target control

        StackLayout _propertyLayout; // this is the property grid content

        HashSet<string> _exceptProperties = new HashSet<string>
        {
            AutomationIdProperty.PropertyName,
            ClassIdProperty.PropertyName,
            "StyleId",
        };

        public ControlPage()
        {
            InitializeComponent();

            BindingContext = new ControlPageViewModel();
        }

        private View ControlTemplate;

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // do the looked to get the right template
            ControlTemplate = new ButtonControlTemplate();
            
            ControlCanvas.Children.Clear();
            ControlCanvas.Children.Add(ControlTemplate);

            _element = (ControlTemplate as IControlTemplate).TargetControl;
            //_propertyLayout = PropertyContainer;

            //OnElementUpdated(_element);

            (BindingContext as ControlPageViewModel).SetElement(_element);
        }


        void OnElementUpdated(View oldElement)
        {
            _propertyLayout.Children.Clear();

            var elementType = _element.GetType();

            var publicProperties = elementType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && !_exceptProperties.Contains(p.Name));

            // BindableProperty used to clean property values
            var bindableProperties = elementType
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(p => p.FieldType.IsAssignableFrom(typeof(BindableProperty)))
                .Select(p => (BindableProperty)p.GetValue(_element));

            foreach (var property in publicProperties)
            {

                //if (property.PropertyType == typeof(Color))
                //{
                //    var colorPicker = new ColorPicker
                //    {
                //        Title = property.Name,
                //        Color = (Color)property.GetValue(_element)
                //    };
                //    colorPicker.ColorPicked += (_, e) => property.SetValue(_element, e.Color);
                //    _propertyLayout.Children.Add(colorPicker);
                //}
                //else if (property.PropertyType == typeof(string))
                //{
                //    _propertyLayout.Children.Add(CreateStringPicker(property));
                //}
                //else if (property.PropertyType == typeof(double) ||
                //    property.PropertyType == typeof(float) ||
                //    property.PropertyType == typeof(int))
                //{
                //    _propertyLayout.Children.Add(
                //        CreateValuePicker(property, bindableProperties.FirstOrDefault(p => p.PropertyName == property.Name)));
                //}
                //else if (property.PropertyType == typeof(bool))
                //{
                //    _propertyLayout.Children.Add(CreateBooleanPicker(property));
                //}
                //else if (property.PropertyType == typeof(Thickness))
                //{
                //    _propertyLayout.Children.Add(CreateThicknessPicker(property));
                //}
                //else
                //{
                //    //_propertyLayout.Children.Add(new Label { Text = $"//TODO: {property.Name} ({property.PropertyType})", TextColor = Color.Gray });
                //}
            }

        }

        View propertyControl;

        void Handle_SelectionChanged(object sender, Xamarin.Forms.SelectionChangedEventArgs e)
        {
            ActiveProperty = e.CurrentSelection[0] as PropertyInfo;
            PropertyToolbar.SetProperty(ActiveProperty.Name);
            
            if (ActiveProperty.PropertyType == typeof(Color))
            {
                propertyControl = new ColorPicker
                {
                    Color = (Color)ActiveProperty.GetValue(_element),
                    ElementInfo = ActiveProperty,
                    Element =  _element
                };
                

                
            }else if (ActiveProperty.PropertyType == typeof(string))
            {
                propertyControl = new TextEntry
                {
                    Element = _element,
                    ElementInfo = ActiveProperty
                };
            }else if (ActiveProperty.PropertyType == typeof(Thickness))
            {
                if (ActiveProperty.Name.ToLower() == "padding")
                {
                    propertyControl = new PaddingProperty
                    {
                        Element = _element,
                        ElementInfo = ActiveProperty
                    };
                }
            }
            
            propertyControl.TranslationX = this.Width;
            Grid.SetRow(propertyControl, 1);
            PropertyContainer.Children.Add(propertyControl);
            propertyControl.TranslateTo(0, 0);
        }

        Dictionary<string, (double min, double max)> _minMaxProperties = new Dictionary<string, (double min, double max)>
        {
            { ScaleProperty.PropertyName, (0d, 1d) },
            { ScaleXProperty.PropertyName, (0d, 1d) },
            { ScaleYProperty.PropertyName, (0d, 1d) },
            { OpacityProperty.PropertyName, (0d, 1d) },
            { RotationProperty.PropertyName, (0d, 360d) },
            { RotationXProperty.PropertyName, (0d, 360d) },
            { RotationYProperty.PropertyName, (0d, 360d) },
            { View.MarginProperty.PropertyName, (-100, 100) },
            { PaddingProperty.PropertyName, (-100, 100) },
        };

        Grid CreateValuePicker(PropertyInfo property, BindableProperty bindableProperty)
        {
            var min = 0d;
            var max = 100d;
            if (_minMaxProperties.ContainsKey(property.Name))
            {
                min = _minMaxProperties[property.Name].min;
                max = _minMaxProperties[property.Name].max;
            }

            var isInt = property.PropertyType == typeof(int);
            var value = isInt ? (int)property.GetValue(_element) : (double)property.GetValue(_element);
            var slider = new Slider(min, max, value);

            var actions = new Grid
            {
                Padding = 0,
                ColumnSpacing = 6,
                RowSpacing = 6,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = 40 }
                }
            };

            actions.AddChild(new Label { Text = property.Name, FontAttributes = FontAttributes.Bold }, 0, 0, 2);

            if (bindableProperty != null)
            {
                actions.AddChild(new Button
                {
                    Text = "X",
                    TextColor = Color.White,
                    BackgroundColor = Color.DarkRed,
                    WidthRequest = 28,
                    HeightRequest = 28,
                    Margin = 0,
                    Padding = 0,
                    Command = new Command(() => _element.ClearValue(bindableProperty))
                }, 1, 0);
            }

            var valueLabel = new Label
            {
                Text = slider.Value.ToString(isInt ? "0" : "0.#"),
                HorizontalOptions = LayoutOptions.End
            };

            slider.ValueChanged += (_, e) =>
            {
                if (isInt)
                    property.SetValue(_element, (int)e.NewValue);
                else
                    property.SetValue(_element, e.NewValue);
                valueLabel.Text = e.NewValue.ToString(isInt ? "0" : "0.#");
            };

            actions.AddChild(slider, 0, 1);
            actions.AddChild(valueLabel, 1, 1);

            return actions;
        }

        Grid CreateThicknessPicker(PropertyInfo property)
        {
            var grid = new Grid
            {
                Padding = 0,
                RowSpacing = 3,
                ColumnSpacing = 3,
                ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = 50 },
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = 30 }
                        },
            };
            grid.AddChild(new Label { Text = property.Name, FontAttributes = FontAttributes.Bold }, 0, 0, 2);

            var val = (Thickness)property.GetValue(_element);
            var sliders = new Slider[4];
            var valueLabels = new Label[4];
            for (int i = 0; i < 4; i++)
            {
                sliders[i] = new Slider
                {
                    VerticalOptions = LayoutOptions.Center,
                    Minimum = 0,
                    Maximum = 100
                };
                var row = i + 1;
                switch (i)
                {
                    case 0:
                        sliders[i].Value = val.Left;
                        grid.AddChild(new Label { Text = nameof(val.Left) }, 0, row);
                        break;
                    case 1:
                        sliders[i].Value = val.Top;
                        grid.AddChild(new Label { Text = nameof(val.Top) }, 0, row);
                        break;
                    case 2:
                        sliders[i].Value = val.Right;
                        grid.AddChild(new Label { Text = nameof(val.Right) }, 0, row);
                        break;
                    case 3:
                        sliders[i].Value = val.Bottom;
                        grid.AddChild(new Label { Text = nameof(val.Bottom) }, 0, row);
                        break;
                }

                valueLabels[i] = new Label { Text = sliders[i].Value.ToString("0") };
                grid.AddChild(sliders[i], 1, row);
                grid.AddChild(valueLabels[i], 2, row);
                sliders[i].ValueChanged += ThicknessChanged;
            }

            void ThicknessChanged(object sender, ValueChangedEventArgs e)
            {
                property.SetValue(_element, new Thickness(sliders[0].Value, sliders[1].Value, sliders[2].Value, sliders[3].Value));
                for (int i = 0; i < valueLabels.Length; i++)
                    valueLabels[i].Text = sliders[i].Value.ToString("0");
            }

            return grid;
        }

        Grid CreateBooleanPicker(PropertyInfo property)
        {
            var grid = new Grid
            {
                Padding = 0,
                ColumnSpacing = 6,
                RowSpacing = 6,
                ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = 50 }
                        }
            };
            grid.AddChild(new Label { Text = property.Name, FontAttributes = FontAttributes.Bold }, 0, 0);
            var boolSwitch = new Switch
            {
                IsToggled = (bool)property.GetValue(_element),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            boolSwitch.Toggled += (_, e) => property.SetValue(_element, e.Value);
            grid.AddChild(boolSwitch, 1, 0);
            _element.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == property.Name)
                {
                    var newVal = (bool)property.GetValue(_element);
                    if (newVal != boolSwitch.IsToggled)
                        boolSwitch.IsToggled = newVal;
                }
            };

            return grid;
        }

        Grid CreateStringPicker(PropertyInfo property)
        {
            var grid = new Grid
            {
                Padding = 0,
                ColumnSpacing = 6,
                RowSpacing = 6
            };
            grid.AddChild(new Label { Text = property.Name, FontAttributes = FontAttributes.Bold }, 0, 0);
            var entry = new Entry
            {
                Text = (string)property.GetValue(_element),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            entry.TextChanged += (_, e) => property.SetValue(_element, e.NewTextValue);
            grid.AddChild(entry, 0, 1);
            _element.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == property.Name)
                {
                    var newVal = (string)property.GetValue(_element);
                    if (newVal != entry.Text)
                        entry.Text = newVal;
                }
            };

            return grid;
        }

        class NamedAction
        {
            public string Name { get; set; }

            public Action<View> Action { get; set; }
        }

        (Func<View> ctor, NamedAction[] methods) GetPicker()
        {
            return (ctor: () =>
            {
                var picker = new Picker();
                picker.Items.Add("item 1");
                picker.Items.Add("item 2");
                return picker;
            }, methods: new[] {
                    new NamedAction {
                        Name = "Add item",
                        Action = (p) => (p as Picker).Items.Add("item")
                    },
                    new NamedAction {
                        Name = "Remove item last item",
                        Action = (p) => {
                            var picker = (Picker)p;
                            if (picker.Items.Count > 0)
                                picker.Items.RemoveAt(picker.Items.Count - 1);
                        }
                    },
                    new NamedAction {
                        Name = "Clear",
                        Action = (p) => (p as Picker).Items.Clear()
                    }
                }
            );
        }


        private void PropertyToolbar_OnBack(object sender, EventArgs e)
        {
            propertyControl.TranslateTo(this.Width, 0);
            PropertyContainer.Children.Remove(propertyControl);
        }

        private async void PropertyToolbar_OnViewSource(object sender, EventArgs e)
        {
            var source = XamlUtil.GetXamlForType(this.ControlTemplate.GetType());
            await Shell.Current.Navigation.PushAsync(new ViewSourcePage
            {
                Source = source
            });
        }
    }

    public static class GridExtension
    {
        public static void AddChild(this Grid grid, View view, int column, int row, int columnspan = 1, int rowspan = 1)
        {
            if (row < 0)
            {
                throw new ArgumentOutOfRangeException("row");
            }
            if (column < 0)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            if (rowspan <= 0)
            {
                throw new ArgumentOutOfRangeException("rowspan");
            }
            if (columnspan <= 0)
            {
                throw new ArgumentOutOfRangeException("columnspan");
            }
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            Grid.SetRow(view, row);
            Grid.SetRowSpan(view, rowspan);
            Grid.SetColumn(view, column);
            Grid.SetColumnSpan(view, columnspan);
            grid.Children.Add(view);
        }
    }
}
