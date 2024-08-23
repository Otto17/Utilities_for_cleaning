/*
   Простая программа для восстановления сканов или загрузок из скрытой системной папки.

   Делал для личного использования под свои нужды.

   Данная программа является свободным программным обеспечением, распространяющимся по лицензии MIT.
   Копия лицензии: https://opensource.org/licenses/MIT
   Copyright (c) 2024 Otto
   Автор: Otto
   Версия: 23.08.24
   GitHub страница:  https://github.com/Otto17/Utilities_for_cleaning
   GitFlic страница: https://gitflic.ru/project/otto/utilities_for_cleaning
   г. Омск 2024
*/


using System;                   // Библиотека предоставляет доступ к базовым классам и функциональности .NET Framework
using System.IO;                // Библиотека отвечает за ввод и вывод данных, включая чтение и запись файлов
using System.Threading.Tasks;   // Библиотека для работы с асинхронным программированием и параллельными задачами


namespace load_mfu
{
    class Program
    {
        static void Main(string[] args)
        {
            //Настройки

            //Исходная папка (откуда копировать)
            string sourceDir = @"E:\$RECYCLER.BIN\МФУ";
            //Целевая папка (куда копировать)
            string targetDir = @"E:\ControlCenter\МФУ";

            //Счетчик скопированных файлов
            int copiedFilesCount = 0;

            try
            {
                //Получаем список всех файлов в исходной папке
                string[] files = Directory.GetFiles(sourceDir);

                //Копируем файлы параллельно
                Parallel.ForEach(files, (currentFile) =>
                {
                    string fileName = Path.GetFileName(currentFile);
                    string destFile = Path.Combine(targetDir, fileName);

                    //Если файл существует в целевой папке, удаляем его для замены
                    if (File.Exists(destFile))
                    {
                        File.Delete(destFile);
                    }

                    //Копируем файл в целевую папку
                    File.Copy(currentFile, destFile);
                    Console.WriteLine($"Скопирован: {fileName} -> {Path.GetFileName(destFile)}");

                    //Увеличиваем счетчик скопированных файлов
                    System.Threading.Interlocked.Increment(ref copiedFilesCount);
                });

                //Выводим количество скопированных файлов через один абзац
                Console.WriteLine("");
                Console.WriteLine($"Всего скопировано файлов: {copiedFilesCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }

            //Пауза для отображения результатов
            Console.WriteLine("Нажмите любую клавишу для завершения...");
            Console.ReadKey();
        }
    }
}
