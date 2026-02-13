using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Atalasoft.Imaging;
using Atalasoft.Imaging.Codec;
using Atalasoft.Imaging.Codec.Pdf;
using Atalasoft.Imaging.WinControls;
using System.IO;
using Atalasoft.PdfDoc;


namespace PdfManipulator
{
    public partial class Form1 : Form
    {
        #region FIELDS
        private string _currentFile = null;
        private Form _instructionForm;
        #endregion FIELDS


        static Form1()
        {
            // allows us to get ImageInfo for user-selected PDFs
            // Default resolution is 96 DPI, but we'll explicitly call it to demonstrate how to define
            // We'll also set (annotation) RenderSettings to none so we get nice clean thumbnails
            PdfDecoder pdfDec = new PdfDecoder();
            pdfDec.Resolution = 96;
            pdfDec.RenderSettings = new RenderSettings() { AnnotationSettings = AnnotationRenderSettings.None };
            RegisteredDecoders.Decoders.Add(pdfDec);
        }

        public Form1()
        {
            Splash splashForm = new Splash();
            splashForm.Show();
            
            InitializeComponent();
        }


        #region Instructions Stuff
        private void Form1_Load(object sender, System.EventArgs e)
        {
            System.Threading.Thread.Sleep(500);

            bool show = true;
            string settings = Application.UserAppDataPath + "\\PdfManipulatorSettings.txt";
            if (System.IO.File.Exists(settings))
            {
                using (System.IO.FileStream reader = new System.IO.FileStream(settings, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
                {
                    show = (reader.ReadByte() == 1);
                }
            }

            if (show) ShowInstructions();
        }

        private void ShowInstructions()
        {
            if (_instructionForm == null)
            {
                _instructionForm = new Instructions();
                _instructionForm.Disposed += new EventHandler(_instructionForm_Disposed);
            }
            _instructionForm.Show();
        }

        private void _instructionForm_Disposed(object sender, EventArgs e)
        {
            _instructionForm = null;
        }
        #endregion Instructions Stuff


        #region Misc Interface Items
        private void linkLabel1_LinkClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.atalasoft.com");
        }
        #endregion Misc Interface Items


        #region File Menu
        private void addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileOpener = new OpenFileDialog();
            fileOpener.Filter = "PDF Files (*.pdf)|*.pdf";

            if (fileOpener.ShowDialog() == DialogResult.OK)
                TryAddingFile(fileOpener.FileName);
        }


        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog fileSaver = new SaveFileDialog();
            fileSaver.Filter = "PDF File (*.pdf)|*.pdf";

            if (fileSaver.ShowDialog() == DialogResult.OK)
                TrySavingFile(fileSaver.FileName);
        }


        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion File Menu


