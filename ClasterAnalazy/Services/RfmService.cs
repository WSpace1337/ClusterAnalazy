using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ClusterVisualizer.Core.Models;

namespace ClusterVisualizer.Services
{
    public class RfmService
    {
        public List<RfmCustomer> LoadFromCsv(string path)
        {
            var lines = File.ReadAllLines(path).ToList();

            if (lines.Count <= 1)
                throw new Exception("CSV is empty.");

            var headers = lines[0]
                .Split(',')
                .Select(h => h.Trim())
                .ToList();

            bool hasClassicRfm =
                HasColumn(headers, "CustomerId") &&
                HasColumn(headers, "Recency") &&
                HasColumn(headers, "Frequency") &&
                HasColumn(headers, "Monetary");

            if (hasClassicRfm)
            {
                return LoadClassicRfm(lines, headers);
            }

            bool hasCustomerSegmentation =
                HasColumn(headers, "Customer Id") &&
                HasColumn(headers, "Years Employed") &&
                HasColumn(headers, "Income") &&
                HasColumn(headers, "DebtIncomeRatio");

            if (hasCustomerSegmentation)
            {
                return LoadFromCustomerSegmentation(lines, headers);
            }

            throw new Exception(
                "CSV must contain either classic RFM columns: CustomerId, Recency, Frequency, Monetary " +
                "or customer segmentation columns: Customer Id, Years Employed, Income, DebtIncomeRatio."
            );
        }

        private List<RfmCustomer> LoadClassicRfm(List<string> lines, List<string> headers)
        {
            int idIndex = GetColumnIndex(headers, "CustomerId");
            int rIndex = GetColumnIndex(headers, "Recency");
            int fIndex = GetColumnIndex(headers, "Frequency");
            int mIndex = GetColumnIndex(headers, "Monetary");

            var customers = new List<RfmCustomer>();

            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');

                if (parts.Length <= Math.Max(Math.Max(idIndex, rIndex), Math.Max(fIndex, mIndex)))
                    continue;

                customers.Add(new RfmCustomer
                {
                    CustomerId = parts[idIndex],
                    Recency = ParseDouble(parts[rIndex]),
                    Frequency = ParseDouble(parts[fIndex]),
                    Monetary = ParseDouble(parts[mIndex])
                });
            }

            CalculateScores(customers);
            return customers;
        }

        private List<RfmCustomer> LoadFromCustomerSegmentation(List<string> lines, List<string> headers)
        {
            int idIndex = GetColumnIndex(headers, "Customer Id");
            int yearsIndex = GetColumnIndex(headers, "Years Employed");
            int incomeIndex = GetColumnIndex(headers, "Income");
            int debtIncomeIndex = GetColumnIndex(headers, "DebtIncomeRatio");

            var customers = new List<RfmCustomer>();

            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');

                if (parts.Length <= Math.Max(Math.Max(idIndex, yearsIndex), Math.Max(incomeIndex, debtIncomeIndex)))
                    continue;

                customers.Add(new RfmCustomer
                {
                    CustomerId = parts[idIndex],

                    // Адаптация под твой файл:
                    // чем меньше DebtIncomeRatio, тем лучше R-score
                    Recency = ParseDouble(parts[debtIncomeIndex]),

                    // чем больше Years Employed, тем лучше F-score
                    Frequency = ParseDouble(parts[yearsIndex]),

                    // чем больше Income, тем лучше M-score
                    Monetary = ParseDouble(parts[incomeIndex])
                });
            }

            CalculateScores(customers);
            return customers;
        }

        private void CalculateScores(List<RfmCustomer> customers)
        {
            var recencyValues = customers.Select(c => c.Recency).OrderBy(x => x).ToList();
            var frequencyValues = customers.Select(c => c.Frequency).OrderBy(x => x).ToList();
            var monetaryValues = customers.Select(c => c.Monetary).OrderBy(x => x).ToList();

            foreach (var c in customers)
            {
                c.RScore = GetScore(c.Recency, recencyValues, higherIsBetter: false);
                c.FScore = GetScore(c.Frequency, frequencyValues, higherIsBetter: true);
                c.MScore = GetScore(c.Monetary, monetaryValues, higherIsBetter: true);

                c.Segment = GetSegment(c);
            }
        }

        private int GetScore(double value, List<double> sortedValues, bool higherIsBetter)
        {
            if (sortedValues.Count == 0)
                return 1;

            int index = sortedValues.FindIndex(v => v >= value);

            if (index < 0)
                index = sortedValues.Count - 1;

            double percentile = (double)index / Math.Max(1, sortedValues.Count - 1);

            int score = (int)Math.Floor(percentile * 5) + 1;

            if (score < 1) score = 1;
            if (score > 5) score = 5;

            if (!higherIsBetter)
                score = 6 - score;

            return score;
        }

        private string GetSegment(RfmCustomer c)
        {
            int totalScore = c.RScore + c.FScore + c.MScore;

            if (totalScore >= 12)
                return "High Value Clients";

            if (totalScore >= 8)
                return "Regular Clients";

            return "Low Value / Risk Clients";
        }

        private bool HasColumn(List<string> headers, string columnName)
        {
            return headers.Any(h =>
                NormalizeHeader(h) == NormalizeHeader(columnName));
        }

        private int GetColumnIndex(List<string> headers, string columnName)
        {
            return headers.FindIndex(h =>
                NormalizeHeader(h) == NormalizeHeader(columnName));
        }

        private string NormalizeHeader(string value)
        {
            return value
                .Trim()
                .Replace(" ", "")
                .Replace("_", "")
                .ToLower();
        }

        private double ParseDouble(string value)
        {
            value = value.Trim().Replace(',', '.');

            return double.Parse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture
            );
        }
    }
}