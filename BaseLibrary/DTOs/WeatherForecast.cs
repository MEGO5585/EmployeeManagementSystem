using System;

namespace BaseLibrary.DTOs;

public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC{ get; set; }
    public int TemperatureF { get; set; }
    public string? Summary{ get; set; }

}
