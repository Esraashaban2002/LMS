module User

open System
open MySql.Data.MySqlClient // Import the MySQL library

open Connction
let connectionString = Connction.connectionString


let showUserDetails conn userId =
    
    let query = "SELECT  name, email, password FROM users WHERE id = @userId"
    let parameters = [ ("@userId", userId) ]

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