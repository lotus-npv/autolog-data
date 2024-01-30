using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Quartz;
using AutoLog.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Logging;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.Json;

namespace AutoLog.Job
{
    internal class WriteLogToDbJob : IJob
    {
        private readonly ILogger<WriteLogToDbJob> _logger;

        public WriteLogToDbJob(ILogger<WriteLogToDbJob> logger)
        {
            _logger = logger;
        }

        //Ham thuc thi tac vu
        public Task Execute(IJobExecutionContext context)
        {
            if (AutoWLog.GetConfigString("SelectDB") == "MongoDb")
            {
                AddLogToMongoDB(AutoWLog.GetConfigString("social"));
            }
            else
            {
                AddLogToSqlSv(AutoWLog.GetConfigString("social"));
            }

            return Task.CompletedTask;
        }

        public void AddLogToMongoDB(string Social)

        {
            List<Log> logs = ReadAuditLog(GetLogPath(Social));

            try
            {
                var client = new MongoClient(AutoWLog.GetConfigString("MongoDb:server"));
                var database = client.GetDatabase(AutoWLog.GetConfigString("MongoDb:database"));

                var collectionNames = database.ListCollectionNames().ToList();

                var collection = database.GetCollection<LogDocument>(AutoWLog.GetConfigString("MongoDb:collection"));
                string logFrom = AutoWLog.GetConfigString("Serilog:Properties:Application");
                var Filter = Builders<LogDocument>.Filter.Eq("LogFrom", logFrom);

                LogDocument LastDocument = new();
                LastDocument = collection.Find(Filter)
                    .Sort(Builders<LogDocument>.Sort.Descending("_id"))
                    .Limit(1)
                    .FirstOrDefault();

                int index = logs.Count - 1;
                //Console.WriteLine($"[Info] LastLine: {LastDocument.DateSourceError}  --  From : {LastDocument.LogFrom}");

                if (LastDocument == null)
                {
                    Console.WriteLine("[Info] Khong tim thay du lieu tren database.");
                    WriteLogByMongo(collection, logs);
                }
                else
                {
                    index = logs.FindIndex(s => s.Timestamp?.Trim() == LastDocument?.DateSourceError?.Trim());
                }

                Console.WriteLine($"[Info] Index :  {index}");
                if (index == -1)
                {
                    WriteLogByMongo(collection, logs);
                }
                else
                {
                    if (index < logs.Count - 1)
                    {
                        List<Log> NewList = logs.GetRange(index + 1, logs.Count - index - 1);
                        WriteLogByMongo(collection, NewList);
                    }
                    else
                    {
                        Console.WriteLine("[Info] Du lieu da cap nhat ");
                    }

                }
                client = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cann't connect to MongoDb.");
            }
        }

        public void WriteLogByMongo(IMongoCollection<LogDocument> collection, List<Log> logs)

        {
            foreach (var item in logs)
            {
                try
                {
                    LogDocument document = ProcessData(item);
                    collection.InsertOne(document);
                    Console.WriteLine("[Info] MongoDb ghi du lieu thanh cong");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ghi log khong thanh cong.");
                    ProcessWhenNotWriteToDatabase(item);
                }
            }
        }


