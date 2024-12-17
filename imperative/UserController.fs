module imperative.UserController

open System
open System.Windows.Forms
open Microsoft.Data.Sqlite
open MySql.Data.MySqlClient
open System.Drawing

open MemberController
open Connection
open BorrowingController

// View Users
let UpdateUserForm()=
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)
    // Create Controls
    let namelLabel = new Label(Text = "Name:", Location = Point(10, 10), AutoSize = true)
    let nameTextBox = new TextBox(Location = Point(100, 10), Width = 200)

    let emailLabel = new Label(Text = "Email:", Location = Point(10, 40), AutoSize = true)
    let emailTextBox = new TextBox(Location = Point(100, 40), Width = 200)

    let passwordLabel = new Label(Text = "Password:", Location = Point(10, 80), AutoSize = true)
    let passwordTextBox = new TextBox(Location = Point(100, 80), Width = 200, PasswordChar = '*')

    let statusLabel = new Label(Location = Point(10, 160), Width = 400, Height = 30)
    
    let updateButton = new Button(Text = "Update User", AutoSize = true, Location = Point(190, 190),BackColor = Color.Blue , ForeColor = Color.White , Font = new Font("sans", 19.0f))
    
    let connectionString = Connection.connectionString
    use conn = new MySqlConnection(connectionString)
    conn.Open() 
    MemberController.showUserDetails conn nameTextBox emailTextBox passwordTextBox statusLabel

    updateButton.Click.Add(fun _ ->
        let connectionString = Connection.connectionString
        use conn = new MySqlConnection(connectionString)
        conn.Open() 
        MemberController.updateUser conn nameTextBox emailTextBox passwordTextBox statusLabel
    )

    // Add Controls to Form
    form.Controls.Add(namelLabel)
    form.Controls.Add(nameTextBox)
    form.Controls.Add(emailLabel)
    form.Controls.Add(emailTextBox)
    form.Controls.Add(passwordLabel)
    form.Controls.Add(passwordTextBox)
    form.Controls.Add(statusLabel)
    form.Controls.Add(updateButton)
    form.Show()

// let showUserForm () =
//     // Initialize the form with a white background color
//     let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)

//     // Create Controls
//     let userEmailLabel = new Label(Text = "Email:", Location = Point(10, 10), AutoSize = true)
//     let uesrEmailTextBox = new TextBox(Location = Point(100, 10), Width = 200)

//     let statusLabel = new Label(Location = Point(10, 160), Width = 400, Height = 30)

//     let showButton = new Button(Text = "Show User Details", AutoSize = true, Location = Point(10, 190), BackColor = Color.LightBlue)

//     // Event Handler to Show Book Details
//     showButton.Click.Add(fun _ -> 
//         let connectionString = Connection.connectionString
//         use conn = new MySqlConnection(connectionString)
//         conn.Open()
//         UpdateUserForm conn uesrEmailTextBox statusLabel
//         form.Controls.Remove(userEmailLabel)
//         form.Controls.Remove(uesrEmailTextBox)
//     )

//     // Add Controls to Form
//     form.Controls.Add(userEmailLabel)
//     form.Controls.Add(uesrEmailTextBox)
//     form.Controls.Add(statusLabel)
//     form.Controls.Add(showButton)

//     form.Show()

let borrowBookForm () =
    // Initialize the form with a white background color
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)

    // Create Controls
    let emailLabel = new Label(Text = "Email:", Location = Point(10, 10), AutoSize = true)
    let emailTextBox = new TextBox(Location = Point(100, 10), Width = 200)
    let bookNameLabel = new Label(Text = "Book Name:", Location = Point(10, 40), AutoSize = true)
    let bookNameTextBox = new TextBox(Location = Point(100, 40), Width = 200)

    let statusLabel = new Label(Location = Point(10, 160), Width = 400, Height = 30)

    let borrowButton = new Button(Text = "Borrowing", AutoSize = true, Location = Point(10, 190), BackColor = Color.Blue)

    // Event Handler to Show Book Details
    borrowButton.Click.Add(fun _ -> 
        let connectionString = Connection.connectionString
        use conn = new MySqlConnection(connectionString)
        conn.Open()
        BorrowingController.borrowBook conn emailTextBox bookNameTextBox statusLabel
    )

    // Add Controls to Form
    form.Controls.Add(emailLabel)
    form.Controls.Add(emailTextBox)
    form.Controls.Add(bookNameLabel)
    form.Controls.Add(bookNameTextBox)
    form.Controls.Add(statusLabel)
    form.Controls.Add(borrowButton)

    form.Show()

let returnBookForm () =
    // Initialize the form with a white background color
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)

    // Create Controls
    let emailLabel = new Label(Text = "Email:", Location = Point(10, 10), AutoSize = true)
    let emailTextBox = new TextBox(Location = Point(100, 10), Width = 200)
    let bookNameLabel = new Label(Text = "Book Name:", Location = Point(10, 40), AutoSize = true)
    let bookNameTextBox = new TextBox(Location = Point(100, 40), Width = 200)

    let statusLabel = new Label(Location = Point(10, 160), Width = 400, Height = 30)

    let returnBookButton = new Button(Text = "Borrowing", AutoSize = true, Location = Point(10, 190), BackColor = Color.Blue)

    // Event Handler to Show Book Details
    returnBookButton.Click.Add(fun _ -> 
        let connectionString = Connection.connectionString
        use conn = new MySqlConnection(connectionString)
        conn.Open()
        BorrowingController.returnBook conn emailTextBox bookNameTextBox statusLabel
    )

    // Add Controls to Form
    form.Controls.Add(emailLabel)
    form.Controls.Add(emailTextBox)
    form.Controls.Add(bookNameLabel)
    form.Controls.Add(bookNameTextBox)
    form.Controls.Add(statusLabel)
    form.Controls.Add(returnBookButton)

    form.Show()
