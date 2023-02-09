using Chia_Metadata;
using Minter_UI.CollectionInformation_ns;
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
        /// <summary>
        /// generates a new attribute control and tries to load attribute suggestions
        /// </summary>
        /// <param name="usedAttributes">an observable collection which contains the attributes which are in use</param>
        /// <param name="attr">the attribute which should be loaded</param>
        public Attribute(SelectedAttributes? usedAttributes, MetadataAttribute? attr = null)
        {
            // set event listener if the observable collection has been modified
            _usedAttributes = usedAttributes;
            if (_usedAttributes != null)
            {
                _usedAttributes.AttributelistChanged += (s, e) => LoadAvailableAttributes();
            }
            // init
            InitializeComponent();
            // load attribute suggestions
            LoadAvailableAttributes();
            // set attribute values from passed attribute
            if (attr != null)
            {
                SetAttribute(attr);
            }
        }
        /// <summary>
        /// an observable collection which contains the attributes which are in use
        /// </summary>
        SelectedAttributes? _usedAttributes = new SelectedAttributes();
        /// <summary>
        /// converts the user control content into a Metadata Attribute element from Chia_Metadata
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// sets the values of the control from a Chia_Metadata.MetadataAttribute
        /// </summary>
        /// <param name="attribute"></param>
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
            if (TraitType_ComboBox == null)
            {
                return;
            }
            // save te currently selected item so that the selection can be restored
            string selectedItem = "";
            string customText = TraitType_ComboBox.Text;
            if (TraitType_ComboBox.SelectedItem != null)
            {
                selectedItem = (string)TraitType_ComboBox.SelectedItem;
            }
           
            // update combobox items source
            List<string> values = new List<string>();
            foreach(MetadataAttribute meta in CollectionInformation.Information.AllMetadataAttributes.Values)
            {
                if (_usedAttributes == null || !_usedAttributes.ContainsAttribute(meta.trait_type) ||
                    ( meta.trait_type == selectedItem || (meta.trait_type == customText && selectedItem == "")))
                {
                    values.Add(new String(meta.trait_type));
                }
            }
            this.TraitType_ComboBox.ItemsSource = values;
            // try to restore previous value
            TraitType_ComboBox.Text = customText;
            if (TraitType_ComboBox.Items.Contains(selectedItem))
            {
                TraitType_ComboBox.SelectedItem = selectedItem;
            }
            OnAttributeChanged(EventArgs.Empty);
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
        /// <summary>
        /// removes this control from the parent and set everything to null in order to allor garbage collection because of a bug
        /// </summary>
        public void Delete()
        {
            // unregister event firt to avoid unintended process starts
            if (_usedAttributes != null)
            {
                _usedAttributes.AttributelistChanged -= (s, e) => LoadAvailableAttributes();
                _usedAttributes.RemoveAttribute(this.TraitType_ComboBox.Text);
            }
            ((Panel)this.Parent).Children.Remove(this);
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
            
        }
        /// <summary>
        /// resets min and max vailue <br/>
        /// also updates the UsedAttributes observable collection
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
            if (key != _previousValue)
            {
                if (_usedAttributes != null)
                {
                    _usedAttributes.RemoveAttribute(_previousValue);
                    _usedAttributes.AddAttribute(key);
                }
                _previousValue = key;
            }
            
        }
        /// <summary>
        /// requised to prevent event fires if the attribute did not change but the selection changed
        /// </summary>
        private string _previousValue = "";
        /// <summary>
        /// fire changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TraitType_ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnAttributeChanged(EventArgs.Empty);
        }
        /// <summary>
        /// fire changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Value_ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnAttributeChanged(EventArgs.Empty);
        }
        /// <summary>
        /// fire changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinValue_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnAttributeChanged(EventArgs.Empty);
        }
        /// <summary>
        /// fire changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaxValue_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnAttributeChanged(EventArgs.Empty);
        }
        /// <summary>
        /// event handler for when the attribute fields changed <br/>
        /// used for the filters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void EventHandler(object sender, EventArgs e);
        /// <summary>
        /// event handler for when the attribute fields changed <br/>
        /// used for the filters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public event EventHandler AttributeChanged;
        /// <summary>
        /// unsubscribe the attribute changed event from the parent control
        /// </summary>
        /// <remarks>
        /// This is required because the AttributeControl removes itself from the parent not vice versa
        /// </remarks>
        public void UnsubscribeFromParentControl()
        {
            if (this.Parent is IParentControl parentControl)
            {
                parentControl.AttributeChanged -= ParentControl_AttributeChanged;
            }
        }
        /// <summary>
        /// this function is nessesary for some magic shit going on and code could be added to it.
        /// However, dont remove it. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParentControl_AttributeChanged(object sender, EventArgs e)
        {
            // Event handling code
        }
        /// <summary>
        /// raises the event
        /// </summary>
        /// <param name="e"></param>
        protected void OnAttributeChanged(EventArgs e)
        {
            if (AttributeChanged != null)
            {
                AttributeChanged(this, e);
            }
        }
    }
    /// <summary>
    /// thir class is beeing used to keep track of the used attributes and to not suggest duplicates
    /// </summary>
    public class SelectedAttributes
    {
        /// <summary>
        /// a dictionary with the attributes which have been selected and how often they have been selected
        /// </summary>
        private  Dictionary<string, int> _selectedAttributes = new Dictionary<string, int>();
        /// <summary>
        /// event for when the collection has been modified. This is important tu update the sugestions of the other subscribed attributes
        /// </summary>
        public event EventHandler AttributelistChanged;
        /// <summary>
        /// a simple check wether an attribute is included already or not
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public  bool ContainsAttribute(string attribute)
        {
            return _selectedAttributes.ContainsKey(attribute);
        }

        /// <summary>
        /// adds an attribute to the dictionary of used attributes.
        /// </summary>
        /// <remarks>
        /// keeps track of the attribute count and fires the changedevent
        /// </remarks>
        /// <param name="attribute"></param>
        public  void AddAttribute(string attribute)
        {
            if (_selectedAttributes.ContainsKey(attribute))
            {
                _selectedAttributes[attribute]++;
            }
            else
            {
                _selectedAttributes.Add(attribute, 1);
                AttributelistChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// romoves an attribute from the used list
        /// </summary>
        /// <remarks>
        /// if an attribute is used more than once, its usage count is decreased instead.
        /// </remarks>
        /// <param name="attribute"></param>
        public  void RemoveAttribute(string attribute)
        {
            if (_selectedAttributes.ContainsKey(attribute))
            {
                _selectedAttributes[attribute]--;
                if (_selectedAttributes[attribute] <= 0)
                {
                    _selectedAttributes.Remove(attribute);
                    AttributelistChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
    /// <summary>
    /// parent control interface which specifies wether the parent should be informed about attribute changes or not (for example to update filters)
    /// </summary>
    public interface IParentControl
    {
        event EventHandler AttributeChanged;
    }
}
