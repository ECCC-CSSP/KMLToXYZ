using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using System.Xml;

namespace KMLToXYZ
{
    public partial class KMLtoXYZ : Form
    {
        List<ColorVal> ColorValList = new List<ColorVal>();

        XmlDocument xmlDoc;
        double doubleLowerLeftCornerX = 0;
        double doubleLowerLeftCornerY = 0;
        double doubleUpperRightCornerX = 0;
        double doubleUpperRightCornerY = 0;
        bool DoFilter = false;

        public class dPoint
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
        }

        public KMLtoXYZ()
        {
            InitializeComponent();
            xmlDoc = new XmlDocument();
            listBoxChartNames.DisplayMember = "Name";
            listBoxChartNames.ValueMember = "ID";
        }


        private void butOpenFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            FileInfo fi = new FileInfo(openFileDialog1.FileName);

            if (!fi.Exists)
            {
                MessageBox.Show(string.Format("File Name [{0}] does not exits.", openFileDialog1.FileName));
                return;
            }

            if (fi.Extension.ToLower() != ".kml")
            {
                MessageBox.Show(string.Format("Please select a valid KML file. Current selection is [{0}]", openFileDialog1.FileName));
                return;
            }

            lblFilePath.Text = openFileDialog1.FileName;

            StringBuilder sb = new StringBuilder();

