using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MandelBrot
{
    public abstract class LabeledInputBox
    {
        public static int OFFSET, LABEL_WIDTH;
        private static Label PREVIOUS;
        public readonly Label label;

        public LabeledInputBox(MandelbrotForm form, string name, string text)
        {
            label = new Label();
            label.Name = "label_" + name;
            label.Text = text;
            label.TextAlign = ContentAlignment.MiddleRight;
            label.Location = new Point(OFFSET, (PREVIOUS?.Bottom ?? 0) + OFFSET);
            label.Width = LABEL_WIDTH;

            PREVIOUS = label;

            form.Controls.Add(label);
        }
    }

    public abstract class BaseInputBox<B, R> : LabeledInputBox where B : Control, new() where R : struct
    {
        public readonly B box;
        public virtual R Value { get; set; }

        public BaseInputBox(MandelbrotForm form, string name, string text, R value)
            :base(form, name, text)
        {
            box = new B();
            box.Location = new Point(OFFSET + LABEL_WIDTH, label.Top + 2);
            box.Name = "input_" + name;

            label.Height = box.Height;

            Value = value;

            form.Controls.Add(label);
            form.Controls.Add(box);
        }
    }

    public class InputBox<R> : BaseInputBox<TextBox, R> where R : struct
    {
        public override R Value
        {
            get => base.Value;
            set
            {
                base.Value = value;
                box.Text = value.ToString();
            }
        }

        public InputBox(MandelbrotForm form, string name, string text, R value)
            : base(form, name, text, value)
        {
            box.LostFocus += (o, e) => { Parse(); form.Draw(); };
        }

        private void Parse()
        {
            box.Text = box.Text.Replace(',', '.');

            try
            {
                Value = (R)Convert.ChangeType(box.Text, typeof(R));
                box.BackColor = Color.White;
            }
            catch (FormatException)
            {
                box.BackColor = Color.Red;
            }
        }
    }

    public class BoolBox : BaseInputBox<Button, bool>
    {
        public override bool Value
        {
            get => base.Value;
            set
            {
                if (value)
                {
                    box.Text = "On";
                    box.BackColor = Color.Green;
                    base.Value = true;
                }
                else
                {
                    box.Text = "Off";
                    box.BackColor = Color.Red;
                    base.Value = false;
                }
            }
        }
        public BoolBox(MandelbrotForm form, string name, string text, bool value)
            : base(form, name, text, value)
        {
            box.Click += (o, e) => Value = !Value;
        }
    }

    public class ColorBox : BaseInputBox<ComboBox, Color>
    {
        public ColorBox(MandelbrotForm form, string name, string text, KnownColor value)
            : base(form, name, text, Color.Empty)
        {
            box.DataSource = ((KnownColor[])Enum.GetValues(typeof(KnownColor))).Skip((int)KnownColor.AliceBlue - 1).Take((int)KnownColor.YellowGreen - (int)KnownColor.AliceBlue + 1).ToArray();
            box.SelectedIndexChanged += (o, e) =>
            {
                Value = Color.FromKnownColor((KnownColor)box.SelectedItem);
                form.Draw();
            };
            form.Shown += (o, e) => box.SelectedIndex = (int)value - (int)KnownColor.AliceBlue;
        }
    }
}
