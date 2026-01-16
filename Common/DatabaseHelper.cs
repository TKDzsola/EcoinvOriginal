using System;

namespace Ecoinv.Common
{
  public static class DatabaseHelper
  {
    public static string BuildConnectionString()
    {
      // Teljes elérési út az adatbázis fájlhoz
      string dbPath = @"c:\Users\prozs\source\repos\Ecoinv\ecinvfbdb4.FDB";

      // Firebird Embedded connection string
      return
          $"Database={dbPath};" +
          $"User=sysdba;" +
          $"Password=masterkey;" +
          $"ServerType=1;" +        // 1 = Embedded
          $"Charset=UTF8;";
    }
  }
}
