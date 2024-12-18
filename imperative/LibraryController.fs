module LibraryController

open System
open Microsoft.Data.Sqlite
open System.Windows.Forms
open System.Drawing
open MySql.Data.MySqlClient

let bookExists (conn: MySqlConnection) (bookIdTextBox: TextBox) (statusLabel: Label) =
    try
        use cmd = new MySqlCommand("SELECT COUNT(*) FROM Book WHERE Book_ID = @bookId", conn)
        cmd.Parameters.AddWithValue("@bookId", bookIdTextBox.Text) |> ignore
        let count = cmd.ExecuteScalar() :?> int64
        count > 0L
    with
    | ex ->
        statusLabel.Text <- sprintf "Error checking if book exists: %s" ex.Message
        statusLabel.ForeColor <- Color.Red
        false

let rec getValidCopies (copiesTextBox: TextBox) (statusLabel: Label) =
    try
        let numberOfCopies = Int32.Parse(copiesTextBox.Text)
        if numberOfCopies <= 0 then
            statusLabel.Text <- "Invalid input. Number of copies must be greater than 0."
            statusLabel.ForeColor <- Color.Red
            getValidCopies copiesTextBox statusLabel
        else
            numberOfCopies
    with
    | :? FormatException ->
        statusLabel.Text <- "Invalid input. Please enter a valid integer value for the Number of copies."
        statusLabel.ForeColor <- Color.Red
        getValidCopies copiesTextBox statusLabel

// Add a new book
let addBook (conn: MySqlConnection) (bookNameTextBox: TextBox) (bookGenerTextBox: TextBox) (authorNameTextBox: TextBox) (copiesTextBox: TextBox) (statusLabel: Label) =
    try
        // Ensure the table exists
        use cmd = new MySqlCommand("CREATE TABLE IF NOT EXISTS Book (Book_ID INT AUTO_INCREMENT PRIMARY KEY, Book_Name VARCHAR(255) NOT NULL, Book_Type VARCHAR(255), Author_Name VARCHAR(255), Number_of_Copies INT NOT NULL)", conn)
        cmd.ExecuteNonQuery() |> ignore
        statusLabel.Text <- "Table checked or created successfully."
        statusLabel.ForeColor <- Color.Green

        let bookName = bookNameTextBox.Text
        let gener = bookGenerTextBox.Text
        let authorName = authorNameTextBox.Text
        let numberOfCopies = getValidCopies copiesTextBox statusLabel

        // Check if the book already exists
        use checkCmd = new MySqlCommand("SELECT COUNT(*) FROM Book WHERE Book_Name = @bookName AND Author_Name = @authorName AND Number_of_Copies = @numberOfCopies", conn)
        checkCmd.Parameters.AddWithValue("@bookName", bookName) |> ignore
        checkCmd.Parameters.AddWithValue("@authorName", authorName) |> ignore
        checkCmd.Parameters.AddWithValue("@numberOfCopies", numberOfCopies) |> ignore
        let bookExists = (checkCmd.ExecuteScalar() :?> int64) > 0

        if bookExists then
            statusLabel.Text <- "The book already exists in the database."
            statusLabel.ForeColor <- Color.Red
            bookNameTextBox.Text <- ""
            bookGenerTextBox.Text <- ""
            authorNameTextBox.Text <- ""
            copiesTextBox.Text <- ""
        else
            // Insert the new book into the database
            use insertCmd = new MySqlCommand("INSERT INTO Book (Book_Name, Book_Type, Author_Name, Number_of_Copies) VALUES (@bookName, @gener, @authorName, @numberOfCopies)", conn)
            insertCmd.Parameters.AddWithValue("@bookName", bookName) |> ignore
            insertCmd.Parameters.AddWithValue("@gener", gener) |> ignore
            insertCmd.Parameters.AddWithValue("@authorName", authorName) |> ignore
            insertCmd.Parameters.AddWithValue("@numberOfCopies", numberOfCopies) |> ignore
            insertCmd.ExecuteNonQuery() |> ignore

            statusLabel.Text <- "Book added successfully!"
            statusLabel.ForeColor <- Color.Green
    with
    | ex ->
        statusLabel.Text <- sprintf "Error adding book: %s" ex.Message
        statusLabel.ForeColor <- Color.Red

