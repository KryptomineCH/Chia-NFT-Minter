using Chia_Metadata;
using Chia_NFT_Minter;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Minter_UI
{
    /// <summary>
    /// Interaction logic for Attribute.xaml
    /// </summary>
    public partial class Attribute : UserControl
    {
        public Attribute(MetadataAttribute attr = null)
        {
            InitializeComponent();
            LoadAvailableAttributes();
            if (attr != null)
            {
                Value = attr;
            }
        }
        public MetadataAttribute Value
        {
            get { 
                MetadataAttribute attribute = new MetadataAttribute(this.TraitType_ComboBox.Text,this.Value_ComboBox.Text);
                int min;
                if (int.TryParse(this.MinValue_TextBox.Text,out min))
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
            set
            {
                this.TraitType_ComboBox.Text = value.trait_type;
                this.Value_ComboBox.Text = value.@value.ToString();
                this.MinValue_TextBox.Text = value.min_value.ToString();
                this.MaxValue_TextBox.Text = value.max_value.ToString();
            }
        }
        private void LoadAvailableAttributes()
        {
            List<string> values = new List<string>();
            foreach(MetadataAttribute meta in CollectionInformation.AllMetadataAttributes.Values)
            {
                values.Add(meta.trait_type);
            }
            this.TraitType_ComboBox.ItemsSource = values;
        }
        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            ((Panel)this.Parent).Children.Remove(this);
        }

        private void TraitType_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.MinValue_TextBox.Text = CollectionInformation.AllMetadataAttributes[e.AddedItems[0].ToString()].min_value.ToString();
            this.MaxValue_TextBox.Text = CollectionInformation.AllMetadataAttributes[e.AddedItems[0].ToString()].max_value.ToString();
        }
    }
}
