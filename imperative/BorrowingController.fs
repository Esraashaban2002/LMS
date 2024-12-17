module imperative.BorrowingController

open System
open Microsoft.Data.Sqlite
open System.Windows.Forms
open System.Drawing
open MySql.Data.MySqlClient


let borrowBook (conn: MySqlConnection) (emailTextBox: TextBox) (bookNameTextBox: TextBox) (statusLabel: Label) =
    try
        let email = emailTextBox.Text
        let bookName = bookNameTextBox.Text

        // Check if the user exists
        use checkUserCmd = new MySqlCommand("SELECT ID FROM Users WHERE Email = @Email", conn)
        checkUserCmd.Parameters.AddWithValue("@Email", email) |> ignore
        let userId = checkUserCmd.ExecuteScalar()

        if userId <> null then
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

                    statusLabel.Text <- "Book borrowed successfully!"
                else
                    statusLabel.Text <- "No copies of this book are available."
            else
                statusLabel.Text <- "Book not found."
        else
            statusLabel.Text <- "User not found."
    with
    | ex -> statusLabel.Text <- (sprintf "Error borrowing book: %s" ex.Message)

let returnBook (conn: MySqlConnection) (emailTextBox: TextBox) (bookNameTextBox: TextBox) (statusLabel: Label) =
    try
        let email = emailTextBox.Text
        let bookName = bookNameTextBox.Text
        // Get User_ID based on the email
        use getUserCmd = new MySqlCommand("SELECT ID FROM Users WHERE Email = @Email", conn)
        getUserCmd.Parameters.AddWithValue("@Email", email) |> ignore
        let userId = getUserCmd.ExecuteScalar()
        if userId <> null then
            // Get Book_ID based on the book name
            use getBookCmd = new MySqlCommand("SELECT Book_ID FROM Book WHERE Book_Name = @BookName", conn)
            getBookCmd.Parameters.AddWithValue("@BookName", bookName) |> ignore
            let bookId = getBookCmd.ExecuteScalar()

            if bookId <> null then
                // Update the borrowing record to set the returned date
                use returnCmd = new MySqlCommand(
                    "UPDATE Borrowings SET Return_Date = CURDATE() 
                     WHERE User_ID = @UserId AND Book_ID = @BookId AND Return_Date IS NULL", 
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

                    statusLabel.Text <- "Book returned successfully!"
                else
                    statusLabel.Text <- "No record found for this borrowing."
            else
                statusLabel.Text <- "Book not found."
        else
            statusLabel.Text <- "User not found."
    with
    | ex -> statusLabel.Text <-(sprintf "Error returning book: %s" ex.Message)

let borrowingHistory (conn: MySqlConnection) (emailTextBox: TextBox) (listBox: DataGridView) (statusLabel: Label) =
    try
        let email = emailTextBox.Text
        listBox.Rows.Clear()
        // Get User_ID based on the email
        use getUserCmd = new MySqlCommand("SELECT ID FROM Users WHERE Email = @Email", conn)
        getUserCmd.Parameters.AddWithValue("@Email", email) |> ignore
        let userId = getUserCmd.ExecuteScalar()
        if userId <> null then
            // Fetch borrowing history for the user
            use cmd = new MySqlCommand(
                "SELECT b.Borrow_ID, bo.Book_Name AS Book_Name, b.Borrow_Date, b.Return_Date 
                 FROM Borrowings b 
                 JOIN Book bo ON b.Book_ID = bo.Book_ID 
                 WHERE b.User_ID = @UserId", 
                conn)
            cmd.Parameters.AddWithValue("@UserId", userId) |> ignore
            use reader = cmd.ExecuteReader()
            if reader.HasRows then
                while reader.Read() do
                    let currentBookName = reader.GetString(1)
                    let currentBorrowedDtate = reader.GetDateTime(2).ToString("yyyy-MM-dd")
                    let currentReturnedDtate = (if not (reader.IsDBNull(3)) then reader.GetDateTime(3).ToString("yyyy-MM-dd") else "Not Returned")
                    listBox.Rows.Add(currentBookName, currentBorrowedDtate, currentReturnedDtate)

            else
                statusLabel.Text <- "No borrowing history found for this user."
        else
            statusLabel.Text <- "User not found."
    with
    | ex -> statusLabel.Text <- (sprintf "Error fetching borrowing history: %s" ex.Message)