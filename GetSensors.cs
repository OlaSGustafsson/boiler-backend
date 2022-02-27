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
  public static class GetSensors
  {
    [Function("GetSensors")]
    public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req,
    FunctionContext executionContext)
    {
      var logger = executionContext.GetLogger("GetSensors");
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
          cmd.CommandText = @"SELECT SensorId, Name, ISNULL(Description, ''), SensorOrder 
                                FROM BoilerSensors 
                                ORDER BY SensorOrder ASC";
          SqlDataReader dataReader = await cmd.ExecuteReaderAsync();
          if (dataReader.HasRows)
          {
            while (dataReader.Read())
            {
              array.Add(new JObject(
                new JProperty("id", dataReader.GetString(0)),
                new JProperty("name", dataReader.GetString(1)),
                new JProperty("description", dataReader.GetString(2)),
                new JProperty("order", dataReader.GetByte(3))
                ));
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
