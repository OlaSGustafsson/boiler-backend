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
  public static class GetSensorIds
  {
    [Function("GetSensorIds")]
    public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req,
        FunctionContext executionContext)
    {
      var logger = executionContext.GetLogger("GetSensorIds");
      logger.LogInformation("C# HTTP trigger function processed a request.");

      var str = Environment.GetEnvironmentVariable("SQLConnection");
      JArray array = new JArray();
      using (SqlConnection connection = new SqlConnection(str))
      {
        connection.Open();
        using (SqlCommand cmd = new SqlCommand())
        {
          cmd.Connection = connection;
          cmd.CommandType = System.Data.CommandType.Text;
          cmd.CommandText = @"SELECT SensorId FROM BoilerSensors ORDER BY SensorOrder ASC";
          SqlDataReader dataReader = await cmd.ExecuteReaderAsync();
          if (dataReader.HasRows)
          {
            while (dataReader.Read())
            {
              array.Add(dataReader.GetString(0));
            }
          }
          dataReader.Close();
        }
        connection.Close();
      }

      var response = req.CreateResponse(HttpStatusCode.OK);
      response.Headers.Add("Content-Type", "application/json; charset=utf-8");

      response.WriteString(array.ToString());

      return response;
    }
  }
}
