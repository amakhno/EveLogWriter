using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogWriter
{
    public partial class Form1 : Form
    {
        string[] Ores = new string[] { "Arkonor",
                "Bistot", "Crokite",
                "Ochre", "Gneiss", "Hedbergite",
                "Hemorphite", "Jaspet", "Kernite",
                "Mercoxit", "Omber", "Plagioclase",
                "Pyroxeres", "Scordite",
                "Spodumain", "Veldspar"};
        int[] Prices = new int[16];

        public Form1()
        {
            InitializeComponent();
            this.webBrowser1.ObjectForScripting = new MyScript();
            toolStripStatusLabel1.Text = "Готов";
        }

        [ComVisible(true)]
        public class MyScript
        {
            public void CallServerSideCode()
            {
                var doc = ((Form1)Application.OpenForms[0]).webBrowser1.Document;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Работаю";
            UserContext usr = new UserContext();
            openFileDialog1.ShowDialog();
            List<LogPosition> localList = new List<LogPosition>();
            DateTime uploadTime = DateTime.UtcNow;
            for (int i = 0; i < openFileDialog1.FileNames.Length; i++)
            {
                string resultDialog = openFileDialog1.FileNames[i];
                if (resultDialog == "openFileDialog1")
                {
                    break;
                }
                string saveFileName = openFileDialog1.SafeFileNames[i];
                if (saveFileName.LastIndexOf(".") > 0)
                {
                    saveFileName = saveFileName.Remove(saveFileName.LastIndexOf("."));
                }
                if (saveFileName.LastIndexOf("(") > 0)
                {
                    saveFileName = saveFileName.Remove(saveFileName.LastIndexOf("(") - 1);
                }
                string cargoName = "";
                string adminName = "";
                try
                {
                    adminName = saveFileName.Split(new char[] { ',' })[0];
                    cargoName = saveFileName.Split(new char[] { ',' })[1];
                }
                catch
                {
                    MessageBox.Show("Проверьте название файла", "Ошибка");
                    return;
                }
                
                StreamReader reader = new StreamReader(resultDialog);
                reader.ReadLine();

                string lineWithoutTags = "";
                while (!reader.EndOfStream)
                {
                    bool isOreLine = false;
                    string currentLine = reader.ReadLine();
                    string[] parseLine;
                    try
                    {
                        parseLine = currentLine.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parseLine.Length != 5)
                        {
                            throw new Exception();
                        }
                    }
                    catch
                    {
                        continue;
                    }
                    string oreType = String.Empty;
                    foreach (string ore in Ores)
                    {
                        lineWithoutTags = RemoveTags(parseLine[4]);
                        if (lineWithoutTags.Contains(ore))
                        {
                            isOreLine = true;
                            oreType = ore;
                            break;
                        }
                    }
                    if (isOreLine)
                    {
                        string[] stringDate = parseLine[0].Split(new char[] { '.', ' ', ':' });
                        DateTime time = ConvertFromStrings(stringDate);
                        string characterName = parseLine[1];
                        int count = Convert.ToInt32(parseLine[3]);
                        if (characterName == cargoName)
                        {
                            continue;
                        }
                        LogPosition pos = new LogPosition { Time = time, CharacterName = characterName, OreType = oreType, Count = count, AdminName = adminName, UploadTime = uploadTime, CargoName = cargoName };
                        localList.Add(pos);
                    }
                }
            }
            foreach (LogPosition pos in localList)
            {
                usr.Logs.Add(pos);
            }
            usr.SaveChanges();
            usr.Dispose();
            toolStripStatusLabel1.Text = "Загрузка успешна";
        }

        private DateTime ConvertFromStrings(string[] source)
        {
            DateTime result = new DateTime(Convert.ToInt32(source[0]), Convert.ToInt32(source[1]), Convert.ToInt32(source[2]), Convert.ToInt32(source[3]), Convert.ToInt32(source[4]), 0);
            return result;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Работаю";
            DateTime fromDate = makeFromDate(dateTimePicker1.Value);
            DateTime toDate = makeToDate(dateTimePicker2.Value);
            string outString = "";
            using (UserContext db = new UserContext())
            {
                IQueryable<IGrouping<string, LogPosition>> groups = null;
                try
                {
                    groups = from p in db.Logs
                             where (p.Time > fromDate && p.Time < toDate)
                             group p by p.CharacterName;
                }
                catch
                {
                    MessageBox.Show("Ошибка подключения к БД", "Ошибка");
                    return;
                }

                foreach (var g in groups)
                {
                    var subGroup = from o in g
                                   group o by o.OreType;
                    outString += g.First().CharacterName + "\n";
                    foreach (var p in subGroup)
                    {
                        int sum = p.Sum(x => x.Count);
                        outString += String.Format("{0} - {1}\n", p.First().OreType, sum);
                    }
                    outString += '\n';
                }
            }
            if (outString.Length < 5)
            {
                richTextBox1.Text = "";
                toolStripStatusLabel1.Text = "Нет данных для отображения";
                return;
            }
            richTextBox1.Text = outString.Remove(outString.Length - 3, 2);
            toolStripStatusLabel1.Text = "Данные отображены";
        }

        static string RemoveTags(string inputString)
        {
            return Regex.Replace(inputString, @"<[^>]*>", String.Empty);
        }


        private DateTime makeFromDate(DateTime source)
        {
            return new DateTime(source.Year, source.Month, source.Day, 0, 0, 0);
        }

        private DateTime makeToDate(DateTime source)
        {
            return new DateTime(source.Year, source.Month, source.Day, 23, 59, 59);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var a = MessageBox.Show("Вы уверены что хоотите очистить базу данных\nза данный период?", "Очистка БД", MessageBoxButtons.YesNo);
            if (a == DialogResult.No)
            {
                return;
            }
            toolStripStatusLabel1.Text = "Работаю";
            DateTime fromDate = makeFromDate(dateTimePicker1.Value);
            DateTime toDate = makeToDate(dateTimePicker2.Value);
            using (UserContext db = new UserContext())
            {
                foreach (LogPosition pos in db.Logs.Where(p => (p.Time > fromDate && p.Time < toDate)))
                {
                    db.Entry(pos).State = System.Data.Entity.EntityState.Deleted;
                }
                db.SaveChanges();
            }
            richTextBox1.Text = "";
            toolStripStatusLabel1.Text = "База очищена";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Navigate("https://ore.cerlestes.de/#site:ore");
            timer1.Enabled = true;
            toolStripStatusLabel1.Text = "Загрузка цен";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (webBrowser1.ReadyState == WebBrowserReadyState.Complete)
            {
                timer1.Enabled = false;
                toolStripStatusLabel1.Text = "Цены загружены";
                Prices = Helpers.GetWebPrices(webBrowser1.Document, Ores);
                UpdatePricesFromMemory();
            }
            else
            {
                timer1.Enabled = false;
                toolStripStatusLabel1.Text = "Тайм-аут операции";
            }
        }

        private void UpdatePricesFromMemory()
        {
            textBoxArkonor.Text = Prices[0].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
            textBoxBistot.Text = Prices[1].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
            textBoxCrokite.Text = Prices[2].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
            textBoxOchre.Text = Prices[3].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
            textBoxGneiss.Text = Prices[4].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
            textBoxHedbergite.Text = Prices[5].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
            textBoxHemorphite.Text = Prices[6].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
            textBoxJaspet.Text = Prices[7].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
            textBoxKernite.Text = Prices[8].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
            textBoxMercoxit.Text = Prices[9].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
            textBoxOmber.Text = Prices[10].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
            textBoxPlagioclase.Text = Prices[11].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
            textBoxPyroxeres.Text = Prices[12].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
            textBoxScordite.Text = Prices[13].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
            textBoxSpodumain.Text = Prices[14].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
            textBoxVeldspar.Text = Prices[15].ToString("N", CultureInfo.CreateSpecificCulture("es-ES"));
        }

        private void UpdatePricesFromFrom()
        {
            try
            {
                Prices[0] = (int)Convert.ToDouble(textBoxArkonor.Text, CultureInfo.CreateSpecificCulture("es-ES"));
                Prices[1] = (int)Convert.ToDouble(textBoxBistot.Text, CultureInfo.CreateSpecificCulture("es-ES"));
                Prices[2] = (int)Convert.ToDouble(textBoxCrokite.Text, CultureInfo.CreateSpecificCulture("es-ES"));
                Prices[3] = (int)Convert.ToDouble(textBoxOchre.Text, CultureInfo.CreateSpecificCulture("es-ES"));
                Prices[4] = (int)Convert.ToDouble(textBoxGneiss.Text, CultureInfo.CreateSpecificCulture("es-ES"));
                Prices[5] = (int)Convert.ToDouble(textBoxHedbergite.Text, CultureInfo.CreateSpecificCulture("es-ES"));
                Prices[6] = (int)Convert.ToDouble(textBoxHemorphite.Text, CultureInfo.CreateSpecificCulture("es-ES"));
                Prices[7] = (int)Convert.ToDouble(textBoxJaspet.Text, CultureInfo.CreateSpecificCulture("es-ES"));
                Prices[8] = (int)Convert.ToDouble(textBoxKernite.Text, CultureInfo.CreateSpecificCulture("es-ES"));
                Prices[9] = (int)Convert.ToDouble(textBoxMercoxit.Text, CultureInfo.CreateSpecificCulture("es-ES"));
                Prices[10] = (int)Convert.ToDouble(textBoxOmber.Text, CultureInfo.CreateSpecificCulture("es-ES"));
                Prices[11] = (int)Convert.ToDouble(textBoxPlagioclase.Text, CultureInfo.CreateSpecificCulture("es-ES"));
                Prices[12] = (int)Convert.ToDouble(textBoxPyroxeres.Text, CultureInfo.CreateSpecificCulture("es-ES"));
                Prices[13] = (int)Convert.ToDouble(textBoxScordite.Text, CultureInfo.CreateSpecificCulture("es-ES"));
                Prices[14] = (int)Convert.ToDouble(textBoxSpodumain.Text, CultureInfo.CreateSpecificCulture("es-ES"));
                Prices[15] = (int)Convert.ToDouble(textBoxVeldspar.Text, CultureInfo.CreateSpecificCulture("es-ES"));
            }
            catch
            {
                MessageBox.Show("Неверный формат числа");
                throw new Exception();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!double.TryParse(textBoxAdmin.Text, out double coeffAdmin) || !double.TryParse(textBoxCargo.Text, out double coeffCargo) || !double.TryParse(textBoxWorker.Text, out double coeffWorker))
            {
                MessageBox.Show("Неверный ввод коэффициентов", "Ошибка");
                return;
            }
            else
            {
                if (!((coeffAdmin <= 100) && (coeffAdmin >= 0)) || !((coeffCargo <= 100) && (coeffCargo >= 0)) || !((coeffWorker <= 100) && (coeffWorker >= 0)))
                {
                    MessageBox.Show("Укажите % от 0 до 100", "Ошибка");
                    return;
                }
                if (coeffAdmin + coeffCargo + coeffWorker > 100)
                {
                    MessageBox.Show("Сумма должна быть не больше 100%", "Ошибка");
                    return;
                }
                coeffAdmin /= 100;
                coeffCargo /= 100;
                coeffWorker /= 100;
            }
            try
            {
                UpdatePricesFromFrom();
            }
            catch
            {
                return;
            }
            toolStripStatusLabel1.Text = "Работаю";
            DateTime fromDate = makeFromDate(dateTimePicker1.Value);
            DateTime toDate = makeToDate(dateTimePicker2.Value);
            dataGridView1.Rows.Clear();

            double sumOut = 0;

            using (UserContext db = new UserContext())
            {
                dataGridView1.Rows.Add("Администраторы", coeffAdmin * 100 + "%");
                //Получуние итого для администарторов
                var groupsByAdminName = from p in db.Logs
                                        where (p.Time > fromDate && p.Time < toDate)
                                        group p by p.AdminName;
                foreach (var g in groupsByAdminName)
                {
                    var groupsByOreType = from o in g
                                          group o by o.OreType;
                    double sum = 0;
                    foreach (var p in groupsByOreType)
                    {
                        string oreType = p.First().OreType;
                        sum += GetSum(oreType, p.Sum(x => x.Count));
                    }
                    sumOut += (sum * coeffAdmin);
                    dataGridView1.Rows.Add(g.First().AdminName, (sum * coeffAdmin).ToString("N", CultureInfo.CreateSpecificCulture("es-ES")));
                }

                dataGridView1.Rows.Add("Грузовики", coeffCargo * 100 + "%");
                //Получуние итого для администарторов
                var groupsByCargoName = from p in db.Logs
                                        where (p.Time > fromDate && p.Time < toDate)
                                        group p by p.CargoName;
                foreach (var g in groupsByCargoName)
                {
                    var groupsByOreType = from o in g
                                          group o by o.OreType;
                    double sum = 0;
                    foreach (var p in groupsByOreType)
                    {
                        string oreType = p.First().OreType;
                        sum += GetSum(oreType, p.Sum(x => x.Count));
                    }
                    sumOut += (sum * coeffCargo);
                    dataGridView1.Rows.Add(g.First().CargoName, (sum * coeffCargo).ToString("N", CultureInfo.CreateSpecificCulture("es-ES")));
                }

                dataGridView1.Rows.Add("Лопаты", coeffWorker * 100 + "%");
                //Получуние итого для лопат
                groupsByCargoName = from p in db.Logs
                                    where (p.Time > fromDate && p.Time < toDate)
                                    group p by p.CharacterName;
                foreach (var g in groupsByCargoName)
                {
                    var groupsByOreType = from o in g
                                          group o by o.OreType;
                    double sum = 0;
                    foreach (var p in groupsByOreType)
                    {
                        string oreType = p.First().OreType;
                        sum += GetSum(oreType, p.Sum(x => x.Count));
                    }
                    sumOut += (sum * coeffWorker);
                    dataGridView1.Rows.Add(g.First().CharacterName, (sum * coeffWorker).ToString("N", CultureInfo.CreateSpecificCulture("es-ES")));
                }

                dataGridView1.Rows.Add("Выплачено", sumOut.ToString("N", CultureInfo.CreateSpecificCulture("es-ES")));
                //Получуние итого
                var groupsByOreType1 = from p in db.Logs
                                       where (p.Time > fromDate && p.Time < toDate)
                                       group p by p.OreType;
                double sum1 = 0;
                foreach (var p in groupsByOreType1)
                {
                    string oreType = p.First().OreType;
                    sum1 += GetSum(oreType, p.Sum(x => x.Count));
                }
                dataGridView1.Rows.Add("Корпа получила", (sum1 - sumOut).ToString("N", CultureInfo.CreateSpecificCulture("es-ES")));
                dataGridView1.Rows.Add("Итого", sum1.ToString("N", CultureInfo.CreateSpecificCulture("es-ES")));
            }
        }

        private double GetSum(string OreType, int count)
        {
            double sum = 0;
            for (int i = 0; i < Ores.Length; i++)
            {
                if (OreType == Ores[i])
                {
                    sum = (double)count * Prices[i];
                }
            }
            return sum;
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            DateTime fromDate = makeFromDate(dateTimePicker1.Value);
            DateTime toDate = makeToDate(dateTimePicker2.Value);
            comboBox1.Items.Clear();
            using (UserContext db = new UserContext())
            {
                try
                {
                    var a = db.Logs.Where(x => (x.Time > fromDate && x.Time < toDate)).Select(x => x.CharacterName).Distinct();
                    foreach (var b in a)
                    {
                        comboBox1.Items.Add(b);
                    }
                    if (comboBox1.Items.Count == 0)
                    {
                        comboBox1.Items.Add("Нет данных");
                    }
                }
                catch
                {
                    MessageBox.Show("Ошибка подключения к БД", "Ошибка");
                    return;
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "Нет данных" || comboBox1.Text == String.Empty)
            {
                return;
            }
            DateTime fromDate = makeFromDate(dateTimePicker1.Value);
            DateTime toDate = makeToDate(dateTimePicker2.Value);
            string outString = "";
            using (UserContext db = new UserContext())
            {
                IQueryable<LogPosition> g = null;
                try
                {
                    g = db.Logs.Where(x => x.CharacterName == comboBox1.Text);
                }
                catch
                {
                    MessageBox.Show("Ошибка подключения к БД", "Ошибка");
                    return;
                }
                var subGroup = from o in g
                               group o by o.OreType;
                outString += g.First().CharacterName + "\n";
                foreach (var p in subGroup)
                {
                    int sum = p.Sum(x => x.Count);
                    outString += String.Format("{0} - {1}\n", p.First().OreType, sum);
                }
                outString += '\n';
            }
            richTextBox1.Text = outString;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var a = MessageBox.Show("Вы уверены что хоотите отменить последнее внесение логов\nв базу данных?", "Очистка БД", MessageBoxButtons.YesNo);
            if (a == DialogResult.No)
            {
                return;
            }
            toolStripStatusLabel1.Text = "Работаю";
            DateTime fromDate = makeFromDate(dateTimePicker1.Value);
            DateTime toDate = makeToDate(dateTimePicker2.Value);
            using (UserContext db = new UserContext())
            {
                DateTime lastTime = db.Logs.Max(x => x.UploadTime);
                foreach (LogPosition pos in db.Logs.Where(p => (p.UploadTime == lastTime)))
                {
                    db.Entry(pos).State = System.Data.Entity.EntityState.Deleted;
                }
                db.SaveChanges();
            }
            richTextBox1.Text = "";
            toolStripStatusLabel1.Text = "База очищена";
        }
    }
}

