module UserPanel

open System
open System.Windows.Forms
open Microsoft.Data.Sqlite
open MySql.Data.MySqlClient
open System.Drawing
open Connection
open Search
open AdminController
open UserController
// Main Form UI to handle user interaction
let createForm () =
    // Initialize the form with a white background color
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)
    let searchButton = new Button(Text = "Search Book", AutoSize = true, Location = Point(20, 20) ,BackColor = Color.Blue , ForeColor = Color.White , Font = new Font("sans", 12.0f))
    let updateUserButton = new Button(Text = "Update User", AutoSize = true, Location = Point(20, 70) ,BackColor = Color.Blue , ForeColor = Color.White , Font = new Font("sans", 12.0f))
    let borrowBookButton = new Button(Text = "Borrow Book", AutoSize = true, Location = Point(180, 30) ,BackColor = Color.Blue , ForeColor = Color.White , Font = new Font("sans", 12.0f))
    let returnBookButton = new Button(Text = "Return Book", AutoSize = true, Location = Point(20, 120) ,BackColor = Color.Blue , ForeColor = Color.White , Font = new Font("sans", 12.0f))
    let histroyButton = new Button(Text = "History of borrowings", AutoSize = true, Location = Point(180, 100) ,BackColor = Color.Blue , ForeColor = Color.White , Font = new Font("sans", 12.0f))

    searchButton.Click.Add(fun _ -> 
       Search.mainForm()
    )
    updateUserButton.Click.Add(fun _ -> 
        UserController.UpdateUserForm()
    )
    borrowBookButton.Click.Add(fun _ -> 
        UserController.borrowBookForm()
    )
    returnBookButton.Click.Add(fun _ -> 
        UserController.returnBookForm()
    )
    histroyButton.Click.Add(fun _ -> 
        UserController.borrowingHistoryForm()
    )


    // Add Controls to Form
    form.Controls.Add(searchButton)
    form.Controls.Add(updateUserButton)
    form.Controls.Add(borrowBookButton)
    form.Controls.Add(returnBookButton)
    form.Controls.Add(histroyButton)

    form.Show()



