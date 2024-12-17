module imperative.AdminPanel

open System
open System.Windows.Forms
open Microsoft.Data.Sqlite
open MySql.Data.MySqlClient
open System.Drawing
open Connection
open AdminController
open UserController
open Search

// Main Form UI to handle user interaction
let createForm () =
    // Initialize the form with a white background color
    let form = new Form(Text = "Library Management System", AutoSize = true, BackColor = Color.White)

    let addButton = new Button(Text = "Add Books", AutoSize = true, Location = Point(20, 20) , BackColor = Color.Pink , ForeColor = Color.White , Font = new Font("sans", 12.0f) )
    let removeButton = new Button(Text = "Remove Book", AutoSize = true, Location = Point(180, 20) , BackColor = Color.Pink , ForeColor = Color.White , Font = new Font("sans", 12.0f))
    let updateButton = new Button(Text = "Update Book", AutoSize = true, Location = Point(20, 70) , BackColor = Color.Pink , ForeColor = Color.White , Font = new Font("sans", 12.0f))
    let searchButton = new Button(Text = "Search Book", AutoSize = true, Location = Point(180, 70) , BackColor = Color.Pink , ForeColor = Color.White , Font = new Font("sans", 12.0f))
    let availableButton = new Button(Text = "Available Books", AutoSize = true, Location = Point(20, 120) , BackColor = Color.Pink , ForeColor = Color.White , Font = new Font("sans", 12.0f))
    let updateUserButton = new Button(Text = "Update User", AutoSize = true, Location = Point(180, 120) , BackColor = Color.Pink , ForeColor = Color.White , Font = new Font("sans", 12.0f))
    let historyButton = new Button(Text = "History of borrowings", AutoSize = true, Location = Point(20, 170) , BackColor = Color.Pink , ForeColor = Color.White , Font = new Font("sans", 12.0f))

    // let listBox = new ListBox(Location = Point(10, 190), Width = 380, Height = 150)

    // Event Handler to Add Book
    addButton.Click.Add(fun _ -> 
       AdminController.AddForm()
    )
    removeButton.Click.Add(fun _ -> 
       AdminController.RemoveForm()
    )
    updateButton.Click.Add(fun _ -> 
       AdminController.UpdateForm()
    )
    searchButton.Click.Add(fun _ -> 
       Search.mainForm()
    )
    availableButton.Click.Add(fun _ -> 
       AdminController.AvailableBooksForm()
    )
    updateUserButton.Click.Add(fun _ -> 
        UserController.UpdateUserForm()
    )
    historyButton.Click.Add(fun _ -> 
       AdminController.borrowingHistoryForm()
    )


    // Add Controls to Form
    form.Controls.Add(addButton)
    form.Controls.Add(removeButton)
    form.Controls.Add(updateButton)
    form.Controls.Add(searchButton)
    form.Controls.Add(availableButton)
    form.Controls.Add(updateUserButton)
    form.Controls.Add(historyButton)

    form.Show()



