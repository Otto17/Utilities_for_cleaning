/*
   Простая программа для переноса файлов с определённым расширением в указанное место.

    Делал для личного использования под свои нужды.

   Данная программа является свободным программным обеспечением, распространяющимся по лицензии MIT.
   Копия лицензии: https://opensource.org/licenses/MIT
   Copyright (c) 2025 Otto
   Автор: Otto
   Версия: 14.02.25
   GitHub страница:  https://github.com/Otto17/Utilities_for_cleaning
   GitFlic страница: https://gitflic.ru/project/otto/utilities_for_cleaning
   г. Омск 2025
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TrimFiles
{
    internal class Program
    {
        static void Main()
        {
            //Настройки

            //Диски и/или папки для поиска файлов
            var searchPaths = new List<string> { @"C:\", @"D:\", @"E:\" };

            //Расширения, по которым будет осуществляться поиск файлов
            var fileExtensions = new List<string> { ".pdf", ".jpg", ".jpeg" };

            //Папки для исключения из поиска
            var excludeDirectories = new List<string> {
                @"C:\Boot", @"C:\cmd", @"C:\Program Files", @"C:\Program Files (x86)",
                @"C:\ProgramData", @"C:\Windows", @"C:\Documents and Settings", @"C:\Users\admin", @"C:\Users\aks", @"C:\Users\user1", @"C:\Users\User2",
                @"C:\Users\продавец\AppData", @"C:\Users\продавец\Local Settings", @"C:\Users\All Users", @"C:\Users\Default", @"C:\Users\Default User",
                @"C:\Users\Все пользователи", @"C:\$RECYCLE.BIN", @"C:\System Volume Information", @"C:\Users\Public\Documents",
                @"C:\Users\Продавец\Application Data", @"C:\Users\Продавец\Cookies", @"C:\Users\Продавец\NetHood", @"C:\Users\Продавец\PrintHood",
                @"C:\Users\Продавец\Recent", @"C:\Users\Продавец\SendTo",  @"C:\Users\Продавец\главное меню",  @"C:\Users\Продавец\Мои документы",
                @"C:\Users\Продавец\Шаблоны",  @"C:\Users\продавец\Documents\Мои видеозаписи",  @"C:\Users\продавец\Documents\мои рисунки",  @"C:\Users\продавец\Documents\Моя музыка",
                @"D:\1CDB8", @"D:\1CDB8R", @"D:\$RECYCLE.BIN", @"D:\System Volume Information",
                @"E:\ControlCenter\МФУ", @"E:\Загрузки", @"E:\$RECYCLE.BIN", @"E:\$RECYCLER.BIN", @"E:\System Volume Information"
            };

            //Путь куда перемещаем найденные файлы
            var destinationPath = @"E:\$RECYCLER.BIN\Download\";

            //Вызываем метод для перемещения файлов
            MoveFiles(searchPaths, fileExtensions, excludeDirectories, destinationPath);

            Console.WriteLine("Операция завершена.");
        }

        //Функция для поиска и перемещения указанных файлов
        //Принимает 4 аргумента: список путей для поиска (searchPaths), список расширений файлов для обработки (fileExtensions), список директорий, которые нужно исключить из процесса (excludeDirectories), и путь назначения для перемещения файлов (destinationPath)
        static void MoveFiles(List<string> searchPaths, List<string> fileExtensions, List<string> excludeDirectories, string destinationPath)
        {
            //Перебираем каждый путь из списка
            foreach (var path in searchPaths)
            {
                //Проверка, существует ли директория по текущему пути "path"
                if (Directory.Exists(path))
                {
                    try
                    {
                        //Начинаем рекурсивное перемещение файлов
                        ProcessDirectory(path, fileExtensions, excludeDirectories, destinationPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка обработки папки {path}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Папка не найдена: {path}");
                }
            }
        }

        //Рекурсивная функция для обработки директорий
        //Принимает 4 аргумента: (currentDir) - текущая обрабатываемая директория, (fileExtensions) - список допустимых расширений файлов, (excludeDirectories) - список исключаемых директорий, и (destinationPath) - путь к целевой директории
        static void ProcessDirectory(string currentDir, List<string> fileExtensions, List<string> excludeDirectories, string destinationPath)
        {
            try
            {
                //Проверяем, не находится ли текущая папка в списке исключений
                if (excludeDirectories.Any(exclude => currentDir.StartsWith(exclude, StringComparison.OrdinalIgnoreCase)))
                {
                    return; // Если да, выполнение метода прерывается
                }

                //Обработка файлов в текущей директории
                foreach (var file in Directory.EnumerateFiles(currentDir).Where(file => fileExtensions.Contains(Path.GetExtension(file).ToLower())))
                {
                    try
                    {
                        string fileName = Path.GetFileName(file);
                        string truncatedFileName = TruncateFileName(fileName, 90); // Обрезаем имя файла до 90 символов
                        var destFile = Path.Combine(destinationPath, truncatedFileName);   // Формируем полный путь до файла

                        //Если файл с таким именем уже существует
                        if (File.Exists(destFile))
                        {
                            //Генерируем порядковый номер к концу имени
                            destFile = GetUniqueFileName(destinationPath, Path.GetFileNameWithoutExtension(truncatedFileName), Path.GetExtension(truncatedFileName));
                        }

                        //Копирование файла из текущей директории в целевую с перезаписью, а затем удаление исходного файла
                        File.Copy(file, destFile, true);
                        File.Delete(file);

                        Console.WriteLine($"Файл перемещён: {file} -> {destFile}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка перемещения файла {file}: {ex.Message}");
                    }
                }

                //Рекурсивно обрабатываем поддиректории
                var directories = Directory.EnumerateDirectories(currentDir);

                foreach (var directory in directories)
                {
                    //Рекурсивный вызов метода "ProcessDirectory" для каждой поддиректории, чтобы повторно обработать их таким же образом
                    ProcessDirectory(directory, fileExtensions, excludeDirectories, destinationPath);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Отказано в доступе к папке: {currentDir}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке директории {currentDir}: {ex.Message}");
            }
        }

        //Метод для усечения длинного имени файла
        //Принимает 2 аргумента: (fileName) имя файла и (maxLength) максимальная длина файла в символах
        private static string TruncateFileName(string fileName, int maxLength)
        {
            if (fileName.Length <= maxLength)
                return fileName;

            string extension = Path.GetExtension(fileName);
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            // Обрезаем имя файла до (maxLength - длина расширения) символов
            int truncatedLength = maxLength - extension.Length;
            if (truncatedLength <= 0)
                throw new ArgumentException("Максимальная длина слишком мала для сохранения расширения файла.");

            return nameWithoutExtension.Substring(0, truncatedLength) + extension;
        }

        //Метод для получения уникального имени файла
        //Принимает 3 аргумента: (destinationPath) путь, куда будет сохранён файл, (baseFileName) базовое имя файла и (extension) расширение файла
        static string GetUniqueFileName(string destinationPath, string baseFileName, string extension)
        {
            // Обрезаем базовое имя, чтобы после добавления GUID длина не превышала 90
            int maxBaseLength = 90 - 11 - extension.Length; // 1 для '_' + 10 для GUID
            if (maxBaseLength < 0) maxBaseLength = 0;

            if (baseFileName.Length > maxBaseLength)
                baseFileName = baseFileName.Substring(0, maxBaseLength);

            string baseNameWithoutSuffix = baseFileName;
            int lastUnderscoreIndex = baseFileName.LastIndexOf('_');

            if (lastUnderscoreIndex > 0)
            {
                string suffix = baseFileName.Substring(lastUnderscoreIndex + 1);
                if (IsValidGuidSuffix(suffix))
                    baseNameWithoutSuffix = baseFileName.Substring(0, lastUnderscoreIndex);
            }

            string guidSuffix;
            string newFileName;
            do
            {
                guidSuffix = Guid.NewGuid().ToString("N").Substring(0, 10);
                newFileName = $"{baseNameWithoutSuffix}_{guidSuffix}{extension}";
            } while (File.Exists(Path.Combine(destinationPath, newFileName)));

            return Path.Combine(destinationPath, newFileName);
        }

        //Метод для проверки валидции GUID
        private static bool IsValidGuidSuffix(string suffix)
        {
            if (suffix.Length != 10)
                return false;

            foreach (char c in suffix)
            {
                if (!Uri.IsHexDigit(c))
                    return false;
            }
            return true;
        }
    }
}
