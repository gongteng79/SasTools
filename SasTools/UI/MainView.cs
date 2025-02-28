using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AntdUI;

namespace SasTools.UI
{
    public partial class MainView : UserControl
    {

        DataTable dataTable;
        public MainView()
        {
            InitializeComponent();
            InitializeTable();
        }

        private void InitializeTable()
        {
            // 定义列
            var columns = new ColumnCollection
            {
                new Column("SN", "SN号"),
                new Column("Torque", "扭力"),
                new Column("TorqueUnit", "扭力单位"),
                new Column("Angle", "角度"),
                new Column("AngleUnit", "角度单位"),
                new Column("Result", "结果"),
                new Column("NumberParticles", "颗数")
            };
            table1.Columns = columns;

            // 绑定数据源
            dataTable = new DataTable();
            dataTable.Columns.Add("SN", typeof(string));
            dataTable.Columns.Add("Torque", typeof(double));
            dataTable.Columns.Add("TorqueUnit", typeof(string));
            dataTable.Columns.Add("Angle", typeof(double));
            dataTable.Columns.Add("AngleUnit", typeof(string));
            dataTable.Columns.Add("Result", typeof(int));
            dataTable.Columns.Add("NumberParticles", typeof(int));
            table1.DataSource = dataTable;
            table1.ColumnDragSort = true;

            // 生成随机数并添加到数据表
            Random random = new Random();
            for (int i = 0; i < 10; i++) // 假设添加10行数据
            {
                DataRow row = dataTable.NewRow();
                row["SN"] = "SN" + (i + 1);
                row["Torque"] = random.NextDouble() * 100; // 生成0到100之间的随机扭力值
                row["TorqueUnit"] = "Nm";
                row["Angle"] = random.NextDouble() * 360; // 生成0到360之间的随机角度值
                row["AngleUnit"] = "°";
                row["Result"] = random.Next(0, 2); // 生成0或1的随机结果值
                row["NumberParticles"] = random.Next(1, 100); // 生成1到100之间的随机颗数值
                dataTable.Rows.Add(row);
            }

            table1.DataSource = dataTable;


            textBox1.Text = "当前正在测试...";
        }
    }
}