            treeViewKMLFile.Nodes.Clear();

            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (StreamReader sr = new StreamReader(lblFilePath.Text))
                {
                    String line;
                    // Read and display lines from the file until the end of
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        sb.AppendLine(line);
                    }
                }

                richTextBoxKMLInput.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            ParseKMLFromRichTextBoxKMLInput();

        }


        private bool ParseKMLFromRichTextBoxKMLInput()
        {

            FileInfo fi = new FileInfo(lblFilePath.Text);

            if (!fi.Exists)
            {
                MessageBox.Show("File does not exist. [" + lblFilePath.Text + "]");
                return false;
            }

            xmlDoc.Load(lblFilePath.Text);

            if (xmlDoc.ChildNodes.Count != 2)
            {
                MessageBox.Show("KML File not well form.");
                return false;
            }

            if (xmlDoc.ChildNodes[1].Name.ToLower() != "kml")
            {
                MessageBox.Show("KML File not well form.");
                return false;
            }

            if (xmlDoc.ChildNodes[1].ChildNodes.Count != 1)
            {
                MessageBox.Show("KML File not well form.");
                return false;
            }

            if (xmlDoc.ChildNodes[1].ChildNodes[0].Name.ToLower() != "document")
            {
                MessageBox.Show("KML File not well form.");
                return false;
            }

            int count = 0;
            //Loop child nodes of Document tag
            foreach (XmlNode n in xmlDoc.ChildNodes[1].ChildNodes[0].ChildNodes)
            {
                TreeNode TopNode;
                string Tag = "";
                Tag = "1,0," + count;

                switch (n.Name.ToLower())
                {
                    case "name":
                        TopNode = new TreeNode();
                        TopNode.Text = n.InnerText.Trim();
                        TopNode.Tag = Tag;
                        treeViewKMLFile.Nodes.Add(TopNode);
                        break;
                    case "folder":
                        ParseChildNode(n, Tag, treeViewKMLFile.Nodes[0]);
                        break;
                    //case "style":
                    //    break;
                    default:
                        break;
                }
                count += 1;
            }

            return true;
        }

        private void ParseChildNode(XmlNode KMLParentNode, string Tag, TreeNode TVParentNode)
        {
            int count = 0;
            //Loop child nodes of Document tag
            TreeNode NewNode = new TreeNode();
            string NewTag = "";
            string NewTag2 = "";
            string NewTag3 = "";

            foreach (XmlNode n in KMLParentNode.ChildNodes)
            {
                NewTag = Tag + "," + count;

                switch (n.Name.ToLower())
                {
                    case "name":
                        NewNode = new TreeNode();
                        NewNode.Text = n.InnerText.Trim();
                        NewNode.Tag = Tag;
                        TVParentNode.Nodes.Add(NewNode);
                        break;
                    case "groundoverlay":
                        //if (!CreateBMPWFile(n))
                        //    return;
                        break;
                    case "placemark":
                        {
                            double Z = 0.0;
                            string NodeText = "";
                            //TreeNode DepthNode = new TreeNode();
                            int count2 = 0;
                            foreach (XmlNode nChild in n.ChildNodes)
                            {
                                NewTag2 = NewTag + "," + count2;

                                switch (nChild.Name.ToLower())
                                {
                                    case "name":
                                        //DepthNode = new TreeNode();
                                        //DepthNode.Text = nChild.InnerText.Trim();
                                        if (!double.TryParse(nChild.InnerText.Trim(), out Z))
                                        {
                                            NodeText = nChild.InnerText.Trim();
                                        }
                                        //DepthNode.Tag = NewTag2;
                                        //NewNode.Nodes.Add(DepthNode);
                                        break;
                                    case "linestring":
                                        {
                                            int count3 = 0;
                                            foreach (XmlNode CoordNode in nChild.ChildNodes)
                                            {
                                                NewTag3 = NewTag2 + "," + count3;

                                                TreeNode ParsedNode;

                                                switch (CoordNode.Name.ToLower())
                                                {
                                                    case "coordinates":
                                                        StringBuilder sb = new StringBuilder();
                                                        sb.Append(CoordNode.InnerText.Trim());
                                                        //List<dPoint> dPoints = new List<dPoint>();
                                                        //dPoints = ParseKMLCoordinates(sb, Z);
                                                        ParsedNode = new TreeNode();
                                                        if (Z == 0.0)
                                                        {
                                                            ParsedNode.Text = NodeText;
                                                        }
                                                        else
                                                        {
                                                            ParsedNode.Text = Z.ToString();
                                                        }
                                                        ParsedNode.Tag = NewTag3;
                                                        NewNode.Nodes.Add(ParsedNode);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                                count3 += 1;
                                            }
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                count2 += 1;
                            }
                        }
                        break;
                    case "folder":
                        ParseChildNode(n, NewTag, NewNode);
                        break;
                    //case "style":
                    //    break;
                    default:
                        break;
                }
                count += 1;
            }

        }

        private List<dPoint> ParseKMLCoordinates(StringBuilder sbCoordinates, double Z)
        {
            List<dPoint> dPoints = new List<dPoint>();

            if (sbCoordinates.Length == 0)
            {
                return null;
            }

            if (sbCoordinates.ToString().Trim().First() != char.Parse("-"))
            {
                return null;
            }


            List<string> coordinateList = sbCoordinates.ToString().Trim().Split(new char[] { char.Parse(" ") }).ToList<string>();
            foreach (string s in coordinateList)
            {
                List<string> StringPoints = s.Trim().Split(new char[] { char.Parse(",") }).ToList<string>();
                try
                {
                    double xVal = double.Parse(StringPoints[0]);
                    double yVal = double.Parse(StringPoints[1]);
                    double zVal = Z;

                    if (DoFilter)
                    {
                        if (xVal >= doubleLowerLeftCornerX && xVal <= doubleUpperRightCornerX)
                        {
                            if (yVal >= doubleLowerLeftCornerY && yVal <= doubleUpperRightCornerY)
                            {
                                dPoints.Add(new dPoint() { X = xVal, Y = yVal, Z = zVal });
                            }
                        }
                    }
                    else
                    {
                        dPoints.Add(new dPoint() { X = xVal, Y = yVal, Z = zVal });
                    }
                }
                catch (Exception)
                {
                    dPoints = null;
                }
            }

            richTextBoxKMLStat.Text = "";
            richTextBoxKMLStat.AppendText("Number of Points: " + dPoints.Count + "\r\n");
            return dPoints;
        }

        private StringBuilder GetdPoints(List<dPoint> dPoints)
        {
            StringBuilder sb = new StringBuilder();

            if (dPoints != null)
            {
                foreach (dPoint dp in dPoints)
                {
                    sb.AppendLine(dp.X + " " + dp.Y + " " + dp.Z);
                }
            }

            return sb;
        }

        private void TreeViewSelected(TreeNode tn)
        {
            List<string> Path = tn.Tag.ToString().Split(new char[] { char.Parse(",") }).ToList<string>();
            StringBuilder sb = new StringBuilder();
            double Z = 0.0;
            double.TryParse(tn.Text, out Z);

            switch (Path.Count)
            {
                case 1:
                    sb.Append(xmlDoc.ChildNodes[int.Parse(Path[0])].InnerText);
                    break;
                case 2:
                    sb.Append(xmlDoc.ChildNodes[int.Parse(Path[0])].ChildNodes[int.Parse(Path[1])].InnerText);
                    break;
                case 3:
                    sb.Append(xmlDoc.ChildNodes[int.Parse(Path[0])].ChildNodes[int.Parse(Path[1])].ChildNodes[int.Parse(Path[2])].InnerText);
                    break;
                case 4:
                    sb.Append(xmlDoc.ChildNodes[int.Parse(Path[0])].ChildNodes[int.Parse(Path[1])].ChildNodes[int.Parse(Path[2])].ChildNodes[int.Parse(Path[3])].InnerText);
                    break;
                case 5:
                    sb.Append(xmlDoc.ChildNodes[int.Parse(Path[0])].ChildNodes[int.Parse(Path[1])].ChildNodes[int.Parse(Path[2])].ChildNodes[int.Parse(Path[3])].ChildNodes[int.Parse(Path[4])].InnerText);
                    break;
                case 6:
                    sb.Append(xmlDoc.ChildNodes[int.Parse(Path[0])].ChildNodes[int.Parse(Path[1])].ChildNodes[int.Parse(Path[2])].ChildNodes[int.Parse(Path[3])].ChildNodes[int.Parse(Path[4])].ChildNodes[int.Parse(Path[5])].InnerText);
                    break;
                case 7:
                    sb.Append(xmlDoc.ChildNodes[int.Parse(Path[0])].ChildNodes[int.Parse(Path[1])].ChildNodes[int.Parse(Path[2])].ChildNodes[int.Parse(Path[3])].ChildNodes[int.Parse(Path[4])].ChildNodes[int.Parse(Path[5])].ChildNodes[int.Parse(Path[6])].InnerText);
                    break;
                case 8:
                    sb.Append(xmlDoc.ChildNodes[int.Parse(Path[0])].ChildNodes[int.Parse(Path[1])].ChildNodes[int.Parse(Path[2])].ChildNodes[int.Parse(Path[3])].ChildNodes[int.Parse(Path[4])].ChildNodes[int.Parse(Path[5])].ChildNodes[int.Parse(Path[6])].ChildNodes[int.Parse(Path[7])].InnerText);
                    break;
                default:
                    break;
            }
            richTextBoxItemXYZOutput.Text = GetdPoints(ParseKMLCoordinates(sb, Z)).ToString();
            richTextBoxAppendXYZOutput.AppendText(richTextBoxItemXYZOutput.Text);

        }
        private void treeViewKMLFile_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DoFilter = false;
            TreeViewSelected(e.Node);
        }

        private void butSaveXYZ_Click(object sender, EventArgs e)
        {
            string FileNameXYZ = "";
            saveFileDialogAppendXYZOutput.DefaultExt = ".xyz";
            saveFileDialogAppendXYZOutput.Filter = "XYZ files|*.xyz";
            saveFileDialogAppendXYZOutput.ShowDialog();

            FileNameXYZ = saveFileDialogAppendXYZOutput.FileName;

            if (!saveFileDialogAppendXYZOutput.CheckPathExists)
            {
                MessageBox.Show("Path of file does not exist. Please check path.");
                return;
            }

            if (saveFileDialogAppendXYZOutput.CheckFileExists)
            {

                if (MessageBox.Show("Do you want to replace it?", "File already exist", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }

            }

            richTextBoxAppendXYZOutput.SaveFile(FileNameXYZ, RichTextBoxStreamType.PlainText);

        }

        private void KMLtoXYZ_Load(object sender, EventArgs e)
        {
            using (BathymetryEntities be = new BathymetryEntities())
            {
                List<BathName> BathyNameList = (from b in be.CHSCharts
                                                where !b.CHSChartName.Contains("SOUNDG")
                                                orderby b.CHSChartName
                                                select new BathName
                                                {
                                                    ID = b.CHSChartID,
                                                    Name = b.CHSChartName
                                                }).ToList<BathName>();

                listBoxChartNames.DataSource = BathyNameList;
                ColorValList = FillColorValues();
            }
        }

        private void butCreateKML_Click(object sender, EventArgs e)
        {
            List<CHSChart> chsChartList = new List<CHSChart>();
            List<CHSDepth> chsDepthList = new List<CHSDepth>();

            // checking if the directory exist if not should create the directory
            DirectoryInfo di = new DirectoryInfo(textBoxDirectoryPath.Text);

            if (!di.Exists)
            {
                di.Create();
            }

            foreach (BathName bn in listBoxChartNames.SelectedItems)
            {
                lblStatusTxt2.Text = "Doing " + bn.Name;
                lblStatusTxt2.Refresh();
                Application.DoEvents();

                using (BathymetryEntities be = new BathymetryEntities())
                {
                    if (checkBoxIncludeSounds.Checked == true)
                    {
                        chsChartList = (from c in be.CHSCharts
                                        where c.CHSChartName.StartsWith(bn.Name)
                                        select c).ToList<CHSChart>();
                    }
                    else
                    {
                        chsChartList = (from c in be.CHSCharts
                                        where c.CHSChartName == bn.Name
                                        select c).ToList<CHSChart>();
                    }
                }

                if (chsChartList.Count == 0)
                {
                    if (checkBoxIncludeSounds.Checked == true)
                    {
                        MessageBox.Show("Error while trying to find [" + bn.Name + "] or [" + bn.Name + "SOUNDG" + "] in CHSCharts");
                    }
                    else
                    {
                        MessageBox.Show("Error while trying to find [" + bn.Name + "] in CHSCharts");
                    }
                }

                foreach (CHSChart chs in chsChartList)
                {
                    StringBuilder sb = new StringBuilder();

                    if (chs.CHSChartName.Contains("SOUNDG"))
                    {
                        TopOfKML(sb, chs.CHSChartName);

                        lblStatusTxt2.Text = "Doing " + chs.CHSChartName + " ... loading data from DB this could take a minute.";
                        lblStatusTxt2.Refresh();
                        Application.DoEvents();

                        using (BathymetryEntities be = new BathymetryEntities())
                        {
                            chsDepthList = (from cd in be.CHSDepths
                                            where cd.CHSChartID == chs.CHSChartID
                                            && cd.LineValue == -999
                                            && cd.Depth >= 0
                                            orderby cd.Depth, cd.LineValue
                                            select cd).ToList<CHSDepth>();
                        }

                        int CountDepth = 0;
                        foreach (CHSDepth d in chsDepthList.OrderBy(d => d.Depth))
                        {
                            CountDepth += 1;
                            lblStatusTxt2.Text = "Doing " + bn.Name + " --- " + CountDepth;
                            lblStatusTxt2.Refresh();
                            Application.DoEvents();
                            double SoundBlockSize = double.Parse(textBoxBlockSize.Text);
                            Application.DoEvents();

                            sb.AppendLine(@"		<Placemark>");
                            sb.AppendLine(@"			<name>-" + d.Depth + "</name>");
                            sb.AppendLine(@"			<styleUrl>#" + GetColorStyleID((double)d.Depth, ColorValList) + "</styleUrl>");
                            sb.AppendLine(@"		    <LineString>");
                            sb.AppendLine(@"				<tessellate>1</tessellate>");
                            sb.AppendLine(@"			    <coordinates>");
                            sb.Append(string.Format("{0},{1},0 ", d.Longitude - SoundBlockSize, d.Latitude - SoundBlockSize));
                            sb.Append(string.Format("{0},{1},0 ", d.Longitude - SoundBlockSize, d.Latitude + SoundBlockSize));
                            sb.Append(string.Format("{0},{1},0 ", d.Longitude + SoundBlockSize, d.Latitude + SoundBlockSize));
                            sb.Append(string.Format("{0},{1},0 ", d.Longitude + SoundBlockSize, d.Latitude - SoundBlockSize));
                            sb.Append(string.Format("{0},{1},0 ", d.Longitude - SoundBlockSize, d.Latitude - SoundBlockSize));
                            sb.AppendLine();
                            sb.AppendLine(@"			    </coordinates>");
                            sb.AppendLine(@"	        </LineString>");
                            sb.AppendLine(@"		</Placemark>");
                        }
                        BottomOfKML(sb);

                        StreamWriter sw = File.CreateText(textBoxDirectoryPath.Text + bn.Name + "SOUNDG.kml");
                        sw.Write(sb);
                        sw.Flush();
                        sw.Close();

                        richTextBoxKMLFile.Text = sb.ToString();

                    }
                    else
                    {
                        TopOfKML(sb, chs.CHSChartName);

                        lblStatusTxt2.Text = "Doing " + chs.CHSChartName + " ... loading data from DB this could take a minute.";
                        lblStatusTxt2.Refresh();
                        Application.DoEvents();

                        using (BathymetryEntities be = new BathymetryEntities())
                        {
                            chsDepthList = (from cd in be.CHSDepths
                                            where cd.CHSChartID == chs.CHSChartID
                                            && cd.Depth >= 0
                                            orderby cd.Depth, cd.LineValue
                                            select cd).ToList<CHSDepth>();

                        }

                        double OldLineValue = -999;
                        int CountDepth = 0;
                        foreach (CHSDepth cd in chsDepthList)
                        {
                            CountDepth += 1;
                            lblStatusTxt2.Text = "Doing " + bn.Name + " --- " + CountDepth;
                            lblStatusTxt2.Refresh();
                            Application.DoEvents();
                            if (cd.LineValue != OldLineValue)
                            {
                                if (CountDepth != 1)
                                {
                                    sb.AppendLine();
                                    sb.AppendLine(@"				</coordinates>");
                                    sb.AppendLine(@"			</LineString>");
                                    sb.AppendLine(@"		</Placemark>");
                                }

                                OldLineValue = (double)cd.LineValue;

                                sb.AppendLine(@"		<Placemark>");
                                if (cd.Depth == 0)
                                {
                                    sb.AppendLine(@"			<name>" + cd.Depth + "</name>");
                                }
                                else
                                {
                                    sb.AppendLine(@"			<name>-" + cd.Depth + "</name>");
                                }
                                sb.AppendLine(@"			<styleUrl>#" + GetColorStyleID((double)cd.Depth, ColorValList) + "</styleUrl>");
                                sb.AppendLine(@"			<LineString>");
                                sb.AppendLine(@"				<tessellate>1</tessellate>");
                                sb.AppendLine(@"				<coordinates>");
                            }
                            else
                            {
                                sb.Append(string.Format("{0},{1},0 ", cd.Longitude, cd.Latitude));
                            }
                        }
                        sb.AppendLine();
                        sb.AppendLine(@"				</coordinates>");
                        sb.AppendLine(@"			</LineString>");
                        sb.AppendLine(@"		</Placemark>");

                        BottomOfKML(sb);

                        StreamWriter sw = File.CreateText(textBoxDirectoryPath.Text + bn.Name + ".kml");
                        sw.Write(sb);
                        sw.Flush();
                        sw.Close();

                        richTextBoxKMLFile.Text = sb.ToString();

                    }

                }
            }
        }
        private void butCreateXYZ_Click(object sender, EventArgs e)
        {
            List<CHSChart> chsChartList = new List<CHSChart>();
            List<CHSDepth> chsDepthList = new List<CHSDepth>();

            // checking if the directory exist if not should create the directory
            DirectoryInfo di = new DirectoryInfo(textBoxDirectoryPath.Text);

            if (!di.Exists)
            {
                di.Create();
            }

            foreach (BathName bn in listBoxChartNames.SelectedItems)
            {
                lblStatusTxt2.Text = "Doing " + bn.Name;
                lblStatusTxt2.Refresh();
                Application.DoEvents();

                using (BathymetryEntities be = new BathymetryEntities())
                {
                    if (checkBoxIncludeSounds.Checked == true)
                    {
                        chsChartList = (from c in be.CHSCharts
                                        where c.CHSChartName.StartsWith(bn.Name)
                                        select c).ToList<CHSChart>();
                    }
                    else
                    {
                        chsChartList = (from c in be.CHSCharts
                                        where c.CHSChartName == bn.Name
                                        select c).ToList<CHSChart>();
                    }
                }

                if (chsChartList.Count == 0)
                {
                    if (checkBoxIncludeSounds.Checked == true)
                    {
                        MessageBox.Show("Error while trying to find [" + bn.Name + "] or [" + bn.Name + "SOUNDG" + "] in CHSCharts");
                    }
                    else
                    {
                        MessageBox.Show("Error while trying to find [" + bn.Name + "] in CHSCharts");
                    }
                }

                foreach (CHSChart chs in chsChartList)
                {
                    StringBuilder sb = new StringBuilder();

                    if (chs.CHSChartName.Contains("SOUNDG"))
                    {
                        //TopOfKML(sb, chs.CHSChartName);

                        lblStatusTxt2.Text = "Doing " + chs.CHSChartName + " ... loading data from DB this could take a minute.";
                        lblStatusTxt2.Refresh();
                        Application.DoEvents();

                        using (BathymetryEntities be = new BathymetryEntities())
                        {
                            chsDepthList = (from cd in be.CHSDepths
                                            where cd.CHSChartID == chs.CHSChartID
                                            && cd.LineValue == -999
                                            && cd.Depth >= 0
                                            orderby cd.Depth, cd.LineValue
                                            select cd).ToList<CHSDepth>();

                        }

                        int CountDepth = 0;
                        foreach (CHSDepth d in chsDepthList.OrderBy(d => d.Depth))
                        {
                            CountDepth += 1;
                            lblStatusTxt2.Text = "Doing " + bn.Name + " --- " + CountDepth;
                            lblStatusTxt2.Refresh();
                            Application.DoEvents();
                            double SoundBlockSize = double.Parse(textBoxBlockSize.Text);
                            Application.DoEvents();

                            sb.AppendLine(string.Format("{0},{1},{2} ", d.Longitude, d.Latitude, (d.Depth*-1.0f)));
                        }

                        StreamWriter sw = File.CreateText(textBoxDirectoryPath.Text + bn.Name + "SOUNDG.xyz");
                        sw.Write(sb);
                        sw.Flush();
                        sw.Close();

                        richTextBoxKMLFile.Text = sb.ToString();

                    }
                    else
                    {

                        lblStatusTxt2.Text = "Doing " + chs.CHSChartName + " ... loading data from DB this could take a minute.";
                        lblStatusTxt2.Refresh();
                        Application.DoEvents();

                        using (BathymetryEntities be = new BathymetryEntities())
                        {
                            chsDepthList = (from cd in be.CHSDepths
                                            where cd.CHSChartID == chs.CHSChartID
                                            select cd).ToList<CHSDepth>();

                        }

                        int CountDepth = 0;
                        foreach (CHSDepth cd in chsDepthList)
                        {
                            CountDepth += 1;
                            lblStatusTxt2.Text = "Doing " + bn.Name + " --- " + CountDepth;
                            lblStatusTxt2.Refresh();
                            Application.DoEvents();

                            sb.AppendLine(string.Format("{0},{1},{2} ", cd.Longitude, cd.Latitude, (cd.Depth*-1.0f)));
                        }

                        StreamWriter sw = File.CreateText(textBoxDirectoryPath.Text + bn.Name + ".xyz");
                        sw.Write(sb);
                        sw.Flush();
                        sw.Close();

                        richTextBoxKMLFile.Text = sb.ToString();

                    }

                }
            }
        }

        private void BottomOfKML(StringBuilder sb)
        {
            sb.AppendLine(@"	</Folder>");
            sb.AppendLine(@"</Document>");
            sb.AppendLine(@"</kml>");
        }
        private string GetColorStyleID(double ColVal, List<ColorVal> ColorValList)
        {
            string ColValStr = "";
            ColorVal lowVal = (from cv in ColorValList where cv.Value <= ColVal orderby cv.Value descending select cv).First();
            ColorVal highVal = (from cv in ColorValList where cv.Value >= ColVal orderby cv.Value select cv).First();

            if ((ColVal - lowVal.Value) < (highVal.Value - ColVal))
            {
                ColValStr = "C_" + lowVal.Value.ToString().Replace(".", "_");
            }
            else
            {
                ColValStr = "C_" + highVal.Value.ToString().Replace(".", "_"); ;
            }
            return ColValStr;
        }
        private List<ColorVal> FillColorValues()
        {
            List<ColorVal> ColorValList = new List<ColorVal>();
            ColorValList.Add(new ColorVal() { Value = -100000, ColorHexStr = "ffffffff" });
            ColorValList.Add(new ColorVal() { Value = 0, ColorHexStr = "ffffffff" });
            ColorValList.Add(new ColorVal() { Value = 0.1, ColorHexStr = "ff0000ff" });
            ColorValList.Add(new ColorVal() { Value = 0.3, ColorHexStr = "ff0033ff" });
            ColorValList.Add(new ColorVal() { Value = 0.5, ColorHexStr = "ff0066ff" });
            ColorValList.Add(new ColorVal() { Value = 0.8, ColorHexStr = "ff0099ff" });
            ColorValList.Add(new ColorVal() { Value = 1, ColorHexStr = "ff00ccff" });
            ColorValList.Add(new ColorVal() { Value = 2, ColorHexStr = "ff00ffff" });
            ColorValList.Add(new ColorVal() { Value = 3, ColorHexStr = "ff00ffcc" });
            ColorValList.Add(new ColorVal() { Value = 5, ColorHexStr = "ff00ff99" });
            ColorValList.Add(new ColorVal() { Value = 7, ColorHexStr = "ff00ff66" });
            ColorValList.Add(new ColorVal() { Value = 10, ColorHexStr = "ff00ff33" });
            ColorValList.Add(new ColorVal() { Value = 12, ColorHexStr = "ff00ff00" });
            ColorValList.Add(new ColorVal() { Value = 15, ColorHexStr = "ff00cc00" });
            ColorValList.Add(new ColorVal() { Value = 20, ColorHexStr = "ff009900" });
            ColorValList.Add(new ColorVal() { Value = 30, ColorHexStr = "ffff0000" });
            ColorValList.Add(new ColorVal() { Value = 45, ColorHexStr = "ffff0033" });
            ColorValList.Add(new ColorVal() { Value = 70, ColorHexStr = "ffff0066" });
            ColorValList.Add(new ColorVal() { Value = 100, ColorHexStr = "ffff0099" });
            ColorValList.Add(new ColorVal() { Value = 140, ColorHexStr = "ffff00cc" });
            ColorValList.Add(new ColorVal() { Value = 200, ColorHexStr = "ffff00ff" });
            ColorValList.Add(new ColorVal() { Value = 250, ColorHexStr = "ffcc00ff" });
            ColorValList.Add(new ColorVal() { Value = 400, ColorHexStr = "ff9900ff" });
            ColorValList.Add(new ColorVal() { Value = 600, ColorHexStr = "ff6600ff" });
            ColorValList.Add(new ColorVal() { Value = 900, ColorHexStr = "ff3300ff" });
            ColorValList.Add(new ColorVal() { Value = 1400, ColorHexStr = "ff0000ff" });
            ColorValList.Add(new ColorVal() { Value = 2000, ColorHexStr = "ffcccccc" });
            ColorValList.Add(new ColorVal() { Value = 3000, ColorHexStr = "ff999999" });
            ColorValList.Add(new ColorVal() { Value = 5000, ColorHexStr = "ff666666" });
            ColorValList.Add(new ColorVal() { Value = 7500, ColorHexStr = "ff333333" });
            ColorValList.Add(new ColorVal() { Value = 10000, ColorHexStr = "ff000000" });
            return ColorValList;
        }
        private string TopOfKML(StringBuilder sb, string DocName)
        {
            BathymetryEntities be = new BathymetryEntities();

            var chsDepthList = (from d in be.CHSDepths where d.LineValue > 0 select d.Depth).Distinct().ToList();

            sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
            sb.AppendLine(@"<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">");
            sb.AppendLine(@"<Document>");
            sb.AppendLine(@"	<name>" + DocName + "</name>");
            foreach (ColorVal ColVal in FillColorValues())
            {
                sb.AppendLine(@"	<Style id=""C_" + ColVal.Value.ToString().Replace(".", "_") + @""">");
                sb.AppendLine(@"		<LineStyle>");
                sb.AppendLine(@"			<color>" + ColVal.ColorHexStr + "</color>");
                sb.AppendLine(@"			<width>1</width>");
                sb.AppendLine(@"		</LineStyle>");
                sb.AppendLine(@"		<PolyStyle>");
                sb.AppendLine(@"			<color>" + ColVal.ColorHexStr + "</color>");
                sb.AppendLine(@"			<width>1</width>");
                sb.AppendLine(@"		</PolyStyle>");
                sb.AppendLine(@"		<IconStyle>");
                sb.AppendLine(@"			<color>" + ColVal.ColorHexStr + "</color>");
                sb.AppendLine(@"			<scale>1.3</scale>");
                sb.AppendLine(@"			<Icon>");
                sb.AppendLine(@"				<href>http://maps.google.com/mapfiles/kml/pushpin/ylw-pushpin.png</href>");
                sb.AppendLine(@"			</Icon>");
                sb.AppendLine(@"			<hotSpot x=""20"" y=""2"" xunits=""pixels"" yunits=""pixels""/>");
                sb.AppendLine(@"		</IconStyle>");
                sb.AppendLine(@"		<LabelStyle>");
                sb.AppendLine(@"			<color>" + ColVal.ColorHexStr + "</color>");
                sb.AppendLine(@"		</LabelStyle>");
                sb.AppendLine(@"	</Style>");
            }

            sb.AppendLine(@"	<Folder>");
            sb.AppendLine(@"	<name>" + DocName + "</name>");

            return sb.ToString();
        }

        private void butCreateAllXYZ_Click(object sender, EventArgs e)
        {
            double SoundBlockSize = double.Parse(textBoxBlockSize.Text);

            List<CHSChart> chsChartList = new List<CHSChart>();
            List<CHSDepth> chsDepthList = new List<CHSDepth>();

            // checking if the directory exist if not should create the directory
            DirectoryInfo di = new DirectoryInfo(textBoxDirectoryPath.Text);

            if (!di.Exists)
            {
                di.Create();
            }

            using (BathymetryEntities be = new BathymetryEntities())
            {
                chsChartList = (from c in be.CHSCharts select c).OrderBy(c => c.CHSChartName).ToList<CHSChart>();
            }

            foreach (CHSChart chs in chsChartList)
            {
                StringBuilder sb = new StringBuilder();

                lblStatusTxt2.Text = "Doing " + chs.CHSChartName + " ... loading data from DB this could take a minute.";
                lblStatusTxt2.Refresh();
                Application.DoEvents();

                using (BathymetryEntities be = new BathymetryEntities())
                {
                    chsDepthList = (from cd in be.CHSDepths
                                    where cd.CHSChartID == chs.CHSChartID
                                    select cd).ToList<CHSDepth>();
                }

                int CountDepth = 0;
                foreach (CHSDepth d in chsDepthList.OrderBy(d => d.Depth))
                {
                    CountDepth += 1;
                    if (CountDepth % 1000 == 0)
                    {
                        lblStatusTxt2.Text = "Doing " + chs.CHSChartName + " --- " + CountDepth;
                        lblStatusTxt2.Refresh();
                        Application.DoEvents();
                    }

                    sb.AppendLine(string.Format("{0},{1},{2} ", d.Longitude, d.Latitude, (d.Depth*-1.0f)));
                }

                StreamWriter sw = File.CreateText(textBoxDirectoryPath.Text + chs.CHSChartName + ".xyz");
                sw.Write(sb);
                sw.Flush();
                sw.Close();
            }
        }
    }
}
class ColorVal
{
    public double Value { get; set; }
    public string ColorHexStr { get; set; }
}
public class BathName
{
    public BathName()
    {
        ID = 0;
        Name = "";
    }

    public int ID { get; set; }
    public string Name { get; set; }
}
