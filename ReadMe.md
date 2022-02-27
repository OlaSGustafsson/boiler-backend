# Title

[Code Traveler - Creating Azure Functions using .NET 5](https://codetraveler.io/2021/05/28/creating-azure-functions-using-net-5/)

## To run locally

On the command line, navigate to the folder containing your Azure Functions CSPROJ and enter the following command: `func start --verbose`

## Database

db-boiler

### Tables

#### BoilerLog

| Column | Datatype |
|:-------|:---------|
|SensorId|nvarchar(50)|
|TimeStamp|int|
|Temperature|decimal(5,2)|

``` sql
CREATE TABLE BoilerLog (
  SensorId nvarchar(50),
  TimeStamp int,
  Temperature decimal(5,2)
)
```

#### BoilerSensors

| Column | Datatype |
|:-------|:---------|
|SensorId|nvarchar(50)|
|Name|nvarchar(50)|
|Description|nvarchar(255)|
|SensorOrder|tinyint|

``` sql
CREATE TABLE BoilerSensors (
  SensorId nvarchar(50),
  Name nvarchar(50),
  Description nvarchar(255),
  SensorOrder tinyint
)
```

