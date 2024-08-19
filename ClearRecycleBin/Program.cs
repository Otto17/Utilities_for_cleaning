/*
   Простая программа, которая при запуске автоматически очищает корзину на всех дисках, не требуя подтверждения от пользователя.

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


using System;                           // Библиотека предоставляет доступ к базовым классам и функциональности .NET Framework
using System.Runtime.InteropServices;   // Библиотека предоставляет классы для работы с взаимодействием между управляемым и неуправляемым кодом, включая работу с COM-объектами и вызовами нативного кода

namespace ClearRecycleBin
{
    class Program
    {
        //Импортируем функцию "SHEmptyRecycleBin" из "shell32.dll"
        [DllImport("shell32.dll", SetLastError = true)]

        static extern uint SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, uint dwFlags);    // Используется для очистки корзины

        //Флаги для "SHEmptyRecycleBin"
        const uint SHERB_NOCONFIRMATION = 0x00000001;   // Отключает подтверждающие диалоги
        const uint SHERB_NOPROGRESSUI = 0x00000002;     // Отключает отображение диалога прогресса
        const uint SHERB_NOSOUND = 0x00000004;          // Отключает звуковое сопровождение

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Очистка корзины...");

                //Очищаем корзину на всех дисках
                uint result = SHEmptyRecycleBin(IntPtr.Zero, null, SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND);

                if (result == 0)
                {
                    Console.WriteLine("Корзина успешно очищена.");
                }
                else
                {
                    Console.WriteLine($"Произошла ошибка при очистке корзины. Код ошибки: {result}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}
