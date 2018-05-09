using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Expression.Encoder;
using Microsoft.Expression.Encoder.Devices;
using Microsoft.Expression.Encoder.Profiles;
using Microsoft.Expression.Encoder.ScreenCapture;

namespace ScreenCapture
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //定义屏幕截图job
        ScreenCaptureJob screencapturejob = new ScreenCaptureJob();
        //设置临时目录
         string path = "c:\\"+Guid.NewGuid().ToString();

        /// <summary>
        /// 开始录制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            screencapturejob.OutputPath = path;
            Collection<EncoderDevice> devices = EncoderDevices.FindDevices(EncoderDeviceType.Audio);//查询当前所有音频设备
            foreach (EncoderDevice device in devices)
            {
                try
                {
                    screencapturejob.AddAudioDeviceSource(device);//将音频设备的声音记录
                }
                catch { 
                //抛出其他异常的音频设备
                }
            }
            //录制开始
            screencapturejob.Start();
        }

        /// <summary>
        /// 停止录制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            label1.Text = "正在处理...";
            screencapturejob.Stop();//停止录制job
            string xescfile = GetxescPath();//获取录制文件路径
            MediaItem src = new MediaItem(xescfile);//加入转换媒体

            //获取屏幕信息
            Rectangle rect = new Rectangle();
            rect =Screen.PrimaryScreen.Bounds;
           // Screen.AllScreens

            //重新定义视频格式
            src.OutputFormat = new WindowsMediaOutputFormat();
            src.OutputFormat.VideoProfile = new AdvancedVC1VideoProfile();
            src.OutputFormat.VideoProfile.Bitrate = new VariableConstrainedBitrate(1000, 1500);
            src.OutputFormat.VideoProfile.Size = new Size(rect.Width, rect.Height);
            src.OutputFormat.VideoProfile.FrameRate = 30;
            src.OutputFormat.VideoProfile.KeyFrameDistance = new TimeSpan(0, 0, 4);

            //重新定义音频格式
            src.OutputFormat.AudioProfile = new WmaAudioProfile();
            src.OutputFormat.AudioProfile.Bitrate = new VariableConstrainedBitrate(128, 192);
            src.OutputFormat.AudioProfile.Codec = AudioCodec.WmaProfessional;
            src.OutputFormat.AudioProfile.BitsPerSample = 24;

            Job encoderjob = new Job();//实例化转换作业
            encoderjob.MediaItems.Add(src);//添加xesc文件
            //encoderjob.ApplyPreset(Presets.VC1HD720pVBR);//设置视频编码
            encoderjob.CreateSubfolder = false;//不创建文件夹
            encoderjob.OutputDirectory = "C:\\output";//设置转换完后的文件保存目录
            encoderjob.EncodeCompleted += job2_EncodeCompleted;

            encoderjob.Encode();

            if (File.Exists(xescfile)) File.Delete(xescfile);
            if (Directory.Exists(path)) Directory.Delete(path);
        }

        void job2_EncodeCompleted(object sender, EncodeCompletedEventArgs e)
        {
            label1.Text = "处理完成...";
        }

        private string GetxescPath()
        {
            string result = "";
            FileInfo[] filelist = new DirectoryInfo(path).GetFiles("*.xesc");
            foreach (FileInfo NextFile in filelist)
            {
                result = NextFile.FullName;
                break;
            }
            return result;
        }
    }
}
