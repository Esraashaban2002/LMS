module Admin 

open System
open MySql.Data.MySqlClient // Import the MySQL library

open Connction
let connectionString = Connction.connectionString


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