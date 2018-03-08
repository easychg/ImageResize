using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace IRSZ
{
    public partial class Form1 : Form
    {
        List<string> listExtention = new List<string>();
        public Form1()
        {
            InitializeComponent();
            Form.CheckForIllegalCrossThreadCalls = false;
            listExtention.AddRange(new string[] { ".jpg", ".gif", ".png",".bmp",".jpeg" });
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
            Application.ExitThread();
        }
        #region 在新线程中运行函数
        /// <summary>
        /// 在新线程中运行函数
        /// </summary>
        /// <param name="func">传入 函数名(无参、无返回值)</param>
        /// <param name="IsBackground">是否为后台线程(后台线程，窗口关闭后就终止线程)</param>
        public static void ThreadNew(VoidFunction func, bool IsBackground)
        {
            Thread th1 = new Thread(new ThreadStart(func));
            th1.IsBackground = IsBackground;//后台线程，窗口关闭后就终止线程
            th1.Start();
        }
        /// <summary>
        /// 在新线程中运行函数
        /// </summary>
        /// <param name="func">传入 函数名(有一个参数、无返回值)</param>
        /// <param name="para">object参数</param>
        /// <param name="IsBackground">是否为后台线程(后台线程，窗口关闭后就终止线程)</param>
        public static Thread ThreadNew(ParamFunction func, object para, bool IsBackground)
        {
            Thread th1 = new Thread(new ParameterizedThreadStart(func));
            //判断状态
            //((int)th1.ThreadState &((int)ThreadState.Running | (int)ThreadState.Suspended) ) == 0
            th1.IsBackground = IsBackground;
            th1.Start(para);
            return th1;
        }
        /// <summary>
        /// 允许线程之间进行操作
        /// </summary>
        public static void OprateBetweenThread()
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
        }

        /// <summary>
        /// 无参的、返回值为void的委托，可以用来做参数名
        /// </summary>
        public delegate void VoidFunction();

        /// <summary>
        /// 有一个参数的、返回值为void的委托，可以用来做参数名
        /// </summary>
        public delegate void ParamFunction(object para);


        #endregion

        private void lb_selectDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtDir.Text = fbd.SelectedPath;
            }
        }

        
        int success = 0; //成功
        int falure = 0; //失败
        int total = 0;
        private void MakeWaterMark()
        {
            success = 0;
            falure = 0;
            total = 0;
            string errmsg = "";
            string strtxtDir = txtDir.Text.Trim();
            if (strtxtDir == "" )
            {
                MessageBox.Show("请选择目录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            else if (Directory.Exists(strtxtDir) == false)
            {
                MessageBox.Show("文件夹不存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            else
            {
                
                List<string> PictureList = new List<string>();
                lb_statusInfo.Text = "状态：正在检索图片…";
                SearchFile(txtDir.Text.Trim(), ref PictureList);
                if (txt_width.Text == "" || txt_height.Text == "") {
                    MessageBox.Show("尺寸不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
                int width = Convert.ToInt32(txt_width.Text);
                int height = Convert.ToInt32(txt_height.Text);
                int qua = trackBar1.Value;
                string error="";
                string mimeType="";
                foreach (string s in PictureList)
                {
                    try
                    {

                        //MakeWaterPic(s, "", "", "");
                        GetThumbnailImage(s, s, width, height, qua, out error, mimeType = "image/jpeg");
                        success++;
                    }
                    catch (Exception er)
                    {
                        falure++;
                        errmsg += er.Message;
                    }
                    total++;
                    lb_statusInfo.Text = "状态：正在为第" + (total + 1) + "张图片调整…";
                }
                lb_statusInfo.Text = "状态：完成！共" + total + ",成功" + success + ",失败" + falure;
                if (errmsg != "") MessageBox.Show(errmsg, "执行完成，部分文件出错信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private string MakeWaterPic(string SourcePicPath, string WaterText, string WaterPath, string SaveName)
        {
            if (File.Exists(SourcePicPath) == false)
            {
                return "-1";//文件不存在
            }

            string extension = Path.GetExtension(SourcePicPath).ToLower();//后缀
            if (listExtention.Contains(extension) == false) throw new Exception("不允许的后缀:" + SourcePicPath + "\n");
            string fileName = "";
            if (SaveName.Trim() != "") fileName = SaveName;
            else fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            
            //加文字水印
            System.Drawing.Image image = System.Drawing.Image.FromFile(SourcePicPath, true);
            int imgwidth = image.Width;
            int imgheight = image.Height;
            using (System.Drawing.Bitmap bitmap = new Bitmap(image.Width, image.Height))
            {

                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))//
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.Clear(System.Drawing.Color.Transparent);
                    g.DrawImage(image, 0, 0, imgwidth, imgheight);//画上原图片

                    image.Dispose();
                    //g.DrawImage(image, 0, 0, image.Width, image.Height);
                    if (WaterText != "")
                    {
                        Font f = new Font("Verdana", 32);
                        Brush b = new SolidBrush(Color.Yellow);
                        g.DrawString(WaterText, f, b, 10, 10);
                    }
                    //g.Dispose();

                    //加图片水印
                    System.Drawing.Image copyImage = System.Drawing.Image.FromFile(WaterPath);
                    //Rectangle[destRect ] 它指定所绘制图像的位置和大小。将图像进行缩放以适合该矩形。
                    //Rectangle[srcRect ] 它指定 image 对象中要绘制的部分。
                    g.DrawImage(copyImage, new Rectangle(imgwidth - copyImage.Width, imgheight - copyImage.Height, copyImage.Width, copyImage.Height), 0, 0, copyImage.Width, copyImage.Height, GraphicsUnit.Pixel);
                    if (File.Exists(SourcePicPath))
                    {
                        File.Delete(SourcePicPath);
                    }
                    //保存加水印过后的图片,删除原始图片
                    // string newPath = fileName + extension;
                    switch (extension)
                    {
                        case ".jpg":
                            bitmap.Save(SourcePicPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                            break;
                        case ".gif":
                            bitmap.Save(SourcePicPath, System.Drawing.Imaging.ImageFormat.Gif);
                            break;
                        case ".png":
                            bitmap.Save(SourcePicPath, System.Drawing.Imaging.ImageFormat.Png);
                            break;
                        case ".jpeg":
                            bitmap.Save(SourcePicPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                            break;
                        case ".bmp":
                            bitmap.Save(SourcePicPath, System.Drawing.Imaging.ImageFormat.Bmp);
                            break;
                        default:
                            throw new Exception("不允许的后缀:" + SourcePicPath);
                    }
                }
            }

            return "1";
            // Response.Redirect(newPath);
            //}
        }
        public void SearchFile(string parentDir, ref List<string> PictureList)
        {
            try
            {
                string[] subFiles = Directory.GetFiles(parentDir);
                string[] subDirs = Directory.GetDirectories(parentDir, "*.*", SearchOption.TopDirectoryOnly);
                PictureList.AddRange(subFiles);
                foreach (string dir in subDirs)
                {
                    SearchFile(dir, ref PictureList);
                }
            }
            catch (Exception ex) { }
        }


        /// <summary>
        /// 生成高质量缩略图（固定宽高），不一定保持原宽高比
        /// </summary>
        /// <param name="destPath">目标保存路径</param>
        /// <param name="srcPath">源文件路径</param>
        /// <param name="width">生成缩略图的宽度，设置为0，则与源图比处理</param>
        /// <param name="height">生成缩略图的高度，设置为0，则与源图等比例处理</param>
        /// <param name="quality">1~100整数,无效值则取默认值95</param>
        /// <param name="mimeType">如 image/jpeg</param>    
        private bool GetThumbnailImage(string destPath, string srcPath, int destWidth, int destHeight, int quality, out string error, string mimeType = "image/jpeg")
        {
            bool retVal = false;
            error = string.Empty;
            //宽高不能小于0
            if (destWidth < 0 || destHeight < 0)
            {
                error = "目标宽高不能小于0";
                return retVal;
            }
            //宽高不能同时为0
            if (destWidth == 0 && destHeight == 0)
            {
                error = "目标宽高不能同时为0";
                return retVal;
            }
            Image srcImage = null;
            Image destImage = null;
            Graphics graphics = null;
            try
            {
                //获取源图像
                srcImage = Image.FromFile(srcPath, false);
                //计算高宽比例
                float d = (float)srcImage.Height / srcImage.Width;
                //如果输入的宽为0，则按高度等比缩放
                if (destWidth == 0)
                {
                    destWidth = Convert.ToInt32(destHeight / d);
                }
                //如果输入的高为0，则按宽度等比缩放
                if (destHeight == 0)
                {
                    destHeight = Convert.ToInt32(destWidth * d);
                }
                //定义画布
                destImage = new Bitmap(destWidth, destHeight);
                //获取高清Graphics
                graphics = GetGraphics(destImage);
                //将源图像画到画布上，注意最后一个参数GraphicsUnit.Pixel
                graphics.DrawImage(srcImage, new Rectangle(0, 0, destWidth, destHeight), new Rectangle(0, 0, srcImage.Width, srcImage.Height), GraphicsUnit.Pixel);
                //如果是覆盖则先释放源资源
                if (destPath == srcPath)
                {
                    srcImage.Dispose();
                }
                //保存到文件，同时进一步控制质量
                SaveImage2File(destPath, destImage, quality, mimeType);
                pictureBox1.Image = Image.FromFile(destPath);
                lbl_len.Text = "文件大小：" + GetFileSize(new FileInfo(destPath).Length);
                retVal = true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                if (srcImage != null)
                    srcImage.Dispose();
                if (destImage != null)
                    destImage.Dispose();
                if (graphics != null)
                    graphics.Dispose();
            }
            return retVal;
        }
        /// <summary>
        /// 获取高清的Graphics
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public Graphics GetGraphics(Image img)
        {
            var g = Graphics.FromImage(img);
            //设置质量
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            //InterpolationMode不能使用High或者HighQualityBicubic,如果是灰色或者部分浅色的图像是会在边缘处出一白色透明的线
            //用HighQualityBilinear却会使图片比其他两种模式模糊（需要肉眼仔细对比才可以看出）
            g.InterpolationMode = InterpolationMode.Default;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            return g;
        }
        /// <summary>
        /// 将Image实例保存到文件,注意此方法不执行 img.Dispose()
        /// 图片保存时本可以直接使用destImage.Save(path, ImageFormat.Jpeg)，但是这种方法无法进行进一步控制图片质量
        /// </summary>
        /// <param name="path"></param>
        /// <param name="img"></param>
        /// <param name="quality">1~100整数,无效值，则取默认值95</param>
        /// <param name="mimeType"></param>
        public void SaveImage2File(string path, Image destImage, int quality, string mimeType = "image/jpeg")
        {
            if (quality <= 0 || quality > 100) quality = 95;
            //创建保存的文件夹
            FileInfo fileInfo = new FileInfo(path);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }
            //设置保存参数，保存参数里进一步控制质量
            EncoderParameters encoderParams = new EncoderParameters();
            long[] qua = new long[] { quality };
            EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            //获取指定mimeType的mimeType的ImageCodecInfo
            var codecInfo = ImageCodecInfo.GetImageEncoders().FirstOrDefault(ici => ici.MimeType == mimeType);
            destImage.Save(path, codecInfo, encoderParams);
        }
        private void btnExec_Click(object sender, EventArgs e)
        {
            ThreadNew(MakeWaterMark, true);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //txtDir.Text = ConfigFile.Instanse["txtDir"];
            //txtMark.Text = ConfigFile.Instanse["txtMark"];
        }

        private void txt_width_TextChanged(object sender, EventArgs e)
        {
            int width = Convert.ToInt32(txt_width.Text);
            int height = Convert.ToInt32(txt_height.Text);
            
            pictureBox1.Width = width;
            pictureBox1.Height = height;
            string error="";
            string mimeType = "image/jpeg";
            //GetThumbnailImage("./test1.jpg", "./test.jpg", width, height, qua, out error, mimeType = "image/jpeg"); //
            //pictureBox1.Image =  Image.FromFile("./test1.jpg");
            bool retVal = false;
            error = string.Empty;
            //宽高不能小于0
            if (width < 0 || height < 0)
            {
                error = "目标宽高不能小于0";
                return ;
            }
            //宽高不能同时为0
            if (width == 0 && height == 0)
            {
                error = "目标宽高不能同时为0";
                return ;
            }
            Image srcImage = null;
            Image destImage = null;
            Graphics graphics = null;
            try
            {
                //获取源图像
                srcImage = Image.FromFile("./test.jpg", false);
                //计算高宽比例
                float d = (float)srcImage.Height / srcImage.Width;
                //如果输入的宽为0，则按高度等比缩放
                if (width == 0)
                {
                    width = Convert.ToInt32(width / d);
                }
                //如果输入的高为0，则按宽度等比缩放
                if (height == 0)
                {
                    height = Convert.ToInt32(height * d);
                }
                //定义画布
                destImage = new Bitmap(width, height);
                //获取高清Graphics
                graphics = GetGraphics(destImage);
                //将源图像画到画布上，注意最后一个参数GraphicsUnit.Pixel
                graphics.DrawImage(srcImage, new Rectangle(0, 0, width, height), new Rectangle(0, 0, srcImage.Width, srcImage.Height), GraphicsUnit.Pixel);
                //如果是覆盖则先释放源资源
                int qua = trackBar1.Value;
                //保存到文件，同时进一步控制质量
                //SaveImage2File(destPath, destImage, quality, mimeType);
                if (qua <= 0 || qua > 100) qua = 95;
                
                //设置保存参数，保存参数里进一步控制质量
                EncoderParameters encoderParams = new EncoderParameters();
                long[] quality = new long[] { qua };
                EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qua);
                encoderParams.Param[0] = encoderParam;
                //获取指定mimeType的mimeType的ImageCodecInfo
                var codecInfo = ImageCodecInfo.GetImageEncoders().FirstOrDefault(ici => ici.MimeType == mimeType);
                if (File.Exists("./test1.jpg"))
                {
                    File.Delete("./test1.jpg");
                }
                destImage.Save("./test1.jpg", codecInfo, encoderParams);
                using (FileStream image = new FileStream("./test1.jpg", FileMode.Open))
                {
                    pictureBox1.Image = Image.FromStream(image);
                }
                lbl_len.Text = "文件大小："+GetFileSize(new FileInfo("./test1.jpg").Length);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                if (srcImage != null)
                    srcImage.Dispose();
                if (destImage != null)
                    destImage.Dispose();
                if (graphics != null)
                    graphics.Dispose();
            }
        }
        private string GetFileSize(long size)
        {
            var num = 1024.00; //byte

            if (size < num)
                return size + "B";
            if (size < Math.Pow(num, 2))
                return (size / num).ToString("f2") + "K"; //kb
            if (size < Math.Pow(num, 3))
                return (size / Math.Pow(num, 2)).ToString("f2") + "M"; //M
            if (size < Math.Pow(num, 4))
                return (size / Math.Pow(num, 3)).ToString("f2") + "G"; //G

            return (size / Math.Pow(num, 4)).ToString("f2") + "T"; //T
        }
    }
}
