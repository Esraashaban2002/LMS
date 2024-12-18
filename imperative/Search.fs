module Search

open System
open System.Windows.Forms
open Microsoft.Data.Sqlite
open MySql.Data.MySqlClient // Import the MySQL library
open System.Drawing
open Connection
open LibraryController

let ShowDateForm (conn: MySqlConnection) (typeSearch: string) (bookTextBox: TextBox) (statusLabel: Label) =
    let form = statusLabel.FindForm()
    // Create Controls
    let bookIdLabel = new Label(Text = "Book ID:", Location = Point(10, 10), AutoSize = true)
    let bookIdTextBox = new TextBox(Location = Point(100, 10), Width = 200)

    let bookNameLabel = new Label(Text = "Book Name:", Location = Point(10, 40), AutoSize = true)
    let bookNameTextBox = new TextBox(Location = Point(100, 40), Width = 200)
    
    let bookGenreLabel = new Label(Text = "Book Gener:", Location = Point(10, 70), AutoSize = true)
    let bookGenreTextBox = new TextBox(Location = Point(100, 70), Width = 200)
    
    let authorNameLabel = new Label(Text = "Author Name:", Location = Point(10, 100), AutoSize = true)
    let authorNameTextBox = new TextBox(Location = Point(100, 100), Width = 200)

    let copiesLabel = new Label(Text = "Number of Copies:", Location = Point(10, 130), AutoSize = true)
    let copiesTextBox = new TextBox(Location = Point(130, 130), Width = 200)

    // Fetch and display book details
    LibraryController.searchBook conn typeSearch bookIdTextBox bookTextBox bookNameTextBox bookGenreTextBox authorNameTextBox copiesTextBox statusLabel

    // Add Controls to Form
    form.Controls.Add(bookIdLabel)
    form.Controls.Add(bookIdTextBox)
    form.Controls.Add(bookNameLabel)
    form.Controls.Add(bookNameTextBox)
    form.Controls.Add(bookGenreLabel)
    form.Controls.Add(bookGenreTextBox)
    form.Controls.Add(authorNameLabel)
    form.Controls.Add(authorNameTextBox)
    form.Controls.Add(copiesLabel)
    form.Controls.Add(copiesTextBox)


let SearchForm (typeSearch: string) =
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)

    // Create Controls
    let bookLabel = new Label(Text = $"Search By {typeSearch}" , Location = Point(10, 10), AutoSize = true)
    let bookTextBox = new TextBox(Location = Point(130, 10), Width = 200)
    let statusLabel = new Label(Location = Point(10, 160), Width = 400, Height = 30)
    let searchButton = new Button(Text = "Search", AutoSize = true, Location = Point(10, 190), BackColor = Color.LightBlue)

    searchButton.Click.Add(fun _ -> 
        let connectionString = Connection.connectionString
        use conn = new MySqlConnection(connectionString)
        conn.Open()
        ShowDateForm conn typeSearch bookTextBox statusLabel
        form.Controls.Remove(bookLabel)
        form.Controls.Remove(bookTextBox)
    )


    form.Controls.Add(bookLabel)
    form.Controls.Add(bookTextBox)
    form.Controls.Add(statusLabel)
    form.Controls.Add(searchButton)
    form.Show()

let createButton (text: string) (location: Point) (typeSearch: string) =
    let button = new Button(Text = text, AutoSize = true, Location = location, BackColor = Color.LightBlue, ForeColor = Color.White, Font = new Font("sans", 14.0f))
    button.Click.Add(fun _ -> 
        SearchForm typeSearch
    )
    button


let mainForm () =
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)
    
    // إنشاء الأزرار باستخدام دالة createButton
    let searchByTitleButton = createButton "Search By Title" (Point(50, 50)) "title"
    let searchByAuthorButton = createButton "Search By Author" (Point(50, 100)) "author"
    let searchByGenreButton = createButton "Search By Genre" (Point(50, 150)) "genre"

    // إضافة الأزرار إلى النموذج
    form.Controls.Add(searchByTitleButton)
    form.Controls.Add(searchByAuthorButton)
    form.Controls.Add(searchByGenreButton)

    form.Show()

