module Connection 

open System
open System.Windows.Forms
open MySql.Data.MySqlClient // Import the MySQL library
open System.Drawing

// Define connection string (update credentials as needed)
let connectionString = "Server=127.0.0.1;Database=libarary_system;User ID=root;Password=1234;"

let mutable UserId: obj option = None 

// Helper function to create the users table if it doesn't exist
let ensureUsersTable (conn: MySqlConnection) =
    try
        use cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS users (id INT AUTO_INCREMENT PRIMARY KEY, name VARCHAR(255) NOT NULL, email VARCHAR(255) NOT NULL UNIQUE, password VARCHAR(8) NOT NULL, role ENUM('Admin', 'User') NOT NULL)", conn)
        cmd.ExecuteNonQuery() |> ignore
    with
    | ex -> printfn "Error creating users table: %s" ex.Message

let rec getValidPassword (passwordTextBox: TextBox) (statusLabel: Label)  =   
            let password = passwordTextBox.Text
            if password.Length < 8 then
                statusLabel.Text <- "Password too short! Please enter a password with at least 8 characters."
                statusLabel.ForeColor <- Color.Red
                getValidPassword passwordTextBox statusLabel // Recursive call to prompt again
            else
                password


let registerUser (expectedRole: string) (nameTextBox: TextBox) (emailTextBox: TextBox) (passwordTextBox: TextBox) (statusLabel: Label)  =
    let password = passwordTextBox.Text
    use conn = new MySqlConnection(connectionString)
    conn.Open()
    ensureUsersTable conn

    try
        // Validate the role
        if expectedRole = "Admin" || expectedRole = "User" then
            use cmd = new MySqlCommand("INSERT INTO users (name, email,password ,role) VALUES (@name, @email, @password, @role)", conn)
            cmd.Parameters.AddWithValue("@name", nameTextBox.Text) |> ignore
            cmd.Parameters.AddWithValue("@email", emailTextBox.Text) |> ignore
            cmd.Parameters.AddWithValue("@password", password) |> ignore
            cmd.Parameters.AddWithValue("@role", expectedRole) |> ignore

            let rows = cmd.ExecuteNonQuery()
            if rows > 0 then
                statusLabel.Text <- sprintf "Welcome, %s! You have successfully registered as a %s." nameTextBox.Text expectedRole
                statusLabel.ForeColor <- Color.Green
            else
                statusLabel.Text <- "Registration failed. Please try again."
                statusLabel.ForeColor <- Color.Red
        else 
            statusLabel.Text <- sprintf "Invalid role. You are not authorized as a %s." expectedRole
            statusLabel.ForeColor <- Color.Red
    with
    | ex -> 
        statusLabel.Text <- sprintf "Error while registering the user: %s" ex.Message
        statusLabel.ForeColor <- Color.Red
        

let login (expectedRole: string) (emailTextBox: TextBox) (passwordTextBox: TextBox) (statusLabel: Label) =
    try
        use conn = new MySqlConnection(connectionString)
        conn.Open()
        ensureUsersTable conn

        use cmd = new MySqlCommand("SELECT id ,name, role FROM users WHERE email = @Email AND password = @Password", conn)
        cmd.Parameters.AddWithValue("@Email", emailTextBox.Text) |> ignore
        cmd.Parameters.AddWithValue("@Password", passwordTextBox.Text) |> ignore

        use reader = cmd.ExecuteReader()
        if reader.Read() then
            let id =reader.GetInt32(0)
            let name = reader.GetString(1)
            let userRole = reader.GetString(2)
            if userRole = expectedRole then
                UserId <- Some id
                statusLabel.Text <- sprintf "Welcome, %s!" name
                statusLabel.ForeColor <- Color.Green
                true // Successful login
            else
                statusLabel.Text <- sprintf "Invalid role. You are not authorized as a %s." expectedRole
                statusLabel.ForeColor <- Color.Red
                false
        else
            statusLabel.Text <- "Invalid email or password."
            statusLabel.ForeColor <- Color.Red
            false
    with
    | ex -> 
        statusLabel.Text <- sprintf "Error during login: %s" ex.Message
        statusLabel.ForeColor <- Color.Red
        false