using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;

namespace Calculator
{
    
    public partial class Form1 : Form
    {
        // Font import See https://stackoverflow.com/questions/556147/how-to-quickly-and-easily-embed-fonts-in-winforms-app-in-c-sharp
            [System.Runtime.InteropServices.DllImport("gdi32.dll")]
            private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
        IntPtr pdv, [System.Runtime.InteropServices.In] ref uint pcFonts);
            private PrivateFontCollection fonts = new PrivateFontCollection();

        const int DisplayPlaces = 12;
        const float DisplayFontSize = 64.0F;

        Calculator calc = new Calculator(DisplayPlaces);
        public Form1()
        {
            InitializeComponent();
            InitializeDisplayFont();
        }

        private void InitializeDisplayFont()
        {
            // See https://stackoverflow.com/questions/556147/how-to-quickly-and-easily-embed-fonts-in-winforms-app-in-c-sharp
            // Freeware Font used http://www.styleseven.com/php/get_product.php?product=Digital-7
            byte[] fontData = Properties.Resources.Digital7Mono_B1g5;
            IntPtr fontPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontData.Length);
            System.Runtime.InteropServices.Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
            uint dummy = 0;
            fonts.AddMemoryFont(fontPtr, Properties.Resources.Digital7Mono_B1g5.Length);
            AddFontMemResourceEx(fontPtr, (uint)Properties.Resources.Digital7Mono_B1g5.Length, IntPtr.Zero, ref dummy);
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(fontPtr);

            txtDisplay.Font = new Font(fonts.Families[0], DisplayFontSize); 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            calc.Zero();
            UpdateDisplay(calc.ToString());
        }

        private void UpdateDisplay(string content)
        {
            txtDisplay.Text = content;
        }

        private void Btn1_Click(object sender, EventArgs e)
        {
            calc.DigitIn("1");
            UpdateDisplay(calc.ToString());
        }

        private void Btn2_Click(object sender, EventArgs e)
        {
            calc.DigitIn("2");
            UpdateDisplay(calc.ToString());
        }

        private void Btn3_Click(object sender, EventArgs e)
        {
            calc.DigitIn("3");
            UpdateDisplay(calc.ToString());
        }

        private void Btn4_Click(object sender, EventArgs e)
        {
            calc.DigitIn("4");
            UpdateDisplay(calc.ToString());
        }

        private void Btn5_Click(object sender, EventArgs e)
        {
            calc.DigitIn("5");
            UpdateDisplay(calc.ToString());
        }

        private void Btn6_Click(object sender, EventArgs e)
        {
            calc.DigitIn("6");
            UpdateDisplay(calc.ToString());
        }

        private void Btn7_Click(object sender, EventArgs e)
        {
            calc.DigitIn("7");
            UpdateDisplay(calc.ToString());
        }

        private void Btn8_Click(object sender, EventArgs e)
        {
            calc.DigitIn("8");
            UpdateDisplay(calc.ToString());
        }

        private void Btn9_Click(object sender, EventArgs e)
        {
            calc.DigitIn("9");
            UpdateDisplay(calc.ToString());
        }

        private void Btn0_Click(object sender, EventArgs e)
        {
            calc.DigitIn("0");
            UpdateDisplay(calc.ToString());
        }

        private void BtnSign_Click(object sender, EventArgs e)
        {
            calc.ChangeSign();
            UpdateDisplay(calc.ToString());
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            calc.Clear();
            UpdateDisplay(calc.ToString());
        }

        private void BtnPoint_Click(object sender, EventArgs e)
        {
            calc.Point();
            UpdateDisplay(calc.ToString());
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            calc.Add();
            UpdateDisplay(calc.ToString());
        }

        private void BtnSubtract_Click(object sender, EventArgs e)
        {
            calc.Subtract();
            UpdateDisplay(calc.ToString());
        }

        private void BtnMultiply_Click(object sender, EventArgs e)
        {
            calc.Multiply();
            UpdateDisplay(calc.ToString());
        }

        private void BtnDivide_Click(object sender, EventArgs e)
        {
            calc.Divide();
            UpdateDisplay(calc.ToString());
        }

        private void BtnEquals_Click(object sender, EventArgs e)
        {
            calc.Equals();
            UpdateDisplay(calc.ToString());

        }
    }
}
