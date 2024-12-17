module imperative.AdminController

open System
open System.Windows.Forms
open Microsoft.Data.Sqlite
open MySql.Data.MySqlClient
open System.Drawing
open LibraryController
open Connection
open BorrowingController



// Add book Form
let AddForm () =
    // Initialize the form with a white background color
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)

    // Create Controls
    let bookNameLabel = new Label(Text = "Book Name:", Location = Point(10, 10),AutoSize = true)
    let bookNameTextBox = new TextBox(Location = Point(100, 10), Width = 200)
    
    let bookGenerLabel = new Label(Text = "Book Gener:", Location = Point(10, 40),AutoSize = true)
    let bookGenerTextBox = new TextBox(Location = Point(100, 40), Width = 200)
    
    let authorNameLabel = new Label(Text = "Author Name:", Location = Point(10, 70),AutoSize = true)
    let authorNameTextBox = new TextBox(Location = Point(100, 70), Width = 200)

    let copiesLabel = new Label(Text = "Number of Copies:", Location = Point(10, 100), AutoSize = true)
    let copiesTextBox = new TextBox(Location = Point(130, 100), Width = 200)

    let statusLabel = new Label(Location = Point(10, 130), Width = 400, Height = 30)

    let addButton = new Button(Text = "Add Books", AutoSize = true, Location = Point(10, 160) , BackColor = Color.Pink , ForeColor = Color.White , Font = new Font("sans", 19.0f) )
    // let listBox = new ListBox(Location = Point(10, 190), Width = 380, Height = 150)

    // Event Handler to Add Book
    addButton.Click.Add(fun _ -> 
        let connectionString = Connection.connectionString
        use conn = new MySqlConnection(connectionString)
        conn.Open()
        // You can call a function to add the book to the database here
        LibraryController.addBook conn bookNameTextBox bookGenerTextBox authorNameTextBox copiesTextBox statusLabel
    )


    // Add Controls to Form
    form.Controls.Add(bookNameLabel)
    form.Controls.Add(bookNameTextBox)

    form.Controls.Add(bookGenerLabel)
    form.Controls.Add(bookGenerTextBox)
    
    form.Controls.Add(authorNameLabel)
    form.Controls.Add(authorNameTextBox)
    
    form.Controls.Add(copiesLabel)
    form.Controls.Add(copiesTextBox)
    
    form.Controls.Add(statusLabel)
    form.Controls.Add(addButton)
    // form.Controls.Add(listBox)

    form.Show()


// Update book Form
let UpdateDateForm (conn: MySqlConnection) (bookIdTextBox: TextBox) (statusLabel: Label) =
    let form = bookIdTextBox.FindForm()
    // Create Controls
    let bookNameLabel = new Label(Text = "Book Name:", Location = Point(10, 40), AutoSize = true)
    let bookNameTextBox = new TextBox(Location = Point(100, 40), Width = 200)
    
    let bookGenerLabel = new Label(Text = "Book Gener:", Location = Point(10, 70), AutoSize = true)
    let bookGenerTextBox = new TextBox(Location = Point(100, 70), Width = 200)
    
    let authorNameLabel = new Label(Text = "Author Name:", Location = Point(10, 100), AutoSize = true)
    let authorNameTextBox = new TextBox(Location = Point(100, 100), Width = 200)

    let copiesLabel = new Label(Text = "Number of Copies:", Location = Point(10, 130), AutoSize = true)
    let copiesTextBox = new TextBox(Location = Point(130, 130), Width = 200)

    // Fetch and display book details
    LibraryController.updateBook conn bookIdTextBox bookNameTextBox bookGenerTextBox authorNameTextBox copiesTextBox statusLabel

    // Add Controls to Form
    form.Controls.Add(bookNameLabel)
    form.Controls.Add(bookNameTextBox)
    form.Controls.Add(bookGenerLabel)
    form.Controls.Add(bookGenerTextBox)
    form.Controls.Add(authorNameLabel)
    form.Controls.Add(authorNameTextBox)
    form.Controls.Add(copiesLabel)
    form.Controls.Add(copiesTextBox)

let UpdateForm () =
    // Initialize the form with a white background color
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)

    // Create Controls
    let bookIdLabel = new Label(Text = "Book ID:", Location = Point(10, 10), AutoSize = true)
    let bookIdTextBox = new TextBox(Location = Point(100, 10), Width = 200)

    let statusLabel = new Label(Location = Point(10, 160), Width = 400, Height = 30)

    let showButton = new Button(Text = "Show Book Details", AutoSize = true, Location = Point(10, 190), BackColor = Color.Pink , ForeColor = Color.White , Font = new Font("sans", 19.0f))

    // Event Handler to Show Book Details
    showButton.Click.Add(fun _ -> 
        let connectionString = Connection.connectionString
        use conn = new MySqlConnection(connectionString)
        conn.Open()
        UpdateDateForm conn bookIdTextBox statusLabel
    )

    // Add Controls to Form
    form.Controls.Add(bookIdLabel)
    form.Controls.Add(bookIdTextBox)
    form.Controls.Add(statusLabel)
    form.Controls.Add(showButton)

    form.Show()

