using System.Collections.Generic;
using System.Net;
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
  public static class GetData
  {
    [Function("GetData")]
    public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
        FunctionContext executionContext)
    {
      var logger = executionContext.GetLogger("GetData");
      logger.LogInformation("C# HTTP trigger function processed a request.");

      var content = await new StreamReader(req.Body).ReadToEndAsync();
      dynamic inData = JsonConvert.DeserializeObject(content);
      string sensorId = inData.SensorId;
      int startTimeStamp = inData.StartTimeStamp;
      int endTimeStamp = inData.EndTimeStamp;

      JObject data = new JObject();
      JArray array = new JArray();

      string strConn = Environment.GetEnvironmentVariable("SQLConnection");
      using (SqlConnection connection = new SqlConnection(strConn))
      {
        connection.Open();
        using (SqlCommand cmd = new SqlCommand())
        {
          cmd.Connection = connection;
          cmd.CommandType = System.Data.CommandType.Text;
          cmd.CommandText = "SELECT SensorId, TimeStamp, Temperature " +
                            "FROM BoilerLog " +
                            "WHERE SensorId = @SensorId " +
                            "AND TimeStamp >= @StartTimeStamp " +
                            "AND TimeStamp <= @EndTimeStamp " +
                            "ORDER BY TimeStamp ASC";
          cmd.Parameters.Add(new SqlParameter("@SensorId", SqlDbType.NVarChar, 50)).Value = sensorId;
          cmd.Parameters.Add(new SqlParameter("@StartTimeStamp", SqlDbType.Int)).Value = startTimeStamp;
          cmd.Parameters.Add(new SqlParameter("@EndTimeStamp", SqlDbType.Int)).Value = endTimeStamp;
          SqlDataReader dataReader = await cmd.ExecuteReaderAsync();
          if (dataReader.HasRows)
          {
            while (dataReader.Read())
            {
              array.Add(new JObject(
                new JProperty("timestamp", dataReader.GetInt32(1).ToString()),
                new JProperty("temperature", dataReader.GetDecimal(2).ToString("G"))
              ));
            }
            data.Add(new JProperty("sensorId", sensorId));
            data.Add(new JProperty("values", array));
          }
          dataReader.Close();
        }
        connection.Close();
      }

      logger.LogInformation(sensorId);

      var response = req.CreateResponse(HttpStatusCode.OK);
      response.Headers.Add("Content-Type", "application/json; charset=utf-8");
      response.Headers.Add("Access-Control-Allow-Origin", "https://web-olaboiler.azurewebsites.net");
      response.WriteString(data.ToString());

      return response;
    }
  }
}
