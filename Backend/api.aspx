<%@ Page Language="C#" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="System.Collections.Generic" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "application/json";
        Response.AppendHeader("Access-Control-Allow-Origin", "*");
        Response.AppendHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        Response.AppendHeader("Access-Control-Allow-Headers", "Content-Type");
        
        if (Request.HttpMethod == "OPTIONS")
        {
            Response.End();
            return;
        }
        
        string action = Request.QueryString["action"] ?? Request.Form["action"] ?? "";
        
        switch (action.ToLower())
        {
            case "ping":
                Ping();
                break;
            case "login":
                Login();
                break;
            case "save":
                SaveData();
                break;
            case "load":
                LoadData();
                break;
            case "execute":
                ExecuteQuery();
                break;
            default:
                Response.Write("{\"success\": false, \"error\": \"Invalid action\"}");
                break;
        }
    }
    
    void Ping()
    {
        try
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                Response.Write("{\"success\": true, \"message\": \"Connected to database\", \"timestamp\": \"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"}");
            }
        }
        catch (Exception ex)
        {
            Response.Write("{\"success\": false, \"error\": \"" + ex.Message.Replace("\"", "'") + "\"}");
        }
    }
    
    void Login()
    {
        try
        {
            string username = Request.Form["username"];
            string password = Request.Form["password"];
            
            // التحقق البسيط
            var users = new Dictionary<string, string>
            {
                {"admin", "123"},
                {"doctor", "doc123"},
                {"reception", "rec123"},
                {"lab", "lab123"},
                {"pharmacy", "ph123"},
                {"nursing", "nur123"}
            };
            
            if (users.ContainsKey(username) && users[username] == password)
            {
                Response.Write("{\"success\": true, \"name\": \"" + username + "\", \"role\": \"" + username + "\", \"token\": \"token_" + Guid.NewGuid().ToString() + "\"}");
            }
            else
            {
                Response.Write("{\"success\": false, \"error\": \"Invalid credentials\"}");
            }
        }
        catch (Exception ex)
        {
            Response.Write("{\"success\": false, \"error\": \"" + ex.Message.Replace("\"", "'") + "\"}");
        }
    }
    
    void SaveData()
    {
        try
        {
            string type = Request.Form["type"];
            string data = Request.Form["data"];
            
            // هنا يمكنك حفظ البيانات في قاعدة البيانات
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO SystemLogs (LogType, LogData, CreatedAt) VALUES (@type, @data, GETDATE())", conn);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@data", data);
                cmd.ExecuteNonQuery();
            }
            
            Response.Write("{\"success\": true, \"message\": \"Data saved successfully\"}");
        }
        catch (Exception ex)
        {
            Response.Write("{\"success\": false, \"error\": \"" + ex.Message.Replace("\"", "'") + "\"}");
        }
    }
    
    void LoadData()
    {
        try
        {
            string type = Request.QueryString["type"];
            
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT LogData FROM SystemLogs WHERE LogType = @type ORDER BY CreatedAt DESC", conn);
                cmd.Parameters.AddWithValue("@type", type);
                
                using (var reader = cmd.ExecuteReader())
                {
                    var dataList = new List<string>();
                    while (reader.Read())
                    {
                        dataList.Add(reader["LogData"].ToString());
                    }
                    
                    var serializer = new JavaScriptSerializer();
                    Response.Write("{\"success\": true, \"data\": " + serializer.Serialize(dataList) + "}");
                }
            }
        }
        catch (Exception ex)
        {
            Response.Write("{\"success\": false, \"error\": \"" + ex.Message.Replace("\"", "'") + "\"}");
        }
    }
    
    void ExecuteQuery()
    {
        try
        {
            string query = Request.Form["query"];
            
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
                
                Response.Write("{\"success\": true, \"message\": \"Query executed successfully\"}");
            }
        }
        catch (Exception ex)
        {
            Response.Write("{\"success\": false, \"error\": \"" + ex.Message.Replace("\"", "'") + "\"}");
        }
    }
    
    SqlConnection GetConnection()
    {
        string connStr = "Server=AlshifaDb.mssql.somee.com;Database=AlshifaDb;User Id=Musab_SQLLogin_1;Password=*******;TrustServerCertificate=True;";
        return new SqlConnection(connStr);
    }
</script>