// Update book
let updateBook (conn: MySqlConnection) (bookIdTextBox: TextBox) (bookNameTextBox: TextBox) (bookGenerTextBox: TextBox) (authorNameTextBox: TextBox) (copiesTextBox: TextBox) (statusLabel: Label) =
    try
        let bookId = Int32.Parse(bookIdTextBox.Text)

        if bookExists conn bookIdTextBox statusLabel then
            // Fetch current values for the book
            use fetchCmd = new MySqlCommand("SELECT Book_Name, Book_Type, Author_Name, Number_of_copies FROM Book WHERE Book_ID = @bookId", conn)
            fetchCmd.Parameters.AddWithValue("@bookId", bookId) |> ignore
            use reader = fetchCmd.ExecuteReader()

            if reader.Read() then
                let currentBookName = reader.GetString(0)
                let currentGener = reader.GetString(1)
                let currentAuthorName = reader.GetString(2)
                let currentNumberOfCopies = reader.GetInt32(3)

                // Display current values in the text boxes
                bookNameTextBox.Text <- currentBookName
                bookGenerTextBox.Text <- currentGener
                authorNameTextBox.Text <- currentAuthorName
                copiesTextBox.Text <- currentNumberOfCopies.ToString()
                
                reader.Close()

                 // Add the update button to the form
                let form = bookIdTextBox.FindForm()
                let updateButton = new Button(Text = "Update Books", AutoSize = true, Location = Point(190, 190) , BackColor = Color.Pink ,  ForeColor = Color.White , Font = new Font("sans", 12.0f) )
                updateButton.Click.Add(fun _ ->
                    let connectionString = Connection.connectionString
                    use conn = new MySqlConnection(connectionString)
                    conn.Open()
                    let bookName = if bookNameTextBox.Text.Trim() = "" then currentBookName else bookNameTextBox.Text
                    let gener = if bookGenerTextBox.Text.Trim() = "" then currentGener else bookGenerTextBox.Text
                    let authorName = if authorNameTextBox.Text.Trim() = "" then currentAuthorName else authorNameTextBox.Text
                    let numberOfCopies = if copiesTextBox.Text.Trim() = "" then currentNumberOfCopies else Int32.Parse(copiesTextBox.Text)

                    // Update the book with new or unchanged values
                    use cmd = new MySqlCommand("UPDATE Book SET Book_Name = @bookName, Book_Type = @gener, Author_Name = @authorName, Number_of_copies = @numberOfCopies WHERE Book_ID = @bookId", conn)
                    cmd.Parameters.AddWithValue("@bookId", bookId) |> ignore
                    cmd.Parameters.AddWithValue("@bookName", bookName) |> ignore
                    cmd.Parameters.AddWithValue("@gener", gener) |> ignore
                    cmd.Parameters.AddWithValue("@authorName", authorName) |> ignore
                    cmd.Parameters.AddWithValue("@numberOfCopies", numberOfCopies) |> ignore
                    cmd.ExecuteNonQuery() |> ignore

                    MessageBox.Show( "Book updated successfully!")
                    form.Close()
                )

                form.Controls.Add(updateButton)
            else
                statusLabel.Text <- "Book not found in the database."
                statusLabel.ForeColor <- Color.Red
        else
            statusLabel.Text <- "Book not found in the database."
            statusLabel.ForeColor <- Color.Red
    with
    | :? FormatException as ex ->
        statusLabel.Text <- sprintf "Invalid input: %s" ex.Message
        statusLabel.ForeColor <- Color.Red
    | :? MySqlException as ex ->
        statusLabel.Text <- sprintf "Database error: %s" ex.Message
        statusLabel.ForeColor <- Color.Red
    | ex ->
        statusLabel.Text <- sprintf "Error updating book: %s" ex.Message
        statusLabel.ForeColor <- Color.Red

