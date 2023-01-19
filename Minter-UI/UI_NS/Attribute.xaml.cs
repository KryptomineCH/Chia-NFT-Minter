using Chia_Metadata;
using Chia_NFT_Minter.CollectionInformation_ns;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Minter_UI.UI_NS
{
    /// <summary>
    /// Attribute corresponds with Chia-NFT-Minter.MetadataAttribute
    /// </summary>
    public partial class Attribute : UserControl
    {
        public Attribute(MetadataAttribute? attr = null)
        {
            InitializeComponent();
            LoadAvailableAttributes();
            if (attr != null)
            {
                SetAttribute(attr);
            }
        }
        public MetadataAttribute GetAttribute()
        {
            MetadataAttribute attribute = new MetadataAttribute(this.TraitType_ComboBox.Text, this.Value_ComboBox.Text);
            int min;
            if (int.TryParse(this.MinValue_TextBox.Text, out min))
            {
                attribute.min_value = min;
            }
            int max;
            if (int.TryParse(this.MaxValue_TextBox.Text, out max))
            {
                attribute.max_value = max;
            }
            return attribute;
        }
        public void SetAttribute(MetadataAttribute attribute)
        {
            if (attribute == null) return;
            if (attribute.trait_type != null)
                this.TraitType_ComboBox.Text = new String(attribute.trait_type);
            if (attribute.@value != null)
            this.Value_ComboBox.Text = attribute.@value.ToString();
            if (attribute.min_value != null)
            this.MinValue_TextBox.Text = attribute.min_value.ToString();
            if (attribute.max_value != null)
                this.MaxValue_TextBox.Text = attribute.max_value.ToString();
        }
        /// <summary>
        /// loads available attributes into combobox suggestions
        /// </summary>
        private void LoadAvailableAttributes()
        {
            List<string> values = new List<string>();
            foreach(MetadataAttribute meta in CollectionInformation.Information.AllMetadataAttributes.Values)
            {
                values.Add(new String(meta.trait_type));
            }
            this.TraitType_ComboBox.ItemsSource = values;
        }
        /// <summary>
        /// removes this attribute from the parent's collection (usually stackpanel or wrappanel)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            Delete();
        }
        public void Delete()
        {
            // unregister events
            this.TraitType_ComboBox.SelectionChanged -= this.TraitType_ComboBox_SelectionChanged;
            this.Delete_Button.Click -= this.Delete_Button_Click;
            // main grid
            this.Main_Grid.Children.Clear();
            this.Main_Grid = null;
            // stack panel
            this.Stack_Panel.Children.Clear();
            this.Stack_Panel = null;
            // textboxes
            this.MinValue_TextBox.Text = null;
            this.MinValue_TextBox = null;
            this.MaxValue_TextBox.Text= null;
            this.MaxValue_TextBox = null;
            //comboboxes
            this.TraitType_ComboBox.ItemsSource= null;
            this.TraitType_ComboBox.SelectedItem= null;
            this.TraitType_ComboBox.Text= null;
            this.TraitType_ComboBox = null;
            this.Value_ComboBox.ItemsSource = null;
            this.Value_ComboBox.SelectedItem = null;
            this.Value_ComboBox.Text = null;
            this.Value_ComboBox = null;
            // delete button
            this.Delete_Button.Content = null;
            this.Delete_Button = null;
            // clear all x:name properties
            this.UnregisterName("Stack_Panel");
            this.UnregisterName("MinValue_TextBox");
            this.UnregisterName("MaxValue_TextBox");
            this.UnregisterName("TraitType_ComboBox");
            this.UnregisterName("Value_ComboBox");
            this.UnregisterName("Main_Grid");
            this.UnregisterName("Delete_Button");
            // remove self from parent
            ((Panel)this.Parent).Children.Remove(this);
        }
        /// <summary>
        /// resets min and max vailue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TraitType_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e == null || e.AddedItems == null || e.AddedItems.Count == 0) return;
            var test = e.AddedItems[0];
            if (test == null) return;
            string key = new String(test.ToString());
            if (CollectionInformation.Information.AllMetadataAttributes.ContainsKey(key))
            {
                this.MinValue_TextBox.Text = new String(CollectionInformation.Information.AllMetadataAttributes[key].min_value.ToString());
                this.MaxValue_TextBox.Text = new String(CollectionInformation.Information.AllMetadataAttributes[key].max_value.ToString());
                this.Value_ComboBox.ItemsSource = CollectionInformation.Information.AllMetadataAttributeValues[key].ToArray();
            }
        }
    }
}
