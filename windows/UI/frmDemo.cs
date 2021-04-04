using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using System.IO;
using System.Drawing.Drawing2D;
using System.Net;
using System.Resources;
using TGMT;
using System.Threading.Tasks;
using IPSS;
using System.Collections.Specialized;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;

namespace TGMTcs
{
    public partial class frmDemo : Form
    {        
        #region global_variable
        VideoCaptureDevice m_videoSource;

        int ANCHOR_WIDTH = 8;
        Size ANCHOR_SIZE;
        int OFFSET;
        float g_scaleX = 1;
        float g_scaleY = 1;
        bool m_isDrag;
        bool frm_retrainCharacterShowing = false;


        public static IPSScar carDetector;

        enum Colision
        {
            TopLeft,
            TopRight,
            BotLeft,
            BotRight,
            None,
        }
        Bitmap g_bmp;

        int selectPointIdx = -1;


        bool m_isFirstLoading = true;

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region common_function

        void SetScaleRatio()
        {
            g_scaleX = (float)g_bmp.Width / picCamera.Width;
            g_scaleY = (float)g_bmp.Height / picCamera.Height;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        void PrintError(string message)
        {
            lblMessage.ForeColor = Color.Red;
            lblMessage.Text = message;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        void PrintSuccess(string message)
        {
            lblMessage.ForeColor = Color.Green;
            lblMessage.Text = message;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        void PrintMessage(string message)
        {
            lblMessage.ForeColor = Color.Black;
            lblMessage.Text = message;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        void StopLoading()
        {
            timerProgressbar.Stop();
            progressBar1.Value = progressBar1.Minimum;
            progressBar1.Visible = false;
            lblMessage.Text = "";
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        void InitCamera()
        {
            cbCamera.Items.Clear();

            FilterInfoCollection videosources = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videosources.Count == 0)
            {
                PrintError("Can not find camera");
            }


            for (int i = 0; i < videosources.Count; i++)
            {
                cbCamera.Items.Add(videosources[i].Name);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        void InitDetector()
        {
            lblMessage.Text = "Loading data, please wait...";
            btnDetect.Enabled = true;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        void ReadPlate()
        {
            if (!rdCamera.Checked && !rdImage.Checked && !rdFolder.Checked)
            {
                PrintError("Source image not selected");
                return;
            }

            if (carDetector == null)
            {
                MessageBox.Show("Please contact to author to fix problem");
                return;
            }

            carDetector.OutputFoler = txtFolderOutput.Text;

            if (rdCamera.Checked) //camera
            {
                if (rdNetworkCamera.Checked)
                {
                    if (streamPlayer.IsPlaying)
                        g_bmp = streamPlayer.GetCurrentFrame();
                }
                if (g_bmp == null)
                {
                    PrintError("Image is null");
                    return;
                }

                Bitmap bmp = (Bitmap)g_bmp.Clone();
                if (bmp == null)
                    return;


                CarPlate result = carDetector.ReadPlate(bmp);
                if (result.bitmap != null)
                    picResult.Image = result.bitmap;

                txtResult.Text = result.text;
                PrintMessage(result.error + " (" + result.elapsedMilisecond.ToString() + " ms)");
            }
            else if (rdImage.Checked) //static image
            {
                if (g_bmp == null)
                {
                    PrintError("Image is null");
                    return;
                }
                picResult.Image = null;
                CarPlate result = carDetector.ReadPlate((Bitmap)g_bmp.Clone());
                txtResult.Text = result.text;

                if (result.bitmap != null)
                    picResult.Image = result.bitmap;
                PrintMessage(result.error + " (" + result.elapsedMilisecond + " ms)");
            }
            else if (rdFolder.Checked) //folder
            {
                if (btnDetect.Text == "Start detect (F5)")
                {
                    if (txtFolderOutput.Text == "")
                    {
                        errorProvider1.SetError(txtFolderOutput, "Folder output is empty");
                        return;
                    }

                    if (txtFolderInput.Text == txtFolderOutput.Text && txtFolderOutput.Text != "")
                    {
                        errorProvider1.SetError(txtFolderOutput, "Folder output must different folder input");
                        return;
                    }
                    if (lstImage.Items.Count == 0)
                    {
                        PrintError( "No input image");
                        return;
                    }

                    g_scaleX = 1;
                    g_scaleY = 1;
                    progressBar1.Visible = true;
                    timerProgressbar.Start();
                    bgWorker1.RunWorkerAsync();

                    btnDetect.Text = "Stop detect (F5)";
                }
                else
                {
                    bgWorker1.CancelAsync();
                    btnDetect.Text = "Start detect (F5)";
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        void StopAllCamera()
        {
            if (streamPlayer != null && streamPlayer.IsPlaying)
                streamPlayer.Stop();

            if (m_videoSource != null)
                m_videoSource.Stop();

            picCamera.Image = null;
            btnSnapshot.Visible = false;
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region form

        public frmDemo()
        {
            InitializeComponent();           

            ANCHOR_SIZE = new Size(ANCHOR_WIDTH, ANCHOR_WIDTH);
            OFFSET = ANCHOR_WIDTH / 2;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void frmDemo_Shown(object sender, EventArgs e)
        {            
            TGMTregistry.GetInstance().Init("IPSScar");
            carDetector = new IPSScar();
            if (carDetector == null)
            {
                return;
            }

            CheckForIllegalCrossThreadCalls = false;
            this.KeyPreview = true;

            txtIpAddress.Text = TGMTregistry.GetInstance().ReadRegValueString("cameraAddress");

            chkEnableLog.Checked = TGMTregistry.GetInstance().ReadRegValueBool("EnableLog");
            carDetector.EnableLog = chkEnableLog.Checked;
            chkCrop.Checked = TGMTregistry.GetInstance().ReadRegValueBool("CropResultImage");
            chkSaveInputImage.Checked = TGMTregistry.GetInstance().ReadRegValueBool("SaveInputImage");

            txtFolderOutput.Text = TGMTregistry.GetInstance().ReadRegValueString("folderOutput");
            txtFailedDir.Text = TGMTregistry.GetInstance().ReadRegValueString("txtFailedDir");
            txtValidDir.Text = TGMTregistry.GetInstance().ReadRegValueString("txtValidDir");
            txtInvalidDir.Text = TGMTregistry.GetInstance().ReadRegValueString("txtInvalidDir");

            this.Text += " " + carDetector.Version;
            this.Text += carDetector.IsLicense ? " (Licensed)" : " (Vui lòng liên hệ: 0939.825.125)";
            
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void frmDemo_FormClosed(object sender, FormClosedEventArgs e)
        {
            StopAllCamera();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void frmDemo_KeyUp(object sender, KeyEventArgs e)
        {
            if (!e.Control)
                return;
            if ((int)e.KeyCode >= 97 && (int)e.KeyCode <= 101)
            {
                selectPointIdx = (int)e.KeyCode - 97;
                picCamera.Refresh();
            }
            else if ((int)e.KeyCode >= 49 && (int)e.KeyCode <= 52)
            {
                selectPointIdx = (int)e.KeyCode - 49;
                picCamera.Refresh();
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void frmDemo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                ReadPlate();
                return;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void timerProgressbar_Tick(object sender, EventArgs e)
        {
            progressBar1.Value += 1;
            if (progressBar1.Value >= progressBar1.Maximum)
                progressBar1.Value = progressBar1.Minimum;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void timerAutoDetect_Tick(object sender, EventArgs e)
        {
            ReadPlate();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void timerDraw_Tick(object sender, EventArgs e)
        {
            picCamera.Refresh();
        }

        #endregion //form

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region select_source

        private void rdCamera_CheckedChanged(object sender, EventArgs e)
        {
            txtResult.Text = txtFilePath.Text = "";


            if (rdCamera.Checked)
            {
                InitCamera();

                grCamera.Visible = true;
                grImage.Visible = false;
                grFolder.Visible = false;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void rdImage_CheckedChanged(object sender, EventArgs e)
        {
            if (rdImage.Checked)
            {
                StopAllCamera();
                grCamera.Visible = false;
                grImage.Visible = true;
                grFolder.Visible = false;

                txtFilePath.Text = TGMTregistry.GetInstance().ReadRegValueString("txtFilePath");
            }
            else
            {
                if (picCamera.Image != null)
                {
                    picCamera.Image.Dispose();
                    picCamera.Image = null;
                }

                if (picResult.Image != null)
                {
                    picResult.Image.Dispose();
                    picResult.Image = null;
                }

                if (g_bmp != null)
                {
                    g_bmp.Dispose();
                    g_bmp = null;
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void rdFolder_CheckedChanged(object sender, EventArgs e)
        {
            txtResult.Text = "";
            if (picCamera.Image != null)
                picCamera.Image.Dispose();
            if (picResult.Image != null)
                picResult.Image = null;

            if (rdFolder.Checked)
            {
                if (m_isFirstLoading)
                {
                    txtFolderInput.Text = TGMTregistry.GetInstance().ReadRegValueString("folderInput");

                    m_isFirstLoading = false;
                }


                StopAllCamera();
                grFolder.Visible = true;
                grCamera.Visible = false;
                grImage.Visible = false;

                g_scaleX = 1;
                g_scaleY = 1;

                picCamera.Visible = false;
                lstImage.Visible = true;
            }
            else
            {
                picCamera.Visible = true;
                lstImage.Visible = false;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void btnDetect_Click(object sender, EventArgs e)
        {
            ReadPlate();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void btnDetect_EnabledChanged(object sender, EventArgs e)
        {
            if (btnDetect.Enabled)
            {
                StopLoading();
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void frmDemo_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if(files.Length == 1)
            {
                rdImage.Checked = true;
                txtFilePath.Text = files[0];                
            }
            else
            {
                m_isDrag = true;
                rdFolder.Checked = true;
                lstImage.Items.Clear();
                foreach (string file in files)
                {
                    lstImage.Items.Add(Path.GetFileName(file));
                }
                txtFolderInput.Text = Path.GetDirectoryName(files[0]);
            }
            ReadPlate();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void frmDemo_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        #endregion //select_source

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region group_image_source

        private void txtFilePath_TextChanged(object sender, EventArgs e)
        {
            if (!File.Exists(txtFilePath.Text))
                return;

            TGMTregistry.GetInstance().SaveRegValue("txtFilePath", txtFilePath.Text);

            if (g_bmp != null)
                g_bmp.Dispose();
            try
            {
                g_bmp = new Bitmap(txtFilePath.Text);
                picCamera.Image = g_bmp;

                SetScaleRatio();
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image files |*.bmp;*.jpg;*.png;*.BMP;*.JPG;*.PNG";
            ofd.ShowDialog();
            if (ofd.FileName != "")
            {
                txtFilePath.Text = ofd.FileName;
            }

        }

        #endregion //group_image_source

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region group_camera_source

        private void txtIpAddress_TextChanged(object sender, EventArgs e)
        {
            TGMTregistry.GetInstance().SaveRegValue("cameraAddress", txtIpAddress.Text);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        void OnCameraFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (g_bmp != null)
                g_bmp.Dispose();

            g_scaleX = (float)eventArgs.Frame.Width / picCamera.Width;
            g_scaleY = (float)eventArgs.Frame.Height / picCamera.Height;

            g_bmp = (Bitmap)eventArgs.Frame.Clone();
            picCamera.Image = g_bmp;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void cbCamera_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConnectLocalCamera();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void rdLocalCamera_CheckedChanged(object sender, EventArgs e)
        {
            PrintMessage("");
            picCamera.Image = null;
            btnSnapshot.Visible = false;
            cbCamera.Enabled = rdLocalCamera.Checked;

            if (rdLocalCamera.Checked)
            {
                if (streamPlayer != null && streamPlayer.IsPlaying)
                    streamPlayer.Stop();

                timerStream.Stop();
                ConnectLocalCamera();
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void rdNetworkCamera_CheckedChanged(object sender, EventArgs e)
        {
            txtIpAddress.Enabled = btnConnectCameraIP.Enabled = rdNetworkCamera.Checked;

            if (rdNetworkCamera.Checked)
            {
                if (m_videoSource != null)
                    m_videoSource.Stop();
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void btnConnectCameraIP_Click(object sender, EventArgs e)
        {
            if (btnConnectCameraIP.Text == "Start")
            {
                string text = txtIpAddress.Text;
                if (text == "")
                {
                    PrintError("URL is null");
                    return;
                }

                string url;
                string username;
                string password;

                if (text.Length - text.Replace(";", "").Length == 2)
                {
                    string[] split = txtIpAddress.Text.Split(';');
                    url = split[0];
                    username = split[1];
                    password = split[2];
                    //if(username != "")
                    //    stream.Password = username;
                    //if(password != "")
                    //    stream.Login = password;
                }
                else
                {
                    url = text;
                }

                var uri = new Uri(url);
                streamPlayer.StartPlay(uri, TimeSpan.FromSeconds(1.0));
                PrintMessage("Connecting...");
            }
            else
            {
                streamPlayer.Stop();
                btnSnapshot.Visible = false;
                picCamera.Image = null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        void ConnectLocalCamera()
        {
            if (cbCamera.Items.Count == 0 || cbCamera.SelectedIndex == -1)
                return;
            if (m_videoSource != null)
            {
                m_videoSource.Stop();
            }
            else
            {
                FilterInfoCollection videosources = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                m_videoSource = new VideoCaptureDevice(videosources[cbCamera.SelectedIndex].MonikerString);
            }

            m_videoSource.NewFrame += new NewFrameEventHandler(OnCameraFrame);
            m_videoSource.Start();
            btnSnapshot.Visible = true;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void HandleStreamFailedEvent(object sender, WebEye.StreamFailedEventArgs e)
        {
            PrintError("Can not connect to camera IP");
        }

        private void streamPlayer_StreamStarted(object sender, EventArgs e)
        {
            timerStream.Start();
            btnConnectCameraIP.Text = "Stop";
            btnSnapshot.Visible = true;
            PrintMessage("Playing");
        }

        private void streamPlayer_StreamStopped(object sender, EventArgs e)
        {
            timerStream.Stop();
            btnConnectCameraIP.Text = "Start";
            btnSnapshot.Visible = false;
            PrintMessage("Stopped");
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void timerStream_Tick(object sender, EventArgs e)
        {
            g_bmp = streamPlayer.GetCurrentFrame();
            g_scaleX = (float)g_bmp.Width / picCamera.Width;
            g_scaleY = (float)g_bmp.Height / picCamera.Height;

            picCamera.Image = g_bmp;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void btnSnapshot_Click(object sender, EventArgs e)
        {
            if (g_bmp == null && !streamPlayer.IsPlaying)
            {
                return;
            }


            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Bitmap Image|*.bmp";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (rdLocalCamera.Checked)
                {
                    g_bmp.Save(saveFileDialog1.FileName);
                }
                else if (rdNetworkCamera.Checked)
                {
                    Bitmap bmp = streamPlayer.GetCurrentFrame();
                    if (bmp != null)
                        bmp.Save(saveFileDialog1.FileName);
                }

            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void chkAutodetect_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutodetect.Checked)
                timerAutoDetect.Start();
            else
                timerAutoDetect.Stop();
        }

        #endregion //group_camera_source

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region group_folder_source

        private void btnSelectFolderInput_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath != "")
            {
                txtFolderInput.Text = fbd.SelectedPath;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void btnSelectFolderOutput_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            if (fbd.SelectedPath != "")
            {
                string dirPath = fbd.SelectedPath;
                txtFolderOutput.Text = dirPath;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void txtFolderInput_TextChanged(object sender, EventArgs e)
        {            
            if (!Directory.Exists(txtFolderInput.Text))
                return;

            TGMTregistry.GetInstance().SaveRegValue("folderInput", txtFolderInput.Text);

            if (!m_isDrag)
            {
                PrintMessage("Loading files...");
                lstImage.Items.Clear();
                bgLoadFile.RunWorkerAsync();
            }
            m_isDrag = false;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void lstImage_Click(object sender, EventArgs e)
        {
            SelectFile();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void lstImage_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            SelectFile();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        void SelectFile()
        {
            if (lstImage.Items.Count == 0 || lstImage.SelectedItems.Count == 0)
            {
                return;
            }

            string fileName = lstImage.SelectedItems[0].Text;


            string inputPath = TGMTutil.CorrectPath(txtFolderInput.Text);
            string failedDir = txtFailedDir.Text != "" ? TGMTutil.CorrectPath(txtFailedDir.Text) : "";
            string outputDir = txtFolderOutput.Text != "" ? TGMTutil.CorrectPath(txtFolderOutput.Text) : "";

            string filePath = "";
            if (txtFolderOutput.Text != "" && File.Exists(outputDir + fileName))
            {
                filePath = outputDir + fileName;
                picResult.ImageLocation = filePath;
                PrintMessage(filePath);
            }
            else if (File.Exists(inputPath + fileName))
            {
                filePath = inputPath + fileName;
                picResult.ImageLocation = filePath;
                PrintMessage(filePath);
            }
            else if (txtFailedDir.Text != "" && File.Exists(failedDir + fileName))
            {
                filePath = failedDir + fileName;
                picResult.ImageLocation = filePath;
                PrintMessage(filePath);
            }
            else
            {
                PrintError("File " + inputPath + fileName + " does not exist");
            }
   
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void txtFolderOutput_TextChanged(object sender, EventArgs e)
        {
            TGMTregistry.GetInstance().SaveRegValue("folderOutput", txtFolderOutput.Text);            
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void chkMoveFail_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMoveFail.Checked)
            {
                if (txtFailedDir.Text == "")
                {
                    chkMoveFail.Checked = false;
                    PrintError("Target directory is empty");
                }
                else
                {
                    Directory.CreateDirectory(txtFailedDir.Text);
                }
            }
        }

        #endregion //group_folder_source


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region BackgroundWorker

        string GetPlateType(CarPlate.PlateType type)
        {
            if (type == CarPlate.PlateType.Long)
                return "Long";
            else if (type == CarPlate.PlateType.Short)
                return "Short";
            else
                return "Unknown";
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void bgWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string inputPath = "";
            if(txtFolderInput.Text != "")
                inputPath = TGMTutil.CorrectPath(txtFolderInput.Text);
            string failedDir = "";
            if (txtFailedDir.Text != "")
                failedDir = TGMTutil.CorrectPath(txtFailedDir.Text);

            string validDir = "";
            if (txtValidDir.Text != "")
                validDir = TGMTutil.CorrectPath(txtValidDir.Text);

            string invalidDir = "";
            if (txtInvalidDir.Text != "")
                invalidDir = TGMTutil.CorrectPath(txtInvalidDir.Text);

            int exactlyCount = 0;
            string content = "";

            for (int i = 0; i < lstImage.Items.Count; i++)
            {
                if (bgWorker1.CancellationPending)
                    return;
                bgWorker1.ReportProgress(i + 1);

                carDetector.OutputFileName = lstImage.Items[i].Text;

                string filePath = inputPath + lstImage.Items[i].Text;
                string ext = filePath.Substring(filePath.Length - 4).ToLower();
                content += Path.GetFileName(filePath) + ",";

                if (ext != ".jpg" && ext != ".png" && ext != ".bmp")
                    continue;
                PrintMessage(i + " / " + lstImage.Items.Count + " " + filePath);

                CarPlate result = carDetector.ReadPlate(filePath);

                if (result.hasPlate)
                {
                    if (lstImage.Items[i].SubItems.Count == 1)
                    {
                        lstImage.Items[i].SubItems.Add(result.text);
                        lstImage.Items[i].SubItems.Add(GetPlateType(result.type));
                    }
                    else
                    {
                        lstImage.Items[i].SubItems[1].Text = result.text;
                        lstImage.Items[i].SubItems[2].Text = GetPlateType(result.type);
                    }
                    lstImage.Items[i].ForeColor = result.isValid ? Color.Blue : Color.Black;
                    content += "x,";

                    if (result.isValid)
                    {
                        exactlyCount++;
                        if (chkMoveValid.Checked)
                        {
                            Task.Run(() => File.Move(inputPath + lstImage.Items[i].Text, validDir + lstImage.Items[i].Text));
                        }
                    }
                    else
                    {
                        if (chkMoveInvalid.Checked)
                        {
                            Task.Run(() => File.Move(inputPath + lstImage.Items[i].Text, invalidDir + lstImage.Items[i].Text));
                        }
                    }
                        
                }
                else
                {
                    if (lstImage.Items[i].SubItems.Count == 1)
                    {
                        lstImage.Items[i].SubItems.Add(result.error);
                    }
                    else
                    {
                        lstImage.Items[i].SubItems[1].Text = result.error;
                    }
                    if (chkMoveFail.Checked)
                    {
                        Task.Run(() => File.Move(inputPath + lstImage.Items[i].Text, failedDir + lstImage.Items[i].Text));
                    }
                    lstImage.Items[i].ForeColor = Color.Red;
                    content += ",";
                }

                content += result.text;

                
                result.Dispose();
                content += "\r\n";

                lstImage.EnsureVisible(i);
            }

            if(inputPath != "")
            {
                File.WriteAllText(Path.GetDirectoryName(inputPath) + "\\_report.csv", content);
            }
            
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void bgWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PrintMessage(e.ProgressPercentage + "/" + lstImage.Items.Count + "(" + (100 * e.ProgressPercentage / lstImage.Items.Count) + " %)");
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void bgWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Visible = false;
            timerProgressbar.Stop();
            btnDetect.Text = "Start detect (F5)";
            if(txtFolderOutput.Text != "")
                PrintMessage("Save report to " + TGMTutil.CorrectPath(txtFolderOutput.Text) + "_report.csv");                      
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void bgLoadFile_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> files = new List<string>();
            lstImage.Items.Clear();
            foreach (string filePath in Directory.GetFiles(txtFolderInput.Text, "*.jpg"))
            {
                files.Add(Path.GetFileName(filePath));
            }
            foreach (string filePath in Directory.GetFiles(txtFolderInput.Text, "*.png"))
            {
                files.Add(Path.GetFileName(filePath));
            }
            foreach (string filePath in Directory.GetFiles(txtFolderInput.Text, "*.bmp"))
            {
                files.Add(Path.GetFileName(filePath));
            }
            e.Result = files;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void bgLoadFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<string> files = (List<string>)e.Result;
            for(int i=0; i<files.Count; i++)
            {
                lstImage.Items.Add(files[i]);
            }
            PrintMessage("Loaded " + lstImage.Items.Count + " images");            
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void chkEnableLog_CheckedChanged(object sender, EventArgs e)
        {
            carDetector.EnableLog = chkEnableLog.Checked;
            TGMTregistry.GetInstance().SaveRegValue("EnableLog", carDetector.EnableLog);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void chkSaveInputImage_CheckedChanged(object sender, EventArgs e)
        {
            carDetector.SaveInputImage = chkSaveInputImage.Checked;
            TGMTregistry.GetInstance().SaveRegValue("SaveInputImage", carDetector.SaveInputImage);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void chkCrop_CheckedChanged(object sender, EventArgs e)
        {
            carDetector.CropResultImage = chkCrop.Checked;
            TGMTregistry.GetInstance().SaveRegValue("CropResultImage", carDetector.CropResultImage);
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string filePath = TGMTutil.CorrectPath(txtFolderInput.Text);
            filePath += lstImage.SelectedItems[0].Text;
            if (!File.Exists(filePath))
            {
                PrintMessage("File does not exist");
            }


            if (e.ClickedItem.Name == "btnCopyPath")
            {
                Clipboard.SetText(filePath);
                PrintMessage("Copied path to clipboard");
            }
            else if (e.ClickedItem.Name == "btnCopyImage")
            {
                StringCollection paths = new StringCollection();
                paths.Add(filePath);
                Clipboard.SetFileDropList(paths);
                PrintMessage("Copied image to clipboard");
            }
            else if (e.ClickedItem.Name == "btnOpenImage")
            {
                System.Diagnostics.Process.Start(filePath);
            }
            else if (e.ClickedItem.Name == "btnDelete")
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(filePath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void txtInvalidDir_TextChanged(object sender, EventArgs e)
        {
            TGMTregistry.GetInstance().SaveRegValue("txtInvalidDir", txtInvalidDir.Text);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void txtFailedDir_TextChanged(object sender, EventArgs e)
        {
            TGMTregistry.GetInstance().SaveRegValue("txtFailedDir", txtFailedDir.Text);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void txtValidDir_TextChanged(object sender, EventArgs e)
        {
            TGMTregistry.GetInstance().SaveRegValue("txtValidDir", txtValidDir.Text);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void FrmDemo_FormClosed(object sender, FormClosedEventArgs e)
        {
            frm_retrainCharacterShowing = false;
        }
    }
}
