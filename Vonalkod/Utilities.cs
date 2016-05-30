using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vonalkod
{
    public class Utilities
    {
        public static void ResetAllControls(Control control)
        {
            foreach (Control c in control.Controls)
            {
                if (c is TextBox)
                {
                    ((TextBox)c).Text = null;
                }

                if (c is ComboBox)
                {
                    ((ComboBox)c).SelectedIndex = -1;
                }

                if (c is TreeView)
                {
                    ((TreeView) c).Nodes.Clear();
                }

                if (control is CheckBox)
                {
                    ((CheckBox) c).Checked = false;
                }

                if (control is ListBox)
                {
                    ((ListBox)c).ClearSelected();
                }

                if (c.HasChildren)
                    ResetAllControls(c);
            }
        }
    }
}
