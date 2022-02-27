using System.Collections.Generic;
using System.Net;
using System.Globalization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System;
using System.Data;
using System.Data.SqlClient;

namespace eldstorp.boiler
{
  public static class GetLatest
  {
    [Function("GetLatest")]
    public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req,
        FunctionContext executionContext)
    {
      var logger = executionContext.GetLogger("GetLatest");
      logger.LogInformation("C# HTTP trigger function processed a request.");

      // var content = await new StreamReader(req.Body).ReadToEndAsync();
      // dynamic data = JsonConvert.DeserializeObject(content);
      // int timeStamp = data.timestamp;
      int totalRows = 0;

      var str = Environment.GetEnvironmentVariable("SQLConnection");
      JObject data = new JObject();
      JArray array = new JArray();
      using (SqlConnection connection = new SqlConnection(str))
      {
        connection.Open();
        using (SqlCommand cmd = new SqlCommand())
        {
          cmd.Connection = connection;
          cmd.CommandType = System.Data.CommandType.Text;
          cmd.CommandText = @"SELECT TOP (SELECT COUNT(SensorId) FROM BoilerSensors) 
                                TimeStamp, log.SensorId, s.Name, Temperature 
                                FROM BoilerLog log 
                                JOIN BoilerSensors s ON log.SensorId = s.SensorId 
                                ORDER BY TimeStamp DESC, s.SensorOrder ASC";
          SqlDataReader dataReader = await cmd.ExecuteReaderAsync();
          Int32 timeStamp = 0;
          if (dataReader.HasRows)
          {
            while (dataReader.Read())
            {
              totalRows += 1;
              timeStamp = dataReader.GetInt32(0);
              array.Add(new JObject(
                new JProperty("id", dataReader.GetString(1)),
                new JProperty("name", dataReader.GetString(2)),
                new JProperty("temperature", dataReader.GetDecimal(3).ToString("G"))
              ));
            }
            data.Add(new JProperty("timestamp", timeStamp.ToString()));
            data.Add(new JProperty("sensors", array));
          }
          dataReader.Close();
        }
        connection.Close();
      }

      var response = req.CreateResponse(HttpStatusCode.OK);
      response.Headers.Add("Content-Type", "application/json; charset=utf-8");
      response.Headers.Add("Access-Control-Allow-Origin", "https://web-olaboiler.azurewebsites.net");

      response.WriteString(data.ToString());

      return response;
    }
  }
}
