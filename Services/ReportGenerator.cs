using RSCHS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace RSCHS.Services
{
    public static class ReportGenerator
    {
        // Простой текстовый отчёт
        public static void GenerateTextReport(Report report)
        {
            try
            {
                string content = CreateReportContent(report);

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"Отчет_МЧС_{report.ID}_{DateTime.Now:yyyyMMdd_HHmm}.txt"
                );

                File.WriteAllText(filePath, content, Encoding.UTF8);

                MessageBox.Show($"Текстовый отчёт сохранён:\n{filePath}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Все отчёты в один файл
        public static void GenerateAllReportsText(List<Report> reports)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== СВОДНЫЙ ОТЧЁТ МЧС ===");
                sb.AppendLine($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
                sb.AppendLine($"Всего отчётов: {reports.Count}");
                sb.AppendLine("=".PadRight(50, '='));

                foreach (var report in reports)
                {
                    sb.AppendLine();
                    sb.AppendLine(CreateReportContent(report));
                    sb.AppendLine("-".PadRight(50, '-'));
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"Все_отчеты_МЧС_{DateTime.Now:yyyyMMdd_HHmm}.txt"
                );

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                MessageBox.Show($"Сводный отчёт сохранён:\n{filePath}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string CreateReportContent(Report report)
        {
            return
                $"ОТЧЁТ №{report.ID}\n" +
                $"{"=".PadRight(40, '=')}\n" +
                $"Дата создания: {report.CreationDate:dd.MM.yyyy HH:mm}\n" +
                $"Место происшествия: {report.IncidentLocation}\n" +
                $"Ответственный: {report.EmployeeName}\n" +
                $"Транспорт: {report.TransportInfo}\n\n" +
                $"СОДЕРЖАНИЕ:\n{report.Content}\n\n" +
                $"Подпись: ___________\n" +
                $"Дата подписания: {DateTime.Now:dd.MM.yyyy}";
        }

        // Методы для совместимости (чтобы не менять MainViewModel)
        public static void GenerateExcelReport(Report report, string filePath = null)
        {
            GenerateTextReport(report); // Просто вызываем текстовую версию
        }

        public static void GenerateAllReportsExcel(List<Report> reports)
        {
            GenerateAllReportsText(reports); // Просто вызываем текстовую версию
        }
    }
}