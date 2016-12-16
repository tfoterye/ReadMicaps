using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;


namespace ReadStationData
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Form2 nForm = new Form2();
            nForm.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int station_id;
            if (!listBox2.Items.Contains(textBox1.Text))
                try
            {
                station_id = int.Parse(textBox1.Text);
                listBox2.Items.Add(textBox1.Text);
                listBox5.Items.Add(QueryStation(station_id));
            }
            catch
            {
                MakeDialog( "请输入正确站号！","错误");
            }
                
            else
                MakeDialog( "已存在，请重新输入！","错误");
        }
       
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox5.SelectedItems.Clear();
            for (int i=0;i<listBox2.SelectedIndices.Count;i++)
            {
                listBox5.SelectedIndex=listBox2.SelectedIndices[i];
            }
            Application.DoEvents();
        }
        private void MakeDialog(string mess, string inf)
        {
            DialogResult a = MessageBox.Show(mess, inf, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (a == DialogResult.OK)
            {
                return;
            }
        }
        private string QueryStation(int sid)
        {
            DataTable dtTemp = new DataTable("station_information"); ;
            DataColumn dc;
            string line; string SName;
            dc = new DataColumn("station_no", System.Type.GetType("System.String"));
            dtTemp.Columns.Add(dc);
            //第1列
            dc = new DataColumn("station_lat", System.Type.GetType("System.Double"));
            dtTemp.Columns.Add(dc);
            //添加第2列
            dc = new DataColumn("station_lon", System.Type.GetType("System.Double"));
            dtTemp.Columns.Add(dc);
            //添加第3列
            dc = new DataColumn("station_height", System.Type.GetType("System.Double"));
            dtTemp.Columns.Add(dc);
            //添加第4列
            dc = new DataColumn("station_name", System.Type.GetType("System.String"));
            dtTemp.Columns.Add(dc);

            DataColumn[] dckey = new DataColumn[] { dtTemp.Columns["station_no"] };
            dtTemp.PrimaryKey = dckey;

            dtTemp.Rows.Clear();
            int iXH = 0;
            Encoding encode1 = fileEncode.GetFileEncodeType("station_information.ini");
            System.IO.StreamReader file = new System.IO.StreamReader("station_information.ini", Encoding.GetEncoding("GB2312"));


            while ((line = file.ReadLine()) != null)
            {
                string[] strArray = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                int s_no = int.Parse(strArray[0]);
                double s_lat = Convert.ToDouble(strArray[1]) / 100;
                double s_lon = Convert.ToDouble(strArray[2]) / 100;
                double s_height = Convert.ToDouble(strArray[3]) / 100;
                //byte[] byteArray = System.Text.Encoding.GetEncoding("GB2312").GetBytes(strArray[6]);
                //Encoding dstEncoding = Encoding.GetEncoding("GB2312");
                //string s_name = dstEncoding.GetString(byteArray);
                string s_name = strArray[6];
                dtTemp.Rows.Add(new object[] { s_no, s_lat, s_lon, s_height, s_name });
                iXH++;
            }

            file.Close();
            try
            {
                object[] strArray = dtTemp.Rows.Find(sid).ItemArray;
                SName = strArray[4].ToString();
            }
            catch
            {
                SName = "查无此站";
            }
            return SName;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox5.SelectedItems.Clear();
            for (int i = 0; i < listBox2.SelectedIndices.Count; i++)
            {
                listBox5.SelectedIndex = listBox2.SelectedIndices[i];
            }
            for (int i = 0; i < listBox2.SelectedIndices.Count; i++)
            {
                listBox2.Items.Remove(listBox2.SelectedItems[i]);
                listBox5.Items.Remove(listBox5.SelectedItems[i]);
                
            }
            Application.DoEvents();
        }

    }
    public class fileEncode
    {//获得文件编码格式的类
        public static System.Text.Encoding GetFileEncodeType(string filename)
        {
            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.BinaryReader br = new System.IO.BinaryReader(fs);
            Byte[] buffer = br.ReadBytes(2);
            br.Close();
            fs.Close();

            if (buffer[0] >= 0xEF)
            {
                if (buffer[0] == 0xEF && buffer[1] == 0xBB)
                {
                    return System.Text.Encoding.UTF8;
                }
                else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                {
                    return System.Text.Encoding.BigEndianUnicode;
                }
                else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                {
                    return System.Text.Encoding.Unicode;

                }
                else
                {
                    return System.Text.Encoding.Default;
                }
            }
            else
            {
                return System.Text.Encoding.Default;
            }
        }

    }
    public class ReadMicapsFile
    {
        public void ReadSurface(string FolderPath, string[] StationNumber, string OutputFilePath)//给出大目录和所有站号，求出含有所有站号的文件
        {
            string SurfacePath = FolderPath;
            string[] dic = new string[] { " " };
            try
            {
                dic = System.IO.Directory.GetDirectories(@SurfacePath, "surface*", SearchOption.AllDirectories);//获取子目录下所有对应的目录
            }
            catch
            {
                //output("error!");
            }
            Parallel.For(0, StationNumber.Count(), (si, LoopState) =>
            {
                if (si >= StationNumber.Count())
                    LoopState.Stop();
                string outfilename = OutputFilePath + @"\" + StationNumber[si] + @"surface.txt";

                for (int i = 0; i < dic.Count(); i++)
                {
                    string[] result_line = SurfaceFolder(dic[i], StationNumber[si]);
                    for (int rl = 0; rl < result_line.Count(); rl++)
                    {
                        System.IO.File.AppendAllText(outfilename, result_line[rl], Encoding.GetEncoding("GB2312"));
                    }
                };
            });
        }

        public string[] SurfaceFolder(string SurferPath, string StationNumber)//给出地面文件具体路径和单个站号，返回字符串数组
        {
            string FilePath = SurferPath + @"\plot\";
            string[] name = new string[] { " " };
            List<string> StationString = new List<string>();
            name = System.IO.Directory.GetFiles(@FilePath, "*.000");
            Parallel.For(0, name.Count(), (i, LoopState) =>
            {
                if (i >= name.Count())
                    LoopState.Stop();
                StationString.Add(CheckMiacaps1(name[i], StationNumber));
            });
            return StationString.ToArray();

        }

        public void ReadHigh(string FolderPath, string[] StationNumber, string OutputFilePath)//给出大目录和所有站号，求出含有所有站号的文件
        {
            string HighPath = FolderPath;
            string[] dic = new string[] { " " };
            try
            {
                dic = System.IO.Directory.GetDirectories(@HighPath, "high*", SearchOption.AllDirectories);//获取子目录下所有对应的目录
            }
            catch
            {
                //output("error!");
            }
            Parallel.For(0, StationNumber.Count(), (si, LoopState) =>
            {
                if (si >= StationNumber.Count())
                    LoopState.Stop();
                string outfilename = OutputFilePath + @"\" + StationNumber[si] + @"high.txt";

                for (int i = 0; i < dic.Count(); i++)
                {
                    string[] result_line = HighFolder(dic[i], StationNumber[si]);
                    for (int rl = 0; rl < result_line.Count(); rl++)
                    {
                        System.IO.File.AppendAllText(outfilename, result_line[rl], Encoding.GetEncoding("GB2312"));
                    }
                }
            });
        }

        public string[] HighFolder(string HighPath, string StationNumber)//给出高空文件具体路径和单个站号，返回字符串数组
        {
            string[] height = { "100", "150", "200", "250", "300", "400", "500", "700", "850", "925", "1000" };
            List<string> StationString = new List<string>();
            StationString.Clear();
            for (int ii = 0; ii < height.Count(); ii++)
            {
                string FilePath = HighPath + @"\plot\" + height[ii] + "\\";
                string[] name = new string[] { " " };
                name = System.IO.Directory.GetFiles(@FilePath, "*.000");
                Parallel.For(0, name.Count(), (i, LoopState) =>
                {
                    if (i >= name.Count())
                        LoopState.Stop();
                    StationString.Add(CheckMiacaps2(name[i], StationNumber));
                });
            }
            return StationString.ToArray();

        }

        public string CheckMiacaps1(string FileName, string StationNumber)//给出地面文件名和站号，返回字符串
        {
            string line; int j = 0;
            Encoding encode1 = fileEncode.GetFileEncodeType(FileName);
            System.IO.StreamReader file = new System.IO.StreamReader(FileName, Encoding.GetEncoding("GB2312"));
            List<string> list = new List<string>();

            while ((line = file.ReadLine()) != null)
            {
                if (!list.Contains(line))  //去除重复的行
                {
                    list.Add(line);

                }
            }
            string FirstLine = list[1];//读取第2行的信息
            foreach (string item in list)
            {
                if (item == null)
                { j++; continue; }
                if (item.Contains(StationNumber))
                    break;
                j++;
            }
            string index_line = StationNumber + RepeatString("\tNaN", 25);
            if (j < list.Count())
            {
                index_line = list[j].ToString() + list[j + 1].ToString();
            }
            return FirstLine + " " + index_line + "\r\n";
        }

        public string CheckMiacaps2(string FileName, string StationNumber)//给出高空文件名和站号，返回字符串
        {
            string line; int j = 0;
            Encoding encode1 = fileEncode.GetFileEncodeType(FileName);
            System.IO.StreamReader file = new System.IO.StreamReader(FileName, Encoding.GetEncoding("GB2312"));
            List<string> list = new List<string>();

            while ((line = file.ReadLine()) != null)
            {
                if (!list.Contains(line))  //去除重复的行
                {
                    list.Add(line);

                }
            }
            string FirstLine = list[1];//读取第2行的信息
            foreach (string item in list)
            {
                if (item == null)
                { j++; continue; }
                if (item.Contains(StationNumber))
                    break;
                j++;
            }
            string index_line = StationNumber + RepeatString("\tNaN", 9);
            if (j < list.Count())
            {
                index_line = list[j].ToString();
            }
            return FirstLine + " " + index_line + "\r\n";
        }

        public string CheckMiacaps3(string FileName, string StationNumber)//给出Micaps3类文件名和站号，返回字符串
        {
            string line; int j = 0;
            Encoding encode1 = fileEncode.GetFileEncodeType(FileName);
            System.IO.StreamReader file = new System.IO.StreamReader(FileName, Encoding.GetEncoding("GB2312"));
            List<string> list = new List<string>();

            while ((line = file.ReadLine()) != null)
            {
                if (!list.Contains(line))  //去除重复的行
                {
                    list.Add(line);

                }
            }
            string FirstLine = list[1];//读取第2行的信息
            foreach (string item in list)
            {
                if (item == null)
                { j++; continue; }
                if (item.Contains(StationNumber))
                    break;
                j++;
            }
            string index_line = StationNumber + RepeatString("\tNaN", 5);
            if (j < list.Count())
            {
                index_line = list[j].ToString();
            }
            return FirstLine + " " + index_line + "\r\n";
        }

        public string[] CheckMiacaps5(string FileName, string StationNumber)//给出Micaps5类文件名和站号，返回字符串
        {
            string line; int j = 0; int jk = 0;
            List<string> TLnPString = new List<string>();
            TLnPString.Clear();
            Encoding encode1 = fileEncode.GetFileEncodeType(FileName);
            System.IO.StreamReader file = new System.IO.StreamReader(FileName, Encoding.GetEncoding("GB2312"));
            List<string> list = new List<string>();
            string FirstLine = list[1];//读取第2行的信息
            foreach (string item in list)
            {
                if (item == null)
                { j++; continue; }
                if (item.Contains(StationNumber))
                    break;
                j++;
            }
            if (j < list.Count())
            {
                for (jk = j + 1; jk < list.Count(); jk++)
                {
                    string[] strArray = list[jk].Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    if (strArray.Count() < 6)
                        break;
                }

                if (jk < list.Count())
                {
                    
                    for (int ii = j; ii < jk; ii++)
                    {
                        TLnPString.Add(list[ii]);
                    }

                }
                if (jk == list.Count())
                {
                    
                    for (int ii = j; ii < jk + 1; ii++)
                    {
                        TLnPString.Add(list[ii]);
                    }

                }
            }
            else
                TLnPString.Add(StationNumber + RepeatString("\tNaN", 5));



            return TLnPString.ToArray();
        }

        public string[] CheckMiacaps4(string FileName)//给出Micaps4类文件名，返回数组字符串
        {
            string line; int j = 0; int jk = 0;
            List<string> GridString = new List<string>();
            GridString.Clear();
            Encoding encode1 = fileEncode.GetFileEncodeType(FileName);
            System.IO.StreamReader file = new System.IO.StreamReader(FileName, Encoding.GetEncoding("GB2312"));
            List<string> list = new List<string>();

            while ((line = file.ReadLine()) != null)
            {
                if (!list.Contains(line))  //去除重复的行
                {
                    list.Add(line);

                }
            }
            string FirstLine = list[1];//读取第2行的信息
            string SecondLine = list[2];//读取第3行的信息
            string ThirdLine = list[3];//读取第4行的信息
            string[] strArray = ThirdLine.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
            int LineLength = Convert.ToInt16(strArray[0]); int StrLength = Convert.ToInt16(strArray[1]);
            int LineCount = 0;
            for (j = 4; j < list.Count();j++ )
            {
                string[] strArray1 = list[j].Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                LineCount = LineCount + strArray1.Count();
                if (LineCount>=LineLength)
                    break;
            }
            int LineTick = j - 3;//每LineTick行是数组实际的一行
            for (jk = 0; jk < StrLength; jk++)
            {
                string TempLine="";
                for (int tem_j=0;tem_j<LineTick;tem_j++)
                {
                    TempLine = TempLine + "\t" + list[jk * LineTick + tem_j + 4];
                }
                GridString.Add(TempLine);
            }
                return GridString.ToArray();

        }

        public string[] CheckMiacaps11(string FileName)//给出Micaps11类文件名，返回数组字符串
        {
            string line; int j = 0; int jk = 0;
            List<string> GridString = new List<string>();
            GridString.Clear();
            Encoding encode1 = fileEncode.GetFileEncodeType(FileName);
            System.IO.StreamReader file = new System.IO.StreamReader(FileName, Encoding.GetEncoding("GB2312"));
            List<string> list = new List<string>();

            while ((line = file.ReadLine()) != null)
            {
                if (!list.Contains(line))  //去除重复的行
                {
                    list.Add(line);

                }
            }
            string FirstLine = list[1];//读取第2行的信息
            string SecondLine = list[2];//读取第3行的信息
            string[] strArray = SecondLine.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
            int LineLength = Convert.ToInt16(strArray[strArray.Count() - 2]); int StrLength = Convert.ToInt16(strArray[strArray.Count() - 1]);
            int LineCount = 0;
            for (j = 3; j < list.Count(); j++)
            {
                string[] strArray1 = list[j].Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                LineCount = LineCount + strArray1.Count();
                if (LineCount >= LineLength)
                    break;
            }
            int LineTick = j - 2;//每LineTick行是数组实际的一行
            for (jk = 0; jk < StrLength; jk++)
            {
                string TempLine = "";
                for (int tem_j = 0; tem_j < LineTick; tem_j++)
                {
                    TempLine = TempLine + "\t" + list[jk * LineTick + tem_j + 3];
                }
                GridString.Add(TempLine);
            }
            return GridString.ToArray();

        }


        public static string RepeatString(string str, int n)
        {
            char[] arr = str.ToCharArray();
            char[] arrDeat = new char[arr.Length * n];
            for (int i = 0; i < n; i++)
            {
                Buffer.BlockCopy(arr, 0, arrDeat, i * arr.Length * 2, arr.Length * 2);
            }
            return new string(arrDeat);
        }

    }
}




