using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WinHostEditor
{
    public partial class MainForm : Form
    {
        private String HostsPath = null;
        private List<DataRow> Rows = null;

        private Sunisoft.IrisSkin.SkinEngine se = null;
        public MainForm()
        {
            InitializeComponent();

            se = new Sunisoft.IrisSkin.SkinEngine();
            se.Active = true;

            se.SkinAllForm = true;
            se.SkinFile = "ref.dat";
        }

        private int GetGridIndex(int Num)
        {
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                if (dataGridView.Rows[i].Cells[0].Value.ToString() == Num.ToString()) return i;
            }

            return -1;
        }

        private int GetDataIndex(int Num)
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                if (Rows[i].Num == Num) return i;
            }

            return -1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            HostsPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            HostsPath += "\\drivers\\etc\\HOSTS";

            RefreshHOSTS();
        }

        private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int idx;

            if (e.ColumnIndex > 2 || e.RowIndex >= dataGridView.Rows.Count) return;
            
            // 域名检测
            if (e.ColumnIndex == 1)
            {
                String Domain = "";
                try
                {
                    Domain = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("域名不能为空。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    idx = GetDataIndex(Convert.ToInt32(dataGridView.Rows[e.RowIndex].Cells[0].Value.ToString()));
                    if (idx == -1)
                    {
                        dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "localhost";
                    }
                    else dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Rows[idx].Domain;

                    return;
                }

                if (Domain.Length == 0 || Domain.Trim().Length == 0)
                {
                    MessageBox.Show("域名不能为空。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    idx = GetDataIndex(Convert.ToInt32(dataGridView.Rows[e.RowIndex].Cells[0].Value.ToString()));
                    if (idx == -1)
                    {
                        dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "localhost";
                    }
                    else dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Rows[idx].Domain;

                    return;
                }
                else
                {
                    dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Domain.Trim();
                }
            }
            else
            // IP地址检测
            if (e.ColumnIndex == 2)
            {
                String IP = "";

                try
                {
                    IP = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("请输入正确IP地址。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    idx = GetDataIndex(Convert.ToInt32(dataGridView.Rows[e.RowIndex].Cells[0].Value.ToString()));
                    if (idx == -1)
                    {
                        dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "0.0.0.0";
                    }
                    else dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Rows[idx].IP;

                    return;
                }

                IP = IP.Trim();
                if (!DataRow.IsIP(IP))
                {
                    MessageBox.Show("请输入正确IP地址。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    idx = GetDataIndex(Convert.ToInt32(dataGridView.Rows[e.RowIndex].Cells[0].Value.ToString()));
                    if (idx == -1)
                    {
                        dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "0.0.0.0";
                    }
                    else dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Rows[idx].IP;

                    return;
                }

                dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = IP.Trim();
            }

            // 修改“里”数据
            String Value = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            idx = GetDataIndex(Convert.ToInt32(dataGridView.Rows[e.RowIndex].Cells[0].Value.ToString()));
            if (e.ColumnIndex == 1)
            {
                Rows[idx].Domain = Value;
            }
            else
            if (e.ColumnIndex == 2)
            {
                Rows[idx].IP = Value;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void RefreshHOSTS()
        {
            Rows = new List<DataRow>();
            dataGridView.Rows.Clear();

            //打开HOSTS文件
            FileStream fs = new FileStream(HostsPath, FileMode.OpenOrCreate);
            StreamReader sr = new StreamReader(fs);

            // 循环读取每一行
            int i = 0;
            sr.BaseStream.Seek(0, SeekOrigin.Begin);
            String strLine = sr.ReadLine();
            while (strLine != null)
            {
                strLine = strLine.Trim();

                if ((strLine.Length > 0 && strLine[0] == '#') || strLine.Length == 0)
                {
                    DataRow Temp = new DataRow();
                    Temp.Num = i;
                    Temp.Valid = false;
                    Temp.Origin = strLine;

                    Rows.Add(Temp);
                }
                else
                {
                    String tmpIP = "";
                    String tmpDomain = "";

                    int j = 0;
                    for (j = 0; j < strLine.Length && strLine[j] != ' ' && strLine[j] != '\t'; j++)
                    {
                        tmpIP += strLine[j];
                    }

                    tmpDomain = strLine.Substring(j).Trim();

                    DataRow Temp = new DataRow();
                    Temp.Num = i;
                    Temp.Domain = tmpDomain;
                    Temp.IP = tmpIP;
                    Temp.Valid = true;

                    Rows.Add(Temp);

                    // 添加到GridView
                    int idx = dataGridView.Rows.Add();
                    dataGridView.Rows[idx].Cells[0].Value = Temp.Num;
                    dataGridView.Rows[idx].Cells[1].Value = Temp.Domain;
                    dataGridView.Rows[idx].Cells[2].Value = Temp.IP;
                }

                i++;
                strLine = sr.ReadLine();
            }

            sr.Close();
            fs.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                // 先把正在编辑的给确定了
                dataGridView.EndEdit();

                FileStream fs = new FileStream(HostsPath, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);

                sw.BaseStream.Seek(0, SeekOrigin.Begin);
                for (int i = 0; i < Rows.Count; i++)
                {
                    String strLine = "";
                    if (Rows[i].Valid)
                    {
                        strLine = Rows[i].IP + "\t" + Rows[i].Domain;
                    }
                    else
                    {
                        strLine = Rows[i].Origin;
                    }

                    sw.WriteLine(strLine);
                }

                sw.Close();
                fs.Close();

                RefreshHOSTS();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n请确认您是否让360安全卫士禁止了此操作。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show("HOSTS表已经成功保存。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int maxNum = Rows.Count == 0 ? 0 : Rows[Rows.Count - 1].Num + 1;
            DataRow row = new DataRow();
            row.Num = maxNum;
            row.Valid = true;
            row.Domain = "localhost";
            row.IP = "0.0.0.0";

            Rows.Add(row);

            int idx = dataGridView.Rows.Add();
            dataGridView.Rows[idx].Cells[0].Value = row.Num.ToString();
            dataGridView.Rows[idx].Cells[1].Value = row.Domain;
            dataGridView.Rows[idx].Cells[2].Value = row.IP;

            dataGridView.CurrentCell = dataGridView.Rows[idx].Cells[1];
            dataGridView.BeginEdit(false);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            dataGridView.EndEdit();

            DataGridViewSelectedRowCollection sRows = dataGridView.SelectedRows;

            for (int i = 0; i < sRows.Count; i++)
            {
                int idx = GetDataIndex(Convert.ToInt32(sRows[i].Cells[0].Value.ToString()));
                if (idx == -1) continue;
                Rows.RemoveAt(idx);

                idx = GetGridIndex(Convert.ToInt32(sRows[i].Cells[0].Value.ToString()));
                dataGridView.Rows.RemoveAt(idx);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            RefreshHOSTS();
        }

        private void 编辑EToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView.ClearSelection();
            dataGridView.Rows[preSelectedRow].Cells[preSelectedCol].Selected = true;

            dataGridView.CurrentCell = dataGridView.Rows[preSelectedRow].Cells[preSelectedCol];
            dataGridView.BeginEdit(false);
        }

        private void 删除DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button4_Click(sender, e);
        }

        private int preSelectedRow = 0;
        private int preSelectedCol = 0;

        private void dataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left) return;
            contextMenuStrip2.Show(Control.MousePosition);

            // 若当前行没有选中
            if (!dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected)
            {
                dataGridView.ClearSelection();
                dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;
            }

            // 若当前行被选中了则不做
            preSelectedRow = e.RowIndex;
            preSelectedCol = e.ColumnIndex;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://xcoder.in/");
        }

        private void dataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
        }

        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                dataGridView.EndEdit();

                DataGridViewSelectedRowCollection sRows = dataGridView.SelectedRows;

                for (int i = 0; i < sRows.Count; i++)
                {
                    int idx = GetDataIndex(Convert.ToInt32(sRows[i].Cells[0].Value.ToString()));
                    if (idx == -1) continue;
                    Rows.RemoveAt(idx);
                }
            }
        }
    }
}
