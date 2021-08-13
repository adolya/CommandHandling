using System;

namespace CommandHandling.WebApi.Sample
{
    public class WeatherForecastInput
    {
        public int Input { get; set; }
    }
    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }
    }

    public class Some
    {
        public WeatherForecast Do(WeatherForecastInput i){
            return new WeatherForecast(){
                TemperatureC = i.Input
            };
        }

        public string Other(int i){
            return $"Responsed - {i}";
        }
    } 
}
