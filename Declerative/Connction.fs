module Connction 

open System
open MySql.Data.MySqlClient // Import the MySQL library
// Define connection string (update credentials as needed)
let connectionString = "Server=127.0.0.1;Database=libarary_system;User ID=root;Password=1234;"

// Ensure the users table exists
let ensureUsersTable conn =
    let query =
        "CREATE TABLE IF NOT EXISTS users (id INT AUTO_INCREMENT PRIMARY KEY, name VARCHAR(255) NOT NULL, email VARCHAR(255) NOT NULL, password VARCHAR(255) NOT NULL, role ENUM('Admin', 'User') NOT NULL)"
    try
        let cmd = new MySqlCommand(query, conn)
        cmd.ExecuteNonQuery() |> ignore
    with ex ->
        failwithf "Error ensuring users table: %s" ex.Message

let executeQuery query parameters conn =
    let cmd = new MySqlCommand(query, conn)

    let rec addParameters parameter =
        match parameter with
        | [] -> () // If the list is empty, stop
        | (key, value) :: tail ->
            cmd.Parameters.AddWithValue(key, value) |> ignore
            addParameters tail // Recursive call for the rest of the list

    // Call the manual parameter-adding function
    addParameters parameters

    // Execute the query
    cmd.ExecuteNonQuery() 


let executeReader conn query parameters =
    let cmd = new MySqlCommand(query, conn)

    // Add parameters to the command using recursion
    let rec addParameters parameter =
        match parameter with
        | [] -> () // Stop if the list is empty
        | (key, value) :: tail ->
            cmd.Parameters.AddWithValue(key, value) |> ignore
            addParameters tail // Recursive call for the rest of the list

    addParameters parameters
    cmd.ExecuteReader()


// Get a valid password
let rec getValidPassword () =
    printf "Enter Password (at least 8 characters): "
    let password = Console.ReadLine()
    if password.Length < 8 then
        printfn "Password too short!"
        getValidPassword ()
    else
        password

// Register a user
let registerUser conn role =
    ensureUsersTable conn
    printf "Enter Name: "
    let name = Console.ReadLine()
    printf "Enter Email: "
    let email = Console.ReadLine()
    let password = getValidPassword ()

    let query = "INSERT INTO users (name, email, password, role) VALUES (@name, @Email, @Password, @Role)"
    let parameters = [
        ("@name", name)
        ("@Email", email)
        ("@Password", password)
        ("@Role", role)
    ]
    try
        executeQuery query parameters conn |> ignore 
        use getIdCmd = new MySqlCommand("SELECT LAST_INSERT_ID();", conn)
        let userId = getIdCmd.ExecuteScalar() :?> int
        printfn "Welcome, %s! You have registered as a %s." name role
        userId
    with ex ->
        failwithf "Error during registration: %s" ex.Message

// Login a user
let login conn expectedRole =
    ensureUsersTable conn
    printf "Enter Email: "
    let email = Console.ReadLine()
    printf "Enter Password: "
    let password = Console.ReadLine()

    let query = "SELECT id, name, role FROM users WHERE email = @Email AND password = @Password"
    let parameters = [
        ("@Email", email)
        ("@Password", password)
    ]
    use reader = executeReader conn query parameters
    if reader.Read() then        
        let userId =reader.GetInt32(0)
        let name = reader.GetString(1)
        let role = reader.GetString(2)
        if role = expectedRole then
            printfn "Welcome, %s!" name
                  
            Some userId
        else
            printfn "Unauthorized role: %s." role
            None
    else
        printfn "Invalid email or password."
        None
