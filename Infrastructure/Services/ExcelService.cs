using System.Reflection;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ExcelService : IExcelService
{
    private readonly ILogger<ExcelService> _logger;

    public ExcelService(ILogger<ExcelService> logger)
    {
        _logger = logger;
    }
}