// Remove book
let Remove1Form () =
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)

    // Create Controls
    let bookIdLabel = new Label(Text = "Book ID:", Location = Point(10, 10), AutoSize = true)
    let bookIdTextBox = new TextBox(Location = Point(100, 10), Width = 200)
    let statusLabel = new Label(Location = Point(10, 160), Width = 400, Height = 30)
    let removeButton = new Button(Text = "Remove", AutoSize = true, Location = Point(10, 190), BackColor = Color.Pink , ForeColor = Color.White , Font = new Font("sans", 19.0f))

    removeButton.Click.Add(fun _ -> 
        let connectionString = Connection.connectionString
        use conn = new MySqlConnection(connectionString)
        conn.Open()
        LibraryController.deleteBookById conn bookIdTextBox statusLabel
    )

    form.Controls.Add(bookIdLabel)
    form.Controls.Add(bookIdTextBox)
    form.Controls.Add(statusLabel)
    form.Controls.Add(removeButton)
    form.Show()

let RemoveForm () =
    // Initialize the form with a white background color
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)

    // Create Controls
    let statusLabel = new Label(Location = Point(10, 160), Width = 400, Height = 30)
    let removeButton = new Button(Text = "Remove One Book", AutoSize = true, Location = Point(100, 20), BackColor = Color.Pink , ForeColor = Color.White , Font = new Font("sans", 19.0f))
    let removeAllButton = new Button(Text = "Remove All Books", AutoSize = true, Location = Point(100, 100), BackColor = Color.Pink , ForeColor = Color.White , Font = new Font("sans", 19.0f))

    // Event Handler to Show Book Details
    removeButton.Click.Add(fun _ -> 
        Remove1Form()
    )

    removeAllButton.Click.Add(fun _ -> 
        let connectionString = Connection.connectionString
        use conn = new MySqlConnection(connectionString)
        conn.Open()
        LibraryController.deleteAllBooks conn statusLabel
    )

    // Add Controls to Form
    form.Controls.Add(statusLabel)
    form.Controls.Add(removeButton)
    form.Controls.Add(removeAllButton)

    form.Show()

// Available books
let AvailableBooksForm () =
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)

    // Create Controls
    let statusLabel = new Label(Location = Point(10, 10), AutoSize= true)
    let listBox = new DataGridView(Location = Point(10, 40), AutoSize= true , AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill)

    // Add columns to DataGridView
    listBox.Columns.Add("BookId", "Book ID")
    listBox.Columns.Add("BookName", "Book Name")
    listBox.Columns.Add("Genre", "Genre")
    listBox.Columns.Add("Author", "Author Name")
    listBox.Columns.Add("Copies", "Number of Copies")

    let connectionString = Connection.connectionString
    use conn = new MySqlConnection(connectionString)
    conn.Open()

    LibraryController.listAvailableBooks conn listBox statusLabel

    form.Controls.Add(statusLabel)
    form.Controls.Add(listBox)
    form.Show()

let borrowingHistoryForm () =
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)

    // Create Controls
    let emailLabel = new Label(Text = "Email:", Location = Point(10, 10), AutoSize = true)
    let emailTextBox = new TextBox(Location = Point(100, 10), Width = 200)
     
    let statusLabel = new Label(Location = Point(10, 40), AutoSize= true)
    let showButton = new Button(Text = "Show History", AutoSize = true, Location = Point(10, 80), BackColor = Color.Pink , ForeColor = Color.White , Font = new Font("sans", 19.0f))
    let listBox = new DataGridView(Location = Point(10, 120), AutoSize= true , AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill)

    // Add columns to DataGridView
    listBox.Columns.Add("BookName", "Book Name")
    listBox.Columns.Add("BorrowedDtate", "Borrowed Dtate")
    listBox.Columns.Add("ReturnedDtate", "Returned Dtate")

    showButton.Click.Add(fun _ ->
        let connectionString = Connection.connectionString
        use conn = new MySqlConnection(connectionString)
        conn.Open()

        BorrowingController.borrowingHistoryAdmin conn emailTextBox listBox statusLabel
    )

    form.Controls.Add(emailLabel)
    form.Controls.Add(emailTextBox)
    form.Controls.Add(statusLabel)
    form.Controls.Add(showButton)
    form.Controls.Add(listBox)
    form.Show()    