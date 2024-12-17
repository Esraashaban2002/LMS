open System
open MySql.Data.MySqlClient // Import the MySQL library
// Define connection string (update credentials as needed)
let connectionString = "Server=127.0.0.1;Database=libarary_system;User ID=root;Password=0000;"

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

let ensureBookExist conn =
    let query = "CREATE TABLE IF NOT EXISTS Book (Book_ID INT AUTO_INCREMENT PRIMARY KEY, Book_Name VARCHAR(255) NOT NULL, Book_Type VARCHAR(255), Author_Name VARCHAR(255), Number_of_copies INT NOT NULL)"
    executeQuery query [] conn |> ignore   
let bookExists (conn: MySqlConnection) (bookId: int) =
    try
        use cmd = new MySqlCommand("SELECT COUNT(*) FROM Book WHERE Book_ID = @bookId", conn)
        cmd.Parameters.AddWithValue("@bookId", bookId) |> ignore
        let count = cmd.ExecuteScalar() :?> int64
        count > 0L
    with
    | ex ->
        printfn "Error checking if book exists: %s" ex.Message
        false
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

let showUserDetails conn =
    printf "Enter your Email: "
    let email = Console.ReadLine()

    let query = "SELECT  name, email, password FROM users WHERE email = @input"
    let parameters = [ ("@input", email) ]

    use reader = executeReader conn query parameters
    if reader.Read() then
        let name = reader.GetString(0)
        let email = reader.GetString(1)
        let password = reader.GetString(2)
        printfn "\nYour Details:"
        printfn "Name: %s" name
        printfn "Email: %s" email
        printfn "Password: %s" password
    else
        printfn "No user found with the given email."
// Update user details
let rec reverse lst A =
    match lst with
    | [] -> A // Base case: return the accumulated result when the list is empty
    | head :: tail -> reverse tail (head :: A)  // Prepend head to accumulator


let buildUpdateQueryAndParams (userId: int) newUserName newEmail newpassword =
    // Recursive function to filter out `None` and construct the query and parameters
    let rec processUpdates updates (accQuery: String) accParams =
        match updates with
        | [] -> accQuery.TrimEnd([| ','; ' ' |]), reverse accParams [] // Final result
        | (Some(field, paramName, value)) :: rest ->
            let newQuery = accQuery + sprintf "%s = %s, " field paramName
            processUpdates rest newQuery ((paramName, value) :: accParams)
        | None :: rest ->
            processUpdates rest accQuery accParams

    // Build the list of potential updates
    let updates =
        [ if newUserName <> "" then Some("name", "@name", newUserName) else None
          if newEmail <> "" then Some("email", "@newEmail", newEmail) else None
          if newpassword <> "" then Some("password", "@password", newpassword) else None ]

    // Call the recursive function to process updates
    let query, (parameters) = processUpdates updates "UPDATE users SET  " []
    let finalQuery: string = query + " WHERE id = @userId"
    
    // Ensure parameters contain @currentEmail at the end
    let finalParams  = parameters @ [("@userId", userId.ToString())]

    finalQuery, finalParams
   

let updateUser conn (userId: int) =
    try
        
        printf "Enter New User Name (leave empty to keep unchanged): "
        let newUserName = Console.ReadLine()
        printf "Enter New Email (leave empty to keep unchanged): "
        let newEmail = Console.ReadLine()
        printf "Enter New Password (leave empty to keep unchanged): "
        let newpassword = Console.ReadLine()

        let query, parameters = buildUpdateQueryAndParams userId newUserName newEmail newpassword
        let rows = executeQuery query parameters conn

        if rows > 0 then
            printfn "%d user(s) updated successfully." rows
        else
            printfn "No user found with the given email."
    with
    | ex -> printfn "Error while updating the user: %s" ex.Message
