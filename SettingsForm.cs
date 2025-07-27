using LazyControl.Models.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.ComboBox;

namespace LazyControl
{
    public partial class SettingsForm : Form
    {
        private AppSettings currentSettings;
        List<ScreenOptions> screenOptions = new List<ScreenOptions>
        {
            new ScreenOptions(1),
            new ScreenOptions(2)
        };

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            currentSettings = SettingsManager.LoadSettings();
            setUpScreenOptions();
            cbb_esc_f1.SelectedIndex = FindComboBoxIndex(cbb_esc_f1, currentSettings.EscF1);
            cbb_esc_f2.SelectedIndex = FindComboBoxIndex(cbb_esc_f2, currentSettings.EscF2);

        }

        private void btn_save_Click(object sender, EventArgs e)
        {

            currentSettings.EscF1 = (int)cbb_esc_f1.SelectedValue;
            currentSettings.EscF2 = (int)cbb_esc_f2.SelectedValue;

            var resultCheck = SettingsManager.checkValidSettings(currentSettings);
            if (resultCheck.valid == false)
            {
                MessageBox.Show(resultCheck.message);
                return;
            }

            SettingsManager.SaveSettings(currentSettings);

            MessageBox.Show("Saved");
        }

        public int FindComboBoxIndex(ComboBox comboBox, int valueToFind)
        {
            return comboBox.Items
                           .Cast<object>()
                           .Select((item, index) => new { item, index })
                           .FirstOrDefault(x => ((ScreenOptions)x.item).Value == valueToFind)?.index ?? -1;
        }

        private void setUpScreenOptions()
        {
            var bindingSource1 = new BindingSource();
            var bindingSource2 = new BindingSource();
            bindingSource1.DataSource = screenOptions;
            bindingSource2.DataSource = screenOptions;
            cbb_esc_f1.DataSource = bindingSource1;
            cbb_esc_f1.DisplayMember = "Label";
            cbb_esc_f1.ValueMember = "Value";
            cbb_esc_f1.DropDownStyle = ComboBoxStyle.DropDownList;

            cbb_esc_f2.DataSource = bindingSource2;
            cbb_esc_f2.DisplayMember = "Label";
            cbb_esc_f2.ValueMember = "Value";
            cbb_esc_f2.DropDownStyle = ComboBoxStyle.DropDownList;
        }
    }
}
