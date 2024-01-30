# ITomoLog & ITomoTask Library
## Serilog custom library

![GitHub repo size](https://img.shields.io/github/repo-size/phamnam91th/Selog_Library)
![GitHub contributors](https://img.shields.io/github/contributors/phamnam91th/Selog_Library)
![GitHub stars](https://img.shields.io/github/stars/phamnam91th/Selog_Library?style=social)
![GitHub forks](https://img.shields.io/github/forks/phamnam91th/Selog_Library?style=social)
![Twitter Follow](https://img.shields.io/twitter/follow/phamnam91th?style=social)

## I: Thêm thư viện vào dự án : (Update)

Sử dụng Nuget:

    dotnet add package ITomoTask --version 1.0.2
    dotnet add package ITomoLog --version 1.0.1

---------------------------------------------------------------------------------------------------------------------------------------------------
    
## II: Sử dụng :

1: Trong file Program.cs thêm thư viện SelogLib:

a: thêm namespace :

    using ITomoLog;
    using ITomoTask;
    
b: thêm dòng lệnh :

    TomoLog.Load(builder); // Nạp dịch vụ serilog
    TomoTask.Load(builder);  // nạp dịch vụ Quartz lập lịch tự động ghi log vào DB
  
  vào sau dòng lệnh :
  
    var builder = WebApplication.CreateBuilder(args);

c: thêm dòng lệnh :

    app.UseMiddleware(typeof(ExceptionHandlingMiddlewareOfSelog));  //thêm Middleware custom để bắt ngoại lệ và ghi log theo cấu hình chung.

  vào sau dòng lệnh :

    var app = builder.Build();


2: Để ghi log  thì thêm thư viện như bước trên , sau đó sử dụng :

    TomoLog.WLog<Class_name>("Nội dung log",Cấp độ log); // ex: TomoLog.WLog<WeatherForecastController>("WeatherForecast not start", Level.Error);
    
  hoặc :
     
    TomoLog.WLog<Class_name>(ngoại lệ,"Nội dung log",Cấp độ log); 
  
  ex:
  
    catch (Exception ex)
    {
        TomoLog.WLog<MathController>(ex,ex.Message,Level.Error);
        return 0;
    }


---------------------------------------------------------------------------------------------------------------------------------------------------



## III : Cấu hình ghi log mở file "appsettings.json" 

Hướng dẫn: Copy nội dung trong file "appsettings.json" chèn thay cho đoạn code mặc định trong file cùng tên nằm ở thư mục gốc của project.
    - chèn thay cho đoạn code mặc định bên dưới:
    
    "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },

1:  Cấu hình cơ sở dữ liệu để ghi log, lựa chọn dùng csdl nào thì nhập tên ở "SelectDB" :


    "Sqlsv": {
    "ketnoi1": "Data Source=your_host,1433;Initial Catalog=your_database;User ID=your_id;Password=your_password;Trusted_Connection=True;MultipleActiveResultSets=true;Integrated Security=false",
    "ketnoi2": "Data Source=your_host,1433;Initial Catalog=your_database;User ID=your_id;Password=password;Trusted_Connection=True;MultipleActiveResultSets=true;Integrated Security=false"
    },

    "MongoDb": {
      "server": "mongodb://user:password@localhost:27017",  
      "database": "your_database",
      "collection": "your_collection"
    },

    "SelectDB": "MongoDb", //neu chon sqlserver thi nhap:   "Sqlsv:ketnoi2"

    
2:  Cấu hình đọc tên tệp log, tên này sẽ tương ứng với tên đặt cho file json ở cấu hình mục 3 :

    "social": "hs_tiki",  // tien to file log , ex: hs_tiki_20230930.json

3:  Cấu hình ghi log vào file Json :


      "Name": "File",
      "Args": {
        "path": "Logs/hs_tiki_.json",
        "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact",
        "rollingInterval": "Day",
        "retainedFileCountLimit": 7
      }

4:  Cấp độ ghi thấp nhất đang để là Debug , các log từ hệ thống là Warning :


    "MinimumLevel": {
          "Default": "Debug",
          "Override": {
            "Microsoft": "Warning",
            "System": "Warning",
            "Microsoft.Hosting.Lifetime": "Warning"
          }
        }


5:  Chia hệ thống ghi log làm 2 phần riêng theo cấp độ khác nhau, mỗi phần sẽ có 1 file log :


    "Filter": [
      {
        "Name": "ByIncludingOnly",
        "Args": {
          "expression": "(@Level = 'Error' or @Level = 'Fatal' or @Level = 'Warning')"
        }
      }
    ]


    "Filter": [
      {
        "Name": "ByIncludingOnly",
        "Args": {
          "expression": "(@Level = 'Information' or @Level = 'Debug')"
        }
      }
    ]


6:  Cấu hình ghi log vào file văn bản, mặc định không dùng:


     "Name": "File",
     "Args": {
       "path": "./logs/RestApiLog.log",
       "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz},[{Level}] [{SourceContext}] [{EventId}] {Message}{NewLine}{Exception}",
       "rollOnFileSizeLimit": true,
       "fileSizeLimitBytes": 4194304,
       "retainedFileCountLimit": 15,
       "rollingInterval": "Minute"
     }

7:  Cấu hình ghi log vào Seq (tùy chọn thêm):

      "Name": "Seq",
      "Args": {
        "ServerUrl": "http://localhost:5341",
        "ApiKey": "zOrnBW2oEL9AmYoPig0n"
      }


8:    Cấu hình ghi log vào Bot telegram (Chỉ sử dụng cho các log quan trọng):

     "Name": "TeleSink",
     "Args": {
       "telegramApiKey": "your_API key",
       "telegramChatId": "your_chat_Id"
     }


---------------------------------------------------------------------------------------------------------------------------------------------------


## IV: Hướng dẫn chi tiết tạo bot telegram :

Bước 1: Truy cập vào Telegram.

Bước 2: Nhập Botfather trên thanh tìm kiếm.

Bước 3: Chọn vào Botfather có tích xanh.

![image](https://github.com/lvtienpm/Common/assets/83792539/f78903fc-e138-43a0-b017-ce04c66c93a5)

Bước 4: Nhấn vào Start.

Bước 5: Đoạn chat được hiển thị > Nhấn vào phần /newbot – create a new bot.

Bước 6: Điền tên bạn mong muốn cho Bot.

![image](https://github.com/lvtienpm/Common/assets/83792539/2c609809-d1a9-49df-86b1-64fe67fb9679)

Bước 7:  Nhấn nút Gửi

Bước 8: Điền tên người dùng cho Bot.

Lưu ý: Tên bạn muốn tạo cho bot phải có đuôi kết thúc bằng chữ “bot”, ví dụ như isFngBot hoặc isFng_bot.

Bước 9: Nhấn nút Gửi

![image](https://github.com/lvtienpm/Common/assets/83792539/65582615-741e-4bd4-8a70-3b4d68332a60)

Bước 10: Hệ thống gửi thông báo thành công xác nhận thành công.

Bước 11: copy API mà Botfather cung cấp và lưu trữ cẩn thận.

Bước 12: Tạo 1 public channel rồi add bot vào đó và set bot làm adminstrators.

Bước 13: Copy tên channel và API key ở trên để cấu hình trong Serilog.








 

  
  

  

  