// Delete a book
let deleteBookById (conn: MySqlConnection) (bookIdTextBox: TextBox) (statusLabel: Label) =
    try
        let bookId = Int32.Parse(bookIdTextBox.Text)
        
        if bookExists conn bookIdTextBox statusLabel then
            MessageBox.Show("You Are Sure To Delete this Book?")
            use cmd = new MySqlCommand("DELETE FROM Book WHERE Book_ID = @bookId", conn)
            cmd.Parameters.AddWithValue("@bookId", bookId) |> ignore
            cmd.ExecuteNonQuery() |> ignore
            statusLabel.Text <-  "Book deleted successfully!"
            statusLabel.ForeColor <- Color.Green
        else
            statusLabel.Text <-  "Book not found in the database."
            statusLabel.ForeColor <- Color.Red
    with
    | ex -> statusLabel.Text <- (sprintf "Error deleting book: %s" ex.Message)
            statusLabel.ForeColor <- Color.Red

// Delete all books
let deleteAllBooks (conn: MySqlConnection) (statusLabel: Label) =
    try
        MessageBox.Show("Are you sure you want to delete all books?")
        use cmd = new MySqlCommand("DELETE FROM Book", conn)
        cmd.ExecuteNonQuery() |> ignore
        statusLabel.Text <- ("All books deleted successfully!")
        statusLabel.ForeColor <- Color.Green
    with
    | ex -> statusLabel.Text <- (sprintf "Error deleting book: %s" ex.Message)
            statusLabel.ForeColor <- Color.Red

// Search book 
let searchBook (conn: MySqlConnection) (typeSearch: string) (bookIdTextBox: TextBox) (bookTextBox: TextBox)  (bookNameTextBox: TextBox) (bookGenreTextBox: TextBox) (authorNameTextBox: TextBox) (copiesTextBox: TextBox) (statusLabel: Label) =
    try
        // let value = bookTextBox.Text
        let (query, parameterName, parameterValue) =
            match typeSearch with
            | "title" -> "SELECT  * FROM Book WHERE Book_Name LIKE @value", "@value", "%" + bookTextBox.Text + "%"
            | "author" -> "SELECT * FROM Book WHERE Author_Name LIKE @value", "@value", "%" + bookTextBox.Text + "%"
            | "genre" -> "SELECT * FROM Book WHERE Book_Type LIKE @value", "@value", "%" + bookTextBox.Text + "%"
            | _ -> raise (ArgumentException "Invalid search choice.")   

        use cmd = new MySqlCommand(query, conn)
        cmd.Parameters.AddWithValue(parameterName, parameterValue) |> ignore
        use reader = cmd.ExecuteReader()
        if reader.Read() then
            let currentBookId = reader.GetInt32(0)
            let currentBookName = reader.GetString(1)
            let currentGenre = reader.GetString(2)
            let currentAuthorName = reader.GetString(3)
            let currentNumberOfCopies = reader.GetInt32(4)

            // Display current values in the text boxes
            bookIdTextBox.Text <- currentBookId.ToString()
            bookNameTextBox.Text <- currentBookName
            bookGenreTextBox.Text <- currentGenre
            authorNameTextBox.Text <- currentAuthorName
            copiesTextBox.Text <- currentNumberOfCopies.ToString()

            reader.Close()

        else  
            statusLabel.Text <- "No books found matching the search criteria."  
            statusLabel.ForeColor <- Color.Red
    with
    | ex -> statusLabel.Text <- sprintf "Error: %s" ex.Message
            statusLabel.ForeColor <- Color.Red

// Availd Books
let listAvailableBooks (conn: MySqlConnection) (listBox: DataGridView) (statusLabel: Label) =
    try
        use cmd = new MySqlCommand("SELECT * FROM Book", conn)
        use reader = cmd.ExecuteReader()
        if reader.HasRows then
            listBox.Rows.Clear()
            while reader.Read() do
                let currentBookId = reader.GetInt32(0)
                let currentBookName = reader.GetString(1)
                let currentGenre = reader.GetString(2)
                let currentAuthorName = reader.GetString(3)
                let currentNumberOfCopies = reader.GetInt32(4)
                listBox.Rows.Add(currentBookId, currentBookName, currentGenre, currentAuthorName, currentNumberOfCopies)
        else
            statusLabel.Text <- "No available books found."
            statusLabel.ForeColor <- Color.Red
    with
    | ex -> statusLabel.Text <- (sprintf "Error displaying available books: %s" ex.Message)
            statusLabel.ForeColor <- Color.Red
