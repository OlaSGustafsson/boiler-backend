using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using System.Data;
using System.Data.SqlClient;

namespace eldstorp.boiler
{
  public static class LogData
  {
    [Function("LogData")]
    public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
        FunctionContext executionContext)
    {
      var logger = executionContext.GetLogger("LogData");
      logger.LogInformation("C# HTTP trigger function processed a request.");

      var content = await new StreamReader(req.Body).ReadToEndAsync();
      dynamic data = JsonConvert.DeserializeObject(content);
      int timeStamp = data.timestamp;
      int totalRows = 0;

      var str = Environment.GetEnvironmentVariable("SQLConnection");
      using (SqlConnection connection = new SqlConnection(str))
      {
        connection.Open();
        foreach (var sensor in data.sensordata)
        {
          using (SqlCommand cmd = new SqlCommand())
          {
            cmd.Connection = connection;
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = "INSERT INTO dbo.BoilerLog (SensorId, TimeStamp, Temperature) " +
                              "VALUES (@Id, @TimeStamp, @Temperature);";
            string sensorId = sensor.id;
            decimal temPerature = sensor.temperature;
            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.NVarChar, 50))
              .Value = sensorId;
            cmd.Parameters.Add(new SqlParameter("@TimeStamp", SqlDbType.Int))
              .Value = timeStamp;
            cmd.Parameters.Add(new SqlParameter("@Temperature", SqlDbType.Decimal) { Precision = 5, Scale = 2 })
              .Value = temPerature;
            totalRows += await cmd.ExecuteNonQueryAsync();
          }
        }
        connection.Close();
      }

      var response = req.CreateResponse(HttpStatusCode.OK);
      response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

      response.WriteString($"Loggade {totalRows} rader");

      return response;
    }
  }
}
