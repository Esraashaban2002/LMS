module Program 

open System
open MySql.Data.MySqlClient // Import the MySQL library
// Define connection string (update credentials as needed)
open Connction
open User 
open Admin 
let connectionString = Connction.connectionString

// Menu for user management
let userManagement (userId) =
    use conn = new MySqlConnection(connectionString)
    conn.Open()

    let rec menu () =
        printfn "1) Show Details "
        printfn "2) Update Details"
        printfn "3) Search Book"
        printfn "4) Borrow Book"
        printfn "5) Return Book"
        printfn "6) History Of Borrow Books"
        printfn "7) Exit"
        printf "Choose an option: "
        match Console.ReadLine() with
        | "1" ->
            User.showUserDetails  conn userId
            menu()
        | "2" ->
            User.updateUser conn userId
            menu()
        | "3" -> Admin.searchBook conn ; menu ()
        | "4" -> printResult (User.borrowBook conn userId); menu()
        | "5" -> printResult (User.returnBook conn userId); menu()
        | "6" -> printHistory (User.borrowingHistory conn userId); menu()
        | "7" -> printfn "Exiting. Thank you!"
        | _ -> printfn "Invalid choice."; menu()

    menu()

let bookMangement () =
    try
        use conn = new MySqlConnection(connectionString)
        conn.Open()
        let rec mainMenu () =
            printfn "\nChoose an option:"
            printfn "1. Add New Book"
            printfn "2. List Available Books"
            printfn "3. Update Book"
            printfn "4. Delete Book"
            printfn "5. Search Book"
            printfn "6. Exit"

            match Console.ReadLine() with
            | "1" -> Admin.addBook conn; mainMenu ()
            | "2" -> Admin.listAvailableBooks conn; mainMenu ()
            | "3" -> Admin.updateBook conn; mainMenu ()
            | "4" -> Admin.deleteBook conn; mainMenu ()
            | "5" -> Admin.searchBook conn; mainMenu ()
            | "6" -> printfn "Exiting..."; ()
            | _ -> printfn "Invalid choice."; mainMenu ()

        mainMenu ()
    with
    | ex -> printfn "Error: %s" ex.Message

let rec startProgram () =
    use conn = new MySqlConnection(connectionString)
    conn.Open()
    printfn "Welcome to the Library System!"
    printfn "1\\  Admin"
    printfn "2\\  User"
    printfn "3\\ Exit"
    printf "Please enter your choice: "
    match Console.ReadLine() with
    | "1" ->
        printfn "1\\ Login"
        printfn "2\\ Register"
        printf "Please enter your choice: "
        match Console.ReadLine() with
        | "1" ->
            // Assuming 'conn' is the MySQL connection object you have
            match Connction.login conn "Admin"  with
            | Some userId ->
                bookMangement ()
            | None ->
                startProgram ()
        | "2" ->
            Connction.registerUser conn "Admin"|> ignore
            bookMangement ()
        | _ -> 
            printfn "Invalid choice. Please try again."
            startProgram ()
    | "2" ->
        printfn "1\\ Login"
        printfn "2\\ Register"
        printf "Please enter your choice: "
        match Console.ReadLine() with
        | "1" ->
            // Assuming 'conn' is the MySQL connection object you have
           match Connction.login conn "User"  with
            | Some userId ->
                userManagement (userId)
            | None ->
                startProgram ()
        | "2" ->
            let userId= Connction.registerUser conn "User"
            userManagement (userId)
        | _ -> 
            printfn "Invalid choice. Please try again."
            startProgram ()
    | "3" ->
        printfn "Thank you for using the Library System. Goodbye!"
    | _ ->
        printfn "Invalid choice. Please try again."
        startProgram ()

// Start the program
startProgram ()