        public void AddLogToSqlSv(string Social)
        {
            List<Log> logs = ReadAuditLog(GetLogPath(Social));
            string SelectDB = AutoWLog.GetConfigString("SelectDB");
            Console.WriteLine("Database :" + AutoWLog.GetConfigString(SelectDB));
            var connection = new SqlConnection(AutoWLog.GetConfigString(SelectDB));
            connection.StatisticsEnabled = true;
            connection.FireInfoMessageEventOnUserErrors = true;
            try
            {
                connection.Open(); // Mở kết nối
                Console.WriteLine("[Info] Ket noi thanh cong");
                string LateLine = GetLateLine(connection);
                Console.WriteLine($"[Info] Late line : {LateLine} ");
                int index = logs.FindIndex(item => item.Timestamp?.Trim() == LateLine.Trim()); // Tim duoc doi tuong cuoi cung duoc ghi vao database
                Console.WriteLine($"[Info] Index : {index}  --  Count : {logs.Count}");

                if (index == -1)
                {
                    Console.WriteLine("[Info] Khong tim thay du lieu trong file log, ghi toan bo noi dung file vao database .");
                    WriteLogBySqlServer(logs, connection); // Ghi vao database
                }
                else
                {
                    if (index < (logs.Count - 1))
                    {
                        Console.WriteLine($"[Info] Mang moi tu {index + 1} -- {logs.Count - index - 1}");
                        List<Log> NewList = logs.GetRange(index + 1, logs.Count - index - 1);  //Tao mot List moi tu dong tiep theo den cuoi danh sach
                        WriteLogBySqlServer(NewList, connection); // Ghi vao database
                    }
                    else
                    {
                        Console.WriteLine("[Info] Da cap nhat du lieu moi nhat");
                    }
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                //TomoLog.WLog<WriteLogToDbJob>(ex,"Cann't connect to database.",Level.Fatal);
                _logger.LogError(ex, "Cann't connect to Sql Server.");
            }
        }

        public static void WriteLogBySqlServer(List<Log> logs, SqlConnection connection)
        {
            Console.WriteLine($"[Ghi Log] {logs.Count} logs");
            foreach (Log log in logs)
            {
                try
                {
                    LogDocument document = ProcessData(log);

                    string insertQuery = "INSERT INTO Log(LogTitle,LogFrom,FunctionNameLog,ErrorMessage,LogLevel,SourceOrigin,IP,DateSourceError,Description,CorrelationId,CreatedAt,CreatedBy,LastModifedAt,LastModifedBy,Flag) VALUES (@logTitle,@logFrom,@functionNameLog,@errorMessage,@logLevel,@sourceOrigin,@ip,@dateSourceError,@description,@correlationId,@createdAt,@createdBy,@lastModifedAt,@lastModifedBy,@flag)";
                    using (SqlCommand command = new(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@logTitle", document.LogTitle);
                        command.Parameters.AddWithValue("@logFrom", document.LogFrom);
                        command.Parameters.AddWithValue("@functionNameLog", document.FunctionNameLog);
                        command.Parameters.AddWithValue("@errorMessage", document.ErrorMessage);
                        command.Parameters.AddWithValue("@logLevel", document.LogLevel);
                        command.Parameters.AddWithValue("@sourceOrigin", document.SourceOrigin);
                        command.Parameters.AddWithValue("@ip", document.IP);
                        command.Parameters.AddWithValue("@dateSourceError", document.DateSourceError);
                        command.Parameters.AddWithValue("@description", document.Description);
                        command.Parameters.AddWithValue("@correlationId", document.CorrelationId);
                        command.Parameters.AddWithValue("@createdAt", document.CreatedAt);
                        command.Parameters.AddWithValue("@createdBy", document.CreatedBy);
                        command.Parameters.AddWithValue("@lastModifedAt", document.LastModifedAt);
                        command.Parameters.AddWithValue("@lastModifedBy", document.LastModifedBy);
                        command.Parameters.AddWithValue("@flag", document.Flag);

                        // Thực thi câu lệnh SQL
                        int row = command.ExecuteNonQuery();

                        if (row == -1)
                        {
                            Console.WriteLine($"[Info] Khong ghi duoc du lieu vao database, vui long kiem tra ...");
                            Console.WriteLine($"[Info] Du lieu se duoc ghi sang file 'backup.json' ");
                            //TomoLog.WLog<WriteLogToDbJob>($"Khong ghi thanh cong log co CorrelationId:{log.CorrelationId}  or Timestam:{log.Timestamp}", Level.Warning);

                            if (log != null)
                            {
                                ProcessWhenNotWriteToDatabase(log);  // Ghi log khong ghi duoc vao database xuong file backup.json
                            }

                        }
                        else
                        {
                            Console.WriteLine($"[Info] Code {row} : Ghi du lieu thanh cong");
                        }

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }


        // Xu ly thong tin file log truoc khi ghi vao database
        public static LogDocument ProcessData(Log log)
        {
            LogDocument document = new();

            document.LogTitle = log.MessageTemplate;
            document.LogFrom = log?.Application;
            string? sourceContext = (log?.SourceContext) ?? "null";

            document.FunctionNameLog = "";
            if (log?.ActionName == null)
            {
                if (document.LogTitle?.Split("|").Length == 3)
                {
                    document.FunctionNameLog = document.LogTitle?.Split("|")[1];
                }
                else
                {
                    document.FunctionNameLog = sourceContext;
                }
            }
            else
            {
                document.FunctionNameLog = log?.ActionName;
            }


            document.ErrorMessage = "";
            if (log?.Exception == null)
            {
                if (document.LogTitle?.Split("|").Length == 3)
                {
                    document.FunctionNameLog = document.LogTitle?.Split("|")[2];
                }
                else
                {
                    document.ErrorMessage = document.LogTitle;
                }
            }
            else
            {
                document.ErrorMessage = log?.Exception;
            }

            document.LogLevel = "";
            if (log?.Level == null)
            {
                if (document.LogTitle?.Split("|").Length == 3)
                {
                    document.LogLevel = document.LogTitle?.Split("|")[0];
                }
                else
                {
                    document.LogLevel = "Information";
                }
            }
            else
            {
                document.LogLevel = log?.Level;
            }

            document.SourceOrigin = "";
            if (log?.RequestPath == null)
            {
                if (document.LogTitle?.Split("|").Length == 3)
                {
                    document.SourceOrigin = document.LogTitle?.Split("|")[1];
                }
                else
                {
                    document.SourceOrigin = log?.Application;
                }
            }
            else
            {
                document.SourceOrigin = log?.RequestPath;
            }

            document.IP = (log?.ClientIp) ?? "null";
            document.DateSourceError = log?.Timestamp;
            document.CorrelationId = (log?.CorrelationId) ?? "null";
            document.Description = log?.EnvironmentName;
            document.CreatedAt = GetDateTime();
            document.CreatedBy = 1;
            document.LastModifedAt = GetDateTime();
            document.LastModifedBy = 1;
            document.Flag = "s";

            return document;
        }


        //Xu ly khi khong ghi duoc log vao database
        public static void ProcessWhenNotWriteToDatabase(Log log)
        {
            string filePath = "./Logs/backup.json";

            List<Log> logList = ReadAuditLog(filePath);

            Console.WriteLine($"[Info] Count logList : {logList.Count}");

            int index = logList.FindIndex(item => item.Timestamp?.Trim() == log.Timestamp?.Trim()); // Tim duoc doi tuong cuoi cung duoc ghi vao database

            if (index == -1)
            {
                using (FileStream fileStream = new(filePath, FileMode.Append, FileAccess.Write))
                {
                    using (StreamWriter streamWriter = new(fileStream))
                    using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter))
                    {
                        // Tùy chỉnh định dạng để có tự động xuống dòng
                        jsonWriter.Formatting = Formatting.Indented;

                        // Sử dụng JsonSerializer để chuyển đổi đối tượng thành JSON và ghi vào tệp tin
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        serializer.Serialize(jsonWriter, log);
                        Console.WriteLine($"[Info] Khong ghi thanh cong log co CorrelationId:{log.CorrelationId}  or Timestam:{log.Timestamp}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"[Info] Log da duoc ghi vao file backup, khong can ghi lai.");
            }

        }


        //Tim dong cuoi cung duoc dich vu ghi len database
        public static string GetLateLine(SqlConnection connection)
        {
            string str = "";
            string logFrom = AutoWLog.GetConfigString("Serilog:Properties:Application");
            string Query = $"SELECT TOP 1 * FROM Log WHERE LogFrom = '{logFrom}' order by LogId DESC ;";
            using (SqlCommand command = new SqlCommand(Query, connection))
            {
                var result = command.ExecuteReader();
                if (result.HasRows)
                {
                    while (result.Read())
                    {
                        str = result.GetString("DateSourceError");
                    }
                }
                else
                {
                    Console.WriteLine("[Info] Khong co du lieu trong database.");
                }
            }
            return str;
        }

        //Lay duong dan full cua file log
        public static string GetLogPath(string social)
        {
            DateTime date = DateTime.Now;
            string year = date.Year.ToString();
            string month = date.Month >= 10 ? date.Month.ToString() : string.Concat("0", date.Month.ToString());
            string day = date.Day >= 10 ? date.Day.ToString() : string.Concat("0", date.Day.ToString());

            string file = string.Concat(social, $"_{year}{month}{day}.json");
            string path = "./Logs/";

            string FullPath = string.Concat(path, file);
            return FullPath;
        }


        // Doc thong tin tu file log
        public static List<Log> ReadAuditLog(string FullPath)
        {

            var list = new List<Log>();

            Console.WriteLine($"[Full Path] {FullPath}");

            if (File.Exists(FullPath))
            {
                using FileStream fileStream = new(FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader streamReader = new(fileStream);
                var serializer = new Newtonsoft.Json.JsonSerializer();
                using var jsonTextReader = new JsonTextReader(streamReader);
                jsonTextReader.SupportMultipleContent = true;

                while (jsonTextReader.Read())
                {
                    JObject obj = JObject.Load(jsonTextReader);
                    var logEntry = JsonConvert.DeserializeObject<Log>(obj.ToString());
                    if (logEntry != null) { list.Add(logEntry); }
                }
            }
            else
            {
                Console.WriteLine($"[Info] Chua co du lieu cho ngay {GetDateTime()}");
            }

            return list;
        }


        // Lay ngay thang 
        public static string GetDateTime()
        {
            string format = "yyyy-MM-dd HH:mm:ss.fff";
            //DateTimeOffset dateTimeOffset = DateTimeOffset.Now;
            DateTime date = DateTime.Now;
            string s = date.ToString(format);

            return s;
        }


    }
}
