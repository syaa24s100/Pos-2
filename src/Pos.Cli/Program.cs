using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.Application.Ports;
using Pos.Cli;
using Pos.Domain.Repositories;
using Pos.Domain.Services;
using Pos.Domain.Strategies;
using Pos.Infrastructure;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var services = new ServiceCollection();

services.AddSingleton<IStudentRepository, InMemoryStudentRepository>();
services.AddSingleton<IRubricRepository, InMemoryRubricRepository>();
services.AddSingleton<IEvaluationRepository, InMemoryEvaluationRepository>();
services.AddSingleton<IResultExporter, CsvResultExporter>();

services.AddSingleton<FinalGradeCalculator>();
services.AddSingleton<IFinalGradeStrategy, WeightedAllCriteriaStrategy>();
services.AddSingleton<IFinalGradeStrategy, WeightedRatedOnlyStrategy>();

services.AddSingleton<PosFacade>();
services.AddSingleton<ConsoleApp>();

using var provider = services.BuildServiceProvider();
provider.GetRequiredService<ConsoleApp>().Run();
