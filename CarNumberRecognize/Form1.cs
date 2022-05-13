using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace CarNumberRecognize
{
    public partial class Form1 : Form
    {
        private NumberPlateRecognizer plateRecognizer;
        private Point startPoint;
        private Mat inputImage;
        public Form1() {
            InitializeComponent();
        }
        void ProcessImage(IInputOutputArray image) {
            List<IInputOutputArray> licenseImagePlateList = new List<IInputOutputArray>();
            List<IInputOutputArray> filteredLicensePlateImageList = new List<IInputOutputArray>();
            List<RotatedRect> licenseBoxList = new List<RotatedRect>();
            List<string> recognizedPlates = plateRecognizer.DetectLicensePlates(image,
                licenseImagePlateList, filteredLicensePlateImageList, licenseBoxList);
            panel1.Controls.Clear();
            startPoint = new Point(10, 10);
            for (int i = 0; i < recognizedPlates.Count; i++) {
                Mat dest = new Mat();
                CvInvoke.VConcat(licenseImagePlateList[i], filteredLicensePlateImageList[i], dest);
                AddLabelAndImage($"Номер: {recognizedPlates[i]}", dest);
            }
            Image<Bgr, byte> outputImage = inputImage.ToImage<Bgr, byte>();
            foreach(RotatedRect rect in licenseBoxList) {
                PointF[] v = rect.GetVertices();
                PointF prevPoint = v[0];
                PointF firstPoint = prevPoint;
                PointF nextPoint = prevPoint;
                PointF lastPoint = nextPoint;
                for(int i = 0; i < v.Length; i++) {
                    nextPoint = v[i];
                    CvInvoke.Line(outputImage, Point.Round(prevPoint), Point.Round(nextPoint), new MCvScalar(0, 0, 255), 5,
                        LineType.EightConnected, 0);
                    prevPoint = nextPoint;
                    lastPoint = prevPoint;

                }
                CvInvoke.Line(outputImage, Point.Round(lastPoint), Point.Round(firstPoint), new MCvScalar(0, 0, 255), 5,
                    LineType.EightConnected, 0);
            }
        }
        void AddLabelAndImage(string labelText, IInputArray image) {
            Label label = new Label();
            label.Text = labelText;
            label.Width = 100;
            label.Height = 40;
            label.Location = startPoint;
            startPoint.Y += label.Height;
            panel1.Controls.Add(label);
            PictureBox box = new PictureBox();
            Mat m = image.GetInputArray().GetMat();
            box.ClientSize = m.Size;
            box.Image = m.Bitmap;
            box.Location = startPoint;
            startPoint.Y += box.Height + 10;
            panel1.Controls.Add(box);
        }

        private void открытьКартинкуToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                if(openFileDialog1.ShowDialog() == DialogResult.OK) {
                    pictureBox1.Image = Image.FromFile(openFileDialog1.FileName);
                }
            }
            catch(Exception ex) {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            plateRecognizer = new NumberPlateRecognizer(@"E:\CarNumberRecognize\CarNumberRecognize\testdata", "rus");
        }

        private void toolStripButton1_Click(object sender, EventArgs e) {
            inputImage = new Mat(openFileDialog1.FileName);
            UMat um = inputImage.GetUMat(AccessType.ReadWrite);
            ProcessImage(um);
        }
    }
}
