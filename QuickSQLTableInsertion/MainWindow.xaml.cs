using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.IO;
using System.Data.SqlClient;

namespace QuickSQLTableInsertion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }

    
    public class TableConstructor
    {
        public List<string> Elements { get; set; }
        public string fileName { get; set; }
        public string TableName { get; set; }

        public static string GatherColumns(string strFileName)
        {
            string tableName = System.IO.Path.GetFileNameWithoutExtension(strFileName);
            string[] headers = File.ReadLines(strFileName).ElementAt(0).Replace(' ', '_').Split(',');
            var inputData = File.ReadLines(strFileName).Skip(1);
            int numColumns = headers.Length;
            string[] columnConstructor = new string[numColumns];
            Regex checksForChars = new Regex("[^0-9.]");
            Regex checksDecimal = new Regex("[.]");
            string buildQuery = null;

            for (int i = 0; i < numColumns; i++)
            {
                int precision = 0;
                int finalscale = 0;
                int decimalScale = 0;
                int varCharNumeral = 0;
                bool containsChars = false;

                foreach (string line in inputData) // First check every line for Non numbers or decimals for columns with VARCHAR.
                {
                    string column = line.Split(',').ElementAt(i);
                    if (checksForChars.IsMatch(column) || checksDecimal.Matches(column).Count >= 2)
                    {
                        containsChars = true;
                    }
                }

                if (containsChars)
                {
                    //Get longest string length, form column constructor
                    foreach (string line in inputData) // First check every line for Non numbers or decimals for columns with VARCHAR.
                    {
                        string column = line.Split(',').ElementAt(i);
                        varCharNumeral = column.Length > varCharNumeral ? column.Length : varCharNumeral;
                        columnConstructor[i] = $"[{headers[i]}] VARCHAR({varCharNumeral})";
                    }
                }
                else
                {
                    foreach (string line in inputData)
                    {
                        string column = line.Split(',').ElementAt(i);
                        precision = column.Length > precision ? column.Length : precision;
                        if (column.ToString().Split('.').Length == 2)
                        {
                            decimalScale = Convert.ToInt32(column.ToString().Split('.').Last().Length);
                            finalscale = decimalScale > finalscale ? decimalScale : finalscale;
                        }
                        columnConstructor[i] = $"[{headers[i]}] NUMERIC({precision},{finalscale})";
                    }
                }

                buildQuery = $"CREATE TABLE {tableName}(" + columnConstructor[0];
                for (int j = 1; j < columnConstructor.Length; j++)
                {
                    buildQuery = buildQuery + (", " + columnConstructor[j]);
                }
                buildQuery = buildQuery + ");";

            }
            return buildQuery;
        }
    }
    
}
