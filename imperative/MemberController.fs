module MemberController

open System
open System.Windows.Forms
open System.Drawing
open MySql.Data.MySqlClient
open Connection

// Update User Function
let updateUser (conn: MySqlConnection)(nameTextBox: TextBox) (emailTextBox: TextBox) (passwordTextBox: TextBox) (statusLabel: Label) =
    match UserId with
    | Some id ->
        try

            let newUserName = nameTextBox.Text
            let newEmail = emailTextBox.Text
            let newpassword = Connection.getValidPassword passwordTextBox statusLabel

            // Build the query dynamically based on provided input
            let query = 
                "UPDATE users SET " +
                (if newUserName <> "" then "name = @name, " else "") +
                (if newEmail <> "" then "email = @newEmail, " else "") +
                (if newpassword <> "" then "password = @password " else "") +
                "WHERE id = @id"

            // Trim any trailing commas from the query
            let sanitizedQuery = query.TrimEnd([| ','; ' ' |])

            use cmd = new MySqlCommand(sanitizedQuery, conn)

            // Add parameters for updated fields only if values are provided
            if newUserName <> "" then cmd.Parameters.AddWithValue("@name", newUserName) |> ignore
            if newEmail <> "" then cmd.Parameters.AddWithValue("@newEmail", newEmail) |> ignore
            if newpassword <> "" then cmd.Parameters.AddWithValue("@password", newpassword) |> ignore

            // Add the parameter for the current email (used for identifying the user)
            cmd.Parameters.AddWithValue("@id", id) |> ignore

            let rows = cmd.ExecuteNonQuery()
            if rows > 0 then
                statusLabel.Text <- sprintf "%d user(s) updated successfully." rows
                statusLabel.ForeColor <- Color.Green
            else
                statusLabel.Text <- "No user found with the given email."
                statusLabel.ForeColor <- Color.Red
        with
        | ex -> statusLabel.Text <- sprintf "Error while updating the user: %s" ex.Message
                statusLabel.ForeColor <- Color.Red
    | None ->
         statusLabel.Text <- "No user is currently logged in. Please log in first."
         statusLabel.ForeColor <- Color.Red
         

// Show User Details Function
let showUserDetails (conn: MySqlConnection) (nameTextBox: TextBox) (emailTextBox: TextBox) (passwordTextBox: TextBox) (statusLabel: Label) =
    match Connection.UserId with
    | Some id ->
        try
            
            let query =
                    
                    "SELECT  name, email, password FROM users WHERE id = @id"

            use cmd = new MySqlCommand(query, conn)
            cmd.Parameters.AddWithValue("@id", id) |> ignore

            use reader = cmd.ExecuteReader()

            if reader.HasRows then
                reader.Read() |> ignore
                nameTextBox.Text <- reader.GetString(0) // Name
                emailTextBox.Text <- reader.GetString(1) // Email
                passwordTextBox.Text <- reader.GetString(2) // Password

            else
                statusLabel.Text <- "No user found with the given  email."
                statusLabel.ForeColor <- Color.Red
        with
        | ex -> statusLabel.Text <- sprintf "Error fetching user details: %s" ex.Message
                statusLabel.ForeColor <- Color.Red
    | None ->
         statusLabel.Text <- "No user is currently logged in. Please log in first."
         statusLabel.ForeColor <- Color.Red
