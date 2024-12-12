using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AKBUtilities;

using MySql.Data.MySqlClient;

namespace Projekt1;
public class LoadCellMeasurementsDataBase
{
    public LoadCellMeasurementsDataBase()
    {
        if (!check_db_exist())
        {
            create_db();
            create_loadcell_measurements();
            return;
        }

        if (!check_table_exist())
            create_loadcell_measurements();

    }

    /// <summary>
    /// create Database
    /// </summary>
    /// <returns></returns>
    public bool create_db()
    {
        string query =
            $@"CREATE DATABASE `{CommonUse.CONST_DB_NAME}` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N'*/;
";
        using MySqlConnection _connection = new (CommonUse.CONST_DB_CONNECTROOT);

        _connection.Open();
        using MySqlCommand cmd = new (query, _connection);

        try
        {
            cmd.ExecuteNonQuery();
            return true;
        } catch (Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    /// create loadcellMeasurement table
    /// </summary>
    /// <returns></returns>
    public bool create_loadcell_measurements()
    {
        string query =
            "CREATE TABLE `loadcellMeasurement` (`dateTime` DATETIME NOT NULL,`LC1` DOUBLE,`LC2` DOUBLE,`LC3` DOUBLE,`LC4` DOUBLE,`LC5` DOUBLE,`LC6` DOUBLE,`LC7` DOUBLE,`LC8` DOUBLE,`LC9` DOUBLE,`LC10` DOUBLE,`LC11` DOUBLE,`LC12` DOUBLE) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

        using MySqlConnection _connection = new (CommonUse.CONST_DB_CONNECTSERVER);

        _connection.Open();
        using MySqlCommand cmd = new (query, _connection);
        try
        {
            cmd.ExecuteNonQuery();
            return true;
        } catch (Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    /// check if application database exists
    /// </summary>
    /// <returns></returns>
    public bool check_db_exist()
    {
        string query =
            $@"SELECT COUNT(*) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{CommonUse.CONST_DB_NAME}'";

        using MySqlConnection _connection = new(CommonUse.CONST_DB_CONNECTROOT);

        _connection.Open();
        using MySqlCommand cmd = new(query, _connection);

        try
        {
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        } catch (Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    /// check if table in database exists
    /// </summary>
    /// <returns></returns>
    public bool check_table_exist()
    {
        string query =
            $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{CommonUse.CONST_DB_NAME}' AND TABLE_NAME = 'loadcellMeasurement'";

        using MySqlConnection _connection = new (CommonUse.CONST_DB_CONNECTSERVER);

        _connection.Open();
        using MySqlCommand cmd = new (query, _connection);

        try
        {
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        } catch (Exception ex)
        {
            return false;
        }
    }

    public bool insert_value(double[] values)
    {
        if (values.Length < 12)
            throw new ArgumentException("@insert_value input parameter has to be integer array of length 12");

        using MySqlConnection _connection = new(CommonUse.CONST_DB_CONNECTSERVER);
        _connection.Open();
        using var _cmd = _connection.CreateCommand();
        _cmd.CommandText = "INSERT INTO loadcellMeasurement ( dateTime"; 
        for (var i = 0; i < values.Length; i++)
        {
            if (values[i] > 0)
            {
                _cmd.CommandText += $",LC{i + 1}";
                _cmd.Parameters.AddWithValue($"@lc{i + 1}", values[i]);
            }
        }
        _cmd.CommandText += ") VALUES( @dateTime";
        for (var i = 0; i < values.Length; i++)
        {
            if (values[i] > 0)
                _cmd.CommandText += $",@lc{i + 1}";
        }
        _cmd.CommandText += ")";
        _cmd.Parameters.AddWithValue($"@dateTime", DateTime.Now);

        var thi = _cmd.ExecuteNonQuery();

        return true;
    }

    public bool get_mean_stdev_analytics(double USL, double LSL, out mean_stdev_stats[] avg)
    {
        avg = new mean_stdev_stats[12];
        var avgcache = new mean_stdev_stats[12];
        Parallel.For(0
                   , avg.Length
                   , i =>
                     {
                         using MySqlConnection _connection = new(CommonUse.CONST_DB_CONNECTSERVER);
                         _connection.Open();

                         using var _cmd = _connection.CreateCommand();

                         //_cmd.Parameters.AddWithValue("@columnName", $"LC{i + 1}");
                         //_cmd.Parameters.AddWithValue("@UPP", $"{USL}");
                         //_cmd.Parameters.AddWithValue("@LOW", $"{LSL}");
                         _cmd.CommandText = $@"
USE {CommonUse.CONST_DB_NAME};

WITH stats AS (
    SELECT 
        AVG(LC{i + 1}) AS mean_value,
        STDDEV(LC{i + 1}) AS stddev_value,
        MIN(LC{i + 1}) AS min_value,
        MAX(LC{i + 1}) AS max_value
    FROM 
        loadcellMeasurement
)
SELECT 
    mean_value,
    stddev_value,
    min_value,
    max_value,
    (USL - mean_value) / (3 * stddev_value) AS Cpu,
    (mean_value - LSL) / (3 * stddev_value) AS Cpl,
    LEAST((USL - mean_value) / (3 * stddev_value), (mean_value - LSL) / (3 * stddev_value)) AS Cpk
FROM 
    stats,
    (SELECT {USL} AS USL, {LSL} AS LSL) AS limits;
";

                         using MySqlDataAdapter adap = new(_cmd);
                         var                    ds   = new DataSet();
                         adap.Fill(ds);
                         var coll = ds.Tables[0].Rows.Cast<DataRow>().ToArray()[0];
                         if (coll.ItemArray.All(x => double.TryParse(x.ToString(), out _)))
                         {
                             avgcache[i] = new mean_stdev_stats(double.Parse(coll.ItemArray[0].ToString())
                                                              , double.Parse(coll.ItemArray[1].ToString())
                                                              , double.Parse(coll.ItemArray[2].ToString())
                                                              , double.Parse(coll.ItemArray[3].ToString())
                                                              , double.Parse(coll.ItemArray[4].ToString())
                                                              , double.Parse(coll.ItemArray[5].ToString())
                                                              , double.Parse(coll.ItemArray[6].ToString()));
                         }

                     });

        avg = avgcache;
        return true;

    }
    public bool get_iqr_analytics(out iqr_stats[] avg)
    {
        var                   avgCache    = new iqr_stats[12];


        Parallel.For(0, avgCache.Length, i =>
        {
            using MySqlConnection _connection = new(CommonUse.CONST_DB_CONNECTSERVER);
            _connection.Open();
            using var _cmd = _connection.CreateCommand();
                                             _cmd.CommandText = $@"
USE {CommonUse.CONST_DB_NAME};
WITH Percentiles AS (
    SELECT 
        LC{i + 1},
        NTILE(4) OVER (ORDER BY LC{i + 1}) AS Quartile
    FROM loadcellmeasurement
),
Q1 AS (
    SELECT MAX(LC{i + 1}) AS Q1
    FROM Percentiles
    WHERE Quartile = 1
),
Q2 AS (
    SELECT MAX(LC{i + 1}) AS Q2
    FROM Percentiles
    WHERE Quartile = 2
),
Q3 AS (
    SELECT MAX(LC{i + 1}) AS Q3
    FROM Percentiles
    WHERE Quartile = 3
)
SELECT
    Q1.Q1,
    Q2.Q2 AS MEDIAN,
    Q3.Q3,
    Q3.Q3 - Q1.Q1 AS IQR,
    Q1.Q1-(1.5*(Q3.Q3 - Q1.Q1)) AS MIN,
    Q3.Q3+(1.5*(Q3.Q3 - Q1.Q1)) AS MAX
FROM Q1, Q2, Q3;
";

                                             using MySqlDataAdapter adap = new(_cmd);
                                             var                    ds   = new DataSet();
                                             adap.Fill(ds);
                                             var coll = ds.Tables[0].Rows.Cast<DataRow>().ToArray()[0];

                                             if (coll.ItemArray.All(x => double.TryParse(x.ToString(), out _)))
                                             {
                                                 avgCache[i] = new iqr_stats(double.Parse(coll.ItemArray[0].ToString())
                                                                           , double.Parse(coll.ItemArray[2].ToString())
                                                                           , double.Parse(coll.ItemArray[4].ToString())
                                                                           , double.Parse(coll.ItemArray[5].ToString())
                                                                           , double.Parse(coll.ItemArray[3].ToString())
                                                                           , double.Parse(coll.ItemArray[1]
                                                                                 .ToString()));
                                             }
        });

        avg = avgCache;
        return true;

    }
    public bool get_yield_analytics(double USL, double LSL, DateTime st, DateTime et, out double[] avg)
    {
        var avgCache = new double[12];


        Parallel.For(0, avgCache.Length, i =>
        {
            using MySqlConnection _connection = new(CommonUse.CONST_DB_CONNECTSERVER);
            _connection.Open();
            using var _cmd = _connection.CreateCommand();
            _cmd.CommandText = $@"
WITH Stats AS (
	SELECT *
    FROM loadcellmeasurement 
    WHERE dateTime BETWEEN '{st:yyyy-MM-dd HH:mm:ss}' AND '{et:yyyy-MM-dd HH:mm:ss}'
    )
SELECT COUNT(*)/(SELECT COUNT(*) FROM Stats)
FROM Stats
WHERE LC1 BETWEEN {LSL} AND {USL}
";

            var yield = _cmd.ExecuteScalar();
            avgCache[i] = double.TryParse(yield.ToString(), out var yd) ? yd : -1.0;
        });

        avg = avgCache;
        return true;

    }

    public void delete_till(DateTime endtime)
    {
        using (var _connection = new MySqlConnection(CommonUse.CONST_DB_CONNECTSERVER))
        {
            _connection.Open();
            string query =
                $"DELETE FROM loadcellMeasurement WHERE dateTime <= '{endtime:yyyy-MM-dd HH:mm:ss}'";

            using (var cmd = new MySqlCommand(query, _connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
    public bool delete_from_to(DateTime startTime, DateTime endtime)
    {
        try
        {
            using (var _connection = new MySqlConnection(CommonUse.CONST_DB_CONNECTSERVER))
            {
                _connection.Open();

                string query =
                    $"DELETE FROM loadcellMeasurement WHERE dateTime BETWEEN '{startTime:yyyy-MM-dd H:mm:ss}' AND '{endtime:yyyy-MM-dd HH:mm:ss}'";

                using (var cmd = new MySqlCommand(query, _connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public record struct mean_stdev_stats
    {
        public double mean;
        public double stddev;
        public double min;
        public double max;
        public double cpu;
        public double cpl;
        public double cpk;

        public mean_stdev_stats(double mean, double stddev, double min, double max, double cpu, double cpl, double cpk)
        {
             this.mean   = mean;
             this.stddev = stddev;
             this.min    = min;
             this.max    = max;
             this.cpu    = cpu;
             this.cpl    = cpl;
             this.cpk    = cpk;
        }
    }

    public record struct iqr_stats
    {
        public double q1;
        public double q3;
        public double min;
        public double max;
        public double iqr;
        public double median;

        public iqr_stats(double q1, double q3, double min, double max, double iqr, double median)
        {
            this.q1  = q1;
            this.q3  = q3;
            this.min = min;
            this.max = max;
            this.iqr = iqr;
            this.median = median;
        }
    }
}
