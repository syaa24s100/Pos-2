using System.Globalization;
using Pos.Application;
using Pos.Application.Dtos;
using Pos.Domain.Common;

namespace Pos.Cli;

public sealed class ConsoleApp
{
    private readonly PosFacade _pos;

    public ConsoleApp(PosFacade pos) => _pos = pos;

    public void Run()
    {
        Console.WriteLine("=== ПОС: Преподавательская Оценка Студентов ===");
        while (true)
        {
            try
            {
                PrintMenu();
                var choice = Ask("Выбор");
                if (choice == "0") return;
                Execute(choice);
            }
            catch (EndOfStreamException)
            {
                return; 
            catch (Exception ex) when (ex is DomainException or UseCaseException)
            {
                Console.WriteLine($"[Ошибка] {ex.Message}");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                Console.WriteLine($"[Ошибка файла] {ex.Message}");
            }
            catch (OverflowException)
            {
                Console.WriteLine("[Ошибка] Слишком большое число.");
            }
        }
    }

    private static void PrintMenu()
    {
        Console.WriteLine();
        Console.WriteLine("1  Добавить студента        2  Список студентов      3  Удалить студента");
        Console.WriteLine("4  Добавить критерий        5  Список критериев      6  Удалить критерий");
        Console.WriteLine("7  Выставить оценку         8  Список оценок         9  Исправить оценку");
        Console.WriteLine("10 Отменить оценку          11 Итог студента         12 Итоги всех");
        Console.WriteLine("13 Сменить правило итога    14 Экспорт итогов в CSV  15 Демо-сценарий");
        Console.WriteLine("16 Переименовать студента");
        Console.WriteLine("0  Выход");
    }

    private void Execute(string choice)
    {
        switch (choice)
        {
            case "1":
                var s = _pos.AddStudent(Ask("ФИО студента"));
                Console.WriteLine($"Добавлен: {s.FullName}");
                break;
            case "2": PrintStudents(); break;
            case "3":
                _pos.RemoveStudent(PickStudent());
                Console.WriteLine("Студент и его оценки удалены.");
                break;
            case "4":
                var c = _pos.AddCriterion(Ask("Название критерия"), AskDecimal("Максимальный балл"), AskDecimal("Вес"));
                Console.WriteLine($"Добавлен критерий: {c.Name} (макс. {c.MaxScore}, вес {c.Weight})");
                break;
            case "5": PrintCriteria(); break;
            case "6":
                _pos.RemoveCriterion(PickCriterion());
                Console.WriteLine("Критерий и оценки по нему удалены.");
                break;
            case "7":
                var g = _pos.IssueGrade(PickStudent(), PickCriterion(), AskDecimal("Балл"), AskOptional("Комментарий (необязательно)"));
                Console.WriteLine($"Выставлено: {g.StudentName} — {g.CriterionName}: {g.Score}/{g.MaxScore}");
                break;
            case "8": PrintGrades(); break;
            case "9":
                var corrected = _pos.CorrectGrade(PickGrade(), AskDecimal("Новый балл"), AskOptional("Комментарий (необязательно)"));
                Console.WriteLine($"Исправлено: {corrected.StudentName} — {corrected.CriterionName}: {corrected.Score}/{corrected.MaxScore}");
                break;
            case "10":
                _pos.CancelGrade(PickGrade());
                Console.WriteLine("Оценка отменена.");
                break;
            case "11": PrintResult(_pos.CalculateFinal(PickStudent())); break;
            case "12":
                var all = _pos.CalculateAll();
                if (all.Count == 0) Console.WriteLine("Студентов нет.");
                foreach (var r in all) PrintResult(r);
                break;
            case "13": ChooseStrategy(); break;
            case "14":
                var path = AskOptional("Файл (по умолчанию results.csv)") ?? "results.csv";
                var n = _pos.ExportResults(path);
                Console.WriteLine($"Экспортировано строк: {n} → {Path.GetFullPath(path)}");
                break;
            case "15": RunDemo(); break;
            case "16":
                var renamed = _pos.RenameStudent(PickStudent(), Ask("Новое ФИО"));
                Console.WriteLine($"Теперь: {renamed.FullName}");
                break;
            default: Console.WriteLine("Неизвестная команда."); break;
        }
    }

    private void PrintStudents()
    {
        var list = _pos.ListStudents();
        if (list.Count == 0) Console.WriteLine("Студентов нет.");
        for (var i = 0; i < list.Count; i++) Console.WriteLine($"{i + 1}. {list[i].FullName}");
    }

    private void PrintCriteria()
    {
        var list = _pos.ListCriteria();
        if (list.Count == 0) Console.WriteLine("Рубрикатор пуст.");
        for (var i = 0; i < list.Count; i++)
            Console.WriteLine($"{i + 1}. {list[i].Name} — макс. {list[i].MaxScore}, вес {list[i].Weight}");
    }

    private void PrintGrades()
    {
        var list = _pos.ListGrades();
        if (list.Count == 0) Console.WriteLine("Оценок нет.");
        for (var i = 0; i < list.Count; i++)
        {
            var e = list[i];
            var comment = string.IsNullOrWhiteSpace(e.Comment) ? "" : $" // {e.Comment}";
            Console.WriteLine($"{i + 1}. {e.StudentName} | {e.CriterionName}: {e.Score}/{e.MaxScore} | {e.Date:dd.MM.yyyy}{comment}");
        }
    }

    private static void PrintResult(ResultDto r) =>
        Console.WriteLine(
            $"{r.StudentName}: {r.Percent:0.00}% ({r.Mark10:0.0} из 10), оценено {r.RatedCriteria}/{r.TotalCriteria}, правило «{r.Strategy}»");

    private void ChooseStrategy()
    {
        Console.WriteLine($"Сейчас: {_pos.CurrentStrategy}");
        foreach (var s in _pos.AvailableStrategies) Console.WriteLine($"  {s}");
        _pos.UseStrategy(Ask("Введите имя правила (all / rated)"));
        Console.WriteLine($"Теперь: {_pos.CurrentStrategy}");
    }

    private Guid PickStudent()
    {
        var list = _pos.ListStudents();
        if (list.Count == 0) throw new UseCaseException("Сначала добавьте студентов.");
        PrintStudents();
        return list[AskIndex("Номер студента", list.Count)].Id;
    }

    private Guid PickCriterion()
    {
        var list = _pos.ListCriteria();
        if (list.Count == 0) throw new UseCaseException("Рубрикатор пуст — добавьте критерии.");
        PrintCriteria();
        return list[AskIndex("Номер критерия", list.Count)].Id;
    }

    private Guid PickGrade()
    {
        var list = _pos.ListGrades();
        if (list.Count == 0) throw new UseCaseException("Оценок пока нет.");
        PrintGrades();
        return list[AskIndex("Номер оценки", list.Count)].Id;
    }


    private static string Ask(string prompt)
    {
        Console.Write($"{prompt}: ");
        var line = Console.ReadLine() ?? throw new EndOfStreamException();
        return line.Trim();
    }

    private static string? AskOptional(string prompt)
    {
        var s = Ask(prompt);
        return s.Length == 0 ? null : s;
    }

    private static decimal AskDecimal(string prompt)
    {
        while (true)
        {
            var text = Ask(prompt).Replace(',', '.');
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                return value;
            Console.WriteLine("Введите число.");
        }
    }

    private static int AskIndex(string prompt, int count)
    {
        while (true)
        {
            if (int.TryParse(Ask(prompt), out var n) && n >= 1 && n <= count) return n - 1;
            Console.WriteLine($"Введите число от 1 до {count}.");
        }
    }

    private void RunDemo()
    {
        if (_pos.ListStudents().Count > 0 || _pos.ListCriteria().Count > 0)
        {
            Console.WriteLine("Демо запускается на пустых данных — перезапустите программу.");
            return;
        }

        Console.WriteLine("--- 1. Студенты ---");
        var ivan = _pos.AddStudent("Иванов Иван");
        var anna = _pos.AddStudent("Петрова Анна");
        var oleg = _pos.AddStudent("Сидоров Олег");
        PrintStudents();

        Console.WriteLine("--- 2. Рубрикатор ---");
        var hw = _pos.AddCriterion("Домашние задания", 10, 0.4m);
        var sem = _pos.AddCriterion("Работа на семинарах", 10, 0.3m);
        var exam = _pos.AddCriterion("Контрольная", 20, 0.3m);
        PrintCriteria();

        Console.WriteLine("--- 3. Оценки ---");
        _pos.IssueGrade(ivan.Id, hw.Id, 9, "Всё сдавал вовремя");
        _pos.IssueGrade(ivan.Id, sem.Id, 8);
        _pos.IssueGrade(ivan.Id, exam.Id, 17);
        _pos.IssueGrade(anna.Id, hw.Id, 10);
        _pos.IssueGrade(anna.Id, exam.Id, 19, "Отлично");
        _pos.IssueGrade(oleg.Id, hw.Id, 5);

        Console.WriteLine("Попытка поставить 12 из 10 (должна быть отклонена доменом):");
        try { _pos.IssueGrade(oleg.Id, sem.Id, 12); }
        catch (DomainException ex) { Console.WriteLine($"[Отклонено] {ex.Message}"); }

        PrintGrades();

        Console.WriteLine("--- 4. Итоги (правило «all») ---");
        _pos.UseStrategy("all");
        foreach (var r in _pos.CalculateAll()) PrintResult(r);

        Console.WriteLine("--- 4а. Итоги (правило «rated») ---");
        _pos.UseStrategy("rated");
        foreach (var r in _pos.CalculateAll()) PrintResult(r);

        Console.WriteLine("--- 5. Экспорт ---");
        var count = _pos.ExportResults("results.csv");
        Console.WriteLine($"Экспортировано строк: {count} → {Path.GetFullPath("results.csv")}");
    }
}