// Display avaliable books
let borrowBook conn userId =
    printf "Enter the Book Name you want to borrow: "
    let bookName = Console.ReadLine()

        // Check if the book exists and fetch its ID and number of copies
    use checkBookCmd = new MySqlCommand("SELECT Book_ID, Number_of_copies FROM Book WHERE Book_Name = @BookName", conn)
    checkBookCmd.Parameters.AddWithValue("@BookName", bookName) |> ignore
    use reader = checkBookCmd.ExecuteReader()

    if reader.Read() then
        let bookId = reader.["Book_ID"] :?> int
        let copies = reader.["Number_of_copies"] :?> int
        reader.Close()

        if copies > 0 then
            // Insert into Borrowing table
            use borrowCmd = new MySqlCommand("INSERT INTO Borrowings (User_ID, Book_ID, Borrow_Date) VALUES (@UserId, @BookId, CURDATE())", conn)
            borrowCmd.Parameters.AddWithValue("@UserId", userId) |> ignore
            borrowCmd.Parameters.AddWithValue("@BookId", bookId) |> ignore
            borrowCmd.ExecuteNonQuery() |> ignore

            // Update the number of copies
            use updateCopiesCmd = new MySqlCommand("UPDATE Book SET Number_of_copies = Number_of_copies - 1 WHERE Book_ID = @BookId", conn)
            updateCopiesCmd.Parameters.AddWithValue("@BookId", bookId) |> ignore
            updateCopiesCmd.ExecuteNonQuery() |> ignore

            Ok "Book borrowed successfully!" // Return a Result
        else
            Error "No copies of this book are available." // Return an Error
    else
        Error "Book not found." // Return an Error
    
let returnBook conn userId=
    printf "Enter the Book Name you want to return: "
    let bookName = Console.ReadLine()

    // Get Book_ID based on the book name
    use getBookCmd = new MySqlCommand("SELECT Book_ID FROM Book WHERE Book_Name = @BookName", conn)
    getBookCmd.Parameters.AddWithValue("@BookName", bookName) |> ignore
    let bookId = getBookCmd.ExecuteScalar()

    if bookId <> null then
        // Update the borrowing record to set the returned date
        use returnCmd = new MySqlCommand(
            "UPDATE Borrowings SET Return_Date = CURDATE() WHERE User_ID = @UserId AND Book_ID = @BookId AND Return_Date IS NULL", 
            conn)
        returnCmd.Parameters.AddWithValue("@UserId", userId) |> ignore
        returnCmd.Parameters.AddWithValue("@BookId", bookId) |> ignore
        let rows = returnCmd.ExecuteNonQuery()

        if rows > 0 then
            // Increment the number of copies
            use updateCopiesCmd = new MySqlCommand(
                "UPDATE Book SET Number_of_copies = Number_of_copies + 1 WHERE Book_ID = @BookId", 
                conn)
            updateCopiesCmd.Parameters.AddWithValue("@BookId", bookId) |> ignore
            updateCopiesCmd.ExecuteNonQuery() |> ignore

            Ok "Book returned successfully!" // Return success message
        else
            Error "No record found for this borrowing." // No borrowing record found
    else
        Error "Book not found." // Book not found
   
let rec readHistory  (reader: MySqlDataReader) acc =
            if reader.Read() then
                // Extract fields manually
                let bookName = reader.GetString(1)
                let borrowDate = reader.GetDateTime(2).ToString("yyyy-MM-dd")
                let returnDate = 
                    if reader.IsDBNull(3) then "Not Returned" 
                    else reader.GetDateTime(3).ToString("yyyy-MM-dd")

                // Add formatted borrowing record to the accumulator
                let record = sprintf "Book Name: %s\nBorrowed Date: %s\nReturned Date: %s\n--------------------" bookName borrowDate returnDate
                readHistory reader (record :: acc)
            else
                acc    
let borrowingHistory conn userId=

        // Fetch borrowing history for the user
        use cmd = new MySqlCommand(
            "SELECT b.Borrow_ID, bo.Book_Name AS Book_Name, b.Borrow_Date, b.Return_Date 
             FROM Borrowings b 
             JOIN Book bo ON b.Book_ID = bo.Book_ID 
             WHERE b.User_ID = @UserId", 
            conn)
        cmd.Parameters.AddWithValue("@UserId", userId) |> ignore
        use reader = cmd.ExecuteReader()

        // Check if there are rows and process them
        if reader.HasRows then
            let history = readHistory reader []  // Reverse the list to maintain order
            Ok history
        else
            Error "No borrowing history found for this user." // No history found

// Function to print results
let printResult result =
    match result with
    | Ok message -> printfn "%s" message
    | Error message -> printfn "Error: %s" message

// Function to print borrowing history
let printHistory history =
    match history with
    | Ok historyList ->
        printfn "Borrowing History:\n%s" (String.concat "\n" historyList)
    | Error msg -> printfn "Error: %s" msg
