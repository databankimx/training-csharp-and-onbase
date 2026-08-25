/*
 * Warning!
 *
 * This is the unedited code downloaded from the Textbook publisher
 * This code does not follow coding standards or best practices and may have bugs/errors
 *
 * Downloaded From:
 * https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Diagnostics;

namespace FileIOAsync
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            this.Text = "Searching...";

            string outputFileName = @"c:\Test\FoundFiles.txt";

            await SearchDirectory(@"c:\Chapter9Samples", "A", outputFileName);

            this.Text = "Finished";

            Process.Start(outputFileName);
        }

        private static async Task SearchDirectory(string searchPath, string searchString, string outputFileName)
        {
            StreamWriter streamWriter = File.CreateText(outputFileName);

            string[] fileNames = Directory.GetFiles(searchPath);
            await FindTextInFilesAsync(fileNames, searchString, streamWriter);

            streamWriter.Close();
        }

        private static async Task FindTextInFilesAsync(string[] fileNames, string searchString, StreamWriter outputFile)
        {
            foreach (string fileName in fileNames)
            {
                if (fileName.ToLower().EndsWith(".txt"))
                {
                    StreamReader streamReader = new StreamReader(fileName);

                    string textOfFile = await streamReader.ReadToEndAsync();
                    streamReader.Close();

                    if (textOfFile.Contains(searchString))
                    {
                        await outputFile.WriteLineAsync(fileName);
                    }
                }
            }
        }
    }
}
