using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inforigami.Regalo.ObjectCompare
{
    public class EnumerableComparisonReport
    {
        public string Generate(string colHeader1, object value1, string colHeader2, object value2)
        {
            var list1 = ((IEnumerable<object>)value1).ToList();
            var list2 = ((IEnumerable<object>)value2).ToList();

            var results = new List<Tuple<string, string>>();

            var longestListLength = Math.Max(list1.Count, list2.Count);
            for (int i = 0; i < longestListLength; i++)
            {
                var col1 = GetReportColumnValue(list1, i);
                var col2 = GetReportColumnValue(list2, i);

                results.Add(Tuple.Create(col1, col2));
            }

            var colWidths = GetReportColumnWidths(results);

            var report = new StringBuilder();
            AppendRowDivider(report, colWidths);
            AppendColumnHeaders(report, colWidths, colHeader1, colHeader2);
            AppendRowDivider(report, colWidths);

            foreach (var result in results)
            {
                AppendRow(report, result, colWidths);
            }

            AppendRowDivider(report, colWidths);

            return report.ToString();
        }

        private static void AppendRow(StringBuilder report, Tuple<string, string> result, Tuple<int, int> colWidths)
        {
            report.AppendFormat(
                "| {0} | {1} |",
                result.Item1.PadRight(colWidths.Item1, ' '),
                result.Item2.PadRight(colWidths.Item2, ' '))
                  .AppendLine();
        }

        private static void AppendColumnHeaders(StringBuilder report, Tuple<int, int> colWidths, string colHeader1, string colHeader2)
        {
            report.AppendFormat(
                "| {0} | {1} |",
                colHeader1.PadRight(colWidths.Item1, ' '),
                colHeader2.PadRight(colWidths.Item2, ' '))
                  .AppendLine();
        }

        private static void AppendRowDivider(StringBuilder report, Tuple<int, int> colWidths)
        {
            report.AppendFormat(
                "|-{0}-|-{1}-|",
                "-".PadRight(colWidths.Item1, '-'),
                "-".PadRight(colWidths.Item2, '-'))
                  .AppendLine();
        }

        private Tuple<int, int> GetReportColumnWidths(IList<Tuple<string, string>> results)
        {
            var map = results.Select(x => new { Col1 = x.Item1.Length, Col2 = x.Item2.Length });
            return Tuple.Create(map.Max(x => x.Col1), map.Max(x => x.Col2));
        }

        private static string GetReportColumnValue(IList<object> list, int index)
        {
            string col;

            var item = index >= list.Count ? null : list[index];

            if (item == null)
            {
                col = "        ";
            }
            else
            {
                var typeName = item.GetType().FullName;

                var val = item.ToString();

                col = val.Equals(typeName) ? typeName : $"{typeName} = {val}";
            }

            return col;
        }
    }
}