// Menu for user management
let userManagement (userId) =
    use conn = new MySqlConnection(connectionString)
    conn.Open()

    let rec menu () =
        printfn "1) Show Details "
        printfn "2) Update Details"
        printfn "3) Borrow Book"
        printfn "4) Return Book"
        printfn "5) History Of Borrow Books"
        printfn "6) Exit"
        printf "Choose an option: "
        match Console.ReadLine() with
        | "1" ->
            showUserDetails conn
            menu()
        | "2" ->
            updateUser conn userId
            menu()
        | "3" ->  printResult (borrowBook conn userId); menu()
        | "4" -> printResult (returnBook conn userId); menu()
        | "5" -> printHistory (borrowingHistory conn userId); menu()
        | "6" -> printfn "Exiting. Thank you!"
        | _ -> printfn "Invalid choice."; menu()

    menu()
let getValidCopies () =
    let rec loop () =
        match Int32.TryParse(Console.ReadLine()) with
        | true, n when n > 0 -> n
        | _ ->
            printfn "Invalid input. Please enter a valid positive number."
            loop()
    loop()
let addBook conn =
   try
        ensureBookExist conn
        printf "Enter Book Name: "
        let bookName = Console.ReadLine()
        printf "Enter Genre: "
        let genre = Console.ReadLine()
        printf "Enter Author Name: "
        let authorName = Console.ReadLine()
        printf "Enter Number of Copies: "
        let numberOfCopies = getValidCopies()

        let query = "INSERT INTO Book (Book_Name, Book_Type, Author_Name, Number_of_copies) VALUES (@BookName, @Genre, @AuthorName, @Copies)"
        executeQuery  query ["@BookName", box bookName; "@Genre", box genre; "@AuthorName", box authorName; "@Copies", box numberOfCopies] conn |> ignore
        printfn "Book added successfully!"
    
    with
    | ex ->
        printfn "Error adding book: %s" ex.Message
// Delete a book
let deleteBook conn =
    printf "Enter BookID to delete: "
    match Int32.TryParse(Console.ReadLine()) with
    | true, bookId ->
        let query = "DELETE FROM Book WHERE Book_ID = @BookId"
        executeQuery  query ["@BookId", box bookId] conn |> ignore
        printfn "Book deleted successfully!"
    | _ -> printfn "Invalid input."

// Display avaliable books

// Update an existing book
let updateBook conn =
    try
        let rec tryAgain () =
            printf "Enter the ID of the book you want to update: "
            let bookId = Int32.Parse(Console.ReadLine())

            if bookExists conn bookId then
                // Fetch current values for the book
                use fetchCmd = new MySqlCommand("SELECT Book_Name, Book_Type, Author_Name, Number_of_copies FROM Book WHERE Book_ID = @bookId", conn)
                fetchCmd.Parameters.AddWithValue("@bookId", bookId) |> ignore
                use reader = fetchCmd.ExecuteReader()

                if reader.Read() then
                    let currentBookName = reader.GetString(0)
                    let currentGener = reader.GetString(1)
                    let currentAuthorName = reader.GetString(2)
                    let currentNumberOfCopies = reader.GetInt32(3)
                    reader.Close()

                    // Prompt for new values or keep the current ones
                    printf "Enter New Book Name (leave empty to keep '%s'): " currentBookName
                    let bookName = 
                        let input = Console.ReadLine()
                        if input.Trim() = "" then currentBookName else input

                    printf "Enter New Genre of Book (leave empty to keep '%s'): " currentGener
                    let gener = 
                        let input = Console.ReadLine()
                        if input.Trim() = "" then currentGener else input

                    printf "Enter New Author Name (leave empty to keep '%s'): " currentAuthorName
                    let authorName = 
                        let input = Console.ReadLine()
                        if input.Trim() = "" then currentAuthorName else input

                    printf "Enter New Number of Copies (leave empty to keep '%d'): " currentNumberOfCopies
                    let numberOfCopies =
                        let input = Console.ReadLine()
                        if input.Trim() = "" then currentNumberOfCopies else Int32.Parse(input)

                    // Update the book with new or unchanged values
                    use cmd = new MySqlCommand("UPDATE Book SET Book_Name = @bookName, Book_Type = @gener, Author_Name = @authorName, Number_of_copies = @numberOfCopies WHERE Book_ID = @bookId", conn)
                    cmd.Parameters.AddWithValue("@bookId", bookId) |> ignore
                    cmd.Parameters.AddWithValue("@bookName", bookName) |> ignore
                    cmd.Parameters.AddWithValue("@gener", gener) |> ignore
                    cmd.Parameters.AddWithValue("@authorName", authorName) |> ignore
                    cmd.Parameters.AddWithValue("@numberOfCopies", numberOfCopies) |> ignore
                    cmd.ExecuteNonQuery() |> ignore

                    printfn "Book updated successfully!"
                else
                    printfn "Book not found in the database."

            else
                printfn "Book not found in the database."
                tryAgain ()

        tryAgain ()
    with
    | ex ->
        printfn "Error updating book: %s" ex.Message
