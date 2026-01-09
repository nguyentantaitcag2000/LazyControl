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
        private Form1 mainForm;
        List<ScreenOptions> screenOptions = new List<ScreenOptions>
        {
            new ScreenOptions(1),
            new ScreenOptions(2)
        };

        List<ShortcutOption> keyOptions = new List<ShortcutOption>
        {
            new ShortcutOption { Label = "Ctrl+J", Value = Keys.Control | Keys.J },
            new ShortcutOption { Label = "Ctrl+K", Value = Keys.Control | Keys.K },
            new ShortcutOption { Label = "Ctrl+L", Value = Keys.Control | Keys.L }
        };
        public SettingsForm(Form1 mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            currentSettings = SettingsManager.LoadSettings();
            setUpScreenOptions();

            // Set giá trị cho ESC + F1/F2
            cbb_esc_f1.SelectedIndex = FindComboBoxIndex(cbb_esc_f1, currentSettings.EscF1);
            cbb_esc_f2.SelectedIndex = FindComboBoxIndex(cbb_esc_f2, currentSettings.EscF2);

            // Set giá trị cho Toggle Mouse Mode hotkey
            cbb_toggle_mouse_mode.SelectedIndex = FindToggleHotkeyIndex(currentSettings.ToggleMouseMode);
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            currentSettings.EscF1 = (int)cbb_esc_f1.SelectedValue;
            currentSettings.EscF2 = (int)cbb_esc_f2.SelectedValue;

            if (cbb_toggle_mouse_mode.SelectedItem is ShortcutOption selected)
            {
                currentSettings.ToggleMouseMode = selected.Value;
            }

            var resultCheck = SettingsManager.checkValidSettings(currentSettings);
            if (resultCheck.valid == false)
            {
                MessageBox.Show(resultCheck.message, "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SettingsManager.SaveSettings(currentSettings);

            // Reload settings trong main form để áp dụng hotkey mới
            mainForm?.ReloadSettings();

            MessageBox.Show("Settings saved successfully!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public int FindComboBoxIndex(ComboBox comboBox, int valueToFind)
        {
            return comboBox.Items
                           .Cast<object>()
                           .Select((item, index) => new { item, index })
                           .FirstOrDefault(x => ((ScreenOptions)x.item).Value == valueToFind)?.index ?? -1;
        }

        private int FindToggleHotkeyIndex(Keys keyToFind)
        {
            return keyOptions.FindIndex(x => x.Value == keyToFind);
        }

        private void setUpScreenOptions()
        {
            var bindingSource1 = new BindingSource();
            var bindingSource2 = new BindingSource();
            var bindingSource3 = new BindingSource();

            bindingSource1.DataSource = screenOptions;
            bindingSource2.DataSource = screenOptions;
            bindingSource3.DataSource = keyOptions;

            cbb_esc_f1.DataSource = bindingSource1;
            cbb_esc_f1.DisplayMember = "Label";
            cbb_esc_f1.ValueMember = "Value";
            cbb_esc_f1.DropDownStyle = ComboBoxStyle.DropDownList;

            cbb_esc_f2.DataSource = bindingSource2;
            cbb_esc_f2.DisplayMember = "Label";
            cbb_esc_f2.ValueMember = "Value";
            cbb_esc_f2.DropDownStyle = ComboBoxStyle.DropDownList;

            cbb_toggle_mouse_mode.DataSource = bindingSource3;
            cbb_toggle_mouse_mode.DisplayMember = "Label";
            cbb_toggle_mouse_mode.ValueMember = "Value";
            cbb_toggle_mouse_mode.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void btn_uninstall_app_Click(object sender, EventArgs e)
        {
            AppInstaller.Uninstall();
        }
    }
}
