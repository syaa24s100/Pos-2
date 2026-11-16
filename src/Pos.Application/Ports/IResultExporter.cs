using Pos.Application.Dtos;

namespace Pos.Application.Ports;

/// <summary>Порт: выгрузка итогов (CSV, JSON и т.д. — решает адаптер).</summary>
public interface IResultExporter
{
    void Export(IEnumerable<ResultDto> results, string path);
}
