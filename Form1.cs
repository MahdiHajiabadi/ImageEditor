using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;


namespace ImageEditor
{
    public partial class Form1 : Form
    {
        Image<Bgr, Byte> My_Image;
        Image<Bgr, Byte> My_image_copy;
        Image<Gray, Byte> gray_image;
        bool gray_in_use = false;
        public Form1()
        {
            InitializeComponent();
        }

        private Image Img;
        private Size OriginalImageSize;
        private Size ModifiedImageSize;

        int cropX;
        int cropY;
        int cropWidth;
        int count = 0;
        int current_count = 0;
        float pX = -1;
        float pY = -1;
        int cropHeight;
        public Pen cropPen, cropPaint;
        public DashStyle cropDashStyle = DashStyle.DashDot;
        public bool Makeselection = false;
        public bool MakePaint = false;
        public bool CreateText = false;
        Bitmap bmp = new Bitmap(3000, 3000);
        Graphics g;
        PictureBox[] Shapes = new PictureBox[10];
        ToolStripMenuItem[] items = new ToolStripMenuItem[10];
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog Dlg = new OpenFileDialog();
            Dlg.Filter = "";
            // string str= Dlg.ToString();
            Dlg.Title = "Select image";
            if (Dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Img = Image.FromFile(Dlg.FileName);
                //Image.FromFile(String) method creates an image from the specifed file, here dlg.Filename contains the name of the file from which to create the image
                LoadImage(PictureBox1);
            }

        }
        private void LoadImage(PictureBox pictureBox1)
        {
            //we set the picturebox size according to image, we can get image width and height with the help of Image.Width and Image.height properties.
            int imgWidth = Img.Width;
            int imghieght = Img.Height;
            Shapes[current_count] = new PictureBox();
            Shapes[current_count].Name = "PictureBox" + count.ToString();
            Shapes[current_count].Width = imgWidth;
            Shapes[current_count].Height = imghieght;
            Shapes[current_count].Image = Img;
            if (current_count <= count)
            {
                current_count++;
            }
            PictureBox1.Width = imgWidth;
            PictureBox1.Height = imghieght;
            PictureBox1.Image = Img;
            pictureBox1.Visible = true;
            PictureBoxLocation();
            OriginalImageSize = new Size(imgWidth, imghieght);
            SetResizeInfo();
        }
        private void PictureBoxLocation()
        {
            int _x = 0;
            int _y = 0;
            if (SplitContainer1.Panel1.Width > PictureBox1.Width)
            {
                _x = (SplitContainer1.Panel1.Width - PictureBox1.Width) / 2;
            }
            if (SplitContainer1.Panel1.Height > PictureBox1.Height)
            {
                _y = (SplitContainer1.Panel1.Height - PictureBox1.Height) / 2;
            }
            PictureBox1.Location = new Point(_x, _y);
        }

        private void SetResizeInfo()
        {

            lbloriginalSize.Text = OriginalImageSize.ToString();
            lblModifiedSize.Text = ModifiedImageSize.ToString();

        }

        private void SplitContainer1_Panel1_Resize(object sender, EventArgs e)
        {
            PictureBoxLocation();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Bitmap bm_source = new Bitmap(PictureBox1.Image);
            // Make a bitmap for the result.
            Bitmap bm_dest = new Bitmap(Convert.ToInt32(ModifiedImageSize.Width), Convert.ToInt32(ModifiedImageSize.Height));
            // Make a Graphics object for the result Bitmap.
            Graphics gr_dest = Graphics.FromImage(bm_dest);
            // Copy the source image into the destination bitmap.
            gr_dest.DrawImage(bm_source, 0, 0, bm_dest.Width + 1, bm_dest.Height + 1);
            // Display the result.
            PictureBox1.Image = bm_dest;
            PictureBox1.Width = bm_dest.Width;
            PictureBox1.Height = bm_dest.Height;
            PictureBoxLocation();

        }