// Check if a book exists
// Function to process and print each book's details
let printBookDetails (reader: MySqlDataReader) =
    printfn "BookID: %d" (reader.GetInt32(0))
    printfn "Book Name: %s" (reader.GetString(1))
    printfn "Genre: %s" (reader.GetString(2))
    printfn "Author Name: %s" (reader.GetString(3))
    printfn "Number Of Copies: %d" (reader.GetInt32(4))
    printfn "--------------------"

// Recursive function to read and print books from the reader
let rec processReader (reader: MySqlDataReader)  =
    if reader.Read() then
        printBookDetails reader
        processReader reader  // Recursive call to read the next row
    else
        ()  // End recursion when no more rows

// Function to search by title
let searchByTitle conn =
    printf "Enter Book Title: "
    let title = Console.ReadLine()
    let query = "SELECT * FROM Book WHERE book_name LIKE @value"
    let parameterName = "@value"
    let parameterValue = "%" + title + "%"
    
    use cmd = new MySqlCommand(query, conn)
    cmd.Parameters.AddWithValue(parameterName, parameterValue) |> ignore
    
    use reader = cmd.ExecuteReader()
    if reader.HasRows then
        printfn "\nBooks found by Title:"
        processReader reader  // Call the recursive function to process rows
    else
        printfn "No books found matching the Title."

// Function to search by author
let searchByAuthor conn =
    printf "Enter Author Name: "
    let author = Console.ReadLine()
    let query = "SELECT * FROM Book WHERE author_name LIKE @value"
    let parameterName = "@value"
    let parameterValue = "%" + author + "%"
    
    use cmd = new MySqlCommand(query, conn)
    cmd.Parameters.AddWithValue(parameterName, parameterValue) |> ignore
    
    use reader = cmd.ExecuteReader()
    if reader.HasRows then
        printfn "\nBooks found by Author:"
        processReader reader  // Call the recursive function to process rows
    else
        printfn "No books found matching the Author."

// Function to search by genre
let searchByGenre conn =
    printf "Enter Genre: "
    let genre = Console.ReadLine()
    let query = "SELECT * FROM Book WHERE book_type LIKE @value"
    let parameterName = "@value"
    let parameterValue = "%" + genre + "%"
    
    use cmd = new MySqlCommand(query, conn)
    cmd.Parameters.AddWithValue(parameterName, parameterValue) |> ignore
    
    use reader = cmd.ExecuteReader()
    if reader.HasRows then
        printfn "\nBooks found by Genre:"
        processReader reader  // Call the recursive function to process rows
    else
        printfn "No books found matching the Genre."

// Main function that calls individual search functions based on user choice
let searchBook conn =
    try
        printfn "Search for a book by:"
        printfn "1. Title"
        printfn "2. Author"
        printfn "3. Genre"
        printf "Please enter your choice: "
        let choice = Console.ReadLine()

        match choice with
        | "1" -> searchByTitle conn
        | "2" -> searchByAuthor conn
        | "3" -> searchByGenre conn
        | _ -> printfn "Invalid choice."
    with
    | ex -> printfn "Error searching for book: %s" ex.Message
let listAvailableBooks conn =
    try
        use cmd = new MySqlCommand("SELECT * FROM Book WHERE Number_of_copies > 0", conn)
        use reader = cmd.ExecuteReader()
        if reader.HasRows then
            printfn "\nAvailable Books:\n"
            processReader reader
        else
            printfn "No available books found."
    with
    | ex -> printfn "Error displaying available books: %s" ex.Message
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
            | "1" -> addBook conn; mainMenu ()
            | "2" -> listAvailableBooks conn; mainMenu ()
            | "3" -> updateBook conn; mainMenu ()
            | "4" -> deleteBook conn; mainMenu ()
            | "5" -> searchBook conn; mainMenu ()
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
            match login conn "Admin"  with
            | Some userId ->
                bookMangement ()
            | None ->
                startProgram ()
        | "2" ->
            registerUser conn "Admin"|> ignore
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
           match login conn "User"  with
            | Some userId ->
                userManagement (userId)
            | None ->
                startProgram ()
        | "2" ->
            let userId= registerUser conn "User"
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