        #region Help Menu
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AtalaDemos.AboutBox.About aboutBox = new AtalaDemos.AboutBox.About("About Atalasoft DotImage PDF Manipulator Demo", "DotImage PDF Manipulator Demo");
            aboutBox.Description = "Demonstrates how to manipulate pages in a PDF, and resave without having to recompose the entire PDF. Includes the ability to reoder pages, remove pages, merge multiple PDF files, and save the result.\r\n\r\nThis Windows Forms application demonstrates the use of features in our PdfDoc class that do NOT require the purchase of DotPdf.";
            aboutBox.ShowDialog();
        }

        private void instructionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowInstructions();
        }
        #endregion Help Menu


        #region NonMenu Event Handlers
        private void thumbnailView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                DeleteItems(thumbnailView1.SelectedItems);
            }
        }
        #endregion NonMenu Event Handlers


        #region Main Working Methods
        private void TryAddingFile(string fileName)
        {
            if (CanOpenThisFileAsPdf(fileName))
            {
                if (AlreadyAddedFile(fileName))
                {
                    MessageBox.Show("This file is already in the collection.");
                }
                else
                {
                    _currentFile = fileName;
                    OnFileAdded(fileName);
                }
            }
            else
            {
                MessageBox.Show("Unable to open the file - it might not be a PDF or it might be damaged.");
            }
        }

        private bool AlreadyAddedFile(string fileName)
        {
            foreach (object o in fileList.Items)
            {
                FileFramePair pair = (FileFramePair)o;
                if (pair.FileName == fileName)
                    return true;
            }
            return false;
        }

        private bool CanOpenThisFileAsPdf(string fileName)
        {
            using (FileStream pdfFileStream = new FileStream(fileName, FileMode.Open))
            {
                PdfDecoder pdfdecoder = new PdfDecoder();
                try
                {
                    pdfdecoder.GetImageInfo(pdfFileStream);
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }

        private void OnFileAdded(string fileName)
        {
            ImageInfo info = RegisteredDecoders.GetImageInfo(fileName);
            int offset = thumbnailView1.Items.Count;
            for (int i = 0; i < info.FrameCount; i++)
            {
                AddEachPageAsThumbnail(fileName, i, offset);
            }
            fileList.Items.Add(new FileFramePair(fileName, 0));
        }

        private void AddEachPageAsThumbnail(string fileName, int page, int offset)
        {
            FileFramePair pair = new FileFramePair(fileName, page);
            thumbnailView1.Items.Add(fileName, page, pair.ToString() + "\nPage" + page);
            thumbnailView1.Items[offset + page].Tag = pair;
        }


        private void TrySavingFile(string fileName)
        {
            if (AlreadyAddedFile(fileName))
            {
                MessageBox.Show("Can't save over one of the source files.");
            }
            else
            {
                SaveTo(fileName);
            }
        }

        private void SaveTo(string outFileName)
        {
            if (!HasSomeThumbnailsLoaded()) return;
            
            PdfDocument[] originals = MakePdfDocuments();
            PdfDocument final = new PdfDocument();

            try
            {
                ProcessThumbnails(thumbnailView1.Items, originals, final, outFileName);
            }
            finally
            {
                CloseAllLoadedPdfDocuments(originals);
            }
        }

        private void ProcessThumbnails(ThumbnailCollection thumbnailCollection, PdfDocument[] originals, PdfDocument final, string outFileName)
        {
            foreach (Thumbnail thumb in thumbnailView1.Items)
            {
                FileFramePair fileFrame = (FileFramePair)thumb.Tag;
                PdfDocument original = GetPdfDocument(fileFrame.FileName, originals);
                if (original == null)
                {
                    MessageBox.Show("Unable to find the original source file " + fileFrame.ToString() + " - this should never happen - skipping file.");
                    continue;
                }
                final.Pages.Add(original.Pages[fileFrame.Frame]);
            }
            using (FileStream outStream = new FileStream(outFileName, FileMode.Create))
            {
                final.Save(outStream);
            }
        }

        private void CloseAllLoadedPdfDocuments(PdfDocument[] originals)
        {
            foreach (PdfDocument doc in originals)
            {
                doc.Pages[0].Stream.Close();
            }
        }

        private bool HasSomeThumbnailsLoaded()
        {
            if (thumbnailView1.Items.Count == 0)
            {
                MessageBox.Show("Cannot create a PDF file from zero thumbnails!");
                return false;
            }
            return true;
        }

        private PdfDocument GetPdfDocument(string fileName, PdfDocument[] originals)
        {
            for (int i = 0; i < fileList.Items.Count; i++)
            {
                if (((FileFramePair)fileList.Items[i]).FileName == fileName)
                {
                    return originals[i];
                }
            }
            return null;
        }

        private PdfDocument[] MakePdfDocuments()
        {
            PdfDocument[] pdfArray = new PdfDocument[fileList.Items.Count];
            for (int i = 0; i < pdfArray.Length; i++)
            {
                FileFramePair pair = (FileFramePair)fileList.Items[i];
                pdfArray[i] = new PdfDocument(pair.FileName);
            }
            return pdfArray;
        }

        private void DeleteItems(SelectedThumbnailCollection collection)
        {
            if (collection == null || collection.Count == 0)
                return;

            Thumbnail[] thumbnails = new Thumbnail[collection.Count];
            collection.CopyTo(thumbnails);
            thumbnailView1.Items.Remove(thumbnails);
        }
        #endregion Main Working Methods
    }
}