        private void DomainUpDown1_SelectedItemChanged(object sender, EventArgs e)
        {
            int percentage = 0;
            try
            {
                percentage = Convert.ToInt32(DomainUpDown1.Text);
                ModifiedImageSize = new Size((OriginalImageSize.Width * percentage) / 100, (OriginalImageSize.Height * percentage) / 100);
                SetResizeInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid Percentage");
                return;
            }

        }
        private void BindDomainIUpDown()
        {
            for (int i = 1; i <= 999; i++)
            {
                DomainUpDown1.Items.Add(i);
            }
            DomainUpDown1.Text = "100";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BindDomainIUpDown();
        }

        # region "-----------------------------Crop Image------------------------------------"



        private void btnMakeSelection_Click(object sender, EventArgs e)
        {
            Makeselection = true;
            btnCrop.Enabled = true;

        }

        private void btnCrop_Click(object sender, EventArgs e)
        {
            Rectangle rect;
            Cursor = Cursors.Default;

            try
            {
                /*if (cropWidth < 1)
                {
                    return;
                }*/
                if (cropWidth < 0)
                {
                    rect = new Rectangle(cropX + cropWidth, cropY, Math.Abs(cropWidth), cropHeight);
                    cropWidth = -1 * cropWidth;
                }
                else
                    rect = new Rectangle(cropX, cropY, cropWidth, cropHeight);

                //Rectangle rect = new Rectangle(cropX, cropY, cropWidth, cropHeight);
                //First we define a rectangle with the help of already calculated points
                Bitmap OriginalImage = new Bitmap(PictureBox1.Image, PictureBox1.Width, PictureBox1.Height);
                //Original image
                Bitmap _img = new Bitmap(cropWidth, cropHeight);
                // for cropinf image
                Graphics g = Graphics.FromImage(_img);
                // create graphics
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                //set image attributes
                g.DrawImage(OriginalImage, 0, 0, rect, GraphicsUnit.Pixel);

                PictureBox1.Image = _img;
                PictureBox1.Width = _img.Width;
                PictureBox1.Height = _img.Height;
                PictureBoxLocation();
                btnCrop.Enabled = false;
            }
            catch (Exception ex)
            {
            }
        }


        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (Makeselection && btnCrop.Enabled == false)
                {
                    PictureBox1.ContextMenuStrip = contextMenuStrip1;
                }

            }
            if (MakePaint == true)
            {
                // Make_Paint.Enabled = true;
                pX = e.X;
                pY = e.Y;
            }
            if (TabControl1.SelectedIndex == 4)
            {
                Point TextStartLocation = e.Location;
                if (CreateText)
                {
                    Cursor = Cursors.IBeam;
                }
            }
            else
            {
                Cursor = Cursors.Default;
                if (Makeselection)
                {

                    try
                    {
                        if (e.Button == System.Windows.Forms.MouseButtons.Left)
                        {
                            Cursor = Cursors.Cross;
                            cropX = e.X;
                            cropY = e.Y;

                            cropPen = new Pen(Color.Red, 1);
                            cropPen.DashStyle = DashStyle.Solid;


                        }
                        PictureBox1.Refresh();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }


        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (MakePaint == true)
            {
                MakePaint = false;
                //    PictureBox1.Image = bmp;
                //Make_Paint.Enabled = false;
            }

            if (Makeselection)
            {
                Cursor = Cursors.Default;
            }

        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            XPosition.Text = "X:" + e.X.ToString();
            YPosition.Text = "Y:" + e.Y.ToString();
            if (MakePaint == true)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    cropPaint = new Pen(Color.Red, 1);
                    cropPaint.DashStyle = DashStyle.Solid;
                    PictureBox1.CreateGraphics().DrawLine(cropPaint, pX, pY, e.X, e.Y);
                    g = Graphics.FromImage(bmp);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.DrawLine(cropPaint, pX, pY, e.X, e.Y);
                    pX = e.X;
                    pY = e.Y;
                }
            }


            if (TabControl1.SelectedIndex == 4)
            {
                Point TextStartLocation = e.Location;
                if (CreateText)
                {
                    Cursor = Cursors.IBeam;
                }
            }
            else
            {
                Cursor = Cursors.Default;
                if (Makeselection)
                {

                    try
                    {
                        if (PictureBox1.Image == null)
                            return;


                        if (e.Button == System.Windows.Forms.MouseButtons.Left)
                        {
                            PictureBox1.Refresh();
                            cropWidth = e.X - cropX;
                            cropHeight = e.Y - cropY;
                            if (cropWidth < 0)
                                PictureBox1.CreateGraphics().DrawRectangle(cropPen, e.X, cropY, Math.Abs(cropWidth), cropHeight);
                            else
                                PictureBox1.CreateGraphics().DrawRectangle(cropPen, cropX, cropY, cropWidth, cropHeight);
                        }



                    }
                    catch (Exception ex)
                    {
                        //if (ex.Number == 5)
                        //    return;
                    }
                }
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {

        }
        # endregion

        private void TrackBarBrightness_Scroll(object sender, EventArgs e)
        {
            DomainUpDownBrightness.Text = TrackBarBrightness.Value.ToString();


            float value = TrackBarBrightness.Value * 0.01f;
            float[][] colorMatrixElements = {
	new float[] {
		1,
		0,
		0,
		0,
		0
	},
	new float[] {
		0,
		1,
		0,
		0,
		0
	},
	new float[] {
		0,
		0,
		1,
		0,
		0
	},
	new float[] {
		0,
		0,
		0,
		1,
		0
	},
	new float[] {
		value,
		value,
		value,
		0,
		1
	}
};
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            ImageAttributes imageAttributes = new ImageAttributes();


            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);



            Image _img = Img;
            //PictureBox1.Image
            Graphics _g = default(Graphics);
            Bitmap bm_dest = new Bitmap(Convert.ToInt32(_img.Width), Convert.ToInt32(_img.Height));
            _g = Graphics.FromImage(bm_dest);
            _g.DrawImage(_img, new Rectangle(0, 0, bm_dest.Width + 1, bm_dest.Height + 1), 0, 0, bm_dest.Width + 1, bm_dest.Height + 1, GraphicsUnit.Pixel, imageAttributes);
            PictureBox1.Image = bm_dest;

        }

        private void btnRotateLeft_Click(object sender, EventArgs e)
        {
            PictureBox1.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            PictureBox1.Refresh();
        }

        private void btnRotateRight_Click(object sender, EventArgs e)
        {
            PictureBox1.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            PictureBox1.Refresh();
        }

        private void btnRotateHorizantal_Click(object sender, EventArgs e)
        {
            PictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipX);
            PictureBox1.Refresh();
        }

        private void btnRotatevertical_Click(object sender, EventArgs e)
        {
            PictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipY);
            PictureBox1.Refresh();
        }

        private void Label7_Click(object sender, EventArgs e)
        {

        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PictureBox1.Image = Img;
            PictureBox1.Width = Img.Width;
            PictureBox1.Height = Img.Height;
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            count++;
            PictureBox1.Visible = false;
            items[count] = new ToolStripMenuItem();
            items[count].Name = "tab" + (count).ToString();
            items[count].Visible = true;
            menuStrip2.Items.Add("Tab" + (count).ToString());
            menuStrip2.Items[count].Tag = (count + 1).ToString();
            menuStrip2.Items[count].Text = "Tab" + (count + 1).ToString();
        }



        private void menuStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var clickedMenuItem = sender as MenuItem;
            ToolStripMenuItem str = (ToolStripMenuItem)(e.ClickedItem);
            string idx = (string)str.Tag;
            int index = Int32.Parse(idx);
            //  MessageBox.Show(index.ToString());
            if (index > current_count)
            {
                PictureBox1.Visible = false;
                return;
            }
            else
            {
                PictureBox1.Visible = true;
                PictureBox1.Height = Shapes[index - 1].Height;
                PictureBox1.Width = Shapes[index - 1].Width;
                PictureBox1.Image = Shapes[index - 1].Image;
            }
        }
        private void Make_Paint_Click(object sender, EventArgs e)
        {
            MakePaint = true;

        }

        private void Layer_Show_Click(object sender, EventArgs e)
        {
            PictureBox1.Image = bmp;

        }

        private void importToNewWorkspaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            count++;
            items[count] = new ToolStripMenuItem();
            items[count].Name = "tab" + (count).ToString();
            items[count].Visible = true;
            menuStrip2.Items.Add("Tab" + (count).ToString());
            menuStrip2.Items[count].Tag = (count + 1).ToString();
            menuStrip2.Items[count].Text = "Tab" + (count + 1).ToString();
            Shapes[current_count] = new PictureBox();
            Shapes[current_count].Width = PictureBox1.Width;
            Shapes[current_count].Height = PictureBox1.Height;
            Shapes[current_count].Image = PictureBox1.Image;
            current_count++;
            Makeselection = false;

        }

        private void Merge_Click(object sender, EventArgs e)
        {
            Bitmap first = new Bitmap(Shapes[0].Image);
            Bitmap second = new Bitmap(PictureBox1.Image);
            Bitmap result = new Bitmap(first.Width, first.Height);
            Graphics g = Graphics.FromImage(result);
            g.DrawImageUnscaled(first, 0, 0);
            g.Flush();
            g.DrawImageUnscaled(second, 0, 0);
            g.Flush();
            PictureBox1.Image = result;
        }

        private void btn_hist_Click(object sender, EventArgs e)
        {
            float[] BlueHist;
            float[] GreenHist;
            float[] RedHist;
            Bitmap image = (Bitmap)PictureBox1.Image;
            Image<Bgr, Byte> img = new Image<Bgr, byte>(image);

            DenseHistogram Histo = new DenseHistogram(255, new RangeF(0, 255));

            Image<Gray, Byte> img2Blue = img[0];
            Image<Gray, Byte> img2Green = img[1];
            Image<Gray, Byte> img2Red = img[2];


            Histo.Calculate(new Image<Gray, Byte>[] { img2Blue }, true, null);
            //The data is here
            //Histo.MatND.ManagedArray
            BlueHist = new float[256];
            Histo.MatND.ManagedArray.CopyTo(BlueHist, 0);

            Histo.Clear();

            Histo.Calculate(new Image<Gray, Byte>[] { img2Green }, true, null);
            GreenHist = new float[256];
            Histo.MatND.ManagedArray.CopyTo(GreenHist, 0);

            Histo.Clear();

            Histo.Calculate(new Image<Gray, Byte>[] { img2Red }, true, null);
            RedHist = new float[256];
            Histo.MatND.ManagedArray.CopyTo(RedHist, 0);
            histogramBox1.ClearHistogram();
            histogramBox2.ClearHistogram();
            histogramBox3.ClearHistogram();
            histogramBox1.GenerateHistograms(img2Red, 256);
            histogramBox1.Refresh();
            histogramBox2.GenerateHistograms(img2Green, 256);
            histogramBox2.Refresh(); histogramBox1.GenerateHistograms(img2Red, 256);
            histogramBox3.GenerateHistograms(img2Blue, 256);
            histogramBox3.Refresh();
          
        }
        private void Grayscale_Click(object sender, EventArgs e)
        {
            Bitmap bm = (Bitmap)Img;
            My_Image = new Image<Bgr, byte>(bm);
            if (My_Image != null)
            {
                if (gray_in_use)
                {
                    gray_in_use = false;
                    PictureBox1.Image = My_Image.ToBitmap();
                    //Convert_btn.Text = "Convert to Gray";
                    Red.Checked = true;
                    Red.Enabled = true;
                    Green.Checked = true;
                    Green.Enabled = true;
                    Blue.Checked = true;
                    Blue.Enabled = true;
                }
                else
                {
                    gray_image = My_Image.Convert<Gray, Byte>();
                    gray_in_use = true;
                    PictureBox1.Image = gray_image.ToBitmap();
                    //Convert_btn.Text = "Convert to Colour";
                    Red.Enabled = false;
                    Green.Enabled = false;
                    Blue.Enabled = false;
                }
            }
            
        }

        private void Mean_Click(object sender, EventArgs e)
        {
            Int16[] mask;
            mask = new Int16[10];
            Bitmap img = new Bitmap(PictureBox1.Image);

            Color c;     
            for (int ii = 0; ii < img.Width; ii++)
            {

                for (int jj = 0; jj < img.Height; jj++)
                {



                    if (ii - 1 >= 0 && jj - 1 >= 0)
                    {

                        c = img.GetPixel(ii - 1, jj - 1);
                        mask[0] = Convert.ToInt16(c.R);
                    }

                    else
                    {

                        mask[0] = 0;

                    }



                    if (jj - 1 >= 0 && ii + 1 < img.Width)
                    {

                        c = img.GetPixel(ii + 1, jj - 1);

                        mask[1] = Convert.ToInt16(c.R);

                    }

                    else

                        mask[1] = 0;



                    if (jj - 1 >= 0)
                    {

                        c = img.GetPixel(ii, jj - 1);

                        mask[2] = Convert.ToInt16(c.R);

                    }

                    else

                        mask[2] = 0;



                    if (ii + 1 < img.Width)
                    {

                        c = img.GetPixel(ii + 1, jj);

                        mask[3] = Convert.ToInt16(c.R);

                    }

                    else

                        mask[3] = 0;



                    if (ii - 1 >= 0)
                    {

                        c = img.GetPixel(ii - 1, jj);

                        mask[4] = Convert.ToInt16(c.R);

                    }

                    else

                        mask[4] = 0;



                    if (ii - 1 >= 0 && jj + 1 < img.Height)
                    {

                        c = img.GetPixel(ii - 1, jj + 1);

                        mask[5] = Convert.ToInt16(c.R);

                    }

                    else

                        mask[5] = 0;



                    if (jj + 1 < img.Height)
                    {

                        c = img.GetPixel(ii, jj + 1);

                        mask[6] = Convert.ToInt16(c.R);

                    }

                    else

                        mask[6] = 0;





                    if (ii + 1 < img.Width && jj + 1 < img.Height)
                    {

                        c = img.GetPixel(ii + 1, jj + 1);

                        mask[7] = Convert.ToInt16(c.R);

                    }

                    else

                        mask[7] = 0;

                    c = img.GetPixel(ii, jj);

                    mask[8] = Convert.ToInt16(c.R);



                    int sum = 0;

                    for (int u = 0; u < 9; u++)

                        sum = sum + mask[u];

                    sum = sum / 9;

                    img.SetPixel(ii, jj, Color.FromArgb(sum, sum, sum));

                }

            }

            PictureBox1.Image = img;
            

            MessageBox.Show("successfully Done");


        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Bitmap bmp =(Bitmap) PictureBox1.Image;
            //Capture capture = new Capture();
            Image<Bgr, byte> image =new Image<Bgr,byte>(bmp) ;
            Image<Bgr, byte> blur = image.SmoothBlur(10, 10, true);
            Image<Bgr, byte> mediansmooth = image.SmoothMedian(15);
            Image<Bgr, byte> bilat = image.SmoothBilatral(7, 255, 34);
            Image<Bgr, byte> gauss = image.SmoothGaussian(3, 3, 34.3, 45.3);
            PictureBox1.Image = mediansmooth.ToBitmap();
        }
        private void ApplyFilter(bool preview)
        {
            Bitmap bmp = (Bitmap)PictureBox1.Image;
            Image<Bgr, byte> image = new Image<Bgr, byte>(bmp);
            Bitmap selectedSource = (Bitmap) PictureBox1.Image;
            Bitmap bitmapResult = null;
                if (comboBox1.SelectedItem.ToString() == "None")
                {
                    bitmapResult = selectedSource;
                }
                else if (comboBox1.SelectedItem.ToString() == "Gaussian")
                {
                    Image<Bgr, byte> gauss = image.SmoothGaussian(3, 3, 34.3, 45.3);
                    bitmapResult = gauss.ToBitmap();
                }
                else if (comboBox1.SelectedItem.ToString() == "Median")
                {
                    Image<Bgr, byte> mediansmooth = image.SmoothMedian(15);
                    bitmapResult = mediansmooth.ToBitmap();
                }

                else if (comboBox1.SelectedItem.ToString() == "Blur")
                {
                    Image<Bgr, byte> blur = image.SmoothBlur(10, 10, true);
                    bitmapResult = blur.ToBitmap();
                }
                else if (comboBox1.SelectedItem.ToString() == "Bilat")
                {
                    Image<Bgr, byte> bilat = image.SmoothBilatral(7, 255, 34);
                    bitmapResult = bilat.ToBitmap();
                }
                
          if (bitmapResult != null)
            {
                if (preview == true)
                {
                    PictureBox1.Image = bitmapResult;
                }
            }            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter(true);
        }
        public long[] GetHistogram(System.Drawing.Bitmap picture)
        {
            long[] myHistogram = new long[256];

            for (int i = 0; i < picture.Size.Width; i++)
                for (int j = 0; j < picture.Size.Height; j++)
                {
                    System.Drawing.Color c = picture.GetPixel(i, j);

                    long Temp = 0;
                    Temp += c.R;
                    Temp += c.G;
                    Temp += c.B;

                    Temp = (int)Temp / 3;
                    myHistogram[Temp]++;
                }

            return myHistogram;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!Red.Checked)
            {
                //Remove Red Spectrum programatically
                Suppress(2);
            }
            else
            {
                //Add Red Spectrum programatically
                Un_Suppress(2);
            }
            PictureBox1.Image = My_image_copy.ToBitmap();
        }

        private void Blue_CheckedChanged(object sender, EventArgs e)
        {
            if (!Blue.Checked)
            {
                //Remove Red Spectrum programatically
                Suppress(0);
            }
            else
            {
                //Add Red Spectrum programatically
                Un_Suppress(0);
            }
            PictureBox1.Image = My_image_copy.ToBitmap();

        }

        private void Green_CheckedChanged(object sender, EventArgs e)
        {
            if (!Green.Checked)
            {
                //Remove Red Spectrum programatically
                Suppress(1);
            }
            else
            {
                //Add Red Spectrum programatically
                Un_Suppress(1);
            }
            PictureBox1.Image = My_image_copy.ToBitmap();
        }
        private void Suppress(int spectrum)
        {
            Bitmap bm = (Bitmap)PictureBox1.Image;
            My_image_copy = new Image<Bgr, byte>(bm);

            for (int i = 0; i < PictureBox1.Height; i++)
            {
                for (int j = 0; j < PictureBox1.Width; j++)
                {
                    My_image_copy.Data[i, j, spectrum] = 0;
                }
            }

        }

        private void Un_Suppress(int spectrum)
        {
            Bitmap originalbm = (Bitmap)Img;
            My_Image = new Image<Bgr, byte>(originalbm);
            Bitmap bm = (Bitmap)PictureBox1.Image;
            My_image_copy = new Image<Bgr, byte>(bm);
            for (int i = 0; i < PictureBox1.Height; i++)
            {
                for (int j = 0; j < PictureBox1.Width; j++)
                {
                    My_image_copy.Data[i, j, spectrum] = My_Image.Data[i, j, spectrum];
                    
                }
            }
        }

        private void Sobel_Click(object sender, EventArgs e)
        {
            Bitmap bm = (Bitmap)PictureBox1.Image;
            Image<Bgr,Single> im1 = new Image<Bgr,Single>( bm);
            Image<Bgr, Single> img_final = (im1.Sobel(0,1,3));
            PictureBox1.Image = img_final.ToBitmap();
            //LineSegment2D[] lines = im1.HoughLines( 0.1, 0.1,1, Math.PI / 45.0,50,100, 1)[0];
            Image<Gray, Byte> gray1 = im1.Convert<Gray, Byte>().PyrDown().PyrUp();
           Image<Gray, Byte> cannyGray = gray1.Canny(20, 0.3);
           // PictureBox1.Image = cannyGray.ToBitmap();
            
        
        }

        private void btn_equalizepic_Click(object sender, EventArgs e)
        {
            Bitmap bm = (Bitmap)PictureBox1.Image;
            Image<Bgr, Byte> imageL = new Image<Bgr, byte>(bm);
            imageL._EqualizeHist();
            PictureBox1.Image = imageL.ToBitmap();
           
        }
    }
}
