using MySqlConnector;
using ESP_Players.Config;

namespace ESP_Players;
public class MySqlDataManager
{
    private static string ConnectionString => new MySqlConnectionStringBuilder
    {
        Server = Configs.GetConfigData().MySql_Host,
        Port = Configs.GetConfigData().MySql_Port,
        Database = Configs.GetConfigData().MySql_Database,
        UserID = Configs.GetConfigData().MySql_Username,
        Password = Configs.GetConfigData().MySql_Password,
        Pooling = true,
        MinimumPoolSize = 0,
        MaximumPoolSize = 100
    }.ConnectionString;

    public static async Task CreateTableIfNotExistsAsync()
    {
        try
        {
            await using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();

            bool tableExists;
            await using (var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'ESP_Toggle_Data'", connection))
            {
                tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;
            }

            const string query = @"
                CREATE TABLE IF NOT EXISTS ESP_Toggle_Data (
                    PlayerSteamID BIGINT UNSIGNED PRIMARY KEY,
                    Toggle_ESP INT NOT NULL DEFAULT 0,
                    DateAndTime DATETIME NOT NULL
                );";

            await using (var command = new MySqlCommand(query, connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            if (tableExists)
            {
                Helper.DebugMessage("Database table already exists - verified structure");
            }
            else
            {
                Helper.DebugMessage("Database table created successfully");
            }
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"DB Init Error: {ex.Message}");
        }
    }

    public static async Task SaveToMySqlAsync(Globals_Static.PersonData data)
    {
        const string insertOrUpdateQuery = @"
            INSERT INTO ESP_Toggle_Data 
                (PlayerSteamID, Toggle_ESP, DateAndTime)
            VALUES 
                (@PlayerSteamID, @Toggle_ESP, @DateAndTime)
            ON DUPLICATE KEY UPDATE 
                Toggle_ESP = VALUES(Toggle_ESP),
                DateAndTime = VALUES(DateAndTime)";
        
        try
        {
            using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            await using var command = new MySqlCommand(insertOrUpdateQuery, connection);
            AddPersonDataParameters(command, data);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"Saving Values In MySql Error: {ex.Message}");
        }
    }

    public static async Task<Globals_Static.PersonData> RetrievePersonDataByIdAsync(ulong steamId)
    {
        const string retrieveQuery = "SELECT * FROM ESP_Toggle_Data WHERE PlayerSteamID = @PlayerSteamID";
        try
        {
            await using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            
            await using var command = new MySqlCommand(retrieveQuery, connection);
            command.Parameters.Add("@PlayerSteamID", MySqlDbType.UInt64).Value = steamId;

            await using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return new Globals_Static.PersonData
                {
                    PlayerSteamID = reader.GetUInt64("PlayerSteamID"),
                    Toggle_ESP = reader.GetInt32("Toggle_ESP"),
                    DateAndTime = reader.GetDateTime("DateAndTime")
                };
            }
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"Retrieve Values In MySql Error: {ex.Message}");
        }
        return new Globals_Static.PersonData();
    }

    public static async Task DeleteOldPlayersAsync()
    {
        if (Configs.GetConfigData().MySql_AutoRemovePlayerOlderThanXDays < 1) return;
        
        try
        {
            int days = Configs.GetConfigData().MySql_AutoRemovePlayerOlderThanXDays;
            const string cleanupQuery = "DELETE FROM ESP_Toggle_Data WHERE DateAndTime < NOW() - INTERVAL @Days DAY";

            await using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();

            await using var cleanupCommand = new MySqlCommand(cleanupQuery, connection);
            cleanupCommand.Parameters.Add("@Days", MySqlDbType.Int32).Value = days;
            await cleanupCommand.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"Delete Old Players In MySql Error: {ex.Message}");
        }
    }

    private static void AddPersonDataParameters(MySqlCommand command, Globals_Static.PersonData data)
    {
        command.Parameters.Add("@PlayerSteamID", MySqlDbType.UInt64).Value = data.PlayerSteamID;
        command.Parameters.Add("@Toggle_ESP", MySqlDbType.Int32).Value = data.Toggle_ESP;
        command.Parameters.Add("@DateAndTime", MySqlDbType.DateTime).Value = data.DateAndTime;
    }
}