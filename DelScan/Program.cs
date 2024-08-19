/*
   Простая программа для переноса всех файлов с исходных папок в целевые с очисткой пустых папок и ротацией файлов по дням.

    Делал для личного использования под свои нужды.

   Данная программа является свободным программным обеспечением, распространяющимся по лицензии MIT.
   Копия лицензии: https://opensource.org/licenses/MIT
   Copyright (c) 2024 Otto
   Автор: Otto
   Версия: 19.08.24
   GitHub страница:  https://github.com/Otto17/Utilities_for_cleaning
   GitFlic страница: https://gitflic.ru/project/otto/utilities_for_cleaning
   г. Омск 2024
*/


using System;                       // Библиотека предоставляет доступ к базовым классам и функциональности .NET Framework
using System.Collections.Generic;   // Библиотека предоставляет возможности для работы с запросами к коллекциям данных
using System.IO;                    // Библиотека отвечает за ввод и вывод данных, включая чтение и запись файлов
using System.Linq;                  // Библиотека позволяет хранить и обрабатывать наборы данных с использованием обобщений, обеспечивая типобезопасность и эффективность


namespace DelScan
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Настройки

            //Списки путей для переноса файлов в "Загрузки"
            var downloadPaths = new List<string> {
                @"C:\Users\продавец\Downloads", @"C:\Users\продавец\Загрузки", @"C:\Users\продавец\Pictures",
                @"C:\Users\продавец\Изображения", @"C:\Users\продавец\Videos", @"C:\Users\продавец\Видео",
                @"C:\Users\продавец\Music", @"C:\Users\продавец\Музыка",
                @"C:\Users\aks\Downloads", @"C:\Users\aks\Pictures", @"C:\Users\aks\Videos", @"C:\Users\aks\Music",
                @"E:\Загрузки"
            };

            //Списки путей для переноса файлов в "МФУ"
            var mfuPaths = new List<string> {
                @"E:\ControlCenter\Email", @"E:\ControlCenter\OCR", @"E:\ControlCenter\МФУ"
            };

            //Пути назначения (целевые папки)
            var downloadDestination = @"E:\$RECYCLER.BIN\Download";
            var mfuDestination = @"E:\$RECYCLER.BIN\МФУ";

            //Исключения для переноса (имена файлов и папок)
            var excludeNames = new List<string> { "desktop.ini", "vuescan.ini", "VueScan" };

            //Периоды удаления файлов в днях
            var deletePeriods = new Dictionary<string, int>
            {
                { downloadDestination, 90 },
                { mfuDestination, 60 }
            };

            //Перемещение файлов и папок
            MoveFilesAndDirectories(downloadPaths, downloadDestination, excludeNames);
            MoveFilesAndDirectories(mfuPaths, mfuDestination, excludeNames);

            //Ротация файлов и папок в целевых директориях
            RotateFilesAndDirectories(downloadDestination);
            RotateFilesAndDirectories(mfuDestination);

            //Удаление файлов старше заданного периода
            DeleteOldFiles(downloadDestination, deletePeriods[downloadDestination]);
            DeleteOldFiles(mfuDestination, deletePeriods[mfuDestination]);

            Console.WriteLine("Операция завершена.");
        }

        //Функция для перемещения папок и файлов с корневых (заданных) папок в целевые папки
        //Принимает 3 аргумента: "sourcePaths" - список исходных путей, "destinationPath" - целевой путь и "excludeNames" - список имен для исключения
        static void MoveFilesAndDirectories(List<string> sourcePaths, string destinationPath, List<string> excludeNames)
        {
            //Обрабатываем каждый путь из списка
            foreach (var path in sourcePaths)
            {
                //Проверяем, существует ли текущая директория
                if (Directory.Exists(path))
                {
                    //Перемещение файлов
                    //Получение всех файлов из текущей директории и её поддиректорий, фильтрация по именам из списка исключений
                    var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                        .Where(file => !excludeNames.Contains(Path.GetFileName(file)) && !excludeNames.Contains(Path.GetFileName(Path.GetDirectoryName(file))));

                    //Обрабатываем каждый файл в полученном списке
                    foreach (var file in files)
                    {
                        try
                        {
                            var destFile = Path.Combine(destinationPath, Path.GetFileName(file));   // Формируем путь к новому файлу

                            //Если файл с таким именем уже есть в целевой папке, формируем новое уникальное имя
                            if (File.Exists(destFile))
                            {
                                destFile = GetUniqueFileName(destinationPath, Path.GetFileNameWithoutExtension(file), Path.GetExtension(file));
                            }

                            ResetFileAttributes(file);  //Сбрасываем атрибуты файла
                            File.Move(file, destFile);  // Перемещаем файл

                            Console.WriteLine($"Файл перемещён: {file} -> {destFile}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка перемещения файла {file}: {ex.Message}");
                        }
                    }

                    //Перемещение папок, включая пустые папки
                    //Получение всех файлов из текущей директории и её поддиректорий, фильтрация по именам из списка исключений
                    var directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
                        .Where(dir => !excludeNames.Contains(Path.GetFileName(dir)) && !excludeNames.Contains(Path.GetFileName(Path.GetDirectoryName(dir))));

                    //Обрабатываем каждую папку из списка
                    foreach (var dir in directories)
                    {
                        try
                        {
                            var destDir = Path.Combine(destinationPath, Path.GetFileName(dir));

                            //Создаём подпапку в целевой папке, если её ещё там нет
                            if (!Directory.Exists(destDir))
                            {
                                Directory.CreateDirectory(destDir);
                            }

                            //Перемещение папки, если она пуста
                            if (!Directory.EnumerateFileSystemEntries(dir).Any())
                            {
                                ResetFileAttributes(dir);           // Сбрасываем атрибуты папки
                                Directory.Delete(dir);              // Удаляем папку из исходной папки
                                Directory.CreateDirectory(destDir); // Создаём папку в целевой директории
                                Console.WriteLine($"Пустая папка перемещена: {dir} -> {destDir}");
                            }
                            else
                            {
                                Console.WriteLine($"Папка перемещена: {dir} -> {destDir}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка перемещения папки {dir}: {ex.Message}");
                        }
                    }

                    //Рекурсивное удаление пустых папок в исходной директории
                    DeleteEmptyDirectoriesRecursively(path, excludeNames, sourcePaths);
                }
                else
                {
                    Console.WriteLine($"Папка не найдена: {path}");
                }
            }
        }

        //Функция рекурсивного удаления пустых папок
        static void DeleteEmptyDirectoriesRecursively(string path, List<string> excludeNames, List<string> rootPaths)
        {
            foreach (var dir in Directory.GetDirectories(path)) // Получаем массив подкаталогов в указанной директории и начинаем цикл по каждому из них
            {
                DeleteEmptyDirectoriesRecursively(dir, excludeNames, rootPaths);    // Рекурсивно удаляем каждый пустой подкаталог

                //Удаляем папку, если она пуста, не исключена и не является корневой
                if (!Directory.EnumerateFileSystemEntries(dir).Any() &&
                    !excludeNames.Contains(Path.GetFileName(dir)) &&
                    !IsRootPath(dir, rootPaths))
                {
                    try
                    {
                        //Если все проверки пройдены
                        ResetFileAttributes(dir);   // Сбрасываем атрибуты папки
                        Directory.Delete(dir);      // Удаляем папку
                        Console.WriteLine($"Пустая папка удалена: {dir}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка удаления пустой папки {dir}: {ex.Message}");
                    }
                }
            }
        }

        //Метод проверки, является ли заданный путь корневым путем из списка корневых путей
        static bool IsRootPath(string path, List<string> rootPaths)
        {
            return rootPaths.Any(root => string.Equals(path.TrimEnd('\\'), root.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase));
        }

        //Метод для ротации файлов и папок
        static void RotateFilesAndDirectories(string destinationPath)
        {
            try
            {
                //В переменной "directories" хранятся все подкаталоги в указанной директории, включая вложенные
                var directories = Directory.GetDirectories(destinationPath, "*", SearchOption.AllDirectories);

                foreach (var dir in directories)
                {
                    var files = Directory.GetFiles(dir);    // Получаем все файлы в текущей директории и сохраняем в "files"

                    //Проходим по каждому файлу в "files"
                    foreach (var file in files)
                    {
                        var destFile = Path.Combine(destinationPath, Path.GetFileName(file));   // Формируем путь

                        //Если файл с тем же именем уже существует в целевом месте, вызывается метод "GetUniqueFileName", чтобы создать уникальное имя для нового файла
                        if (File.Exists(destFile))
                        {
                            destFile = GetUniqueFileName(destinationPath, Path.GetFileNameWithoutExtension(file), Path.GetExtension(file));
                        }

                        ResetFileAttributes(file);  // Сбрасываем атрибуты файлу
                        File.Move(file, destFile);  // Перемещаем файл
                    }

                    //Удаляем пустую папку
                    if (!Directory.EnumerateFileSystemEntries(dir).Any())
                    {
                        ResetFileAttributes(dir);   // Сбрасываем атрибуты папки
                        Directory.Delete(dir);      // Удаляем папку
                        Console.WriteLine($"Папка удалена: {dir}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка ротации папок в {destinationPath}: {ex.Message}");
            }
        }

        //Метод удаления старых файлов старше заданного периода
        static void DeleteOldFiles(string path, int days)
        {
            try
            {
                var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)    // Получает список всех файлов по указанному пути и во всех подкаталогах
                    .Where(f => File.GetLastWriteTime(f).AddDays(days) < DateTime.Now);     // Фильтрует файлы, оставляя только те, которые были изменены более "days" дней назад

                //Запускаем цикл по всем найденным файлам
                foreach (var file in files)
                {
                    try
                    {
                        ResetFileAttributes(file);  // Сбрасываем атрибуты файлу
                        File.Delete(file);          // Удаляем файл
                        Console.WriteLine($"Удалён файл: {file}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка удаления файла {file}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления старых файлов в {path}: {ex.Message}");
            }
        }

        //Метод для генерации уникального имени файла
        static string GetUniqueFileName(string destinationPath, string baseFileName, string extension)
        {
            string newFileName = baseFileName + extension;  // Создаём начальное имя файла с расширением
            int counter = 1;                                // Счётчик для создания уникального имени

            //Цикл выполняется, пока файл с текущем именем существует в указанной директории
            while (File.Exists(Path.Combine(destinationPath, newFileName)))
            {
                newFileName = $"{baseFileName}{counter}{extension}";    // Формируем новое имя, прибавляя счётчик
                counter++;                                              // Инкрементируем счётчик
            }

            return Path.Combine(destinationPath, newFileName);  // Возвращаем полное уникальное имя файла, включая путь
        }

        //Метод для сброса атрибутов файлу или папке
        static void ResetFileAttributes(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);   // Получаем текущие атрибуты по указанному пути

            //Проверяем, установлен ли атрибут "Только для чтения"
            if (attributes.HasFlag(FileAttributes.ReadOnly))
            {
                attributes &= ~FileAttributes.ReadOnly; // Если атрибут "Только для чтения" установлен, то удаляем его
            }

            File.SetAttributes(path, attributes);   // Устанавливаем новые атрибуты файлу или папке
        }
    }
}
