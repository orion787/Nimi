using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nimi.UI
{
    partial class CardForm
    {
        /// <summary>
        /// Обязательная для WinForms: инициализирует базовую форму.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // CardForm
            // 
            this.ClientSize = new Size(800, 600);
            this.Name = "CardForm";
            this.Text = "Учёт партнёров";
            this.ResumeLayout(false);
        }
    }
}
