using Csv;
using FaceONNX;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using Newtonsoft.Json;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FeaCard
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private XUnit toXunit(double value) => XUnit.FromMillimeter(value);

        private void defButton_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "";
            filebox.Text = "";
            defButton.Enabled = false;
            using (OpenFileDialog opendialog = new OpenFileDialog())
            {
                opendialog.Filter = "json files (*.json)|*.json";
                opendialog.RestoreDirectory = true;
                if (opendialog.ShowDialog() == DialogResult.OK)
                {
                    var definitionFile = opendialog.FileName;

                    var state = new DefinitionState();
                    state.directory = Directory.GetParent(definitionFile).FullName;

                    state.downloadFolder = Path.Combine(state.directory, "downloads");
                    //if (Directory.Exists(state.downloadFolder))
                    //Directory.Delete(state.downloadFolder, true);
                    if (!Directory.Exists(state.downloadFolder))
                        Directory.CreateDirectory(state.downloadFolder);

                    try
                    {
                        var csvFile = Path.Combine(state.directory, "data.csv");
                        var csv = File.ReadAllText(csvFile);
                        state.csvData = CsvReader.ReadFromText(csv).ToList();
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show("Failed to load data.csv file: ");
                        MessageBox.Show(e.ToString());
                        defButton.Enabled = true;
                        return;
                    }

                    try
                    {
                        state.definition = JsonConvert.DeserializeObject<Definition>(File.ReadAllText(definitionFile));
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show("Failed to load and parse definition file: " + error.Message);
                        MessageBox.Show(e.ToString());
                        defButton.Enabled = true;
                        return;
                    }

                    executeDefinition(state);

                }
            }
            defButton.Enabled = true;
        }

        private void executeDefinition(DefinitionState state)
        {
            Command lastCmd = null;
            try
            {
                state.document = new PdfDocument();
                state.document.Info.Title = "Created by Aurora";
                while (true)
                {
                    foreach (var cmd in state.definition.layout)
                    {
                        lastCmd = cmd;
                        executeCommand(state, state.definition, cmd);
                        if (state.finished)
                            break;
                    }
                    if (state.finished)
                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed executing command {lastCmd.ToString()} at csvData Index: " + (state.cvsDataIndex + 1));
                MessageBox.Show(e.ToString());
            }
            string pdfFile = "";
            try
            {
                pdfFile = Path.Combine(state.directory, $"generated_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.pdf");
                state.document.Save(pdfFile);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to save pdf file {pdfFile}");
                MessageBox.Show(e.ToString());
            }
            filebox.Text = pdfFile;
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(pdfFile)
            {
                UseShellExecute = true
            };
            p.Start();
        }

        private void executeCommand(DefinitionState state, Definition definition, Command command)
        {
            if (state.finished)
                return;
            switch (command.cmd)
            {
                case CommandType.NEW_PAGE:
                    var page = state.document.AddPage();
                    page.Width = toXunit(command.width);
                    page.Height = toXunit(command.height);
                    state.pageMap.Remove(command.name);
                    state.pageMap[command.name] = page;
                    state.graphicsMap[command.name] = XGraphics.FromPdfPage(page);
                    break;
                case CommandType.OFFSET:
                    state.x = command.x;
                    state.y = command.y;
                    break;
                case CommandType.INCREMENT:
                    state.x += command.x;
                    state.y += command.y;
                    break;
                case CommandType.REPEAT:
                    for (int loop = 0; loop < command.count; loop++)
                        for (int t = 0; t < command.commands.Length; t++)
                            executeCommand(state, definition, command.commands[t]);
                    break;
                case CommandType.DRAW:
                    var gfx = state.graphicsMap[command.page];
                    var item = definition.items.Where(x => x.name == command.item).FirstOrDefault();
                    drawItem(command, state, gfx, item);
                    break;
                case CommandType.NEXT_CSV_DATA:
                    state.cvsDataIndex++;
                    statusLabel.Text = "PROCESSED CSV DATA " + state.cvsDataIndex;
                    statusLabel.Refresh();
                    if (state.cvsDataIndex >= state.csvData.Count)
                    {
                        statusLabel.Text = "FINISHED PROCESSING " + state.cvsDataIndex;
                        statusLabel.Refresh();
                        state.finished = true;
                    }
                    break;
            }

        }

        public class CheckResult
        {
            public string text { get; set; }
            public bool isStatic { get; set; }
        }

        private void drawItem(Command command, DefinitionState state, XGraphics gfx, Item item)
        {
            foreach (var element in item.elements)
            {
                switch (element.type)
                {
                    case ElementType.IMAGE:
                        var textCheck = fieldCheck(state, element.data.Trim());


                        if (textCheck.text.Contains("http"))
                        {
                            //Thread.Sleep(2000);
                            //var referenceCheck = fieldCheck(state, element.reference.Trim());

                            var idFile = state.csvData[state.cvsDataIndex]["ID"] + ".jpeg";
                            var convertedFile = Path.Combine(state.downloadFolder, idFile);
                            if (File.Exists(convertedFile))
                            {
                                textCheck.text = convertedFile;
                            }
                            else
                            {
                                textCheck.text = textCheck.text.Substring(textCheck.text.IndexOf("http"));
                                if (textCheck.text.Contains("drive.google"))
                                {
                                    textCheck.text = textCheck.text.Replace("open?", "uc?");// + "&export=download";
                                }
                                try
                                {
                                    var downloadFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                                    statusLabel.Text = "DOWNLOADING IMAGE " + state.cvsDataIndex;
                                    statusLabel.Refresh();
                                    //using (WebClient client = new WebClient())
                                    //{
                                    //    client.DownloadFile(new Uri(textCheck.text), downloadFile);
                                    //}


                                    ProcessStartInfo start = new ProcessStartInfo();
                                    start.FileName = "gdown";
                                    start.Arguments = textCheck.text + " -O \"" + downloadFile + "\"";
                                    start.UseShellExecute = false;
                                    start.CreateNoWindow = true;
                                    start.RedirectStandardOutput = true;
                                    start.RedirectStandardError = true;

                                    Process process = Process.Start(start);
                                    Thread thread = new Thread(new ThreadStart(() =>
                                    {
                                        using (StreamReader reader = process.StandardOutput)
                                        {
                                            string result = reader.ReadToEnd();
                                            Console.Write(result);
                                        }
                                    }));
                                    thread.IsBackground = true; thread.Start();

                                    var thread2 = new Thread(new ThreadStart(() =>
                                    {
                                        using (StreamReader reader = process.StandardError)
                                        {
                                            string result = reader.ReadToEnd();
                                            Console.Write(result);
                                        }
                                    }));
                                    thread2.IsBackground = true; thread2.Start();

                                    process.WaitForExit();



                                    using (var downloadStream = File.OpenRead(downloadFile))
                                    {
                                        using (var convertedStream = File.OpenWrite(convertedFile))
                                        {
                                            using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                                            {

                                                imageFactory.Load(downloadStream)
                                                    .AutoRotate()
                                                    .Format(new JpegFormat())
                                                    .Quality(100);

                                                imageFactory.Save(convertedStream);
                                            }
                                        }
                                    }
                                    textCheck.text = convertedFile;
                                }
                                catch (Exception e)
                                {
                                    if (File.Exists(convertedFile))
                                        File.Delete(convertedFile);
                                    errorBox.Text = errorBox.Text + "Failed to convert item " + (state.cvsDataIndex + 1) + " for file " + convertedFile+"\r\n";
                                    errorBox.Refresh();
                                    textCheck.text = "";
                                }

                            }


                        }

                        if (string.IsNullOrEmpty(textCheck.text.Trim()))
                            return;

                        var filename = Path.Combine(state.directory, textCheck.text);
                        XImage logoImage;
                        if (element.face)
                        {
                            logoImage = XImageFromCroppedFace(filename);
                            if (logoImage == null) //use the image as is if face not found
                                logoImage = XImage.FromFile(filename);
                        }
                        else
                        {
                            if (textCheck.isStatic)
                                logoImage = getStaticImage(filename);
                            else
                                logoImage = XImage.FromFile(filename);
                        }

                        XRect logoRect = new XRect(toXunit(state.x + command.x + element.x),
                            toXunit(state.y + command.y + element.y),
                            toXunit(element.width),
                            toXunit(element.height)
                            );
                        gfx.DrawImage(logoImage, logoRect);
                        break;
                    case ElementType.TEXT:
                        XPoint labelPoint = new XPoint(
                            toXunit(state.x + command.x + element.x),
                            toXunit(state.y + command.y + element.y));

                        var textCheck2 = fieldCheck(state, element.data.Trim());

                        XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode);

                        var labelFont = new XFont(element.font, element.size, element.style, options);
                        var align = element.align == TextAlign.LEFT ? XStringFormats.BottomLeft : (element.align == TextAlign.CENTER ? XStringFormats.BottomCenter : XStringFormats.BottomRight);
                        gfx.DrawString(textCheck2.text, labelFont, new XSolidBrush(convertColor(element.color)), labelPoint, align);
                        break;
                    case ElementType.BARCODE:
                        var barcodeValue = fieldCheck(state, element.data.Trim());

                        XRect barcodeRect = new XRect(toXunit(state.x + command.x + element.x),
                            toXunit(state.y + command.y + element.y),
                            toXunit(element.width),
                            toXunit(element.height)
                            );

                        BarcodeLib.Barcode b = new BarcodeLib.Barcode();
                        Image img = b.Encode(element.barcodeType, barcodeValue.text, Color.Black, Color.White, 512, 256);

                        MemoryStream strm = new MemoryStream();
                        img.Save(strm, System.Drawing.Imaging.ImageFormat.Png);
                        XImage xfoto = XImage.FromStream(() => { strm.Position = 0; return strm; });
                        gfx.DrawImage(xfoto, barcodeRect);

                        break;
                }
            }
        }

        private XImage XImageFromCroppedFace(string filename)
        {
            try
            {
                using var faceDetector = new FaceDetector(0.75f, 0.5f);
                using var bitmap = new Bitmap(filename);
                var output = faceDetector.Forward(bitmap);

                if (output.Length < 1)
                    return null;

                var rectangle = output[0];
                var marginX = rectangle.Width / 20;
                var marginY = rectangle.Height / 5;
                rectangle = new Rectangle(rectangle.X - marginX, rectangle.Y - marginY, rectangle.Width + (marginX * 2), rectangle.Height + (marginY * 2));

                rectangle = new Rectangle(Math.Max(0, rectangle.X), Math.Max(0, rectangle.Y),
                    (rectangle.Width + Math.Max(0, rectangle.X)) > bitmap.Width ? (bitmap.Width - Math.Max(0, rectangle.X)) : (rectangle.Width),
                    (rectangle.Height + Math.Max(0, rectangle.Y)) > bitmap.Height ? (bitmap.Height - Math.Max(0, rectangle.Y)) : (rectangle.Height));

                Image cropped = new Bitmap(rectangle.Width, rectangle.Height);
                var g = Graphics.FromImage(bitmap);

                var gc = Graphics.FromImage(cropped);

                gc.DrawImage(bitmap,
                    new RectangleF(0, 0, rectangle.Width, rectangle.Height),
                    new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height),
                    GraphicsUnit.Pixel);

                MemoryStream strm = new MemoryStream();
                cropped.Save(strm, System.Drawing.Imaging.ImageFormat.Png);
                XImage xcropped = XImage.FromStream(() => { strm.Position = 0; return strm; });
                return xcropped;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private CheckResult fieldCheck(DefinitionState state, string text)
        {
            if (!(text.Contains('<') && text.Contains('>')))
            {
                return new CheckResult { text = text, isStatic = true };
            }
            StringBuilder result = new StringBuilder();
            StringBuilder cap = new StringBuilder();
            bool capture = false;
            for (int t = 0; t < text.Length; t++)
            {
                var c = text[t];
                if (c == '<')
                {
                    capture = true;
                    continue;
                }
                if (c == '>')
                {
                    capture = false;
                    var field = cap.ToString();
                    cap.Clear();
                    result.Append(state.csvData[state.cvsDataIndex][field]);
                    continue;
                }
                if (capture)
                    cap.Append(c);
                else
                    result.Append(c);
            }
            return new CheckResult { text = result.ToString(), isStatic = false };
        }

        private XColor convertColor(string hexa)
        {
            var cc1 = System.Drawing.ColorTranslator.FromHtml(hexa);
            return XColor.FromArgb(cc1.A, cc1.R, cc1.G, cc1.B);
        }

        private Dictionary<string, XImage> staticImageCache = new Dictionary<string, XImage>();

        public XImage getStaticImage(string filename)
        {
            if (!staticImageCache.ContainsKey(filename))
            {
                var image = XImage.FromFile(filename);
                staticImageCache[filename] = image;
            }
            return staticImageCache[filename];
        }
    }

    public class DefinitionState
    {
        public Definition definition;
        public List<ICsvLine> csvData = new List<ICsvLine>();
        public PdfDocument document;
        public Dictionary<string, PdfPage> pageMap = new Dictionary<string, PdfPage>();
        public Dictionary<string, XGraphics> graphicsMap = new Dictionary<string, XGraphics>();
        public double x;
        public double y;
        public int cvsDataIndex;
        public bool finished;
        public string directory;
        public string downloadFolder;
    }

    public class Definition
    {

        public Command[] layout;

        public Item[] items;

    }

    public class Command
    {
        public CommandType cmd;
        public double x;
        public double y;
        public double width;
        public double height;
        public int count = 1;
        public Command[] commands;
        public string item;
        public string name;
        public string page;
    }

    public enum CommandType
    {
        OFFSET,
        REPEAT,
        DRAW,
        INCREMENT,
        NEW_PAGE,
        NEXT_CSV_DATA
    }

    public class Item
    {
        public string name;
        public Element[] elements;
    }

    public class Element
    {
        public ElementType type;
        public string data;
        public string reference = "";
        public double x;
        public double y;
        public double width;
        public double height;
        public double size;
        public XFontStyle style = XFontStyle.Regular;
        public string font = "Arial";
        public string color = "#000000";
        public BarcodeLib.TYPE barcodeType = BarcodeLib.TYPE.CODE128;
        public TextAlign align = TextAlign.LEFT;
        public bool face;
    }

    public enum TextAlign
    {
        LEFT,
        CENTER,
        RIGHT
    }

    public enum ElementType
    {
        IMAGE,
        TEXT,
        BARCODE
    }

}
